//--------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//--------------------------------------------------------------------------

using Microsoft.Diagnostics.Tracing;

namespace EtwPerformanceProfiler
{
    abstract class EventAggregator
    {
        protected static int GetSessionId(TraceEvent traceEvent)
        {
            return (int)traceEvent.PayloadValue(NavEventsPayloadIndexes.SessionIdPayloadIndex);
        }

        protected static bool GetStatementIndexAndEventType(TraceEvent traceEvent, out int statementIndex, out EventType eventType)
        {
            switch ((int)traceEvent.ID)
            {
                case NavEvents.ALFunctionStart:
                    statementIndex = NavEventsPayloadIndexes.ALFunctionNamePayloadIndex;
                    eventType = EventType.StartMethod;
                    break;
                case NavEvents.ALFunctionStop:
                    statementIndex = NavEventsPayloadIndexes.ALFunctionNamePayloadIndex;
                    eventType = EventType.StopMethod;
                    break;
                case NavEvents.ALFunctionStatement:
                    statementIndex = NavEventsPayloadIndexes.ALStatementPayloadIndex;
                    eventType = EventType.Statement;
                    break;
                case NavEvents.SqlExecuteScalarStart:
                    statementIndex = NavEventsPayloadIndexes.SqlStatementPayloadIndex;
                    eventType = EventType.StartMethod;
                    break;
                case NavEvents.SqlExecuteScalarStop:
                    statementIndex = NavEventsPayloadIndexes.SqlStatementPayloadIndex;
                    eventType = EventType.StopMethod;
                    break;
                case NavEvents.SqlExecuteNonQueryStart:
                    statementIndex = NavEventsPayloadIndexes.SqlStatementPayloadIndex;
                    eventType = EventType.StartMethod;
                    break;
                case NavEvents.SqlExecuteNonQueryStop:
                    statementIndex = NavEventsPayloadIndexes.SqlStatementPayloadIndex;
                    eventType = EventType.StopMethod;
                    break;
                case NavEvents.SqlExecuteReaderStart:
                    statementIndex = NavEventsPayloadIndexes.SqlStatementPayloadIndex;
                    eventType = EventType.StartMethod;
                    break;
                case NavEvents.SqlExecuteReaderStop:
                    statementIndex = NavEventsPayloadIndexes.SqlStatementPayloadIndex;
                    eventType = EventType.StopMethod;
                    break;
                case NavEvents.SessionOpened:
                    statementIndex = NavEventsPayloadIndexes.ConnectionTypePayloadIndex;
                    eventType = EventType.StartMethod;
                    break;
                case NavEvents.SessionClosed:
                    statementIndex = NavEventsPayloadIndexes.ConnectionTypePayloadIndex;
                    eventType = EventType.StopMethod;
                    break;
                default:
                    statementIndex = -1;
                    eventType = EventType.None;
                    return false;
            }

            return true;
        }
    }
}
