using System.ComponentModel.DataAnnotations;

namespace _MicroserviceTemplate_.WebApi.Models.Settings
{
    /// <summary>
    /// Add settings entry view model.
    /// </summary>
    public class AddSettingsEntryViewModel
    {
        /// <summary>
        /// Identifier.
        /// </summary>
        [Required]
        public string Id { get; set; }

        /// <summary>
        /// Value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Description.
        /// </summary>
        public string Description { get; set; }
    }
}