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
    interface IEventAggregator
    {
        /// <summary>
        /// Initializes state of the <see cref="IEventAggregator"/>
        /// </summary>
        void Initialize();

        /// <summary>
        /// Finishes aggregation.
        /// </summary>
        /// <param name="buildAggregatedCallTree">true if the aggregated call is to be built.</param>
        void FinishAggregation(bool buildAggregatedCallTree = true);

        /// <summary>
        /// Traverses the call stack tree.
        /// </summary>
        /// <returns>Flatten call tree.</returns>
        IEnumerable<AggregatedEventNode> FlattenCallTree();

        /// <summary>
        /// The callback which is called every time new event appears.
        /// </summary>
        /// <param name="traceEvent">The trace event.</param>
        void AddEtwEventToAggregatedCallTree(TraceEvent traceEvent);

        /// <summary>
        /// Calculates maximum relative time stamp.
        /// </summary>
        /// <returns>Maximum relative time stamp.</returns>
        double MaxRelativeTimeStamp();

        /// <summary>
        /// Suspend event processing.
        /// </summary>
        void Suspend();

        /// <summary>
        /// Resume event processing.
        /// </summary>
        void Resume();
    }
}
