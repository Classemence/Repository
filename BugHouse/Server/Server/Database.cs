using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MySql.Data.Types;

namespace Server
{


    class Database
    {
        private static string host;
        private static string database;
        private static string user;
        private static string password;
        private static string usersTable;
        private static string gamesTable;
        private static string messagesTable;
        MySqlConnection dbConn;

        static Database(){
            usersTable = "users";
            gamesTable = "games";
            messagesTable = "messages";
            host = "localhost";
            database = "bughouse_db";
            user = "root";
            password = "";
        }

        public Database()
        {
            dbConn = null;
        }
        public void ConnectToDatabase()
        {
            string connectionString;
            // Create a provider string from our details

            connectionString = "Data Source=" + host + ";Database=" + database + ";User ID=" + user + ";Password=" + password;

            // Create a new connection to the database (this doesn't actually connect yet - we have to call .Open for that!)
            try
            {
                dbConn = new MySqlConnection(connectionString);
                dbConn.Open();
            }
            catch (MySqlException e)
            {
                Console.WriteLine(e.Message);
            }
            Console.WriteLine("Connected");
        }

        public bool InsertUser(User newUser)
        {
            string query = "INSERT INTO " + usersTable + " (username, password, nickname, mail) VALUES('" + newUser.username + "','" + newUser.password + "','" + newUser.nickname + "','" + newUser.emailAdress + "');";
            MySqlCommand cmd = new MySqlCommand(query, dbConn);
            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public void PrepareDatabaseTable(string tableName)
        {
            string connectionString;
            // Create a provider string from our details

            connectionString = "Data Source=" + host + ";Database=" + database + ";User ID=" + user + ";Password=" + password;
            MySqlConnection dbConn = null;

            // Create a new connection to the database (this doesn't actually connect yet - we have to call .Open for that!)
            try
            {
                dbConn = new MySqlConnection(connectionString);
                dbConn.Open();

                //Creating and preparing table
                string sqlCommand = "CREATE TABLE " + tableName + "(id int NOT NULL AUTO_INCREMENT, username varchar(255), password varchar(512), PRIMARY KEY(ID))";
                MySqlCommand command = new MySqlCommand(sqlCommand, dbConn);
                command.ExecuteNonQuery();

                sqlCommand = "INSERT INTO " + tableName + "(username, password) VALUES('Pepa','1234')";
                command = new MySqlCommand(sqlCommand, dbConn);
                command.ExecuteNonQuery();

                Console.WriteLine("Succesfully created");

            }
            catch (MySqlException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Checks whether username is unique, if yes it registers new user
        /// </summary>
        /// <param name="newUser"></param>
        /// <returns>bool value indicates if registration was OK</returns>
        public bool RegisterUser(User newUser)
        {

            List<string> users = SelectUsers(newUser.username);

            // users.Count ==0 means that username is unique
            Console.WriteLine("Count of users");
            Console.WriteLine(users.Count);
            if (users.Count == 0)
            {
                return InsertUser(newUser);
            }
            else return false;


        }

        public List<string> SelectUsers(string username, string password)
        {
            string query = "SELECT id from " + usersTable + " WHERE username='" + username + "' AND password='" + password + "';";
            MySqlCommand cmd = new MySqlCommand(query, dbConn);
            using (MySqlDataReader dataReader = cmd.ExecuteReader())
            {
                List<string> results = new List<string>();

                while (dataReader.Read())
                {
                    results.Add(dataReader["id"].ToString());
                }
                return results;
            }
        }

        public List<string> SelectUsers(string username)
        {
            string query = "SELECT id from " + usersTable + " WHERE username='" + username + "';";
            MySqlCommand cmd = new MySqlCommand(query, dbConn);
            using (MySqlDataReader dataReader = cmd.ExecuteReader())
            {
                List<string> results = new List<string>();

                while (dataReader.Read())
                {
                    results.Add(dataReader["id"].ToString());
                }
                return results;
            }
        }

        /// <summary>
        /// Inserts new game into db
        /// </summary>
        /// <param name="user1">Winning team White player</param>
        /// <param name="user2">Winning team Black player</param>
        /// <param name="user3">Losing team White player</param>
        /// <param name="user4">Losing team Black player</param>
        /// <param name="bpgn"></param>
        /// <returns></returns>
        public bool InsertGame(User user1, User user2, User user3, User user4, string bpgn)
        {
            string query = "INSERT INTO " + gamesTable + @"(username1, username2, username3, username4, bpgn)
                            VALUES('" + user1.username + "','" + user2.username + "','" + user3.username + "','" + user4.username + "','" + bpgn + "');";
            MySqlCommand cmd = new MySqlCommand(query, dbConn);
            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        /// <summary>
        /// Inserts message into db. Message is already escaped.
        /// </summary>
        /// <param name="userFrom"></param>
        /// <param name="userTo"></param>
        /// <param name="message">escaped string: message</param>
        /// <returns></returns>
        public bool InsertMessage(User userFrom, User userTo, string message)
        {
            string query = "INSERT INTO " + messagesTable + " (message, usernameFrom, usernameTo) VALUES('" + message + "','" + userFrom.username+ "','" + userTo.username  + "');";
            MySqlCommand cmd = new MySqlCommand(query, dbConn);
            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public List<string> GetMessagesFromUser(User user)
        {
            string username = user.username;

            string query = "SELECT message,usernameFrom,usernameTo FROM " + messagesTable + " WHERE usernameFrom='" + username + "' OR usernameTo='" + username + "';";
            MySqlCommand cmd = new MySqlCommand(query, dbConn);
            using (MySqlDataReader dataReader = cmd.ExecuteReader())
            {
                List<string> results = new List<string>();

                while (dataReader.Read())
                {
                    results.Add(dataReader["id"].ToString());
                }
                return results;
            }
        }
       
    }

}
