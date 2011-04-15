using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Divan
{
    /// <summary>
    /// Sort criteria for single field
    /// </summary>
    public class CouchSortCriteria
    {
        /// <summary>
        /// Name of the field on which to sort data
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// Sorting order for the field
        /// </summary>
        public OrderType Order { get; set; }

        /// <summary>
        /// Type of the sorting order
        /// </summary>
        public enum OrderType
        {
            Ascending,
            Descending
        }
    }
}
