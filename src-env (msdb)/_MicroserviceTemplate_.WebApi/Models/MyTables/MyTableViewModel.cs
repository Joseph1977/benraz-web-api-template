using System;

namespace _MicroserviceTemplate_.WebApi.Models.MyTables
{
    /// <summary>
    /// My table view model.
    /// </summary>
    public class MyTableViewModel
    {
        /// <summary>
        /// Identifier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Create time in UTC.
        /// </summary>
        public DateTime CreateTimeUtc { get; set; }

        /// <summary>
        /// Update time in UTC.
        /// </summary>
        public DateTime UpdateTimeUtc { get; set; }
    }
}
