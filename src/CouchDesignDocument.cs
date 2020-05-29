using System;
using System.Collections;
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
	public class CouchDesignDocument : CouchDocument, IEquatable<CouchDesignDocument>,ICouchDesignDocument
	{

		public Dictionary<string, ICouchViewDefinition> Views = new Dictionary<string, ICouchViewDefinition>();
		public Dictionary<string, ICouchListDefinition> Lists = new Dictionary<string, ICouchListDefinition>();
		public Dictionary<string, CouchShowDefinition> Shows = new Dictionary<string, CouchShowDefinition>();

		public IList<CouchViewDefinition> ViewDefinitions = new List<CouchViewDefinition>();
		public IList<CouchListDefinition> ListDefinitions = new List<CouchListDefinition>();
		public IList<CouchShowDefinition> ShowDefinitions = new List<CouchShowDefinition>();

		// This List is only used if you also have Couchdb-Lucene installed
		public IList<CouchLuceneViewDefinition> LuceneDefinitions = new List<CouchLuceneViewDefinition>();

		public string Language = "javascript";
		public ICouchDatabase Owner { get; set; }


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

/*
		public override void ReadJson(JObject obj)
		{
			CouchDocument.ReadIdAndRev((ICouchDocument)this, obj);
			if (obj["language"] != null)
				this.Language = (string)Extensions.Value<string>((IEnumerable<JToken>)obj["language"]);
			this.ViewDefinitions = (IList<CouchViewDefinition>)new List<CouchViewDefinition>();
			JObject jobject1 = (JObject)obj["views"];
			using (IEnumerator<JProperty> enumerator = jobject1.Properties().GetEnumerator())
			{
				while (((IEnumerator)enumerator).MoveNext())
				{
					JProperty current = enumerator.Current;
					CouchViewDefinition couchViewDefinition = new CouchViewDefinition(current.Name, this);
					couchViewDefinition.ReadJson((JObject)jobject1[current.Name]);
					if (this.Views.ContainsKey(current.Name))
						this.Views.Remove(current.Name);
					this.Views.Add(current.Name, (ICouchViewDefinition)couchViewDefinition);
					this.ViewDefinitions.Add(couchViewDefinition);
				}
			}
			this.ListDefinitions = (IList<CouchListDefinition>)new List<CouchListDefinition>();
			JObject jobject2 = (JObject)obj["lists"];
			if (jobject2 != null)
			{
				using (IEnumerator<JProperty> enumerator = jobject2.Properties().GetEnumerator())
				{
					while (((IEnumerator)enumerator).MoveNext())
					{
						JProperty current = enumerator.Current;
						CouchListDefinition couchListDefinition = new CouchListDefinition(current.Name, this);
						var test = jobject2[current.Name];
						couchListDefinition.ReadJson(((object)jobject2[current.Name]).ToString());
						if (this.Lists.ContainsKey(current.Name))
							this.Lists.Remove(current.Name);
						this.Lists.Add(current.Name, (ICouchListDefinition)couchListDefinition);
						this.ListDefinitions.Add(couchListDefinition);
					}
				}
			}
			this.ShowDefinitions = (IList<CouchShowDefinition>)new List<CouchShowDefinition>();
			JObject jobject3 = (JObject)obj["shows"];
			if (jobject3 != null)
			{
				using (IEnumerator<JProperty> enumerator = jobject3.Properties().GetEnumerator())
				{
					while (((IEnumerator)enumerator).MoveNext())
					{
						JProperty current = enumerator.Current;
						CouchShowDefinition couchShowDefinition = new CouchShowDefinition(current.Name, this);
						couchShowDefinition.ReadJson(((object)jobject3[current.Name]).ToString());
						if (this.Shows.ContainsKey(current.Name))
							this.Shows.Remove(current.Name);
						this.Shows.Add(current.Name, couchShowDefinition);
						this.ShowDefinitions.Add(couchShowDefinition);
					}
				}
			}
			JObject jobject4 = (JObject)obj["fulltext"];
			if (jobject4 == null)
				return;
			using (IEnumerator<JProperty> enumerator = jobject4.Properties().GetEnumerator())
			{
				while (((IEnumerator)enumerator).MoveNext())
				{
					JProperty current = enumerator.Current;
					CouchLuceneViewDefinition luceneViewDefinition = new CouchLuceneViewDefinition(current.Name, this);
					luceneViewDefinition.ReadJson((JObject)jobject4[current.Name]);
					this.LuceneDefinitions.Add(luceneViewDefinition);
				}
			}
		}
*/

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
				//				v.ReadJson((JObject)views[property.Name]);
				var view = views[property.Name] as JObject;
				v.ReadJson(view);
				if (Views.ContainsKey(property.Name)) Views.Remove(property.Name);
				Views.Add(property.Name, v);
				ViewDefinitions.Add(v);
			}


			var lists = (JObject)obj["lists"];
			if (lists != null)
			{
				foreach (var property in lists.Properties())
				{
					//					l.ReadJson((JObject)lists[property.Name]);
					//					l.ReadJson(((object)lists[property.Name]).ToString());
					var list = lists[property.Name] as JObject;
					var l = new CouchListDefinition(property.Name, this);
					if (list == null) continue;
					l.ReadJson(list);
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
					var show = shows[property.Name] as JObject;
					if (show == null) continue;
					var s = new CouchShowDefinition(property.Name, this);
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