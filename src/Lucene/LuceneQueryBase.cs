using System.Collections.Generic;
using System.Net;

namespace Divan.Lucene
{
    /// <summary>
    /// Abstract base class for Couch Lucene queries
    /// </summary>
    public abstract class LuceneQueryBase
    {
        /// <summary>
        /// View used with this query
        /// </summary>
        protected CouchViewDefinitionBase View;

        /// <summary>
        /// Options used with this query
        /// </summary>
        protected Dictionary<string, string> Options = new Dictionary<string, string>();

        /// <summary>
        /// POST data to use when request is being made
        /// </summary>
        public string PostData;

        /// <summary>
        /// Result of the last query execution
        /// </summary>
        public CouchLuceneViewResult Result;

        /// <summary>
        /// Clear all options
        /// </summary>
        public virtual void ClearOptions()
        {
            Options.Clear();
        }

        /// <summary>
        /// The query to run (e.g, subject:hello). If not specified, the default field is searched.
        /// </summary>
        public LuceneQueryBase Q(string value)
        {
            Options["q"] = value;
            return this;
        }

        /// <summary>
        /// When this option is specified, returned resultset will contain full documents (not just key/value/_id combination)
        /// </summary>
        /// <returns></returns>
        public LuceneQueryBase IncludeDocuments()
        {
            Options["include_docs"] = "True";
            return this;
        }

        /// <summary>
        /// Specify how many results to skip from beginning
        /// </summary>
        /// <param name="value">Count of records to skip</param>
        /// <returns>Modified query</returns>
        public LuceneQueryBase Skip(int value)
        {
            Options["skip"] = value.ToString();
            return this;
        }

        /// <summary>
        /// Specify how many results to return
        /// </summary>
        /// <param name="value">Count of results to return in resultset</param>
        /// <returns>Modified query</returns>
        public LuceneQueryBase Limit(int value)
        {
            Options["limit"] = value.ToString();
            return this;
        }

        /// <summary>
        /// Execute query and return resultset
        /// </summary>
        /// <typeparam name="T">Type of the resultset</typeparam>
        /// <returns>Result of the query execution</returns>
        public abstract T GetResult<T>() where T : CouchLuceneViewResult, new();

        /// <summary>
        /// Execute query and return generic resultset
        /// </summary>
        /// <returns>Generic resultset</returns>
        public CouchLuceneGenericViewResult GetResult()
        {
            try
            {
                return GetResult<CouchLuceneGenericViewResult>();
            }
            catch (WebException e)
            {
                throw CouchException.Create("Query failed", e);
            }
        }

        /// <summary>
        /// Get request which will be generated for this query
        /// </summary>
        /// <returns>Request associated with this query</returns>
        public ICouchRequest Request()
        {
            var req = View.Request().QueryOptions(Options);
            if (PostData != null)
            {
                req.Data(PostData).Post();
            }
            return req;
        }

        /// <summary>
        /// Get request which will be generated for this query as string
        /// </summary>
        /// <returns>Request associated with this query as string</returns>
        public string String()
        {
            return Request().String();
        }
        
        /// <summary>
        /// Perform sorting on resultset
        /// </summary>
        /// <param name="criteria">Sorting criteria</param>
        /// <returns>Modified query</returns>
        public abstract LuceneQueryBase Sort(IEnumerable<CouchSortCriteria> criteria);
    }
}
