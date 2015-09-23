using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace Server
{
    public class Client
    {
        Socket clientSocket;
        StreamReader sr;
        StreamWriter sw;
        NetworkStream ns;


        string clientId;
        Server server;
        public bool logged;
        public string clientUniqueId;
        public DateTime checkpointTime;

        // Keys for crypted RSA communication
        private string publicKey;
        private string privateKey;
        public const int keySize = 1048;

        // Thread for communication
        Thread messageListeningThread;
        private volatile bool messageListeningThreadShouldStop = false;

        // Timer for checking if client is still active
        Timer timer;
        const int destructionLoginTime = 2000;




        public Client(Socket clientSocket, string clientId, Server server)
        {
            this.clientSocket = clientSocket;
            ns = new NetworkStream(clientSocket);
            sr = new StreamReader(ns);
            sw = new StreamWriter(ns);
            sw.AutoFlush = true;
            this.clientId = clientId;
            this.server = server;

            // It also sets selfDestruction. We want to handle only active connections
            int destructionTime = 10000;
            SelfDestructionInitialize(destructionTime);
        }


        private void CheckForInnactivity(){

        }

        public void GenerateNewKeys()
        {
            AsymmetricEncryption.GenerateKeys(keySize, out publicKey, out privateKey);
        }

        public string GetPrivateKey()
        {
            return privateKey;
        }

        public string GetPublicKey()
        {
            return publicKey;
        }

        

        public int GetKeySize() { return keySize; }


        /// <summary>
        /// This method listens for incoming messages from networkStream and sends responses to them. It runs in background
        /// </summary>
        public void ListenAndProcessRequests()
        {
            checkpointTime = DateTime.Now;

            Console.WriteLine("Sending a message");
            while (true)
            {
                if (messageListeningThreadShouldStop) break;
                if (ns.DataAvailable)
                {

                    string request = sr.ReadLine();
#if DEBUG
                    Console.WriteLine("Request: " + request);
#endif
                    string response = server.ProcessIncomingRequest(request, this);
                    if (ns.CanWrite)
#if DEBUG
                    Console.WriteLine("Response: " + response);
#endif
                    try
                    {
                        //TODO: Probably here I had "ObjectDisposedException" when client closed and server wanted to send data. Have to check it later!
                        sw.WriteLine(response);
                    }
                    catch (Exception e)
                    {
#if DEBUG
                        Console.WriteLine(e.Message);
#endif
                    }

                }
            }
        }

        /// <summary>
        /// This client should destroy itself after some period of time to not block other clients.
        /// </summary>
        /// <param name="timeOfDestruction">Time in milliseconds after which this client will destroy itself</param>
        private void SelfDestructionInitialize(int destructionTime)
        {
            timer = new Timer(new TimerCallback(SelfDestruction));
            timer.Change(destructionTime, Timeout.Infinite);
        }


        private void SelfDestruction(object o)
        {
            messageListeningThreadShouldStop = true;
            clientSocket.Close();
            ns.Close();
            server.RemoveClient(this);
#if DEBUG
            Console.WriteLine("I destroyed myself muhaha :D ");
            Console.WriteLine("Remianing clients: " + server.GetUnloggedClientCount());
#endif
        }

        
        public void StartMessageListening(Thread messageListeningThread)
        {
            this.messageListeningThread = messageListeningThread;
            messageListeningThread.Start();
        }


        public void SwitchIntoLoginState()
        {
            // Logged user has to send message every two seconds. Otherwise it will be disconnected.
            timer.Change(destructionLoginTime, Timeout.Infinite);

            logged = true;

        }

        public void UpdateDestructionTime()
        {
#if DEBUG
            Console.WriteLine("Updating destruction time.. ");
            Console.WriteLine("Logged Clients: " + server.GetLoggedClientCount());
#endif
            timer.Change(destructionLoginTime, Timeout.Infinite);
            
        }
    }
}
