using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Divan;

namespace ConsoleApplication1
{
	class Program
	{
		static void Main(string[] args)
		{
			string host = "localhost";
			int port = 5984;
			var server = new CouchServer(host, port);
			var db = server.GetDatabase("messages");
			Console.WriteLine("Created database 'trivial'");
		}


	}
}
