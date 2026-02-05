using Benraz.Infrastructure.Common.EntityBase;
using System;
using System.Collections.Generic;

namespace _MicroserviceTemplate_.Domain.MyTables
{
    /// <summary>
    /// My table
    /// </summary>
    public class MyTable : AggregateRootBase<Guid>
    {
        /// <summary>
        /// Value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Creates my table.
        /// </summary>
        /// <param name="id">Identifier.</param>
        /// <param name="value">Value.</param>
        /// <param name="description">Description.</param>
        public MyTable(Guid id, string value, string description = null)
            : this()
        {
            Id = id;
            Value = value;
            Description = description;
        }

        /// <summary>
        /// Creates my table.
        /// </summary>
        public MyTable()
        {
        }

        /// <summary>
        /// Returns default my table entries.
        /// </summary>
        /// <returns>Default my table entries.</returns>
        public static IEnumerable<MyTable> GetDefaultValues()
        {
            var defaultValues = new List<MyTable>();

            return defaultValues;
        }
    }
}
