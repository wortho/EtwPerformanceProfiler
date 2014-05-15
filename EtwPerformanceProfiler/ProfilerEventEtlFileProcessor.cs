//--------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//--------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;

namespace EtwPerformanceProfiler
{
    /// <summary>
    /// This class should be used to aggregate event from the multiple session.
    /// </summary>
    internal class ProfilerEventEtlFileProcessor : IDisposable
    {
        /// <summary>
        /// Has the object been disposed.
        /// </summary>
        private bool isDisposed;

        internal void ProcessEtlFile(string etlFilePath)
        {
            // Open the file
            using (var source = new ETWTraceEventSource(etlFilePath))
            {
                // DynamicTraceEventParser knows about EventSourceEvents
                var parser = new DynamicTraceEventParser(source);

                // Set up a callback for every event that prints the event
                parser.All += this.AddEtwEventToProfilerEventAggregator;

                // Read the file, processing the callbacks.  
                source.Process();

                // Close the file.
            }
        }

        /// <summary>
        /// The callback which is called every time new event appears.
        /// </summary>
        /// <param name="traceEvent">The trace event.</param>
        internal void AddEtwEventToProfilerEventAggregator(TraceEvent traceEvent)
        {
        }

        /// <summary>
        /// Traverses the call stack tree.
        /// </summary>
        /// <returns>Flatten call tree.</returns>
        internal IEnumerable<AggregatedEventNode> FlattenCallTree()
        {
            throw new NotImplementedException();
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

                this.isDisposed = true;
            }
        }
    }
}
