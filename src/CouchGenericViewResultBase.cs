using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Divan
{
    /// <summary>
    /// Basic implementation of IGenericViewResult interface
    /// </summary>
    public class CouchGenericViewResultBase : IGenericViewResult
    {
        private readonly ICouchViewResultRows _resultSet;

        public CouchGenericViewResultBase(ICouchViewResultRows resultSet)
        {
            _resultSet = resultSet;
        }

        public IEnumerable<T> Values<T>() where T : new()
        {
            var list = new List<T>();
            foreach (JToken row in _resultSet.Rows())
            {
                list.Add(row["value"].Value<T>());
            }
            return list;
        }
		
        public IEnumerable<T> ValueDocuments<T>() where T : ICanJson, new()
        {
            return RetrieveDocuments<T>("value");
        }

        public IEnumerable<T> ValueDocumentsWithIds<T>() where T : ICouchDocument, new()
        {
            return RetrieveDocumentsWithIds<T>("value");
        }
		
        public IEnumerable<T> ValueDocuments<T>(Func<T> ctor)
        {
            return RetrieveArbitraryDocuments("value", ctor);
        }

        public T ValueDocument<T>() where T : ICanJson, new()
        {
            return RetrieveDocument<T>("value");
        }

        public T ArbitraryValueDocument<T>(Func<T> ctor)
        {
            return RetrieveArbitraryDocument("value", ctor);
        }

        public IEnumerable<T> ArbitraryValueDocuments<T>(Func<T> ctor)
        {
            return RetrieveArbitraryDocuments("value", ctor);
        }

        public IEnumerable<T> Documents<T>() where T : ICouchDocument, new()
        {
            return RetrieveDocuments<T>("doc");
        }

        public IEnumerable<T> ArbitraryDocuments<T>(Func<T> ctor)
        {
            return RetrieveArbitraryDocuments("doc", ctor);
        }

        public IEnumerable<CouchJsonDocument> Documents()
        {
            return RetrieveDocuments<CouchJsonDocument>("doc");
        }

        public T Document<T>() where T : ICouchDocument, new()
        {
            return RetrieveDocument<T>("doc");
        }

        public T ArbitraryDocument<T>(Func<T> ctor)
        {
            return RetrieveArbitraryDocument("doc", ctor);
        }

        protected virtual IEnumerable<T> RetrieveDocuments<T>(string docOrValue) where T : ICanJson, new()
        {
            var list = new List<T>();
            foreach (JToken row in _resultSet.Rows())
            {
                var doc = new T();
                if (row[docOrValue] == null)
                    continue;

                doc.ReadJson(row[docOrValue].Value<JObject>());
                list.Add(doc);
            }
            return list;
        }
		
		protected virtual IEnumerable<T> RetrieveDocumentsWithIds<T>(string docOrValue) where T : ICouchDocument, new()
        {
            var list = new List<T>();
			var found = new Dictionary<string, T>();
            foreach (JToken row in _resultSet.Rows())
            {
				var ids = row[docOrValue].Value<JArray>();
				foreach (JToken id in ids) {
					var stringId = id.Value<string>();
					if (!found.ContainsKey(stringId)) {
						var doc = new T();
	               	 	doc.Id = stringId;
						found[stringId] = doc;
						list.Add(doc);
					}
				}
            }
            return list;
        }
		
        protected virtual T RetrieveDocument<T>(string docOrValue) where T : ICanJson, new()
        {
            foreach (JToken row in _resultSet.Rows())
            {
                var doc = new T();
                doc.ReadJson(row[docOrValue].Value<JObject>());
                return doc;
            }
            return default(T);
        }
        protected virtual IEnumerable<T> RetrieveArbitraryDocuments<T>(string docOrValue, Func<T> ctor)
        {
            var list = new List<T>();
            foreach (JToken row in _resultSet.Rows())
            {
                var doc = new CouchDocumentWrapper<T>(ctor);
                doc.ReadJson(row[docOrValue].Value<JObject>());
                list.Add(doc.Instance);
            }
            return list;
        }

        protected virtual T RetrieveArbitraryDocument<T>(string docOrValue, Func<T> ctor)
        {
            foreach (JToken row in _resultSet.Rows())
            {
                var doc = new CouchDocumentWrapper<T>(ctor);
                doc.ReadJson(row[docOrValue].Value<JObject>());
                return doc.Instance;
            }
            return default(T);
        }

        public IEnumerable<CouchQueryDocument> RowDocuments()
        {
            return RowDocuments<CouchQueryDocument>();
        }

        public IEnumerable<T> RowDocuments<T>() where T : ICanJson, new()
        {
            var list = new List<T>();
            foreach (JObject row in _resultSet.Rows())
            {
                var doc = new T();
                doc.ReadJson(row);
                list.Add(doc);
            }
            return list;
        }

        public IEnumerable<T> ArbitraryRowDocuments<T>(Func<T> ctor)
        {
            var list = new List<T>();
            foreach (JObject row in _resultSet.Rows())
            {
                var doc = new CouchDocumentWrapper<T>(ctor);
                doc.ReadJson(row);
                list.Add(doc.Instance);
            }
            return list;
        }
    }
}
