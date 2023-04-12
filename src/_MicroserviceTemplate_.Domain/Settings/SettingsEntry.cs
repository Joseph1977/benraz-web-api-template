using Benraz.Infrastructure.Common.EntityBase;
using System.Collections.Generic;

namespace _MicroserviceTemplate_.Domain.Settings
{
    /// <summary>
    /// Settings entry.
    /// </summary>
    public class SettingsEntry : AggregateRootBase<string>
    {
        /// <summary>
        /// Settings entry value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Settings description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Creates settings entry.
        /// </summary>
        /// <param name="id">Identifier.</param>
        /// <param name="value">Value.</param>
        /// <param name="description">Description.</param>
        public SettingsEntry(string id, string value, string description = null)
            : this()
        {
            Id = id;
            Value = value;
            Description = description;
        }

        /// <summary>
        /// Creates settings entry.
        /// </summary>
        public SettingsEntry()
        {
        }

        /// <summary>
        /// Returns default settings entries.
        /// </summary>
        /// <returns>Default settings entries.</returns>
        public static IEnumerable<SettingsEntry> GetDefaultValues()
        {
            var defaultValues = new List<SettingsEntry>();

            return defaultValues;
        }
    }
}