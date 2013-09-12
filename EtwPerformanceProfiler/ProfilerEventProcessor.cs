//--------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//--------------------------------------------------------------------------

namespace EtwPerformanceProfiler
{
    using System;
    using System.Collections.Generic;
    using Diagnostics.Tracing;
    using ETWPerformanceProfiler;

    /// <summary>
    /// Defines the event processor class.
    /// </summary>
    internal class ProfilerEventProcessor : IDisposable
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
        /// The name of the event source.
        /// </summary>
        private const string ProviderName = "Microsoft-DynamicsNAV-Server";
      
        /// <summary>
        /// The profiling session id
        /// </summary>
        private readonly int profilingSessionId;

        /// <summary>
        /// The threashold value. The aggregated call three will be filtered on values greater than the threshold.
        /// </summary>
        private readonly int threshold;

        /// <summary>
        /// The profiler event list
        /// </summary>
        private readonly IList<ProfilerEvent> profilerEventList;

        /// <summary>
        /// The associated event processor
        /// </summary>
        private EtwEventProcessor etwEventProcessor;

        /// <summary>
        /// A flag specifying whether this instance has been disposed.
        /// </summary>
        private bool isDisposed;

        /// <summary>
        /// The cumulated aggregated call tree
        /// </summary>
        private AggregatedEventNode aggregatedCallTree;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfilerEventProcessor"/> class.
        /// </summary>
        /// <param name="sessionId">The session id.</param>
        /// <param name="threshold">The threshold value. The aggregated call tree will only show events greater than this.</param>
        internal ProfilerEventProcessor(int sessionId, int threshold = 0)
        {
            this.profilingSessionId = sessionId;

            this.threshold = threshold;

            this.profilerEventList = new List<ProfilerEvent>();

            this.aggregatedCallTree = null;

            this.etwEventProcessor = new EtwEventProcessor(ProviderName, traceEvent => this.EtwEventHandler(traceEvent, this.profilerEventList));
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Starts a profiling session.
        /// </summary>
        internal void Start()
        {
            this.etwEventProcessor.StartProcessing();
        }

        /// <summary>
        /// Stops the profiling session.
        /// </summary>
        /// <param name="buildAggregatedCallTree">true if the aggregated call is to be built.</param>
        internal void Stop(bool buildAggregatedCallTree = true)
        {
            if (this.etwEventProcessor != null)
            {
                this.etwEventProcessor.Dispose();
                this.etwEventProcessor = null;                
            }

            if (buildAggregatedCallTree)
            {
                this.aggregatedCallTree = this.BuildAggregatedCallTree(this.profilerEventList);

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
        /// Disposes the object.
        /// </summary>
        /// <param name="disposing">True if managed resources should be disposed; otherwise, false.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.isDisposed)
                {
                    return;
                }

                this.Stop(buildAggregatedCallTree: false);

                this.isDisposed = true;
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
                if ((node.Duration100ns/10000) < this.threshold)
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
        /// Builds the accumulated result of processing the stored ETW events
        /// </summary>
        /// <param name="profilerEvents">The list of profiler events.</param>
        /// <returns>An instance of an AggregatedEventNode tree.</returns>
        internal AggregatedEventNode BuildAggregatedCallTree(IList<ProfilerEvent> profilerEvents)
        {
            this.aggregatedCallTree = new AggregatedEventNode();
            AggregatedEventNode currentAggregatedEventNode = this.aggregatedCallTree;

            for (int i = 0; i < profilerEvents.Count; i++)
            {
                ProfilerEvent currentProfilerEvent = profilerEvents[i];

                if (currentProfilerEvent.Type == EventType.Statement)
                {
                    // If there are two consequent statements then first we need to calculate the duration for the previous
                    // one and pop it from the statement call stack. 
                    // Then we push the current statement event into the stack
                    if (currentAggregatedEventNode.Parent != null && currentAggregatedEventNode.EvaluatedType == EventType.Statement)
                    {
                        currentAggregatedEventNode = currentAggregatedEventNode.PopEventFromCallStackAndCalculateDuration(currentProfilerEvent.TimeStamp100ns);
                    }

                    currentAggregatedEventNode = currentAggregatedEventNode.PushEventIntoCallStack(currentProfilerEvent);

                    continue;
                }

                if (currentProfilerEvent.Type == EventType.StartMethod)
                {
                    // If it is the root method or if it is SQL event we also push start event into the stack.
                    if (currentAggregatedEventNode.Parent == null || currentProfilerEvent.IsSqlEvent)
                    {
                        currentAggregatedEventNode = currentAggregatedEventNode.PushEventIntoCallStack(currentProfilerEvent);
                    }

                    currentAggregatedEventNode.EvaluatedType = EventType.StartMethod;

                    continue;
                }

                // Method stop.
                if (currentProfilerEvent.Type == EventType.StopMethod)
                {
                    // First need to calculate the duration for the previous statement
                    // and pop it from the statement call stack. 
                    if (currentAggregatedEventNode.Parent != null && currentAggregatedEventNode.EvaluatedType == EventType.Statement)
                    {
                        currentAggregatedEventNode = currentAggregatedEventNode.PopEventFromCallStackAndCalculateDuration(currentProfilerEvent.TimeStamp100ns);
                    }

                    // We should never pop root event. This can happen if we miss some events in the begining.
                    if (currentAggregatedEventNode.Parent != null && 
                        (currentProfilerEvent.IsSqlEvent || // Always close sql events.
                        i == profilerEvents.Count - 1 || // Current event is the last one.
                        profilerEvents[i + 1].IsSqlEvent || // The next event is sql. It should be start event.
                        profilerEvents[i + 1].Type != EventType.StartMethod || // Next event is not the start.
                        currentAggregatedEventNode.OriginalType == EventType.StartMethod)) // Current eggregated event is start so we need to pop it.
                    {
                        currentAggregatedEventNode = currentAggregatedEventNode.PopEventFromCallStackAndCalculateDuration(currentProfilerEvent.TimeStamp100ns);
                    }
                }
            }

            return this.aggregatedCallTree;
        }

        /// <summary>
        /// The callback which is called every time new event appears.
        /// </summary>
        /// <param name="traceEvent">The trace event.</param>
        /// <param name="traceEventlist">A list of accumulated profiler events.</param>
        private void EtwEventHandler(TraceEvent traceEvent, IList<ProfilerEvent> traceEventlist)
        {
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

            int sessionId = (int)traceEvent.PayloadValue(SessionIdPayloadIndex);

            if (sessionId != this.profilingSessionId)
            {
                // we are interested only in events for the profiling session
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

            ProfilerEvent profilerEvent = new ProfilerEvent
                {
                    Type = type,
                    ObjectType = objectType,
                    ObjectId = objectId,
                    LineNo = lineNo,
                    StatementName = (string)traceEvent.PayloadValue(statementLoadIndex),
                    TimeStamp100ns = traceEvent.TimeStamp100ns
                };

            traceEventlist.Add(profilerEvent);
        }       
    }
}
