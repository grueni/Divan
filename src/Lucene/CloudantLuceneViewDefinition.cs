using System;

namespace Divan.Lucene
{
    /// <summary>
    /// View definition for Lucene query on Cloudant service
    /// </summary>
    public class CloudantLuceneViewDefinition : CouchViewDefinitionBase, IEquatable<CloudantLuceneViewDefinition>
    {
        /// <summary>
        /// Basic constructor used in ReadJson() etc.
        /// </summary>
        /// <param name="name">View name used in URI.</param>
        /// <param name="doc">A design doc, can also be created on the fly.</param>
        public CloudantLuceneViewDefinition(string name, CouchDesignDocument doc) : base(name, doc) { }
        
        /// <summary>
        /// Constructor used for permanent views, see CouchDesignDocument.
        /// </summary>
        /// <param name="name">View name.</param>
        /// <param name="index">Index function.</param>
        /// <param name="doc">Parent document.</param>
        public CloudantLuceneViewDefinition(string name, string index, CouchDesignDocument doc)
            : base(name, doc)
        {
            Index = index;
        }

        public string Index { get; set; }

        public CloudantLuceneQuery Query()
        {
            return Db().Query(this);
        }

        public override string Path()
        {
            return "_search";
        }

        public bool Equals(CloudantLuceneViewDefinition other)
        {
            return Name.Equals(other.Name) && Index.Equals(other.Index);
        }
    }
}
