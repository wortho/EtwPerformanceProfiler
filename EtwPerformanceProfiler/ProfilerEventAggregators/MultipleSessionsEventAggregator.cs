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
    /// It processes event from different sessions.
    /// </summary>
    internal class MultipleSessionsEventAggregator : EventAggregator
    {
        private readonly Dictionary<int, SingleSessionEventAggregator> sessionAggregators = new Dictionary<int, SingleSessionEventAggregator>();

        /// <summary>
        /// The threashold value. The aggregated call three will be filtered on values greater than the threshold.
        /// </summary>
        private readonly long threshold;

        /// <summary>
        /// Creates a new instance of the <see cref="MultipleSessionsEventAggregator"/> class.
        /// </summary>
        /// <param name="threshold">The threshold value. The aggregated call tree will only show events greater than this.</param>
        internal MultipleSessionsEventAggregator(int threshold)
        {
            this.threshold = threshold;
        }

        /// <summary>
        /// The callback which is called every time new event appears.
        /// </summary>
        /// <param name="traceEvent">The trace event.</param>
        internal void AddEtwEventToProfilerEventAggregator(TraceEvent traceEvent)
        {
            int statementIndex;
            EventType eventType;
            if (!GetStatementIndexAndEventType(traceEvent, out statementIndex, out eventType))
            {
                return;
            }

            // We can check sessions id only here after we filtered out non Nav events.
            int sessionId = GetSessionId(traceEvent);

            SingleSessionEventAggregator sessionAggregator;
            if (!this.sessionAggregators.TryGetValue(sessionId, out sessionAggregator))
            {
                sessionAggregator = new SingleSessionEventAggregator(sessionId, this.threshold);
                sessionAggregator.Initialize();
                this.sessionAggregators[sessionId] = sessionAggregator;
            }

            sessionAggregator.AddEtwEventToAggregatedCallTree(traceEvent, sessionId, statementIndex, eventType);
        }

        /// <summary>
        /// Finishes aggregation.
        /// </summary>
        /// <param name="buildAggregatedCallTree">true if the aggregated call is to be built.</param>
        internal void FinishAggregation(bool buildAggregatedCallTree = true)
        {
            foreach (var singleSessionEventAggregator in this.sessionAggregators)
            {
                singleSessionEventAggregator.Value.FinishAggregation(buildAggregatedCallTree);
            }
        }

        /// <summary>
        /// Traverses the call stack tree.
        /// </summary>
        /// <returns>Flatten call tree.</returns>
        internal IEnumerable<AggregatedEventNode> FlattenCallTree()
        {
            foreach (var singleSessionEventAggregator in this.sessionAggregators)
            {
                foreach (var sessionAggregatedEventNode in singleSessionEventAggregator.Value.FlattenCallTree())
                {
                    //TODO: Check if we need to remove + 1;
                    sessionAggregatedEventNode.Depth += 1;

                    yield return sessionAggregatedEventNode;
                }
            }
        }
    }
}
