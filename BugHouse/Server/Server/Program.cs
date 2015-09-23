#define debug



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using System.Security;



namespace Server
{


    class Program
    {
        static void Main(string[] args)
        {

            
            Console.WriteLine("Hello to my Server");

            Server server = new Server();
            server.RunLocalServer();




            // Server is accepting new clients at this point
            Console.WriteLine("The server is running properly now");

          /*  while (true)
            {
                Console.WriteLine("Zmáčkni klávesu abys viděl kolik je tu klientů");
                Console.ReadLine();
                Console.WriteLine(server.GetClientCount());
            }*/


         /*   // Connecting to Database
            Database db = new Database();
           // db.ConnectToDatabase();

            db.PrepareDatabaseTable("user3");
            */
        }
    }

    

    
    
}
