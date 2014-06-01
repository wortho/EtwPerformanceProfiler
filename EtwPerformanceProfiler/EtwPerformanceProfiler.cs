//--------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//--------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace EtwPerformanceProfiler
{
    /// <summary>
    /// Represents the performance profiler class to be used in AL.
    /// </summary>
    public class EtwPerformanceProfiler : IDisposable
    {
        /// <summary>
        /// The maximum length of the statement which can be inserted into the table.
        /// </summary>
        private const int MaxStatementNameLength = 250;

        /// <summary>
        /// The associated event processor.
        /// </summary>
        private DynamicProfilerEventProcessor dynamicProfilerEventProcessor;

        /// <summary>
        /// The call tree of all the aggregated method and SQL statement calls parsed from the ETW events
        /// </summary>
        private IEnumerator<AggregatedEventNode> callTree;

        /// <summary>
        /// Gets the call tree's current statement's owning object id.
        /// </summary>
        public int CallTreeCurrentStatementOwningObjectId
        {
            get
            {
                return this.callTree.Current.ObjectId;
            }
        }
            
        /// <summary>
        /// Gets the call tree's current statement.
        /// </summary>
        public string CallTreeCurrentStatement
        {
            get
            {
                string statementName = this.callTree.Current.StatementName;

                if (statementName.Length > MaxStatementNameLength)
                {
                    statementName = statementName.Substring(0, MaxStatementNameLength);
                }

                return statementName;
            }
        }
            
        /// <summary>
        /// Gets the current line number on the call tree.
        /// </summary>
        public int CallTreeCurrentStatementLineNo
        {
            get
            {
                return this.callTree.Current.LineNo;
            }
        }

        /// <summary>
        /// Gets call tree's current statements duration in miliseconds
        /// </summary>
        public long CallTreeCurrentStatementDurationMs
        {
            get
            {
                return (long)this.callTree.Current.DurationMSec;
            }
        }

        /// <summary>
        /// Gets the call tree' current current statement's depth.
        /// </summary>
        public int CallTreeCurrentStatementIndentation
        {
            get
            {
                return this.callTree.Current.Depth;
            }
        }

        /// <summary>
        /// Gets the current object type on the call tree. 
        /// </summary>
        public int CallTreeCurrentStatementOwningObjectType
        {
            get
            {
                string objectType = this.callTree.Current.ObjectType;

                // Empty object type consider to be the table.
                // It should be empty only for the SQL queries.
                if (string.IsNullOrEmpty(objectType))
                {
                    return 0;
                }

                if (0 == String.Compare(objectType, "Table", System.StringComparison.OrdinalIgnoreCase))
                {
                    return 0;
                }

                if (0 == String.Compare(objectType, "Report", System.StringComparison.OrdinalIgnoreCase))
                {
                    return 3;
                }

                if (0 == String.Compare(objectType, "CodeUnit", System.StringComparison.OrdinalIgnoreCase))
                {
                    return 5;
                }

                if (0 == String.Compare(objectType, "XmlPort", System.StringComparison.OrdinalIgnoreCase))
                {
                    return 6;
                }

                if (0 == String.Compare(objectType, "Page", System.StringComparison.OrdinalIgnoreCase))
                {
                    return 8;
                }

                if (0 == String.Compare(objectType, "Query", System.StringComparison.OrdinalIgnoreCase))
                {
                    return 9;
                }

                throw new InvalidOperationException("Invalid object type.");
            }
        }

        /// <summary>
        /// Gets the call tree' current current statement's hit count.
        /// </summary>
        public int CallTreeCurrentStatementHitCount
        {
            get
            {
                return this.callTree.Current.HitCount;
            }
        }

        /// <summary>
        /// Starts ETW profiling.
        /// </summary>
        /// <param name="sessionId">The session unique identifier.</param>
        /// <param name="threshold">The filter value in milliseconds. Values greater then this will only be shown.</param>
        public void Start(int sessionId, int threshold = 0)
        {
            this.dynamicProfilerEventProcessor = new DynamicProfilerEventProcessor(sessionId, threshold);

            this.dynamicProfilerEventProcessor.Start();
        }

        /// <summary>
        /// Stops profiling and aggregates the events
        /// </summary>
        public void Stop()
        {
            this.dynamicProfilerEventProcessor.Stop();

            this.callTree = this.dynamicProfilerEventProcessor.FlattenCallTree().GetEnumerator();
        }

        /// <summary>
        /// Analyzes events from the ETL file and aggregates events from the multiple sessions.
        /// </summary>
        /// <param name="etlFilePath">ETL file to be analyzed.</param>
        /// <param name="threshold">The filter value in milliseconds. Values greater then this will only be shown.</param>
        public void AnalyzeETLFile(string etlFilePath, int threshold = 0)
        {
            if (this.dynamicProfilerEventProcessor != null)
            {
                this.dynamicProfilerEventProcessor.Dispose();
                this.dynamicProfilerEventProcessor = null;
            }

            using (ProfilerEventEtlFileProcessor profilerEventEtlFileProcessor = new ProfilerEventEtlFileProcessor(etlFilePath, threshold))
            {
                profilerEventEtlFileProcessor.ProcessEtlFile();

                this.callTree = profilerEventEtlFileProcessor.FlattenCallTree().GetEnumerator();
            }
        }

        /// <summary>
        /// Calls the tree move next.
        /// </summary>
        /// <returns></returns>
        public bool CallTreeMoveNext()
        {
            return this.callTree.MoveNext();
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
                if (this.dynamicProfilerEventProcessor != null)
                {
                    this.dynamicProfilerEventProcessor.Dispose();
                }

                this.callTree = null;
            }
        }
    }
}
