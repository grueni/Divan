using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Divan
{
	public class CouchListDefinition : CouchViewDefinition,ICouchListDefinition
	{
		public string List {get; set;}
		public string ViewName { get; set;}

		public CouchListDefinition(string name, CouchDesignDocument doc) : base(name, doc) {}


		public CouchListDefinition(string name, string list, CouchDesignDocument doc): base(name, doc)
		{
			List = list;
		}

		new public void WriteJson(JsonWriter writer)
		{
			writer.WritePropertyName(Name);
			writer.WriteValue(List);
		}

		public void ReadJson(string obj)
		{
			this.List = obj;
		}

		new public void ReadJson(JObject obj)
		{
			List = (string)obj;
		}

		public override string Path()
		{
			if (Doc.Id == "_design/")
			{
				return Name;
			}
			return Doc.Id + "/_list/" + Name + "/" + ViewName;
		}

		new public CouchListQuery Query()
		{
			return Doc.Owner.Query(this);
		}
	}
}
