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
    /// Defines the various event types that are collected
    /// </summary>
    internal enum EventType
    {
        /// <summary>
        /// Method start.
        /// </summary>
        StartMethod,

        /// <summary>
        /// Method stop
        /// </summary>
        StopMethod,

        /// <summary>
        /// Statement.
        /// </summary>
        Statement,

        /// <summary>
        /// Not an event.
        /// </summary>
        None
    }

    /// <summary>
    /// Defines the data structure for an ETW event issued by the NAV server
    /// </summary>
    internal struct ProfilerEvent
    {
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        internal EventType Type { get; set; }

        /// <summary>
        /// Gets or sets the type of the object.
        /// </summary>
        internal string ObjectType { get; set; }

        /// <summary>
        /// Gets or sets the object id.
        /// </summary>
        internal int ObjectId { get; set; }

        /// <summary>
        /// Gets or sets the line no.
        /// </summary>
        internal int LineNo { get; set; }

        /// <summary>
        /// Gets or sets the name of the Statement.
        /// </summary>
        internal string StatementName { get; set; }

        /// <summary>
        /// Gets or sets the time stamp in 100ns.
        /// </summary>
        internal double TimeStampRelativeMSec { get; set; }

        /// <summary>
        /// Returns true if this is the Sql event.
        /// </summary>
        internal bool IsNoneAlEvent
        {
            get { return this.ObjectId == 0; }
        }

        /// <summary>
        /// Determines if the current object is equal to the <paramref name="obj"/>
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>true, if the two instances are equal</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is ProfilerEvent))
            {
                return false;
            }

            ProfilerEvent other = (ProfilerEvent)obj;

            return this.Type.Equals(other.Type) && this.ObjectType.Equals(other.ObjectType)
                   && this.ObjectId.Equals(other.ObjectId) && this.LineNo.Equals(other.LineNo)
                   && this.StatementName.Equals(other.StatementName) &&
                   this.TimeStampRelativeMSec.Equals(other.TimeStampRelativeMSec);
        }

        /// <summary>
        /// Determines if the two instances of <see cref="ProfilerEvent"/> are equal.
        /// </summary>
        /// <param name="lfh">The first instance</param>
        /// <param name="rhs">The second instance</param>
        /// <returns>true, if the two instances are equal</returns>
        public static bool operator ==(ProfilerEvent lfh, ProfilerEvent rhs)
        {
            return lfh.Equals(rhs);
        }

        /// <summary>
        /// Determines if the two instances of <see cref="ProfilerEvent"/> are not equal.
        /// </summary>
        /// <param name="lhs">The first instance</param>
        /// <param name="rhs">The second instance</param>
        /// <returns>true, if the two instances are not equal</returns>
        public static bool operator !=(ProfilerEvent lhs, ProfilerEvent rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Generates a hash code for the  object.
        /// </summary>
        /// <returns>A hash code</returns>
        public override int GetHashCode()
        {
            return this.Type.GetHashCode() ^ this.ObjectId.GetHashCode() ^ this.ObjectType.GetHashCode()
                   ^ this.StatementName.GetHashCode() ^ this.LineNo.GetHashCode() ^ this.TimeStampRelativeMSec.GetHashCode();
        }
    }
}
