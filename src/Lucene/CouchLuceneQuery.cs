using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Divan.Lucene
{
    /// <summary>
    /// A Lucene query with all its options. This class overlaps with CouchQuery but I could not find
    /// a nice way to use inheritance and still keep a fluent style interface without going into generics HELL.
    /// 
    /// You can perform all types of queries using Lucene's default query syntax:
    ///     http://lucene.apache.org/java/2_4_0/queryparsersyntax.html
    /// 
    /// The _body field is searched by default which will include the extracted text from all attachments.
    /// </summary>
    public class CouchLuceneQuery : LuceneQueryBase
    {
        /// <summary>
        /// Indicates if ETag should be used
        /// </summary>
        protected bool CheckETag;

        /// <summary>
        /// Create by specifying view to use with this query
        /// </summary>
        /// <param name="view">View to use</param>
        public CouchLuceneQuery(CouchViewDefinitionBase view)
        {
            View = view;
        }

        /// <summary>
        /// The analyzer used to convert the query string into a query object. 
        /// </summary>
        public CouchLuceneQuery Analyzer(string value)
        {
            Options["analyzer"] = value;
            return this;
        }

        /// <summary>
        /// Specify a JSONP callback wrapper. The full JSON result will be prepended
        /// with this parameter and also placed with parentheses.
        /// </summary>
        public CouchLuceneQuery Callback(string value)
        {
            Options["callback"] = value;
            return this;
        }

        /// <summary>
        /// Setting this to true disables response caching (the query is executed every time)
        /// and indents the JSON response for readability.
        /// </summary>
        public LuceneQueryBase Debug()
        {
            Options["debug"] = "True";
            return this;
        }
        
        /// <summary>
        /// Usually couchdb-lucene determines the Content-Type of its response based on the
        /// presence of the Accept header. If Accept contains "application/json", you get
        /// "application/json" in the response, otherwise you get "text/plain;charset=utf8".
        /// Some tools, like JSONView for FireFox, do not send the Accept header but do render
        /// "application/json" responses if received. Setting force_json=true forces all response 
        /// to "application/json" regardless of the Accept header.
        /// </summary>
        public CouchLuceneQuery ForceJson()
        {
            Options["force_json"] = "True";
            return this;
        }

        /// <summary>
        /// (EXPERT) if true, returns a json response with a rewritten query and term frequencies.
        /// This allows correct distributed scoring when combining the results from multiple nodes.
        /// </summary>
        public CouchLuceneQuery Rewrite()
        {
            Options["rewrite"] = "True";
            return this;
        }

        public override LuceneQueryBase Sort(IEnumerable<CouchSortCriteria> criteria)
        {
            // If no sort criteria has been given, leave the method:
            if (null == criteria || 0 == criteria.Count())
                return this;

            var itemCount = 0;
            var text = new StringBuilder();
            foreach (var item in criteria)
            {
                if (itemCount > 0)
                    text.Append(',');

                text.Append(item.Order == CouchSortCriteria.OrderType.Ascending ? "/" : "\\");
                text.Append(item.Field);
                ++itemCount;
            }

            Options["sort"] = text.ToString();
            return this;
        }

        /// <summary>
        /// If you set the stale option to ok, couchdb-lucene may not perform any
        /// refreshing on the index. Searches may be faster as Lucene caches important
        /// data (especially for sorting). A query without stale=ok will use the latest
        /// data committed to the index.
        /// </summary>
        public CouchLuceneQuery Stale()
        {
            Options["stale"] = "ok";
            return this;
        }

        /// <summary>
        /// Tell this query to do a HEAD request first to see
        /// if ETag has changed and only then do the full request.
        /// This is only interesting if you are reusing this query object.
        /// </summary>
        public CouchLuceneQuery CheckETagUsingHead()
        {
            CheckETag = true;
            return this;
        }

        public override T GetResult<T>()
        {
            if (Options["q"] == null)
            {
                throw CouchException.Create("Lucene query failed, you need to specify Q(<Lucene-query-string>).");
            }
            var req = Request();

            if (Result == null)
            {
                Result = new T();
            }
            else
            {
                // Tell the request what we already have
                req.Etag(Result.etag);
                if (CheckETag)
                {
                    // Make a HEAD request to avoid transfer of data
                    if (req.Head().Send().IsETagValid())
                    {
                        return (T)Result;
                    }
                    // Set back to GET before proceeding below
                    req.Get();
                }
            }

            JObject json = req.Parse();
            if (json != null) // ETag did not match, view has changed
            {
                Result.Result(json, View);
                Result.etag = req.Etag();
            }
            return (T)Result;
        }

        /// <summary>
        /// Checks if cached Result is still valid
        /// </summary>
        /// <returns>Indication if cached Result is valid</returns>
        public bool IsCachedAndValid()
        {
            // If we do not have a result it is not cached
            if (Result == null)
            {
                return false;
            }
            ICouchRequest req = View.Request().QueryOptions(Options);
            req.Etag(Result.etag);
            return req.Head().Send().IsETagValid();
        }
    }
}