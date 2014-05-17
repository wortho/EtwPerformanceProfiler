//--------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//--------------------------------------------------------------------------

using System;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;

namespace EtwPerformanceProfiler
{
    /// <summary>
    /// Use this class to process ETL life.
    /// </summary>
    internal class EtwEventFileProcessor
    {
        /// <summary>
        /// The callback which is called every time new event appears.
        /// </summary>
        private readonly Action<TraceEvent> traceEventHandler;

        /// <summary>
        /// The ETL file to process.
        /// </summary>
        private readonly string etlFilePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="EtwEventFileProcessor"/> class.
        /// </summary>
        /// <param name="etlFilePath">The ETL file to process.</param>
        /// <param name="traceEventHandler">The callback which is called every time new event appears.</param>
        internal EtwEventFileProcessor(string etlFilePath, Action<TraceEvent> traceEventHandler)
        {
            this.etlFilePath = etlFilePath;
            this.traceEventHandler = traceEventHandler;
        }

        internal void ProcessEtlFile()
        {
            // Open the file
            using (var source = new ETWTraceEventSource(this.etlFilePath))
            {
                // DynamicTraceEventParser knows about EventSourceEvents
                var parser = new DynamicTraceEventParser(source);

                // Set up a callback for every event that prints the event
                parser.All += this.traceEventHandler;

                // Read the file, processing the callbacks.  
                source.Process();

                // Close the file.
            }
        }
    }
}
