using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;
using System.Data;
using System.Configuration;
using System.Threading;

namespace WZWVAPI
{
    internal static class DatabaseHandler
    {
        private static readonly Semaphore semaphore = new Semaphore(10, 10);
        private static string ConnectionString = ConfigurationManager.ConnectionStrings["WZWVTestDatabase"].ConnectionString;
       // private static string ConnectionString = ConfigurationManager.ConnectionStrings["WZWVDataDatabase"].ConnectionString;

        private static MySqlConnection OpenConnection(bool LogStats)
        {
            MySqlConnection Connection = new MySqlConnection(ConnectionString);
            
            if (!semaphore.WaitOne(10000))
            {
                new WebsiteException(new Exception("Maximum connections reached \n" + QueryTrace.GetTrace()));
                return Connection;
            }

            try
            {
                Connection.Open();

                if (LogStats)
                {
                    try
                    {
                        DatabaseStats.CurrentStats.AddDatabaseHit();
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            catch (Exception e)
            {
                throw new CouldNotConnectException("Could not open the connection to the SQL server, Check your server settings. /n" + e.ToString());
            }

            return Connection;
        }

        /// <summary>
        /// Executes a Query that does not return results
        /// </summary>
        /// <param name="Command">MySQLCommand</param>
        internal static void ExecuteNonQuery(MySqlCommand Command, bool LogStats)
        {
            MySqlConnection Connection = null;

            try
            {
                Connection = OpenConnection(LogStats);
                Command.Connection = Connection;
                QueryTrace.AddQuery(Command.CommandText);
                Command.ExecuteNonQuery();
            }
            catch (CouldNotConnectException e)
            {
                throw new Exception("Database connection not valid. \n" + e.ToString());
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                CloseConnection(Connection);
                QueryTrace.RemoveQuery(Command.CommandText);
            }
        }

        /// <summary>
        /// Executes a query the returns a datareader
        /// </summary>
        /// <param name="Command">MysqlCommand, Add params and query before using this method.</param>
        /// <returns>MysqlDataReader</returns>
        internal static MySqlDataReader ExecuteQuery(MySqlCommand Command, bool LogStats)
        {
            MySqlConnection Connection = null;
            MySqlDataReader Reader;

            try
            {
                Connection = OpenConnection(LogStats);
                Command.Connection = Connection;
                QueryTrace.AddQuery(Command.CommandText);
                Reader = Command.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (CouldNotConnectException e)
            {
                throw new Exception("Database connection not valid. \n" + e.ToString());
            }
            catch (Exception e)
            {
                throw e;
            }

            return Reader;

        }

        private static void CloseConnection(MySqlConnection Connection)
        {
            if (Connection != null)
            {
                if (Connection.State == ConnectionState.Open)
                {
                    semaphore.Release();
                    Connection.Close();
                }
            }
        }

        internal static void CloseConnectionByReader(MySqlDataReader Reader)
        {
            Reader.Close();
            semaphore.Release();
        }
    }

    class CouldNotConnectException : Exception
    {
        public CouldNotConnectException(string message)
            : base(message)
        {
        }
    }
}