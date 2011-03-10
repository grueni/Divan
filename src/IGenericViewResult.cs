using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Divan
{
    public interface IGenericViewResult
    {
        /// <summary>
        /// Return all found values of given type
        /// </summary>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <returns>All found values.</returns>
        IEnumerable<T> Values<T>() where T : new();

        /// <summary>
        /// Return all found values as documents of given type
        /// </summary>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <returns>All found values.</returns>
        IEnumerable<T> ValueDocuments<T>() where T : ICanJson, new();

        /// <summary>
        /// Return all ids in value as documents of given type.
        /// </summary>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <returns>All found documents.</returns>
        IEnumerable<T> ValueDocumentsWithIds<T>() where T : ICouchDocument, new();

        /// <summary>
        /// Return first value found as document of given type.
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        /// <returns>First value found or null if not found.</returns>
        IEnumerable<T> ValueDocuments<T>(Func<T> ctor);

        T ValueDocument<T>() where T : ICanJson, new();

        T ArbitraryValueDocument<T>(Func<T> ctor);

        IEnumerable<T> ArbitraryValueDocuments<T>(Func<T> ctor);

        /// <summary>
        /// Return all found docs as documents of given type
        /// </summary>
        /// <typeparam name="T">Type of documents.</typeparam>
        /// <returns>List of documents found.</returns>
        IEnumerable<T> Documents<T>() where T : ICouchDocument, new();

        IEnumerable<T> ArbitraryDocuments<T>(Func<T> ctor);

        /// <summary>
        /// Return all found docs as CouchJsonDocuments.
        /// </summary>
        /// <returns>List of documents found.</returns>
        IEnumerable<CouchJsonDocument> Documents();

        /// <summary>
        /// Return first document found as document of given type
        /// </summary>
        /// <typeparam name="T">Type of document</typeparam>
        /// <returns>First document found or null if not found.</returns>
        T Document<T>() where T : ICouchDocument, new();

        T ArbitraryDocument<T>(Func<T> ctor);

        IEnumerable<CouchQueryDocument> RowDocuments();

        IEnumerable<T> RowDocuments<T>() where T : ICanJson, new();

        IEnumerable<T> ArbitraryRowDocuments<T>(Func<T> ctor);
    }
}
