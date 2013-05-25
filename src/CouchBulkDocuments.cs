using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Divan
{
    /// <summary>
    /// Only used as psuedo doc when doing bulk updates/inserts.
    /// </summary>
    public class CouchBulkDocuments : ICanJson
    {
        public CouchBulkDocuments(IEnumerable<ICouchDocument> docs)
        {
            Docs = docs;
        }

        public IEnumerable<ICouchDocument> Docs { get; private set; }

        #region ICouchBulk Members

        public int Count()
        {
            return Docs.Count();
        }

        public virtual void WriteJson(JsonWriter jsonWriter)
        {
            jsonWriter.WritePropertyName("docs");
            jsonWriter.WriteStartArray();
            foreach (ICouchDocument doc in Docs)
            {
					 //jsonWriter.WriteStartObject();
					 //doc.WriteJson(jsonWriter);
					 //jsonWriter.WriteEndObject();
					 if (!(doc is ISelfContained))
					 {
						 jsonWriter.WriteStartObject();
						 doc.WriteJson(jsonWriter);
						 jsonWriter.WriteEndObject();
					 }
					 else
						 doc.WriteJson(jsonWriter);
				}
            jsonWriter.WriteEndArray();
        }

        public virtual void ReadJson(JObject obj)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}