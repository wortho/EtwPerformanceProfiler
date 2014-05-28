//--------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//--------------------------------------------------------------------------

namespace EtwPerformanceProfiler
{
    /// <summary>
    /// Events defeined by the Nav server
    /// </summary>
    class NavEvents
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

        #region Service calls events

        /// <summary>
        /// An event id for CreateServiceSessionStart event.
        /// </summary>
        public const int CreateServiceSessionStart = 312;

        /// <summary>
        /// An event id for CreateServiceSessionStop event.
        /// </summary>
        public const int CreateServiceSessionStop = 313;

        /// <summary>
        /// An event id for EndServiceSessionStart event.
        /// </summary>
        public const int EndServiceSessionStart = 314;

        /// <summary>
        /// An event id for EndServiceSessionStop event.
        /// </summary>
        public const int EndServiceSessionStop = 315;

        #endregion

        #region Session events
        /// <summary>
        /// The session opened event.
        /// </summary>
        public const int SessionOpened = 500;

        /// <summary>
        /// The session closed event.
        /// </summary>
        public const int SessionClosed = 501;
        #endregion
    }
}
