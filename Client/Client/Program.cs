using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            
            Client client = new Client();
            Console.WriteLine("Starting Engine");
            TestingConsole testingConsole = new TestingConsole(client);
            testingConsole.RunTestingEngine();


        }
    }
}
    
   

