using System.ComponentModel.DataAnnotations;

namespace _MicroserviceTemplate_.WebApi.Models.MyTables
{
    /// <summary>
    /// Add my table view model.
    /// </summary>
    public class AddMyTableViewModel
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
