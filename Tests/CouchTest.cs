using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Newtonsoft.Json;

namespace Divan.Test
{
    /// <summary>
    /// Unit tests for Divan. Operates in a separate CouchDB database called divan_unit_tests.
    /// If you are not running a CouchDB on localhost:5984 you will need to first edit
    /// the Tests/App.config file to point somewhere else.
    /// 
    /// NOTE: App.config is copied to Tests/bin/Debug/Divan.Test.dll.config as a
    /// Custom Command in the Makefile! if someone can tell me how to handle this better on
    /// Mono/Monodevelop I am all ears.
    /// 
    /// Run from command line using something like:rbitr
    /// 	nunit-console2 --labels -run=Divan.Test.CouchTest Tests/bin/Debug/Tests.dll
    /// </summary>
    [TestFixture]
    public class CouchTest
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            var protocol = ConfigurationManager.AppSettings["CouchDB.Protocol"] ?? "http";
            var host = ConfigurationManager.AppSettings["CouchDB.Host"] ?? "localhost";
            var port = Convert.ToInt32(ConfigurationManager.AppSettings["CouchDB.Port"] ?? "5984");
            var user = ConfigurationManager.AppSettings["CouchDB.User"] ?? "";
            var pass = ConfigurationManager.AppSettings["CouchDB.Password"] ?? "";
            server = new CouchServer(protocol,host, port, user, pass);
            DbName = GetNewDbName();
            db = server.GetNewDatabase(DbName);
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                db.Delete();
            }
            catch
            {
            }
        }

        #endregion

        private ICouchServer server;
        private ICouchDatabase db;
        private string DbName;

        private static string GetNewDbName()
        {
            return "divan_unit_tests" + DateTime.Now.Ticks;
        }

        [Test]
        public void ShouldCheckChangedDocument()
        {
            CouchJsonDocument doc = db.CreateDocument("{\"CPU\": \"Intel\"}");
            CouchJsonDocument doc2 = db.GetDocument(doc.Id);
            Assert.That(db.HasDocumentChanged(doc), Is.False);
            doc2.Obj["CPU"] = JToken.FromObject("AMD");
            db.WriteDocument(doc2);
            Assert.That(db.HasDocumentChanged(doc), Is.True);
        }

        [Test]
        public void ShouldCountDocuments()
        {
            Assert.That(db.CountDocuments(), Is.EqualTo(0));
            db.CreateDocument("{\"CPU\": \"Intel\"}");
            Assert.That(db.CountDocuments(), Is.EqualTo(1));
        }

        [Test]
        public void ShouldCreateDocument()
        {
            var doc = new CouchJsonDocument("{\"CPU\": \"Intel\"}");
            ICouchDocument cd = db.CreateDocument(doc);
            Assert.That(db.CountDocuments(), Is.EqualTo(1));
            Assert.That(cd.Id, Is.Not.Null);
            Assert.That(cd.Rev, Is.Not.Null);
        }

        [Test]
        public void ShouldCreateDocuments()
        {
            const string doc = "{\"CPU\": \"Intel\"}";
            var doc1 = new CouchJsonDocument(doc);
            var doc2 = new CouchJsonDocument(doc);
            IList<ICouchDocument> list = new List<ICouchDocument> { doc1, doc2 };
            db.SaveDocuments(list, true);
            Assert.That(db.CountDocuments(), Is.EqualTo(2));
            Assert.That(doc1.Id, Is.Not.Null);
            Assert.That(doc1.Rev, Is.Not.Null);
            Assert.That(doc2.Id, Is.Not.Null);
            Assert.That(doc2.Rev, Is.Not.Null);
            Assert.That(doc1.Id, Is.Not.EqualTo(doc2.Id));
        }

        [Test]
        public void ShouldCreateDocumentWithSpecifiedId()
        {
            const string doc = "{\"_id\": \"foo\"}";
            var doc1 = new CouchJsonDocument(doc);
            db.SaveDocument(doc1);
            Assert.That(db.CountDocuments(), Is.EqualTo(1));
            Assert.That(doc1.Id, Is.EqualTo("foo"));
            Assert.That(doc1.Rev, Is.Not.Null);
        }

        [Test, ExpectedException(typeof(CouchNotFoundException))]
        public void ShouldDeleteDatabase()
        {
            db.Delete();
            Assert.That(server.HasDatabase(db.Name), Is.EqualTo(false));
            server.DeleteDatabase(db.Name); // one more time should fail
        }

        [Test]
        public void ShouldDeleteDocuments()
        {
            const string doc = "{\"CPU\": \"Intel\"}";
            CouchJsonDocument doc1 = db.CreateDocument(doc);
            CouchJsonDocument doc2 = db.CreateDocument(doc);
            if (String.Compare(doc1.Id, doc2.Id) < 0)
            {
                db.DeleteDocuments(doc1.Id, doc2.Id);
            }
            else
            {
                db.DeleteDocuments(doc2.Id, doc1.Id);
            }
            Assert.That(db.HasDocument(doc1.Id), Is.False);
            Assert.That(db.HasDocument(doc2.Id), Is.False);
        }

        [Test, ExpectedException(typeof(CouchException))]
        public void ShouldFailCreateDatabase()
        {
            server.CreateDatabase(db.Name); // one more time should fail
        }

        [Test]
        public void ShouldGetDatabaseNames()
        {
            bool result = server.GetDatabaseNames().Contains(db.Name);
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void ShouldGetDocument()
        {
            const string doc = "{\"CPU\": \"Intel\"}";
            CouchJsonDocument oldDoc = db.CreateDocument(doc);
            CouchJsonDocument newDoc = db.GetDocument(oldDoc.Id);
            Assert.That(oldDoc.Id, Is.EqualTo(newDoc.Id));
            Assert.That(oldDoc.Rev, Is.EqualTo(newDoc.Rev));
        }

        [Test]
        public void ShouldGetDocuments()
        {
            const string doc = "{\"CPU\": \"Intel\"}";
            CouchJsonDocument doc1 = db.CreateDocument(doc);
            CouchJsonDocument doc2 = db.CreateDocument(doc);
            var ids = new List<string> { doc1.Id, doc2.Id };

            // Bulk request for multiple keys.
            var docs = db.GetDocuments(ids);
            Assert.That(doc1.Id, Is.EqualTo(docs.First().Id));
            Assert.That(doc2.Id, Is.EqualTo(docs.Last().Id));

            var keys = new List<object> { doc1.Id, doc2.Id };
            // Bulk query on a view for multple keys.
            docs = db.QueryAllDocuments().Keys(keys).IncludeDocuments().GetResult().Documents();
            Assert.That(doc1.Id, Is.EqualTo(docs.First().Id));
            Assert.That(doc2.Id, Is.EqualTo(docs.Last().Id));
        }

        [Test]
        public void ShouldReturnNullWhenNotFound()
        {
            var doc = db.GetDocument<CouchJsonDocument>("jadda");
            Assert.That(doc, Is.Null);
            CouchJsonDocument doc2 = db.GetDocument("jadda");
            Assert.That(doc2, Is.Null);
        }

        [Test]
        public void ShouldSaveDocumentWithId()
        {
            var doc = new CouchJsonDocument("{\"_id\":\"123\", \"CPU\": \"Intel\"}");
            ICouchDocument cd = db.SaveDocument(doc);
            Assert.That(db.CountDocuments(), Is.EqualTo(1));
            Assert.That(cd.Id, Is.Not.Null);
            Assert.That(cd.Rev, Is.Not.Null);
        }

        [Test]
        public void ShouldSaveDocumentWithoutId()
        {
            var doc = new CouchJsonDocument("{\"CPU\": \"Intel\"}");
            ICouchDocument cd = db.SaveDocument(doc);
            Assert.That(db.CountDocuments(), Is.EqualTo(1));
            Assert.That(cd.Id, Is.Not.Null);
            Assert.That(cd.Rev, Is.Not.Null);
        }


        private class LittleCar
        {
            public string _id, _rev;
            public string Make, Model, docType;
        }

        [Test]
        public void ShouldSaveArbitraryDocument()
        {
            var littleCar = new LittleCar() { docType = "car", Make = "Yugo", Model = "Hell if i know" };
            littleCar = db.SaveArbitraryDocument(littleCar);
            Assert.IsNotNull(littleCar._id);
        }

        [Test]
        public void ShouldSaveArbitraryDocuments()
        {
            var littleCar1 = new LittleCar { docType = "car", Make = "Make1" };
            var littleCar2 = new LittleCar { docType = "car", Make = "Make2" };
            var docs = new List<LittleCar> { littleCar1, littleCar2 };

            db.SaveArbitraryDocuments(docs, true);
            var documentIds = db.GetAllDocuments().Select(doc => doc.Id);
            var loadedCars = db.GetArbitraryDocuments(documentIds, () => new LittleCar());
            
            Assert.AreEqual(littleCar1.Make, loadedCars.ElementAt(0).Make);
            Assert.AreEqual(littleCar2.Make, loadedCars.ElementAt(1).Make);
        }

        [Test]
        public void ShouldLoadArbitraryDocument()
        {
            var firstCar = new LittleCar() { docType = "car", Make = "Yugo", Model = "Hell if i know" };
            firstCar = db.SaveArbitraryDocument(firstCar);
            var otherCar = db.GetArbitraryDocument<LittleCar>(firstCar._id, () => new LittleCar());
            Assert.IsNotNull(otherCar);
            Assert.IsNotNull(otherCar._id);
        }

        [Test]
        public void ShouldStoreGetAndDeleteAttachment()
        {
            var doc = new CouchJsonDocument("{\"CPU\": \"Intel\"}");
            ICouchDocument cd = db.CreateDocument(doc);
            var attachmentName = "someAttachment.txt";
            Assert.That(db.HasAttachment(cd, attachmentName), Is.False);
            db.WriteAttachment(cd, attachmentName, "jabbadabba", "text/plain");
            Assert.That(db.HasAttachment(cd, attachmentName), Is.True);

            using (var response = db.ReadAttachment(cd, attachmentName))
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    Assert.That(reader.ReadToEnd(), Is.EqualTo("jabbadabba"));
                }
            }

            db.WriteAttachment(cd, attachmentName, "jabbadabba-doo", "text/plain");
            Assert.That(db.HasAttachment(cd, attachmentName), Is.True);

            using (var response = db.ReadAttachment(cd, attachmentName))
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    Assert.That(reader.ReadToEnd(), Is.EqualTo("jabbadabba-doo"));
                }
            }

            db.DeleteAttachment(cd, attachmentName);

            Assert.That(db.HasAttachment(cd, attachmentName), Is.False);
        }

        [Test, ExpectedException(typeof(CouchConflictException))]
        public void ShouldThrowConflictExceptionOnAlreadyExists()
        {
            const string doc = "{\"CPU\": \"Intel\"}";
            CouchJsonDocument doc1 = db.CreateDocument(doc);
            var doc2 = new CouchJsonDocument(doc) { Id = doc1.Id };
            db.WriteDocument(doc2);
        }

        [Test, ExpectedException(typeof(CouchConflictException))]
        public void ShouldThrowConflictExceptionOnStaleWrite()
        {
            const string doc = "{\"CPU\": \"Intel\"}";
            CouchJsonDocument doc1 = db.CreateDocument(doc);
            CouchJsonDocument doc2 = db.GetDocument(doc1.Id);
            doc1.Obj["CPU"] = JToken.FromObject("AMD");
            db.SaveDocument(doc1);
            doc2.Obj["CPU"] = JToken.FromObject("Via");
            db.SaveDocument(doc2);
        }

        [Test, ExpectedException(typeof(CouchNotFoundException))]
        public void ShouldHaveExtendedExceptionMessages()
        {
            try
            {
                db.Query("invalid", "view").GetResult();
            }
            catch (CouchException ex)
            {
                Assert.That(ex.Message.Contains("reason: "), String.Format("Expected extended exception text, with 'reason' field, received '{0}'", ex.Message));
            }
        }

        [Test]
        public void ShouldUseETagForView()
        {
            var design = db.NewDesignDocument("computers");
            design.AddView("by_cpumake",
                           @"function(doc) {
                        emit(doc.CPU, doc);
                    }");
            db.WriteDocument(design);

            CouchJsonDocument doc1 = db.CreateDocument("{\"CPU\": \"Intel\"}");
            db.CreateDocument("{\"CPU\": \"AMD\"}");
            db.CreateDocument("{\"CPU\": \"Via\"}");
            db.CreateDocument("{\"CPU\": \"Sparq\"}");

            CouchQuery query = db.Query("computers", "by_cpumake").StartKey("Intel").EndKey("Via").CheckETagUsingHead();
            // Query has no result yet so should not be cached
            Assert.That(query.IsCachedAndValid(), Is.False);
            query.GetResult();
            // Now it is cached and should be valid
            Assert.That(query.IsCachedAndValid(), Is.True);
            // Make a change invalidating the view
            db.SaveDocument(doc1);
            // It should now be false
            Assert.That(query.IsCachedAndValid(), Is.False);
            query.GetResult();
            // And now it should be cached again
            Assert.That(query.IsCachedAndValid(), Is.True);
            query.GetResult();
            // Still cached of course
            Assert.That(query.IsCachedAndValid(), Is.True);
        }

        [Test]
        public void ShouldWriteDocument()
        {
            var doc = new CouchJsonDocument("{\"_id\":\"123\", \"CPU\": \"Intel\"}");
            ICouchDocument cd = db.WriteDocument(doc);
            Assert.That(db.CountDocuments(), Is.EqualTo(1));
            Assert.That(cd.Id, Is.Not.Null);
            Assert.That(cd.Rev, Is.Not.Null);
        }

        [Test]
        public void ShouldSyncDesignDocuments()
        {
            var design = db.NewDesignDocument("computers");
            design.AddView("by_cpumake",
                           @"function(doc) {
                        emit(doc.CPU, doc);
                    }");
            db.SynchDesignDocuments(); // This writes them to the db.

            var db2 = server.GetDatabase(DbName);
            design = db2.NewDesignDocument("computers");
            design.AddView("by_cpumake",
                           @"function(doc) {
                        emit(doc.CPU, nil);
                    }");
            db2.SynchDesignDocuments(); // This should detect difference and overwrite the one in the db

            Assert.That(db.GetDocument<CouchDesignDocument>("_design/computers").ViewDefinitions[0].Map,
                        Is.EqualTo(
                            @"function(doc) {
                        emit(doc.CPU, nil);
                    }"));
        }

        /// <summary>
        /// Test that keys can be given as C# types representing proper JSON values:
        ///  string, number, true, false, null, JSON array and JSON object.
        /// </summary>
        [Test]
        public void ShouldGetProperJsonValueForQueryKey()
        {
            var query = db.Query("test", "test");
            query.Key("a string");
            Assert.That(query.Options["key"].Equals("\"a string\""));
            query.Key(12);
            Assert.That(query.Options["key"].Equals("12"));
            query.Key(-12.0);
            Assert.That(query.Options["key"].Equals("-12.0"));
            query.Key(true);
            Assert.That(query.Options["key"].Equals("true"));
            query.Key(false);
            Assert.That(query.Options["key"].Equals("false"));
            query.Key(null);
            Assert.That(query.Options["key"].Equals("null"));

            query.Key(new[] { "one", "two" });
            var json = Regex.Replace(query.Options["key"], @"\s", ""); // removes all whitespace
            Assert.That(json.Equals("[\"one\",\"two\"]"));

            var dict = new Dictionary<string, string>();
            dict["one"] = "two";
            dict["three"] = "four";
            query.Key(dict);
            json = Regex.Replace(query.Options["key"], @"\s", ""); // removes all whitespace
            Assert.That(json.Equals("{\"one\":\"two\",\"three\":\"four\"}"));

            query.Key("one", "two");
            json = Regex.Replace(query.Options["key"], @"\s", ""); // removes all whitespace
            Assert.That(json.Equals("[\"one\",\"two\"]"));
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void ShouldHandleStreaming()
        {
            var design = new CouchDesignDocument("test", db);
            design.AddView("docs", "function (doc) { emit(null, null); } ");
            design.Synch();

            var doc = new CouchJsonDocument("{\"_id\":\"123\", \"CPU\": \"Intel\"}");
            db.SaveDocument(doc);

            var result = db.Query("test", "docs").StreamResult<CouchDocument>();

            Assert.That(result.FirstOrDefault() != null, "there should be one doc present");

            try
            {
                // this should throw an invalid operation exception
                result.FirstOrDefault();
            }
            finally
            {
                db.DeleteDocument(doc);
                db.DeleteDocument(design);
            }
        }

        /// <summary>
        /// Test that arbitrary documents can be saved and read without the need of WriteJson and ReadJson methods.
        /// Needs a change in CouchDocumentWrapper.
        /// Id as guid will be overwritten with an readable Id just for sake of easy reading in Fuson.
        /// </summary>
        [Test]
        public void ShouldSaveAndCompareECarArbitraryDocuments()
        {
            var shortDistanceEcar = new ECar("town", false);
            var longDistanceEcar = new ECar("weekendtrips", true);
            shortDistanceEcar = db.SaveArbitraryDocument<ECar>(shortDistanceEcar);
            longDistanceEcar = db.SaveArbitraryDocument<ECar>(longDistanceEcar);
            Console.WriteLine("shortDistanceEcar Id={0} Rev={1}", shortDistanceEcar.Id, shortDistanceEcar.Rev);
            Assert.That(shortDistanceEcar.Rev, Is.Not.Null);
            Assert.That(longDistanceEcar.Rev, Is.Not.Null);
            var test1 = db.GetArbitraryDocument<ECar>(shortDistanceEcar.Id, () => new ECar());
            Assert.That(shortDistanceEcar.Distance == test1.Distance && shortDistanceEcar.name.Equals(test1.name));
            var test2 = db.GetArbitraryDocument<ECar>(longDistanceEcar.Id, () => new ECar());
            Assert.That(longDistanceEcar.Distance == test2.Distance && longDistanceEcar.name.Equals(test2.name));
            longDistanceEcar.Distance = longDistanceEcar.Distance * 2;
            var rev0 = longDistanceEcar.Rev;
            longDistanceEcar = db.SaveArbitraryDocument<ECar>(longDistanceEcar);
            Assert.That(!rev0.Equals(longDistanceEcar.Rev), "Revisions must be different!");
            db.DeleteDocument(shortDistanceEcar.Id, shortDistanceEcar.Rev);
            db.DeleteDocument(longDistanceEcar.Id, longDistanceEcar.Rev);
        }

        private class Car : CouchDocument
        {
            public string Make;
            public string Model;
            public int HorsePowers;
        }

        /// <summary>
        /// This class allows the use of the serialization capabilities of Jsonsoft.
        /// There is no need to override WriteJson or ReadJson and to define each properties to serialize.
        /// You may choose the optin or the optout approach. 
        /// In case of MemberSerialization.OptOut you have to define the properties which should not be serialized with JsonIgnore.
        /// Instances of the class must be handled as arbitrary documents with divan.
        /// The use of own guids is optional.
        /// </summary>
        [JsonObject(MemberSerialization.OptOut)]
        private class DCICouchDocument 
        {
            private readonly JsonSerializer serializer = new JsonSerializer();

            private String _id;
            private String _rev;

            public DCICouchDocument()
            {
                _id = getNewGUID().ToString();
            }

            public DCICouchDocument(String type)
                : this()
            {
                this.type = type;
            }

            public String type { get; set; }

            public String name { get; set; }

            public Guid getGUID(String guid)
            {
                return Guid.Parse(guid);
            }

            public Guid getNewGUID()
            {
                return Guid.NewGuid();
            }

            [DefaultSettingValue("")]
            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, PropertyName = "_id")]
            public String Id { get { return _id; } set { _id = value; } }

            [DefaultSettingValue("")]
            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, PropertyName = "_rev")]
            public String Rev { get { return _rev; } set { _rev = value; } }

        }

        private class ECar : DCICouchDocument
        {
            static readonly String _type = "ECar";
            private Boolean _hasRangeExtender = false;
            private int _distance = 100;
            public Boolean HasRangeExtender { get { return _hasRangeExtender; } set { if (value) { Distance = 500; _hasRangeExtender = value; } } }
            public int Distance { get { return _distance; } set { _distance = value; } }
            public ECar()
            {
            }
            public ECar(String name, Boolean HasRangeExtender)
                : base(_type)
            {
                this.name = name;
                this.HasRangeExtender = HasRangeExtender;
            }
            public ECar(String Id, String name, Boolean HasRangeExtender)
                : base(_type)
            {
                this.Id = Id;
                this.name = name;
                this.HasRangeExtender = HasRangeExtender;
            }
        }
    }
}