using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Server
{
    public class Server
    {

        Socket mainSocket;
        Database mainDatabase;

        private List<Client> unloggedClients;
        private List<Client> loggedClients;




        public Server()
        {
            unloggedClients = new List<Client>();
            loggedClients = new List<Client>();
        }


        private string GenerateUniqueId()
        {
            return Guid.NewGuid().ToString("N");
        }

        public int GetUnloggedClientCount()
        {
            return unloggedClients.Count;
        }

        public int GetLoggedClientCount()
        {
            return loggedClients.Count;
        }

        /// <summary>
        /// It gets a request from network, process it, and returns response
        /// </summary>
        /// <returns>Returns response message</returns>
        public string ProcessIncomingRequest(string request, Client client)
        {
            string[] requestSplit = request.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            string response = null;
#if DEBUG
            Console.WriteLine("Processing request: "+ request);
            Console.WriteLine("RequestSplit[0]: " + requestSplit[0]);
#endif
            switch (requestSplit[0])
            {
                case "PUBLICKEY-REQUEST":
                    client.GenerateNewKeys();
                    response = "PUBLICKEY-RESPONSE_" + client.GetPublicKey();
                    break;
                case "LOGIN":
                    response = ProcessLoginRequest(requestSplit, client);
                    break;
                case "REGISTER":
                    response = ProcessRegisterRequest(requestSplit, client);
                    break;
                case "PING":
                    client.UpdateDestructionTime();
                    response = "OK";
                    break;
            }

#if DEBUG
            Console.WriteLine("Request processed_Response is: " + response);
#endif
            return response;

        }

        /// <summary>
        /// Processing login request
        /// </summary>
        /// <param name="requestSplit">Format: LOGIN_username_password</param>
        /// <returns>Response message for client</returns>
        public string ProcessLoginRequest(string[] requestSplit, Client client)
        {
            //TODO: What if this user is already logged in ? (On different computer?)

            if (requestSplit.Length != 3) return null;
            string username = requestSplit[1];
            string password = AsymmetricEncryption.DecryptText(requestSplit[2], client.GetKeySize(), client.GetPrivateKey());

            List<string> userSelect = mainDatabase.SelectUsers(username, password);
            if (userSelect.Count == 1)
            {
                //We have to update status of our client to 'connected'
                SwitchClientIntoLoginState(client);
                return "LOGIN_OK";
            }
            return "LOGIN_NOK";
        }


        /// <summary>
        /// Register new user into database
        /// </summary>
        /// <param name="requestSplit">Format: REGISTER_username_password</param>
        /// <returns>Response message for client</returns>
        public string ProcessRegisterRequest(string[] requestSplit, Client client)
        {
            if (requestSplit.Length != 3) return null;
            string username = requestSplit[1];

            string password = AsymmetricEncryption.DecryptText(requestSplit[2], client.GetKeySize(), client.GetPrivateKey());

            User user = new User(username, password, "mailadress");
            
             bool insertSussesful =  mainDatabase.RegisterUser(user);
             if (insertSussesful)
             {
                 return "REGISTRATION_OK";
             }
             else return "REGISTRATION_NOK";
        }


        public void RemoveClient(Client clientToRemove)
        {
            if (clientToRemove.logged)
            {
                lock (loggedClients)
                {
                    loggedClients.Remove(clientToRemove);
                }
            }
            else
            {
                lock (unloggedClients)
                {
                    unloggedClients.Remove(clientToRemove);
                }
            }
        }

        public void RunLocalServer()
        {
            mainDatabase = new Database();
            mainDatabase.ConnectToDatabase();

            unloggedClients = new List<Client>();
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Loopback, 8888));
            socket.Listen(10);
            mainSocket = socket;
            Thread socketListening = new Thread(SocketListening);
            socketListening.Start();

            Console.WriteLine("Database test");
            List<string> users = mainDatabase.SelectUsers("Pepa", "1234");
            for (int i = 0; i < users.Count; i++)
            {
                Console.WriteLine(users[i]);
            }
        }

        private void SocketListening()
        {
            while (true)
            {
                Console.WriteLine("Waiting for Client... ");
                Socket newSocket = mainSocket.Accept();
                Client newClient;


                lock (unloggedClients)
                {
                    newClient = new Client(newSocket, GenerateUniqueId(), this);
                    unloggedClients.Add(newClient);
                }

                Thread messageListening = new Thread(newClient.ListenAndProcessRequests);
                newClient.StartMessageListening(messageListening);

            }
        }

        private void SwitchClientIntoLoginState(Client client)
        {
            lock (loggedClients)
            {
                unloggedClients.Remove(client);
            }
            lock (unloggedClients)
            {
                loggedClients.Add(client);
            }

            client.SwitchIntoLoginState();
        }

    }
}
