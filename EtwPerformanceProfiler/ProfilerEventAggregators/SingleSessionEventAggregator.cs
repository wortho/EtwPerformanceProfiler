//--------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//--------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Diagnostics.Tracing;

namespace EtwPerformanceProfiler
{
    /// <summary>
    /// This class is responsible for aggregating events and building the call tree.
    /// it processes events from single session. 
    /// </summary>
    internal class SingleSessionEventAggregator : EventAggregator, IEventAggregator
    {
        internal const string StartEventIsMissing = "Start event is missing: ";
        internal const string StopEventIsMissing = "Stop event is missing: ";

        #region Private members

        /// <summary>
        /// The profiling session id
        /// </summary>
        private readonly int profilingSessionId;

        /// <summary>
        /// The threashold value. The aggregated call three will be filtered on values greater than the threshold.
        /// </summary>
        private readonly long threshold;

        /// <summary>
        /// The cumulated aggregated call tree
        /// </summary>
        private AggregatedEventNode aggregatedCallTree;

        /// <summary>
        /// Current aggregated event in the call tree.
        /// </summary>
        private AggregatedEventNode currentAggregatedEventNode;

        /// <summary>
        /// Previous profiler event which was processed.
        /// </summary>
        ProfilerEvent? previousProfilerEvent;

        private TraceEvent previousTraceEvent;

        //TODO: Move to separate class
        /// <summary>
        /// The key is the string and the value is exactly the same string.
        /// The idea is that for the equal strings we will use exactly the same string object.
        /// It saves memory because event list contains a lot of identical strings. 
        /// </summary>
        private readonly Dictionary<string, string> statementCache;

        private bool firstEvent;

        /// <summary>
        /// <c>true</c> if event processing is suspended.
        /// </summary>
        private bool suspended = false;

        #endregion

        /// <summary>
        /// Creates a new instance of the <see cref="SingleSessionEventAggregator"/> class.
        /// </summary>
        /// <param name="sessionId">The session id.</param>
        /// <param name="threshold">The threshold value. The aggregated call tree will only show events greater than this.</param>
        internal SingleSessionEventAggregator(int sessionId, long threshold = 0)
        {
            this.profilingSessionId = sessionId;

            this.threshold = threshold;

            this.statementCache = new Dictionary<string, string>();
        }

        /// <summary>
        /// Initializes state of the <see cref="DynamicProfilerEventProcessor"/>
        /// </summary>
        public void Initialize()
        {
            // create the root node for the session
            this.aggregatedCallTree = new AggregatedEventNode
            {
                StatementName = "Session: " + this.profilingSessionId + ";",
                SessionId = this.profilingSessionId
            };

            this.currentAggregatedEventNode = aggregatedCallTree;

            this.previousProfilerEvent = null;

            this.firstEvent = true;
        }

        /// <summary>
        /// Finishes aggregation.
        /// </summary>
        /// <param name="buildAggregatedCallTree">true if the aggregated call is to be built.</param>
        public void FinishAggregation(bool buildAggregatedCallTree = true)
        {
            if (buildAggregatedCallTree)
            {
                AddProfilerEventToAggregatedCallTree(this.previousProfilerEvent, null, ref this.currentAggregatedEventNode);

                this.aggregatedCallTree.CalcMinMaxRelativeTimeStampMSec();

                if (this.aggregatedCallTree != null)
                {
                    this.ReduceTree(this.aggregatedCallTree);
                }
            }
        }

        /// <summary>
        /// Traverses the call stack tree.
        /// </summary>
        /// <returns>Flatten call tree.</returns>
        public IEnumerable<AggregatedEventNode> FlattenCallTree()
        {
            // Update duration on root node
            this.aggregatedCallTree.DurationMSec = 0;
            foreach (var aggregatedEventNode in this.aggregatedCallTree.Children)
            {
                this.aggregatedCallTree.DurationMSec += aggregatedEventNode.DurationMSec;
            }

            return FlattenCallTree(this.aggregatedCallTree);
        }

        /// <summary>
        /// The callback which is called every time new event appears.
        /// </summary>
        /// <param name="traceEvent">The trace event.</param>
        public void AddEtwEventToAggregatedCallTree(TraceEvent traceEvent)
        {
            if (this.suspended)
            {
                return;
            }

            int statementIndex;
            EventType eventType;
            EventSubType eventSubType;
            string statement;
            if (!GetStatementIndexAndEventType(traceEvent, out statementIndex, out statement, out eventType, out eventSubType))
            {
                return;
            }

            // We can check sessions id only here after we filtered out non Nav events.
            int sessionId = GetSessionId(traceEvent);

            this.AddEtwEventToAggregatedCallTree(traceEvent, sessionId, statementIndex, statement, eventType, eventSubType);
        }

