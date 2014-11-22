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
    public class DynamicProfilerEventProcessor : IDisposable
    {
        #region Private members

        public const int MultipleSessionsId = -1;
      
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
        private readonly IEventAggregator eventAggregator;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicProfilerEventProcessor"/> class.
        /// </summary>
        /// <param name="sessionId">The session id.</param>
        /// <param name="threshold">The threshold value. The aggregated call tree will only show events greater than this.</param>
        public DynamicProfilerEventProcessor(int sessionId, long threshold = 0)
        {
            if (sessionId == MultipleSessionsId)
            {
                this.eventAggregator = new MultipleSessionsEventAggregator(threshold);    
            }
            else
            {
                this.eventAggregator = new SingleSessionEventAggregator(sessionId, threshold);    
            }

            this.etwEventDynamicProcessor = new EtwEventDynamicProcessor(ProviderName, this.eventAggregator.AddEtwEventToAggregatedCallTree);
        }

        /// <summary>
        /// Finalize makes sure that we dispose the object correctly.
        /// </summary>
        ~DynamicProfilerEventProcessor()
        {
            this.Dispose();
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
        public void Start()
        {
            this.Initialize();

            this.etwEventDynamicProcessor.StartProcessing();
        }

        /// <summary>
        /// Initializes state of the <see cref="DynamicProfilerEventProcessor"/>
        /// </summary>
        public void Initialize()
        {
            this.eventAggregator.Initialize();
        }

        /// <summary>
        /// Stops the profiling session.
        /// </summary>
        /// <param name="buildAggregatedCallTree">true if the aggregated call is to be built.</param>
        public void Stop(bool buildAggregatedCallTree = true)
        {
            if (this.etwEventDynamicProcessor != null)
            {
                this.etwEventDynamicProcessor.Dispose();
                this.etwEventDynamicProcessor = null;                
            }

            this.eventAggregator.FinishAggregation(buildAggregatedCallTree);
        }

        /// <summary>
        /// Traverses the call stack tree.
        /// </summary>
        /// <returns>Flatten call tree.</returns>
        public IEnumerable<AggregatedEventNode> FlattenCallTree()
        {
            return this.eventAggregator.FlattenCallTree();
        }

        /// <summary>
        /// Calculates maximum relative time stamp.
        /// </summary>
        /// <returns>Maximum relative time stamp.</returns>
        public double MaxRelativeTimeStamp()
        {
            return this.eventAggregator.MaxRelativeTimeStamp();
        }

        /// <summary>
        /// Suspend event processing.
        /// </summary>
        public void Suspend()
        {
            this.eventAggregator.Suspend();
        }

        /// <summary>
        /// Resume event processing.
        /// </summary>
        public void Resume()
        {
            this.eventAggregator.Resume();
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
