using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Divan.Lucene
{
    /// <summary>
    /// Implementation of Lucene query for Cloudant service
    /// </summary>
    public class CloudantLuceneQuery : LuceneQueryBase
    {
        /// <summary>
        /// Create by specifying view to use with this query
        /// </summary>
        /// <param name="view">View to use</param>
        public CloudantLuceneQuery(CloudantLuceneViewDefinition view)
        {
            View = view;
        }

        /// <summary>
        /// Specify custom indexer to use with this query (default is: design/lucene/_view/index)
        /// </summary>
        /// <param name="index"></param>
        /// <returns>Modified query</returns>
        public LuceneQueryBase Index(string index)
        {
            Options["index"] = index;
            return this;
        }

        /// <summary>
        /// If specified, boolean operation used between multiple terms in search will be OR (default is AND)
        /// </summary>
        /// <returns>Modified query</returns>
        public LuceneQueryBase DefaultOr()
        {
            Options["default_or"] = "True";
            return this;
        }

        /// <summary>
        /// Field to search over if field is not explicitely stated in search term
        /// </summary>
        /// <param name="field">Name of the field used as default</param>
        /// <returns>Modified query</returns>
        public LuceneQueryBase DefaultField(string field)
        {
            Options["default_field"] = field;
            return this;
        }

        public override LuceneQueryBase Sort(IEnumerable<CouchSortCriteria> criteria)
        {
            // If no sort criteria has been given, leave the method:
            if (null == criteria || 0 == criteria.Count())
                return this;

            if (criteria.Count() > 1)
                throw new NotImplementedException("Cloudant service does not support sorting over multiple fields right now.");

            var item = criteria.First();
            Options["sort"] = item.Field;
            
            if (item.Order == CouchSortCriteria.OrderType.Descending)
                Options["descending"] = "True";

            return this;
        }

        public override T GetResult<T>()
        {
            if (Options["q"] == null)
            {
                throw CouchException.Create("Lucene query failed, you need to specify Q(<Lucene-query-string>).");
            }
            var req = Request();

            Result = new T();
            JObject json = req.Parse();
            Result.Result(json, View);

            return (T) Result;
        }
    }
}
