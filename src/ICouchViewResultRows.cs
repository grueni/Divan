using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Divan
{
    /// <summary>
    /// Interface to Couch resultset returning Rows after query is performed
    /// </summary>
    public interface ICouchViewResultRows
    {
        /// <summary>
        /// Retrieve rows contained in the resultset
        /// </summary>
        /// <returns>Rows contained in the resultset</returns>
        JEnumerable<JToken> Rows();
    }
}
