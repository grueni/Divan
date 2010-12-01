using System;
using System.Collections.Generic;
using System.Linq;
using Divan.Lucene;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Divan
{
	/// <summary>
	/// A named design document in CouchDB. Holds CouchViewDefinitions and CouchLuceneViewDefinitions (if you use Couchdb-Lucene).
	/// </summary>
	public class CouchDesignDocument : CouchDocument, IEquatable<CouchDesignDocument>
	{

		public Dictionary<string, ICouchViewDefinition> Views = new Dictionary<string, ICouchViewDefinition>();
		public Dictionary<string, CouchListDefinition> Lists = new Dictionary<string, CouchListDefinition>();
		public Dictionary<string, CouchShowDefinition> Shows = new Dictionary<string, CouchShowDefinition>();

		public IList<CouchViewDefinition> ViewDefinitions = new List<CouchViewDefinition>();
		public IList<CouchListDefinition> ListDefinitions = new List<CouchListDefinition>();
		public IList<CouchShowDefinition> ShowDefinitions = new List<CouchShowDefinition>();

		// This List is only used if you also have Couchdb-Lucene installed
		public IList<CouchLuceneViewDefinition> LuceneDefinitions = new List<CouchLuceneViewDefinition>();

		public string Language = "javascript";
		public ICouchDatabase Owner;

		public CouchDesignDocument(string documentId, ICouchDatabase owner)
			: base("_design/" + documentId)
		{
			Owner = owner;
		}

		public CouchDesignDocument()
		{
			
		}

		/// <summary>
		/// Add view without a reduce function.
		/// </summary>
		/// <param name="name">Name of view</param>
		/// <param name="map">Map function</param>
		/// <returns></returns>
		public CouchViewDefinition AddView(string name, string map)
		{
			return AddView(name, map, null);
		}


		/// <summary>
		/// Add view with a reduce function.
		/// </summary>
		/// <param name="name">Name of view</param>
		/// <param name="map">Map function</param>
		/// <param name="reduce">Reduce function</param>
		/// <returns></returns>
		public CouchViewDefinition AddView(string name, string map, string reduce)
		{
			var def = new CouchViewDefinition(name, map, reduce, this);
			Views.Add(name, def);
			ViewDefinitions.Add(def);
			return def;
		}

		public CouchListDefinition AddList(string name, string list)
		{
			var def = new CouchListDefinition(name, list, this);
			Lists.Add(name, def);
			ListDefinitions.Add(def);
			return def;
		}

		public CouchShowDefinition AddShow(string name, string show)
		{
			var def = new CouchShowDefinition(name, show, this);
			Shows.Add(name, def);
			ShowDefinitions.Add(def);
			return def;
		}

		public void RemoveViewNamed(string viewName)
		{
			RemoveView(FindView(viewName));
		}

		private CouchViewDefinition FindView(string name)
		{
			return ViewDefinitions.Where(x => x.Name == name).First();
		}

		public void RemoveView(CouchViewDefinition view)
		{
			view.Doc = null;
			if (Views.ContainsKey(view.Name))
				Views.Remove(view.Name);
			ViewDefinitions.Remove(view);
		}

		/// <summary>
		/// Add Lucene fulltext view.
		/// </summary>
		/// <param name="name">Name of view</param>
		/// <param name="index">Index function</param>
		/// <returns></returns>
		public CouchLuceneViewDefinition AddLuceneView(string name, string index)
		{
			var def = new CouchLuceneViewDefinition(name, index, this);
			LuceneDefinitions.Add(def);
			return def;
		}

		/// <summary>
		/// Add a Lucene view with a predefined index function that will index EVERYTHING.
		/// </summary>
		/// <returns></returns>
		public CouchLuceneViewDefinition AddLuceneViewIndexEverything(string name)
		{
			return AddLuceneView(name,
								 @"function(doc) {
									var ret = new Document();

									function idx(obj) {
									for (var key in obj) {
										switch (typeof obj[key]) {
										case 'object':
										idx(obj[key]);
										break;
										case 'function':
										break;
										default:
										ret.add(obj[key]);
										break;
										}
									}
									};

									idx(doc);

									if (doc._attachments) {
									for (var i in doc._attachments) {
										ret.attachment(""attachment"", i);
									}
									}}");
		}

		// All these three methods duplicated for Lucene views, perhaps we should hold them all in one List?
		public void RemoveLuceneViewNamed(string viewName)
		{
			RemoveLuceneView(FindLuceneView(viewName));
		}

		private CouchLuceneViewDefinition FindLuceneView(string name)
		{
			return LuceneDefinitions.Where(x => x.Name == name).First();
		}

		public void RemoveLuceneView(CouchLuceneViewDefinition view)
		{
			view.Doc = null;
			LuceneDefinitions.Remove(view);
		}

		/// <summary>
		/// If this design document is missing in the database,
		/// or if it is different - then we save it overwriting the one in the db.
		/// </summary>
		public void Synch()
		{
			if (!Owner.HasDocument(this)) {
				Owner.SaveDocument(this);
			} else
			{
				var docInDb = Owner.GetDocument<CouchDesignDocument>(Id);
				if (!docInDb.Equals(this)) {
					// This way we forcefully save our version over the one in the db.
					Rev = docInDb.Rev;
					Owner.WriteDocument(this);
				}
			}
		}

		public override void WriteJson(JsonWriter writer)
		{
			WriteIdAndRev(this, writer);
			writer.WritePropertyName("language");
			writer.WriteValue(Language);
			writer.WritePropertyName("views");
			writer.WriteStartObject();
			foreach (var definition in ViewDefinitions)
			{
				definition.WriteJson(writer);
			}
			writer.WriteEndObject();

			writer.WritePropertyName("lists");
			writer.WriteStartObject();
			foreach (var definition in ListDefinitions)
			{
				definition.WriteJson(writer);
			}
			writer.WriteEndObject();

			writer.WritePropertyName("shows");
			writer.WriteStartObject();
			foreach (var definition in ShowDefinitions)
			{
				definition.WriteJson(writer);
			}
			writer.WriteEndObject();


			
			// If we have Lucene definitions we write them too
			if (LuceneDefinitions.Count > 0)
			{
				writer.WritePropertyName("fulltext");
				writer.WriteStartObject();
				foreach (var definition in LuceneDefinitions)
				{
					definition.WriteJson(writer);
				}
				writer.WriteEndObject();
			}
		}

		public override void ReadJson(JObject obj)
		{
			ReadIdAndRev(this, obj);
			if (obj["language"] != null)
				Language = obj["language"].Value<string>();
			ViewDefinitions = new List<CouchViewDefinition>();
			var views = (JObject)obj["views"];

			foreach (var property in views.Properties())
			{
				var v = new CouchViewDefinition(property.Name, this);
				v.ReadJson((JObject)views[property.Name]);
				if (Views.ContainsKey(property.Name)) Views.Remove(property.Name);
				Views.Add(property.Name, v);
				ViewDefinitions.Add(v);
			}


			var lists = (JObject)obj["lists"];
			if (lists != null)
			{
				foreach (var property in views.Properties())
				{
					var l = new CouchListDefinition(property.Name, this);
					l.ReadJson((JObject)lists[property.Name]);
					if (Lists.ContainsKey(property.Name)) Lists.Remove(property.Name);
					Lists.Add(property.Name, l);
					ListDefinitions.Add(l);
				}
			}


			var shows = (JObject)obj["shows"];
			if (shows != null)
			{
				foreach (var property in shows.Properties())
				{
					var s = new CouchShowDefinition(property.Name, this);
					var show = shows[property.Name] as JObject;
					if (show == null) continue;
					s.ReadJson(show);
					if (Shows.ContainsKey(property.Name)) Shows.Remove(property.Name);
					Shows.Add(property.Name, s);
					ShowDefinitions.Add(s);
				}
			}



			var fulltext = (JObject)obj["fulltext"];
			// If we have Lucene definitions we read them too
			if (fulltext != null)
			{
				foreach (var property in fulltext.Properties())
				{
					var v = new CouchLuceneViewDefinition(property.Name, this);
					v.ReadJson((JObject) fulltext[property.Name]);
					LuceneDefinitions.Add(v);
				}
			}
		}

		public bool Equals(CouchDesignDocument other)
		{
			return Id.Equals(other.Id) && Language.Equals(other.Language) && ViewDefinitions.SequenceEqual(other.ViewDefinitions);
		}
	}
}