        /// <summary>
        /// Calculates maximum relative time stamp.
        /// </summary>
        /// <returns>Maximum relative time stamp.</returns>
        public double MaxRelativeTimeStamp()
        {
            return this.aggregatedCallTree.MaxRelativeTimeStampMSec;
        }

        internal void AddEtwEventToAggregatedCallTree(TraceEvent traceEvent, int sessionId, int statementIndex, string statementName, EventType eventType, EventSubType eventSubType)
        {
            if (sessionId != this.profilingSessionId)
            {
                // we are interested only in events for the profiling session
                return;
            }

            if (this.firstEvent)
            {
                this.firstEvent = false;

                this.aggregatedCallTree.StatementName += " User: " + GetUserName(traceEvent) + ";";
            }

            string objectType = string.Empty;
            int objectId = 0;
            if (eventSubType == EventSubType.AlEvent)
            {
                // We don't have object type and id for the non AL events.

                objectType = (string)traceEvent.PayloadValue(NavEventsPayloadIndexes.ObjectTypePayloadIndex);
                objectId = (int)traceEvent.PayloadValue(NavEventsPayloadIndexes.ObjectIdPayloadIndex);
            }

            int lineNo = 0;
            if ((int)traceEvent.ID == NavEvents.ALFunctionStatement)
            {
                // Only statements have line numbers.

                lineNo = (int)traceEvent.PayloadValue(NavEventsPayloadIndexes.LineNoPayloadIndex);
            }

            string statement;
            if (statementIndex == NavEventsPayloadIndexes.NonPayloadIndex)
            {
                statement = statementName;
            }
            else
            {
                if (!string.IsNullOrEmpty(statementName))
                {
                    statement = statementName + (string)traceEvent.PayloadValue(statementIndex);
                }
                else
                {
                    statement = (string)traceEvent.PayloadValue(statementIndex);
                }
            }

            statement = this.GetStatementFromTheCache(statement);

            ProfilerEvent? currentProfilerEvent = new ProfilerEvent
            {
                SessionId = sessionId,
                Type = eventType,
                SubType = eventSubType,
                ObjectType = objectType,
                ObjectId = objectId,
                LineNo = lineNo,
                StatementName = statement,
                TimeStampRelativeMSec = traceEvent.TimeStampRelativeMSec
            };

            if (AddProfilerEventToAggregatedCallTree(this.previousProfilerEvent, currentProfilerEvent, ref this.currentAggregatedEventNode))
            {
                this.previousProfilerEvent = currentProfilerEvent;
            }

            this.previousTraceEvent = traceEvent;
        }

        /// <summary>
        /// For the equal strings function returns exactly the same string object with the value equal to the parameter.
        /// It saves memory because event list contains a lot of identical strings. 
        /// </summary>
        /// <param name="statement">The key value.</param>
        /// <returns>The cached string value.</returns>
        internal string GetStatementFromTheCache(string statement)
        {
            string cachedStatement;

            if (this.statementCache.TryGetValue(statement, out cachedStatement))
            {
                return cachedStatement;
            }

            this.statementCache[statement] = statement;

            return statement;
        }

        /// <summary>
        /// Processes aggregated event. This method calls either <see cref="AggregatedEventNode.PushEventIntoCallStack"/> or 
        /// <see cref="AggregatedEventNode.PopEventFromCallStackAndCalculateDuration"/> on the <see cref="currentAggregatedEventNode"/>.
        /// </summary>
        /// <param name="previousProfilerEvent">Previous profiler event which was processed.</param>
        /// <param name="currentProfilerEvent">Profiler event which currently being processed.</param>
        /// <param name="currentAggregatedEventNode">Current aggregated event in the call tree.</param>
        /// <returns>Returns <c>true</c> if event was valid and should not be skipped.</returns>
        internal static bool AddProfilerEventToAggregatedCallTree(
            ProfilerEvent? previousProfilerEvent,
            ProfilerEvent? currentProfilerEvent,
            ref AggregatedEventNode currentAggregatedEventNode)
        {
            bool skipCurrentEvent;
            if (PopEventFromCallStackIfSomeEventsWereMissing(previousProfilerEvent, currentProfilerEvent, ref currentAggregatedEventNode, out skipCurrentEvent))
            {
                return skipCurrentEvent;
            }

            PopEventFromCallStackForPreviousAlStopMethodEvent(previousProfilerEvent, currentProfilerEvent, ref currentAggregatedEventNode);

            if (!currentProfilerEvent.HasValue)
            {
                return true;
            }

            switch (currentProfilerEvent.Value.Type)
            {
                case EventType.Statement:
                    return AddStatementToAggregatedCallTree(currentProfilerEvent, ref currentAggregatedEventNode);
                case EventType.StartMethod:
                    return AddStartMethodToAggregatedCallTree(previousProfilerEvent, currentProfilerEvent, ref currentAggregatedEventNode);
                case EventType.StopMethod:
                    return AddStopMethodToAggregatedCallTree(currentProfilerEvent, ref currentAggregatedEventNode);
            }

            return true;
        }

