using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace Divan
{
	public class CouchShowQuery: CouchQuery
	{
		public CouchShowQuery(CouchShowDefinition show) : base(show)
		{
		}

		new public  string GetResult()
		{
			try
			{
				var sb = new StringBuilder();
				var reader = new StreamReader(Request().Response().GetResponseStream());
				sb.Append(reader.ReadLine());
				return sb.ToString();
			}
			catch (WebException e)
			{
				throw CouchException.Create("Query failed", e);
			}
		}

	}
}
