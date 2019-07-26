//--------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//--------------------------------------------------------------------------

using Microsoft.Diagnostics.Tracing;
using System.Collections.Generic;

namespace EtwPerformanceProfiler
{
    internal class EventAggregator
    {
        /// <summary>
        /// The key is the string and the value is exactly the same string.
        /// The idea is that for the equal strings we will use exactly the same string object.
        /// It saves memory because event list contains a lot of identical strings. 
        /// </summary>
        private readonly Dictionary<string, string> statementCache;

        /// <summary>
        /// Creates a new instance of the <see cref="EventAggregator"/> class.
        /// </summary>
        internal EventAggregator()
        {
            this.statementCache = new Dictionary<string, string>();
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

        protected static bool TraceEventToCollect(TraceEvent traceEvent)
        {
            switch ((int)traceEvent.ID)
            {
                case NavEvents.ALFunctionStart:
                case NavEvents.ALFunctionStop:
                case NavEvents.ALFunctionStatement:
                case NavEvents.SqlExecuteScalarStart:
                case NavEvents.SqlExecuteScalarStop:
                case NavEvents.SqlExecuteNonQueryStart:
                case NavEvents.SqlExecuteNonQueryStop:
                case NavEvents.SqlExecuteReaderStart:
                case NavEvents.SqlExecuteReaderStop:
                case NavEvents.SqlReadNextResultStart:
                case NavEvents.SqlReadNextResultStop:
                case NavEvents.SqlReadNextRowStart:
                case NavEvents.SqlReadNextRowStop:
                case NavEvents.SqlCommitStart:
                case NavEvents.SqlCommitStop:
                case NavEvents.CreateServiceSessionStart:
                case NavEvents.CreateServiceSessionStop:
                case NavEvents.EndServiceSessionStart:
                case NavEvents.EndServiceSessionStop:
                    return true;
                default:
                    return false;
            }

        }

        protected int GetSessionId(TraceEvent traceEvent)
        {
            int sessionId = (int)traceEvent.PayloadByName("sessionId");
            return sessionId;
        }

        protected ProfilerEvent? GetProfilerEvent(TraceEvent traceEvent)
        {
            string statement = "";
            EventType eventType = EventType.None;
            EventSubType eventSubType = EventSubType.None;
            string objectType = string.Empty;
            int objectId = 0;
            int lineNo = 0;

            int sessionId = GetSessionId(traceEvent);

            switch ((int)traceEvent.ID)
            {
                case NavEvents.ALFunctionStart:
                    statement = (string)traceEvent.PayloadByName("functionName");
                    objectType = (string)traceEvent.PayloadByName("objectType");
                    objectId = (int)traceEvent.PayloadByName("objectId");
                    eventType = EventType.StartMethod;
                    eventSubType = EventSubType.AlEvent;
                    break;
                case NavEvents.ALFunctionStop:
                    statement = (string)traceEvent.PayloadByName("functionName");
                    objectType = (string)traceEvent.PayloadByName("objectType");
                    objectId = (int)traceEvent.PayloadByName("objectId");
                    eventType = EventType.StopMethod;
                    eventSubType = EventSubType.AlEvent;
                    break;
                case NavEvents.ALFunctionStatement:
                    statement = (string)traceEvent.PayloadByName("statement");
                    objectType = (string)traceEvent.PayloadByName("objectType");
                    objectId = (int)traceEvent.PayloadByName("objectId");
                    lineNo = (int)traceEvent.PayloadByName("lineNumber");
                    eventType = EventType.Statement;
                    eventSubType = EventSubType.AlEvent;
                    break;
                case NavEvents.SqlExecuteScalarStart:
                    statement = (string)traceEvent.PayloadByName("sqlStatement");
                    eventType = EventType.StartMethod;
                    eventSubType = EventSubType.SqlEvent;
                    break;
                case NavEvents.SqlExecuteScalarStop:
                    statement = (string)traceEvent.PayloadByName("sqlStatement");
                    eventType = EventType.StopMethod;
                    eventSubType = EventSubType.SqlEvent;
                    break;
                case NavEvents.SqlExecuteNonQueryStart:
                    statement = (string)traceEvent.PayloadByName("sqlStatement");
                    eventType = EventType.StartMethod;
                    eventSubType = EventSubType.SqlEvent;
                    break;
                case NavEvents.SqlExecuteNonQueryStop:
                    statement = (string)traceEvent.PayloadByName("sqlStatement");
                    eventType = EventType.StopMethod;
                    eventSubType = EventSubType.SqlEvent;
                    break;
                case NavEvents.SqlExecuteReaderStart:
                    statement = (string)traceEvent.PayloadByName("sqlStatement");
                    eventType = EventType.StartMethod;
                    eventSubType = EventSubType.SqlEvent;
                    break;
                case NavEvents.SqlExecuteReaderStop:
                    statement = (string)traceEvent.PayloadByName("sqlStatement");
                    eventType = EventType.StopMethod;
                    eventSubType = EventSubType.SqlEvent;
                    break;
                case NavEvents.SqlReadNextResultStart:
                    statement = (string)traceEvent.PayloadByName("sqlStatement");
                    eventType = EventType.StartMethod;
                    eventSubType = EventSubType.SqlEvent;
                    break;
                case NavEvents.SqlReadNextResultStop:
                    statement = (string)traceEvent.PayloadByName("sqlStatement");
                    eventType = EventType.StopMethod;
                    eventSubType = EventSubType.SqlEvent;
                    break;
                case NavEvents.SqlReadNextRowStart:
                    statement = (string)traceEvent.PayloadByName("sqlStatement");
                    eventType = EventType.StartMethod;
                    eventSubType = EventSubType.SqlEvent;
                    break;
                case NavEvents.SqlReadNextRowStop:
                    statement = (string)traceEvent.PayloadByName("sqlStatement");
                    eventType = EventType.StopMethod;
                    eventSubType = EventSubType.SqlEvent;
                    break;
                case NavEvents.SqlCommitStart:
                    statement = "SQL COMMIT";
                    eventType = EventType.StartMethod;
                    eventSubType = EventSubType.SqlEvent;
                    break;
                case NavEvents.SqlCommitStop:
                    statement = "SQL COMMIT";
                    eventType = EventType.StopMethod;
                    eventSubType = EventSubType.SqlEvent;
                    break;
                case NavEvents.CreateServiceSessionStart:
                    statement = "Open Session: " + (string)traceEvent.PayloadByName("connectionType");
                    eventType = EventType.StartMethod;
                    eventSubType = EventSubType.SystemEvent;
                    break;
                case NavEvents.CreateServiceSessionStop:
                    statement = "Open Session: " + (string)traceEvent.PayloadByName("connectionType"); ;
                    eventType = EventType.StopMethod;
                    eventSubType = EventSubType.SystemEvent;
                    break;
                case NavEvents.EndServiceSessionStart:
                    statement = "Close Session: " + (string)traceEvent.PayloadByName("connectionType"); ;
                    eventType = EventType.StartMethod;
                    eventSubType = EventSubType.SystemEvent;
                    break;
                case NavEvents.EndServiceSessionStop:
                    statement = "Close Session: " + (string)traceEvent.PayloadByName("connectionType"); ;
                    eventType = EventType.StopMethod;
                    eventSubType = EventSubType.SystemEvent;
                    break;
                default:
                    return null;
            }

            return new ProfilerEvent
            {
                SessionId = sessionId,
                Type = eventType,
                SubType = eventSubType,
                ObjectType = objectType,
                ObjectId = objectId,
                LineNo = lineNo,
                StatementName = GetStatementFromTheCache(statement),
                TimeStampRelativeMSec = traceEvent.TimeStampRelativeMSec
            };

        }

    }
}
