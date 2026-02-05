using System;

namespace _MicroserviceTemplate_.WebApi.Models.Settings
{
    /// <summary>
    /// Settings entry view model.
    /// </summary>
    public class SettingsEntryViewModel
    {
        /// <summary>
        /// Identifier.
        /// </summary>
        public string Id { get; set; }

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