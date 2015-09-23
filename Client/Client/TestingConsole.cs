
#define DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Client
{
    class TestingConsole
    {

        Client client;

        public TestingConsole(Client client) { this.client = client; }
        public void RunTestingEngine()
        {
            RunMainPage();
        }

        private void RunMainPage()
        {
            Console.Clear();
            Console.WriteLine("Nacházíte se na hlavní obrazovce. ");
            Console.WriteLine("1: Přihlásit");
            Console.WriteLine("2: Registrovat");
            Console.WriteLine();
            Console.WriteLine("0: Exit");

            string choice = Console.ReadLine();
            ProcessChoice(choice);
        }

        private void ProcessChoice(string choice)
        {
            switch (choice)
            {
                case "1":
                    RunLoginPage();
                    break;

                case "2":
                    RunRegisterPage();
                    break;
                case "MainPage_1":
                    RunChatWindow();
                    break;
                
                case "MainPage_0":
                    client.Logout();
                    RunMainPage();
                    break;

                default:
                    return;

            }
        }

        private void RunChatWindow()
        {
            Console.Clear();
            Console.WriteLine("Zde se bude chatovat jak o život :D ");
        }

        private void RunGame()
        {
            Console.Clear();
            Console.WriteLine("Gratulujeme, byl jste úspěšně přihlášen do hry :-)");

            Console.WriteLine("Vyberte dále co chcete dělat: ");
            Console.WriteLine("1: Hrát hru");
            Console.WriteLine("0: Odhlásit se");

            string option = "MainPage_"+Console.ReadLine();
            ProcessChoice(option);


        }

        private void RunRegisterPage()
        {
            Console.Clear();
            Console.WriteLine("Nacházíte se na registrační obrazovce");
            Console.WriteLine("Zadejte jméno");
            string username = Console.ReadLine();
            Console.WriteLine("Zadejte heslo");
            string password = Console.ReadLine();
            try
            {
                bool registerSuccess = client.RegisterNewUserOnServer(username, password);
                if (registerSuccess) Console.WriteLine("Registrace proběhla úspěšně");
                else Console.WriteLine("Registrace byla neúspěšná");
                Console.WriteLine("Zmáčkněte klávesu");
                Console.ReadLine();
                RunMainPage();
            }
            catch (Exception e)
            {
                Console.WriteLine("Přihlášení se nepovedlo");
                Console.WriteLine(e.Message);
                Console.WriteLine("Zmáčkněte klávesu.. ");
                Console.ReadLine();
                RunMainPage();
            }
        }


        private void RunLoginPage()
        {
            Console.Clear();
            Console.WriteLine("Nacházíte se na přihlašovací obrazovce");
            Console.WriteLine("Zadejte jméno");
            string username = Console.ReadLine();
            Console.WriteLine("Zadejte heslo");
            string password = Console.ReadLine();
            try
            {
                if (client.LoginToServer(username, password))
                {
                    RunGame();
                }
                else throw new System.Security.Authentication.AuthenticationException();
            }
            catch (Exception e)
            {
                Console.WriteLine("Přihlášení se nepovedlo");
                Console.WriteLine(e.Message);
                Console.WriteLine("Zmáčkněte klávesu.. ");
                Console.ReadLine();
                RunMainPage();
            }

        }
    }
}
