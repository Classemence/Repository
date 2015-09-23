using System;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace Client
{
    class Client
    {
        Socket clientSocket;
        NetworkStream ns;
        StreamReader sr;
        StreamWriter sw;

        Thread pingingThread;
        bool connected = false;
        const int pingPeriod = 1000;


        public void ConnectToServer()
        {
            Console.WriteLine("Connecting to server..");
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Connect(IPAddress.Loopback, 8888);
            Console.WriteLine("Connected");
            BuildNetworkStream();
            BuildStreamWriterAndStreamReader(ns);
            connected = true;

        }

        private void BuildNetworkStream()
        {
            ns = new NetworkStream(clientSocket);


        }

        private void BuildStreamWriterAndStreamReader(NetworkStream ns)
        {
            sr = new StreamReader(ns);
            sw = new StreamWriter(ns);
            sw.AutoFlush = true;
        }


        public string EncryptTextWithPublicKey(string message, string publicKeyXML){

            //This constant uses the server too. Thus here can´t be another value.
            const int  keySize = 1048;

            string encryptedMessage = AsymmetricEncryption.EncryptText(message, keySize, publicKeyXML);

            return encryptedMessage;

        }

        private string GetAnswerFromServer()
        {
            // Server has two seconds to respond
            DateTime answerExpirationTime = DateTime.Now.AddSeconds(2);

            while (true)
            {
#if DEBUG
                Console.WriteLine("Getting response from server");
#endif
                if (answerExpirationTime < DateTime.Now)
                {
#if DEBUG
                    return "ERROR";
#endif
                }
                if (ns.DataAvailable)
                {
                    string answer = sr.ReadLine();
#if DEBUG
                    Console.WriteLine("Answer: " + answer);
#endif
                    return answer;
                }
            }
        }

        /// <summary>
        /// It separates the public key string from the publicKeyResponse which has format of "PUBLICKEY-RESPONSE_publicKey"
        /// </summary>
        /// <param name="publicKeyAnswer">Whole publicKeyResponse in format: "PUBLICKEY-RESPONSE_publicKey"</param>
        /// <returns></returns>
        private string GetPublicKeyFromPublicKeyResponse(string publicKeyResponse)
        {

            string[] publicKeyResponseSplit = publicKeyResponse.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            if (publicKeyResponseSplit[0] != "PUBLICKEY-RESPONSE" || publicKeyResponseSplit.Length != 2) return null;
            else return publicKeyResponseSplit[1];
        }

        /// <summary>
        /// This method gets password and hash it with SHA1. 
        /// </summary>
        /// <param name="password"></param>
        /// <returns>Hashed password</returns>
        private string HashPassword(string password)
        {

            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(password);
            SHA1 sha = new SHA1CryptoServiceProvider();
            byte[] hashedPasswordBytes = sha.ComputeHash(bytes);
            string hashedPassword = Convert.ToBase64String(hashedPasswordBytes);
            return hashedPassword;

        }

        /// <summary>
        /// It builds up a connection to server. When someting fails, it throws proper exception.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public bool LoginToServer(string username, string password)
        {
            try
            {

                ConnectToServer();

                // It receives new generated RSA public key form server
                SendRequestForPublicKeyToServer();
                string publicKeyXML = GetPublicKeyFromPublicKeyResponse(GetAnswerFromServer());

                // Check if error occured
                if (publicKeyXML == null) return false;



                // Sends register request with password encrypted
                SendLoginRequestToServer(username, HashPassword(password), publicKeyXML);
                string answer = GetAnswerFromServer();


                switch (answer)
                {
                    case "LOGIN_OK":
                        SwitchClientIntoLoginState();
                        return true;
                    case "LOGIN_NOK":
                        ns = null;
                        sr = null;
                        sw = null;
                        clientSocket = null;
                        connected = false;

                        return false;
                }
                return false;
            }
            catch
            {
                ns = null;
                sr = null;
                sw = null;
                clientSocket = null;
                connected = false;
                throw;
            }

        }

        public void Logout()
        {
            connected = false;
        }

        public void SendMessageToServer(string message)
        {
            sw.WriteLine(message);
        }

        private void SendLoginRequestToServer(string username, string password, string publicKeyXML)
        {
            string loginRequest = "LOGIN_" + username + "_" + EncryptTextWithPublicKey(password, publicKeyXML);
            SendMessageToServer(loginRequest);
        }

        private void SendRegisterRequestToServer(string username, string password, string publicKeyXML)
        {
            string registerRequest = "REGISTER_" + username + "_" + EncryptTextWithPublicKey(password,publicKeyXML) ;
            SendMessageToServer(registerRequest);
        }

        private void SendRequestForPublicKeyToServer()
        {
            string publicKeyRequest = "PUBLICKEY-REQUEST";
            SendMessageToServer(publicKeyRequest);
        }

        public void ServerPinging()
        {
            while (true)
            {
                if (!connected) break;
                string pingMessage = "PING_ID";
                SendMessageToServer(pingMessage);
                Thread.Sleep(pingPeriod);
            }
        }

        public void StartServerPinging()
        {
            Thread serverPinging = new Thread(ServerPinging);
            pingingThread = serverPinging;
            serverPinging.Start();
        }

        public void SwitchClientIntoLoginState(){

            // Now we will ping the server every pingPeriod-miliseconds to let him know, were still alive
            StartServerPinging();
            connected = true;
        }

   

        /// <summary>
        /// This method connects to server. It gets public key from server and then sends encrypted register request to server.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>Bool value indicates whether the registration was successful</returns>
        public bool RegisterNewUserOnServer(string username, string password)
        {
            try
            {
                ConnectToServer();

                // It receives new generated RSA public key form server
                SendRequestForPublicKeyToServer();
                string publicKeyXML = GetPublicKeyFromPublicKeyResponse(GetAnswerFromServer());

                // Check if error occured
                if (publicKeyXML == null) return false;

                

                // Sends register request with password encrypted
                SendRegisterRequestToServer(username, HashPassword(password), publicKeyXML);
                string answer = GetAnswerFromServer();
#if DEBUG
                Console.WriteLine("Registration Communication");
                Console.WriteLine("Answer from server: " + answer);
#endif 

                switch (answer)
                {
                    case "REGISTRATION_OK":
                        return true;
                    case "REGISTRATION_NOK":
                        ns = null;
                        sr = null;
                        sw = null;
                        clientSocket = null;
                        connected = false;
                        return false;
                }
                return false;
            }
            catch
            {
                ns = null;
                sr = null;
                sw = null;
                clientSocket = null;
                connected = false;
                throw;
            }

        }


    }
}


