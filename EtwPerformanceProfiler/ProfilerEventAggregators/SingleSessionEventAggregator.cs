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
    internal class SingleSessionEventAggregator : EventAggregator
    {
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

        /// <summary>
        /// The key is the string and the value is exactly the same string.
        /// The idea is that for the equal strings we will use exactly the same string object.
        /// It saves memory because event list contains a lot of identical strings. 
        /// </summary>
        private readonly Dictionary<string, string> statementCache;

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
        internal void Initialize()
        {
            this.aggregatedCallTree = new AggregatedEventNode()
            {
                StatementName = "Session " + this.profilingSessionId
            };

            this.currentAggregatedEventNode = aggregatedCallTree;

            this.previousProfilerEvent = null;
        }

        /// <summary>
        /// Finishes aggregation.
        /// </summary>
        /// <param name="buildAggregatedCallTree">true if the aggregated call is to be built.</param>
        internal void FinishAggregation(bool buildAggregatedCallTree = true)
        {
            if (buildAggregatedCallTree)
            {
                AddProfilerEventToAggregatedCallTree(this.previousProfilerEvent, null, ref this.currentAggregatedEventNode);

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
        internal IEnumerable<AggregatedEventNode> FlattenCallTree()
        {
            return FlattenCallTree(this.aggregatedCallTree);
        }

        /// <summary>
        /// The callback which is called every time new event appears.
        /// </summary>
        /// <param name="traceEvent">The trace event.</param>
        internal void AddEtwEventToAggregatedCallTree(TraceEvent traceEvent)
        {
            int statementIndex;
            EventType eventType;
            if (!GetStatementIndexAndEventType(traceEvent, out statementIndex, out eventType))
            {
                return;
            }

            // We can check sessions id only here after we filtered out non Nav events.
            int sessionId = GetSessionId(traceEvent);

            this.AddEtwEventToAggregatedCallTree(traceEvent, sessionId, statementIndex, eventType);
        }

        internal void AddEtwEventToAggregatedCallTree(TraceEvent traceEvent, int sessionId, int statementIndex, EventType eventType)
        {
            if (sessionId != this.profilingSessionId)
            {
                // we are interested only in events for the profiling session
                return;
            }

            string objectType = string.Empty;
            int objectId = 0;
            if (statementIndex != NavEventsPayloadIndexes.SqlStatementPayloadIndex &&
                statementIndex != NavEventsPayloadIndexes.ConnectionTypePayloadIndex)
            {
                // We don't have object type and id for the SQL events.

                objectType = (string) traceEvent.PayloadValue(NavEventsPayloadIndexes.ObjectTypePayloadIndex);
                objectId = (int) traceEvent.PayloadValue(NavEventsPayloadIndexes.ObjectIdPayloadIndex);
            }

            int lineNo = 0;
            if ((int) traceEvent.ID == NavEvents.ALFunctionStatement)
            {
                // Only statements have line numbers.

                lineNo = (int) traceEvent.PayloadValue(NavEventsPayloadIndexes.LineNoPayloadIndex);
            }

            string statement = (string) traceEvent.PayloadValue(statementIndex);

            statement = this.GetStatementFromTheCache(statement);

            ProfilerEvent? currentProfilerEvent = new ProfilerEvent
            {
                Type = eventType,
                ObjectType = objectType,
                ObjectId = objectId,
                LineNo = lineNo,
                StatementName = statement,
                TimeStampRelativeMSec = traceEvent.TimeStampRelativeMSec
            };

            AddProfilerEventToAggregatedCallTree(this.previousProfilerEvent, currentProfilerEvent, ref this.currentAggregatedEventNode);

            this.previousProfilerEvent = currentProfilerEvent;
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
        internal static void AddProfilerEventToAggregatedCallTree(
            ProfilerEvent? previousProfilerEvent,
            ProfilerEvent? currentProfilerEvent,
            ref AggregatedEventNode currentAggregatedEventNode)
        {
            if (previousProfilerEvent.HasValue && previousProfilerEvent.Value.Type == EventType.StopMethod)
            {
                // We have alrady calculated duration for the previous statement
                // and pop it from the statement call stack. 

                if (currentAggregatedEventNode.Parent != null && // We should never pop root event. This can happen if we miss some events in the begining.
                    (!currentProfilerEvent.HasValue || // Previous event is the last one.
                     (currentProfilerEvent.Value.IsNoneAlEvent && !previousProfilerEvent.Value.IsNoneAlEvent) || // The current event is sql. It comes after stop event. Need to pop the the current aggregated node. Only close AL events.
                     currentProfilerEvent.Value.Type != EventType.StartMethod || // The current event is not the start. If it is start event when we are in the nested call. One statement several methods.
                     currentAggregatedEventNode.OriginalType == EventType.StartMethod)) // ??????
                {
                    currentAggregatedEventNode = currentAggregatedEventNode.PopEventFromCallStackAndCalculateDuration(previousProfilerEvent.Value.TimeStampRelativeMSec);
                }
            }

            if (!currentProfilerEvent.HasValue)
            {
                return;
            }

            // Statement.
            if (currentProfilerEvent.Value.Type == EventType.Statement)
            {
                // If there are two consequent statements then first we need to calculate the duration for the previous
                // one and pop it from the statement call stack. 
                // Then we push the current statement event into the stack
                if (currentAggregatedEventNode.Parent != null && currentAggregatedEventNode.EvaluatedType == EventType.Statement)
                {
                    currentAggregatedEventNode = currentAggregatedEventNode.PopEventFromCallStackAndCalculateDuration(currentProfilerEvent.Value.TimeStampRelativeMSec);
                }

                currentAggregatedEventNode = currentAggregatedEventNode.PushEventIntoCallStack(currentProfilerEvent.Value);

                return;
            }

            // Method start.
            if (currentProfilerEvent.Value.Type == EventType.StartMethod)
            {
                // If it is the root method or if it is SQL event we also push start event into the stack.
                if (currentAggregatedEventNode.Parent == null || 
                    currentProfilerEvent.Value.IsNoneAlEvent || 
                    (previousProfilerEvent.HasValue && previousProfilerEvent.Value.IsNoneAlEvent))
                {
                    currentAggregatedEventNode = currentAggregatedEventNode.PushEventIntoCallStack(currentProfilerEvent.Value);
                }

                currentAggregatedEventNode.EvaluatedType = EventType.StartMethod;

                return;
            }

            // Method stop.
            if (currentProfilerEvent.Value.Type == EventType.StopMethod)
            { 
                if (currentAggregatedEventNode.Parent != null)
                {
                    if (currentAggregatedEventNode.EvaluatedType == EventType.Statement || // We need to calculate duration for the previous statement and pop it from the statement call stack.
                        currentProfilerEvent.Value.IsNoneAlEvent) // Always close sql events.
                    {
                        currentAggregatedEventNode = currentAggregatedEventNode.PopEventFromCallStackAndCalculateDuration(currentProfilerEvent.Value.TimeStampRelativeMSec);
                    }
                }
            }
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
    }
}
