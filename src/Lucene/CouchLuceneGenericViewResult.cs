using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Divan.Lucene
{
    public class CouchLuceneGenericViewResult : CouchLuceneViewResult, IGenericViewResult
    {
        private readonly CouchGenericViewResultBase _worker;

        public CouchLuceneGenericViewResult()
        {
            _worker = new CouchGenericViewResultBase(this);
        }

        public IEnumerable<T> Values<T>() where T : new()
        {
            return _worker.Values<T>();
        }

        public IEnumerable<T> ValueDocuments<T>() where T : ICanJson, new()
        {
            return _worker.ValueDocuments<T>();
        }

        public IEnumerable<T> ValueDocumentsWithIds<T>() where T : ICouchDocument, new()
        {
            return _worker.ValueDocumentsWithIds<T>();
        }

        public IEnumerable<T> ValueDocuments<T>(Func<T> ctor)
        {
            return _worker.ValueDocuments(ctor);
        }

        public T ValueDocument<T>() where T : ICanJson, new()
        {
            return _worker.ValueDocument<T>();
        }

        public T ArbitraryValueDocument<T>(Func<T> ctor)
        {
            return _worker.ArbitraryValueDocument(ctor);
        }

        public IEnumerable<T> ArbitraryValueDocuments<T>(Func<T> ctor)
        {
            return _worker.ArbitraryValueDocuments(ctor);
        }

        public IEnumerable<T> Documents<T>() where T : ICouchDocument, new()
        {
            return _worker.Documents<T>();
        }

        public IEnumerable<T> ArbitraryDocuments<T>(Func<T> ctor)
        {
            return _worker.ArbitraryDocuments(ctor);
        }

        public IEnumerable<CouchJsonDocument> Documents()
        {
            return _worker.Documents();
        }

        public T Document<T>() where T : ICouchDocument, new()
        {
            return _worker.Document<T>();
        }

        public T ArbitraryDocument<T>(Func<T> ctor)
        {
            return _worker.ArbitraryDocument(ctor);
        }

        public IEnumerable<CouchQueryDocument> RowDocuments()
        {
            return _worker.RowDocuments();
        }

        public IEnumerable<T> RowDocuments<T>() where T : ICanJson, new()
        {
            return _worker.RowDocuments<T>();
        }

        public IEnumerable<T> ArbitraryRowDocuments<T>(Func<T> ctor)
        {
            return _worker.ArbitraryRowDocuments(ctor);
        }
    }
}