        private static bool PopEventFromCallStackIfSomeEventsWereMissing(
            ProfilerEvent? previousProfilerEvent,
            ProfilerEvent? currentProfilerEvent,
            ref AggregatedEventNode currentAggregatedEventNode,
            out bool skipCurrentEvent)
        {
            if (currentAggregatedEventNode.Parent != null &&
                currentAggregatedEventNode.OriginalType == EventType.StartMethod &&
                currentAggregatedEventNode.SubType == EventSubType.SqlEvent &&
                currentProfilerEvent.HasValue && !(currentProfilerEvent.Value.Type == EventType.StopMethod && currentProfilerEvent.Value.SubType == EventSubType.SqlEvent))
            {
                // TODO: Here we have two consecutive start events. First event is of the SQL subtype. This should never happen because SQL queries cannot be nested.
                // TODO: It could indicates an issue in evening.
                // TODO: Need to pop previous event and push current.

                ProfilerEvent missingProfilerEvent = new ProfilerEvent
                {
                    SessionId = currentAggregatedEventNode.SessionId,
                    ObjectType = currentAggregatedEventNode.ObjectType,
                    ObjectId = currentAggregatedEventNode.ObjectId,
                    LineNo = currentAggregatedEventNode.LineNo,
                    Type = EventType.StartMethod,
                    StatementName = StopEventIsMissing + currentAggregatedEventNode.StatementName
                };

                currentAggregatedEventNode = currentAggregatedEventNode.PopEventFromCallStackAndCalculateDuration(previousProfilerEvent.Value.TimeStampRelativeMSec);

                currentAggregatedEventNode = currentAggregatedEventNode.PushEventIntoCallStack(missingProfilerEvent);
                currentAggregatedEventNode = currentAggregatedEventNode.PopEventFromCallStackAndCalculateDuration(missingProfilerEvent.TimeStampRelativeMSec);

                if (currentProfilerEvent.Value.Type == EventType.StartMethod || currentProfilerEvent.Value.Type == EventType.Statement)
                {
                    currentAggregatedEventNode = currentAggregatedEventNode.PushEventIntoCallStack(currentProfilerEvent.Value);
                }

                skipCurrentEvent = true;
                return true;
            }

            if (currentAggregatedEventNode.Parent != null &&
                previousProfilerEvent.HasValue && previousProfilerEvent.Value.Type == EventType.StopMethod &&
                currentProfilerEvent.HasValue && currentProfilerEvent.Value.Type == EventType.StopMethod &&
                currentProfilerEvent.Value.IsAlEvent != currentAggregatedEventNode.IsAlEvent)
            {
                //TODO: We hit this block for example in the case if Codeunit 1 trigger is called and it does not have any code.
                //TODO: We should consider if we want to fix it in the product.
                // Skip this event. Should never happen. Indicates a issue in the event generation. 
                // Some events were missed.

                // Create fake start event.
                ProfilerEvent profilerEvent = currentProfilerEvent.Value;
                profilerEvent.Type = EventType.StartMethod;
                profilerEvent.StatementName = StartEventIsMissing + profilerEvent.StatementName;

                currentAggregatedEventNode = currentAggregatedEventNode.PushEventIntoCallStack(profilerEvent);
                currentAggregatedEventNode = currentAggregatedEventNode.PopEventFromCallStackAndCalculateDuration(currentProfilerEvent.Value.TimeStampRelativeMSec);

                skipCurrentEvent = false;
                return true;
            }

            skipCurrentEvent = false;
            return false;
        }

