using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Divan
{
	public class CouchShowDefinition: CouchViewDefinition
	{
		public string Show {get; set;}
		public string DocId { get; set;}

		public CouchShowDefinition(string name, CouchDesignDocument doc) : base(name, doc) {}


		public CouchShowDefinition(string name, string list, CouchDesignDocument doc): base(name, doc)
		{
			Show = list;
		}

		new public void WriteJson(JsonWriter writer)
		{
			writer.WritePropertyName(Name);
			writer.WriteValue(Show);
		}

		public void ReadJson(string obj)
		{
			this.Show = obj;
		}

		new public void ReadJson(JObject obj)
		{
			Show = (string)obj;
		}

		public override string Path()
		{
			if (Doc.Id == "_design/")
			{
				return Name;
			}
			return Doc.Id + "/_show/" + Name + "/" + DocId;
		}

		new public CouchShowQuery Query()
		{
			return Doc.Owner.Query(this);
		}
	}
}
