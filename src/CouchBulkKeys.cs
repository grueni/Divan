using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Divan
{
    /// <summary>
    /// Only used as psuedo doc when doing bulk reads.
    /// </summary>
    public class CouchBulkKeys : ICanJson
    {
        public CouchBulkKeys(IEnumerable<object> keys)
        {
            Keys = keys.ToArray();
        }

        public CouchBulkKeys()
        {
        }

        public CouchBulkKeys(object[] keys)
        {
            Keys = keys;
        }

        public object[] Keys { get; set; }

        #region ICouchBulk Members

        public virtual void WriteJson(JsonWriter writer)
        {
            writer.WritePropertyName("keys");
            writer.WriteStartArray();
            foreach (var id in Keys)
            {
                //tretiy3
				if (id is JToken)
				{
					writer.WriteRawValue(id.ToString());
				}
				else
				{
					writer.WriteValue(id);
				}
            }
            writer.WriteEndArray();
        }

        public virtual void ReadJson(JObject obj)
        {
            throw new NotImplementedException();
        }

        public int Count()
        {
            return Keys.Count();
        }

        #endregion
    }
}