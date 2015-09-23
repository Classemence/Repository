using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class User
    {
        public string username;
        public int id;
        public string password;
        public string nickname;
        public string emailAdress;

        public User(string username, int id)
        {
            this.username = username;
            this.id = id;
        }

        /// <summary>
        /// Creating new user
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        public User(string login, string password, string emailAdress)
        {
            this.password = password;
            this.username = login;
            this.nickname = login;
            this.emailAdress = emailAdress;
        }

    }

    public struct Message
    {
        string usernameFrom;
        string usernameTo;

        //message is escaped
        string message;

        public Message(string usernameFrom, string usernameTo, string message)
        {
            this.usernameFrom = usernameFrom;
            this.usernameTo = usernameTo;
            this.message = message;
        }
    }
}
