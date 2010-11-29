using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace Divan
{
	public class CouchListQuery : CouchQuery
	{

		public CouchListQuery(CouchListDefinition list) : base(list)
		{
		}

        new public Stream GetResult()
		{
			try
			{
                //var sb = new StringBuilder();
				//var reader = new StreamReader(Request().Response().GetResponseStream());
                //sb.Append(reader.ReadLine());
                //return sb.ToString();
                return Request().Response().GetResponseStream();
			}
			catch (WebException e)
			{
				throw CouchException.Create("Query failed", e);
			}
		}

	}
}
