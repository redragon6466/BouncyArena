
using Mono.Data.SqliteClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Data
{
    public sealed class DataService : IDataService
    {
        private const string _databaseName = "viewers";
        private const string _connectionString = @"URI=file:D:\Database\Blob.db";
        private string[] _headers = { "id", "viewerName", "balance", "subscriberLevel", "isFollower", "bitsSpent", "channelPointsSpent", "lastInteraction" };

        private static DataService instance = null;
        private static readonly object padlock = new object();

        DataService()
        {
        }

        public static DataService Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new DataService();
                    }
                    return instance;
                }
            }
        }


        public bool CheckDatabase()
        {
            SqliteConnection con = new SqliteConnection();
            try
            {

                con = new SqliteConnection(_connectionString);
                con.Open();


                //Check Headers
                var headerscmd = new SqliteCommand("SELECT * FROM viewers LIMIT 1", con);
                var dr = headerscmd.ExecuteReader();
                if (dr.FieldCount != _headers.Length)
                {
                    return false;
                }
                for (var i = 0; i < dr.FieldCount; i++)
                {
                    if (dr.GetName(i) != _headers[i])
                    {
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                if (con != null && con.State != System.Data.ConnectionState.Closed)
                {
                    con.Close();
                }
                Debug.Log(e.Message + ", " + e.InnerException.Message);
                return false;
            }
            return true;
        }

        public bool CreateDatabase()
        {
            SqliteConnection con = new SqliteConnection();
            try
            {

               con = new SqliteConnection(_connectionString);
                con.Open();

                Debug.Log("Connection opened");

                string stm = "SELECT SQLITE_VERSION()";
                var cmd = new SqliteCommand(stm, con);

                cmd.CommandText = "DROP TABLE IF EXISTS viewers";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"CREATE TABLE viewers(id INTEGER PRIMARY KEY,
                    viewerName TEXT, balance INT, subscriberLevel TEXT, isFollower TEXT, bitsSpent INT, channelPointsSpent INT, lastInteraction TEXT)";
                cmd.ExecuteNonQuery();


                con.Close();

            }
            catch (Exception e)
            {
                if (con != null && con.State != System.Data.ConnectionState.Closed)
                {
                    con.Close();
                }
                Debug.Log(e.Message + ", " + e.InnerException.Message);
                return false;
            }
            return true;
        }

        public int GetBalance(string viewerName)
        {
            using (var dbConnection = new SqliteConnection(_connectionString))
            {
                dbConnection.Open();

                using (var dbCommand = dbConnection.CreateCommand())
                {

                    dbCommand.CommandText = string.Format("SELECT * FROM {0} WHERE {1} = @whereValue", _databaseName, "viewerName");


                    SqliteParameter whereParam = new SqliteParameter("@whereValue", viewerName);

                    dbCommand.Parameters.Add(whereParam);
                    using (var dbReader = dbCommand.ExecuteReader())
                    {
                        while (dbReader.Read())
                        {
                            return dbReader.GetInt32(2);
                        }
                    }
                }
            }

            return 0;
        }

        public DateTime GetLastLogin(string viewerName)
        {
            throw new NotImplementedException();
        }

        public string GetSubscriberLevel(string viewerName)
        {
            throw new NotImplementedException();
        }

        public Viewer GetViewer(string viewerName)
        {
            SqliteConnection con = new SqliteConnection();
            try
            {

                con = new SqliteConnection(_connectionString);
                con.Open();

                var cmdText = string.Format("SELECT * FROM {0}", _databaseName);

                var cmd = new SqliteCommand(cmdText, con);
                SqliteDataReader rdr = cmd.ExecuteReader();

                Debug.Log(rdr.FieldCount);
                Viewer viewer = null;
                while (rdr.Read())
                {
                    if (rdr.GetString(1) == viewerName)
                    {
                        Enum.TryParse(rdr.GetString(3), out SubscriptionTierEnum subTier);
                        bool.TryParse(rdr.GetString(4), out bool isFollower);
                        DateTime.TryParse(rdr.GetString(7), out DateTime time);

                        viewer = new Viewer
                        {
                            ViewerName = viewerName,
                            Balance = rdr.GetInt32(2),
                            SubsriberLevel = subTier,
                            IsFollower = isFollower,
                            BitsSpent = rdr.GetInt32(5),
                            ChannelPointsSpent = rdr.GetInt32(6),
                            LastInteraction = time
                        };
                    }
                }

                return viewer;
            }
            catch (Exception e)
            {
                if (con != null && con.State != System.Data.ConnectionState.Closed)
                {
                    con.Close();
                }
                Debug.Log(e.Message + ", " + e.InnerException.Message);
                return null;
            }
        }

        public bool NewViewer(string viewerName)
        {
            if (GetViewer(viewerName) != null)
            {
                return false;
            }
            SqliteConnection con = new SqliteConnection();
            try
            {
                var viewer = new Viewer
                {
                    ViewerName = viewerName,
                    Balance = 1000,
                    SubsriberLevel = SubscriptionTierEnum.Viewer,
                    IsFollower = false,
                    BitsSpent = 0,
                    ChannelPointsSpent = 0,
                    LastInteraction = new DateTime()
                };

                var cmdText = viewer.CreateInsertStatement(_databaseName);
                con = new SqliteConnection(_connectionString);

                con.Open();
                var cmd = new SqliteCommand(cmdText, con);

                cmd.ExecuteNonQuery();
               
                con.Close();
                return true;
            }
            catch (Exception e)
            {
                if (con != null && con.State != System.Data.ConnectionState.Closed)
                {
                    con.Close();
                }
                Debug.Log(e.Message + ", " + e.InnerException.Message);
                return false;
            }
        }

        public string TestDatabase()
        {
            string cs = @"URI=file:C:\Database\test.db";

            var connectionString = new SqliteConnectionStringBuilder(cs)
            {
                
            }.ToString();

            try
            {
                using (var con = new SqliteConnection(cs))
                {
                    con.Open();

                    using (var cmd = new SqliteCommand("", con))
                    {
                        cmd.CommandText = "DROP TABLE IF EXISTS cars";
                        cmd.ExecuteNonQuery();

                        cmd.CommandText = @"CREATE TABLE cars(id INTEGER PRIMARY KEY,
                    name TEXT, price INT)";
                        cmd.ExecuteNonQuery();
                    }


                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                throw;
            }
            
            return "";
           
        }

        public bool UpdateBalance(string viewerName, int amount)
        {
            var balance = GetBalance(viewerName);
            if (balance == -1)
            {
                return false;
            }

            using (var dbConnection = new SqliteConnection(_connectionString))
            {
                dbConnection.Open();

                using (var dbCommand = dbConnection.CreateCommand())
                {

                    dbCommand.CommandText = string.Format("UPDATE {0} SET {1} = @setValue WHERE {2} = @whereValue", _databaseName, "balance", "viewerName");
                    
                    
                    SqliteParameter setParam = new SqliteParameter("@setValue", balance + amount);
                    SqliteParameter whereParam = new SqliteParameter("@whereValue", viewerName);

                    dbCommand.Parameters.Add(setParam);
                    dbCommand.Parameters.Add(whereParam);

                    //Debug.Log(dbCommand.CommandText);

                    dbCommand.ExecuteNonQuery();
                }
            }

            return true;
        }

        public bool UpdateBitsSpent(string viewerName, int amount)
        {
            throw new NotImplementedException();
        }

        public bool UpdateChannelPointsSpent(string viewerName, int amount)
        {
            throw new NotImplementedException();
        }

        public bool UpdateFollowerStatus(string viewerName, bool isFollower)
        {
            throw new NotImplementedException();
        }

        public bool UpdateSubscriberTier(string viewerName, SubscriptionTierEnum newTier)
        {
            throw new NotImplementedException();
        }
    }
}
