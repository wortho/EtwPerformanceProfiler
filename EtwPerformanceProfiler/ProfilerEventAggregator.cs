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
    /// </summary>
    internal class ProfilerEventAggregator
    {
        #region SQL events defeined by the Nav server
        /// <summary>
        /// The SQL execute scalar start event.
        /// </summary>
        internal const int SqlExecuteScalarStart = 1;
        
        /// <summary>
        /// The SQL execute scalar stop event.
        /// </summary>
        internal const int SqlExecuteScalarStop = 2;
        
        /// <summary>
        /// The SQL execute non query start event.
        /// </summary>
        internal const int SqlExecuteNonQueryStart = 3;
        
        /// <summary>
        /// The SQL execute non query stop event.
        /// </summary>
        internal const int SqlExecuteNonQueryStop = 4;
        
        /// <summary>
        /// The event id for the SQL execute reader start event.
        /// </summary>
        internal const int SqlExecuteReaderStart = 5;
        
        /// <summary>
        /// The event id for the SQL execute reader stop event.
        /// </summary>
        internal const int SqlExecuteReaderStop = 6;
        
        /// <summary>
        /// The event id for the SQL read next result start event.
        /// </summary>
        internal const int SqlReadNextResultStart = 7;
        
        /// <summary>
        /// The event id for the SQL read next result stop event
        /// </summary>
        internal const int SqlReadNextResultStop = 8;
        
        /// <summary>
        /// The event id for the SQL read next row start event
        /// </summary>
        internal const int SqlReadNextRowStart = 9;
        
        /// <summary>
        /// The event id for the SQL read next row stop event
        /// </summary>
        internal const int SqlReadNextRowStop = 10;
        
        /// <summary>
        /// The event id for the SQL begin transaction start event
        /// </summary>
        internal const int SqlBeginTransactionStart = 11;
        
        /// <summary>
        /// The event id for the SQL begin transaction stop event.
        /// </summary>
        internal const int SqlBeginTransactionStop = 12;

        /// <summary>
        /// The event id for the SQL prepare start event.
        /// </summary>
        internal const int SqlPrepareStart = 13;
        
        /// <summary>
        /// The event id for the SQL prepare stop event event.
        /// </summary>
        internal const int SqlPrepareStop = 14;
        
        /// <summary>
        /// The event id for the SQL open connection start event.
        /// </summary>
        internal const int SqlOpenConnectionStart = 15;

        /// <summary>
        /// The event id for the SQL open connection stop event.
        /// </summary>
        internal const int SqlOpenConnectionStop = 16;

        /// <summary>
        /// The event id for the SQL commit start event.
        /// </summary>
        internal const int SqlCommitStart = 17;
        
        /// <summary>
        /// The event id for the SQL commit stop event
        /// </summary>
        internal const int SqlCommitStop = 18;
        
        /// <summary>
        /// The event id for the SQL rollback start event.
        /// </summary>
        internal const int SqlRollbackStart = 19;
        
        /// <summary>
        /// The event id for the SQL rollback stop event
        /// </summary>
        internal const int SqlRollbackStop = 20;
        #endregion

        #region AL events defined by the Nav server.
        /// <summary>
        /// The event id for the AL method start event.
        /// </summary>
        internal const int ALFunctionStart = 400;

        /// <summary>
        /// An event id for AL method stop event.
        /// </summary>
        internal const int ALFunctionStop = 401;

        /// <summary>
        /// An event id for failed AL methods.
        /// </summary>
        internal const int AFunctionFailed = 402;

        /// <summary>
        /// The event id for the AL Statement event.
        /// </summary>
        internal const int ALFunctionStatement = 403;
        #endregion

        #region Payload indexes
        /// <summary>
        /// The index of the tenant id payload parameter as defined in the ETW manifest.
        /// </summary>
        private const int TenantIdPayloadIndex = 0;

        /// <summary>
        /// The index of the session id payload parameter as defined in the ETW manifest.
        /// </summary>
        private const int SessionIdPayloadIndex = 1;

        /// <summary>
        /// The index of the user name payload parameter as defined in the ETW manifest.
        /// </summary>
        private const int UserNamePayloadIndex = 2;

        /// <summary>
        /// The index of the SQL statement payload parameter as defined in the ETW manifest.
        /// </summary>
        private const int SqlStatementPayloadIndex = 3;

        /// <summary>
        /// The index of the object type payload parameter as defined in the ETW manifest.
        /// </summary>
        private const int ObjectTypePayloadIndex = 3;

        /// <summary>
        /// The index of the object id payload parameter as defined in the ETW manifest.
        /// </summary>
        private const int ObjectIdPayloadIndex = 4;

        /// <summary>
        /// The index of the function name payload parameter as defined in the ETW manifest.
        /// </summary>
        private const int ALFunctionNamePayloadIndex = 5;

        /// <summary>
        /// The index of the line number payload parameter as defined in the ETW manifest.
        /// </summary>
        private const int LineNoPayloadIndex = 6;

        /// <summary>
        /// The index of the statement payload parameter as defined in the ETW manifest.
        /// </summary>
        private const int ALStatementPayloadIndex = 7;
        #endregion

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
        /// Initializes a new instance of the <see cref="ProfilerEventAggregator"/> class.
        /// </summary>
        /// <param name="sessionId">The session id.</param>
        /// <param name="threshold">The threshold value. The aggregated call tree will only show events greater than this.</param>
        internal ProfilerEventAggregator(int sessionId, long threshold = 0)
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
            this.aggregatedCallTree = new AggregatedEventNode();
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
            int sessionId = (int)traceEvent.PayloadValue(SessionIdPayloadIndex);

            if (sessionId != this.profilingSessionId)
            {
                // we are interested only in events for the profiling session
                return;
            }

            int statementLoadIndex;
            EventType type;
            switch ((int)traceEvent.ID)
            {
                case ALFunctionStart:
                    statementLoadIndex = ALFunctionNamePayloadIndex;
                    type = EventType.StartMethod;
                    break;
                case ALFunctionStop:
                    statementLoadIndex = ALFunctionNamePayloadIndex;
                    type = EventType.StopMethod;
                    break;
                case ALFunctionStatement:
                    statementLoadIndex = ALStatementPayloadIndex;
                    type = EventType.Statement;
                    break;
                case SqlExecuteScalarStart:
                    statementLoadIndex = SqlStatementPayloadIndex;
                    type = EventType.StartMethod;
                    break;
                case SqlExecuteScalarStop:
                    statementLoadIndex = SqlStatementPayloadIndex;
                    type = EventType.StopMethod;
                    break;
                case SqlExecuteNonQueryStart:
                    statementLoadIndex = SqlStatementPayloadIndex;
                    type = EventType.StartMethod;
                    break;
                case SqlExecuteNonQueryStop:
                    statementLoadIndex = SqlStatementPayloadIndex;
                    type = EventType.StopMethod;
                    break;
                case SqlExecuteReaderStart:
                    statementLoadIndex = SqlStatementPayloadIndex;
                    type = EventType.StartMethod;
                    break;
                case SqlExecuteReaderStop:
                    statementLoadIndex = SqlStatementPayloadIndex;
                    type = EventType.StopMethod;
                    break;
                default:
                    return;
            }

            string objectType = string.Empty;
            int objectId = 0;            
            if (statementLoadIndex != SqlStatementPayloadIndex)
            {
                // We don't have object type and id for the SQL events.

                objectType = (string)traceEvent.PayloadValue(ObjectTypePayloadIndex);
                objectId = (int)traceEvent.PayloadValue(ObjectIdPayloadIndex);
            }

            int lineNo = 0;
            if ((int)traceEvent.ID == ALFunctionStatement)
            {
                // Only statements have line numbers.

                lineNo = (int)traceEvent.PayloadValue(LineNoPayloadIndex);
            }

            string statement = (string)traceEvent.PayloadValue(statementLoadIndex);

            statement = this.GetStatementFromTheCache(statement);

            ProfilerEvent? currentProfilerEvent = new ProfilerEvent
                {
                    Type = type,
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
                    (previousProfilerEvent.Value.IsSqlEvent || // Always close sql events.
                     !currentProfilerEvent.HasValue || // Previous event is the last one.
                     currentProfilerEvent.Value.IsSqlEvent || // The current event is sql. It comes after stop event. Need to pop the the current aggregated node.
                     currentProfilerEvent.Value.Type != EventType.StartMethod || // The current event is not the start. If it is start event when we are in the nested call. 
                     currentAggregatedEventNode.OriginalType == EventType.StartMethod))
                {
                    currentAggregatedEventNode = currentAggregatedEventNode.PopEventFromCallStackAndCalculateDuration(previousProfilerEvent.Value.TimeStampRelativeMSec);
                }
            }

            if (!currentProfilerEvent.HasValue)
            {
                return;
            }

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

            if (currentProfilerEvent.Value.Type == EventType.StartMethod)
            {
                // If it is the root method or if it is SQL event we also push start event into the stack.
                if (currentAggregatedEventNode.Parent == null || currentProfilerEvent.Value.IsSqlEvent)
                {
                    currentAggregatedEventNode = currentAggregatedEventNode.PushEventIntoCallStack(currentProfilerEvent.Value);
                }

                currentAggregatedEventNode.EvaluatedType = EventType.StartMethod;

                return;
            }

            // Method stop.
            if (currentProfilerEvent.Value.Type == EventType.StopMethod)
            {
                // First need to calculate duration for the previous statement
                // and pop it from the statement call stack. 
                if (currentAggregatedEventNode.Parent != null && currentAggregatedEventNode.EvaluatedType == EventType.Statement)
                {
                    currentAggregatedEventNode = currentAggregatedEventNode.PopEventFromCallStackAndCalculateDuration(currentProfilerEvent.Value.TimeStampRelativeMSec);
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