        private static void PopEventFromCallStackForPreviousAlStopMethodEvent(
            ProfilerEvent? previousProfilerEvent,
            ProfilerEvent? currentProfilerEvent,
            ref AggregatedEventNode currentAggregatedEventNode)
        {
            if (previousProfilerEvent.HasValue && previousProfilerEvent.Value.Type == EventType.StopMethod &&
                previousProfilerEvent.Value.SubType != EventSubType.SqlEvent)
            {
                // We have already calculated duration for the previous statement
                // and pop it from the statement call stack. 

                // We should never pop root event. This can happen if we miss some events in the begining.
                if (currentAggregatedEventNode.Parent != null)
                {
                    if (!currentProfilerEvent.HasValue || // Previous event is the last one.
                        // The current event is none AL event. It comes after stop event. Need to pop the the current aggregated node. Only close AL events.
                        (currentProfilerEvent.Value.IsNonAlEvent && previousProfilerEvent.Value.IsAlEvent) ||
                        // Here we have statement after function call.
                        currentProfilerEvent.Value.Type == EventType.Statement ||
                        // Here we have funtion call in the end of other function.
                        currentProfilerEvent.Value.Type == EventType.StopMethod ||
                        // If we have two consecutive root method calls
                        (currentAggregatedEventNode.OriginalType == EventType.StartMethod &&
                         currentAggregatedEventNode.IsAlEvent))
                    {
                        currentAggregatedEventNode = currentAggregatedEventNode.PopEventFromCallStackAndCalculateDuration(previousProfilerEvent.Value.TimeStampRelativeMSec);
                    }
                }
            }
        }

        private static bool AddStatementToAggregatedCallTree(ProfilerEvent? currentProfilerEvent, ref AggregatedEventNode currentAggregatedEventNode)
        {
            // If there are two consecutive statements then first we need to calculate the duration for the previous
            // one and pop it from the statement call stack. 
            // Then we push the current statement event into the stack
            if (currentAggregatedEventNode.Parent != null && currentAggregatedEventNode.EvaluatedType == EventType.Statement)
            {
                currentAggregatedEventNode = currentAggregatedEventNode.PopEventFromCallStackAndCalculateDuration(currentProfilerEvent.Value.TimeStampRelativeMSec);
            }

            currentAggregatedEventNode = currentAggregatedEventNode.PushEventIntoCallStack(currentProfilerEvent.Value);

            return true;
        }

        private static bool AddStartMethodToAggregatedCallTree(ProfilerEvent? previousProfilerEvent, ProfilerEvent? currentProfilerEvent, ref AggregatedEventNode currentAggregatedEventNode)
        {
            // If it is the root method or if it is non AL event we also push start event into the stack.
            if (currentAggregatedEventNode.Parent == null ||
                currentProfilerEvent.Value.IsNonAlEvent ||
                (previousProfilerEvent.HasValue && previousProfilerEvent.Value.IsNonAlEvent))
            {
                currentAggregatedEventNode = currentAggregatedEventNode.PushEventIntoCallStack(currentProfilerEvent.Value);
            }

            currentAggregatedEventNode.EvaluatedType = EventType.StartMethod;

            return true;
        }

        private static bool AddStopMethodToAggregatedCallTree(ProfilerEvent? currentProfilerEvent, ref AggregatedEventNode currentAggregatedEventNode)
        {
            if (currentAggregatedEventNode.Parent != null)
            {
                // We need to calculate duration for the previous statement and pop it from the statement call stack.
                if (currentAggregatedEventNode.EvaluatedType == EventType.Statement ||
                    // Always close non events.
                    currentProfilerEvent.Value.IsNonAlEvent)
                {
                    currentAggregatedEventNode = currentAggregatedEventNode.PopEventFromCallStackAndCalculateDuration(currentProfilerEvent.Value.TimeStampRelativeMSec);
                }
            }

            return true;
        }

        /// <summary>
        /// Traverses the call stack tree.
        /// </summary>
        /// <param name="aggregatedCallTree">Aggregated call tree to traverse.</param>
        /// <returns>Flatten call tree.</returns>
        private static IEnumerable<AggregatedEventNode> FlattenCallTree(AggregatedEventNode aggregatedCallTree)
        {
            yield return aggregatedCallTree;

            foreach (AggregatedEventNode node in aggregatedCallTree.Children)
            {
                foreach (AggregatedEventNode childNode in FlattenCallTree(node))
                {
                    yield return childNode;
                }
            }
        }

        /// <summary>
        /// Traverses the call stack tree.
        /// </summary>
        /// <returns>reduces the tree by removing all nodes that are below the threshold call tree.</returns>
        private void ReduceTree(AggregatedEventNode rootNode)
        {
            if (this.threshold <= 0)
            {
                return;
            }

            for (int index = 0; index < rootNode.Children.Count; index++)
            {
                AggregatedEventNode node = rootNode.Children[index];
                if (node.DurationMSec < this.threshold)
                {
                    node.Children.Clear();
                    rootNode.Children.Remove(node);
                    index--;
                }
                else
                {
                    this.ReduceTree(node);
                }
            }
        }

        /// <summary>
        /// Suspend event processing.
        /// </summary>
        public void Suspend()
        {
            this.suspended = true;
        }

        /// <summary>
        /// Resume event processing.
        /// </summary>
        public void Resume()
        {
            this.suspended = false;
        }
    }
}