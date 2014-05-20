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
    /// Payload indexes of Nav events
    /// </summary>
    class NavEventsPayloadIndexes
    {
        #region Payload indexes
        /// <summary>
        /// Identifies that payload index does not exist.
        /// </summary>
        internal const int NonPayloadIndex = -1;

        /// <summary>
        /// The index of the tenant id payload parameter as defined in the ETW manifest.
        /// </summary>
        internal const int TenantIdPayloadIndex = 0;

        /// <summary>
        /// The index of the session id payload parameter as defined in the ETW manifest.
        /// </summary>
        internal const int SessionIdPayloadIndex = 1;

        /// <summary>
        /// The index of the user name payload parameter as defined in the ETW manifest.
        /// </summary>
        internal const int UserNamePayloadIndex = 2;

        /// <summary>
        /// The index of the connection type payload parameter as defined in the ETW manifest.
        /// </summary>
        internal const int ConnectionTypePayloadIndex = 3;

        /// <summary>
        /// The index of the SQL statement payload parameter as defined in the ETW manifest.
        /// </summary>
        internal const int SqlStatementPayloadIndex = 3;

        /// <summary>
        /// The index of the object type payload parameter as defined in the ETW manifest.
        /// </summary>
        internal const int ObjectTypePayloadIndex = 3;

        /// <summary>
        /// The index of the object id payload parameter as defined in the ETW manifest.
        /// </summary>
        internal const int ObjectIdPayloadIndex = 4;

        /// <summary>
        /// The index of the function name payload parameter as defined in the ETW manifest.
        /// </summary>
        internal const int ALFunctionNamePayloadIndex = 5;

        /// <summary>
        /// The index of the line number payload parameter as defined in the ETW manifest.
        /// </summary>
        internal const int LineNoPayloadIndex = 6;

        /// <summary>
        /// The index of the statement payload parameter as defined in the ETW manifest.
        /// </summary>
        internal const int ALStatementPayloadIndex = 7;
        #endregion

    }
}
