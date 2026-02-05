using System.ComponentModel.DataAnnotations;

namespace _MicroserviceTemplate_.WebApi.Models.MyTables
{
    /// <summary>
    /// Add my table view model.
    /// </summary>
    public class AddMyTableViewModel
    {
        /// <summary>
        /// Value.
        /// </summary>
        [Required]
        public string Value { get; set; }

        /// <summary>
        /// Description.
        /// </summary>
        public string Description { get; set; }
    }
}
