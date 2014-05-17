//--------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//--------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using ETWPerformanceProfiler;

namespace EtwPerformanceProfiler
{
    /// <summary>
    /// Defines the event processor class.
    /// </summary>
    internal class DynamicProfilerEventProcessor : IDisposable
    {
        #region Private members
      
        /// <summary>
        /// The name of the event source.
        /// </summary>
        private const string ProviderName = "Microsoft-DynamicsNAV-Server";

        /// <summary>
        /// A flag specifying whether this instance has been disposed.
        /// </summary>
        private bool isDisposed;

        /// <summary>
        /// The associated event processor.
        /// </summary>
        private EtwEventDynamicProcessor etwEventDynamicProcessor;

        /// <summary>
        /// The associated event aggregator.
        /// </summary>
        private readonly SingleSessionEventAggregator singleSessionEventAggregator;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicProfilerEventProcessor"/> class.
        /// </summary>
        /// <param name="sessionId">The session id.</param>
        /// <param name="threshold">The threshold value. The aggregated call tree will only show events greater than this.</param>
        internal DynamicProfilerEventProcessor(int sessionId, long threshold = 0)
        {
            this.singleSessionEventAggregator = new SingleSessionEventAggregator(sessionId, threshold);

            this.etwEventDynamicProcessor = new EtwEventDynamicProcessor(ProviderName, this.singleSessionEventAggregator.AddEtwEventToAggregatedCallTree);
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
            this.Initialize();

            this.etwEventDynamicProcessor.StartProcessing();
        }

        /// <summary>
        /// Initializes state of the <see cref="DynamicProfilerEventProcessor"/>
        /// </summary>
        private void Initialize()
        {
            this.singleSessionEventAggregator.Initialize();
        }

        /// <summary>
        /// Stops the profiling session.
        /// </summary>
        /// <param name="buildAggregatedCallTree">true if the aggregated call is to be built.</param>
        internal void Stop(bool buildAggregatedCallTree = true)
        {
            if (this.etwEventDynamicProcessor != null)
            {
                this.etwEventDynamicProcessor.Dispose();
                this.etwEventDynamicProcessor = null;                
            }

            this.singleSessionEventAggregator.FinishAggregation(buildAggregatedCallTree);
        }

        /// <summary>
        /// Traverses the call stack tree.
        /// </summary>
        /// <returns>Flatten call tree.</returns>
        internal IEnumerable<AggregatedEventNode> FlattenCallTree()
        {
            return this.singleSessionEventAggregator.FlattenCallTree();
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
    }
}
