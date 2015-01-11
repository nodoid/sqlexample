using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using SQLite;

namespace sqlexample
{
    public class DBManager
    {
        public DBManager()
        {
            SQLite3.Config(SQLite3.ConfigOption.Serialized);
            dbLock = new object();
            string documents = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            pConnectionString = Path.Combine(documents, "mydb.db");
            connectionString = string.Format("{0}; New=true; Version=3;PRAGMA locking_mode=EXCLUSIVE; PRAGMA journal_mode=WAL; PRAGMA cache_size=20000; PRAGMA page_size=32768; PRAGMA synchronous=off", pConnectionString);
        }

        private string pConnectionString, connectionString;
        private object dbLock;

        public string DBPath
        {
            get
            {
                return pConnectionString;
            }
        }

        public string ConnectionString
        {
            get
            {
                return connectionString;
            }
        }

        #region SetupAndDelete

        public bool SetupDB()
        {
            lock (dbLock)
            {
                try
                {
                    using (var sqlCon = new SQLiteConnection(ConnectionString))
                    {
                        sqlCon.CreateTable<TestTable>();
                        sqlCon.CreateTable<Videos>();
                        sqlCon.Execute(DBConstants.DBClauseVacuum);
                    }
                    return true;	
                }
                catch (SQLiteException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    throw ex;
                } 
            }
        }

        public void DropATable(string table)
        {
            lock (this.dbLock)
            {
                using (SQLiteConnection sqlCon = new SQLiteConnection(ConnectionString))
                {       
                    sqlCon.Execute(DBConstants.DBClauseSyncOff);
                    sqlCon.BeginTransaction();
                    try
                    {
                        string drop = string.Format("DELETE FROM {0}", table);
                        sqlCon.Execute(drop);
                        sqlCon.Commit();
                        sqlCon.Execute(DBConstants.DBClauseVacuum);
                    }
                    catch (Exception ex)
                    {
                        #if(DEBUG)
                        System.Diagnostics.Debug.WriteLine("Error in DropATable! {0}--{1}", ex.Message, ex.StackTrace);
                        #endif
                        sqlCon.Rollback();
                    }
                }
            }
        }

        public void CleanUpDB()
        {	
            lock (this.dbLock)
            {
                using (SQLiteConnection sqlCon = new SQLiteConnection(ConnectionString))
                {		
                    sqlCon.Execute(DBConstants.DBClauseSyncOff);
                    sqlCon.BeginTransaction();
                    try
                    {
                        sqlCon.Execute("DELETE FROM TestTable");
                        sqlCon.Execute("DELETE FROM Videos");
                        sqlCon.Commit();
                        sqlCon.Execute(DBConstants.DBClauseVacuum);
                    }
                    catch (Exception ex)
                    {
                        #if(DEBUG)
                        System.Diagnostics.Debug.WriteLine("Error in CleanUpDB! {0}--{1}", ex.Message, ex.StackTrace);
                        #endif
                        sqlCon.Rollback();
                    }
                }
            }
        }

        #endregion

        #region Setters

        #region DeleteObject

        public void DeleteObject<T>(T imp) where T:IIdentity
        {
            lock (dbLock)
            {
                using (var sqlcon = new SQLiteConnection(ConnectionString))
                {
                    sqlcon.Execute(DBConstants.DBClauseSyncOff);
                    sqlcon.BeginTransaction();
                    try
                    {
                        var command = string.Format("UPDATE {0} SET id={1}, __updatedAt={2}, is_deleted={3} WHERE id={4}", typeof(T).ToString(), imp.id, DateTime.Now, true, 
                                          imp.id);
                        sqlcon.Execute(command);
                        sqlcon.Commit(); 
                    }
                    catch (Exception ex)
                    {
                        #if DEBUG
                        Console.WriteLine("Error in DeleteObject - {0}--{1}", ex.Message, ex.StackTrace);
                        #endif
                        sqlcon.Rollback();
                    }
                }
            }
        }

        #endregion

        #region Videos

        public void AddOrUpdateVideos(List<Videos> vids)
        {
            foreach (var l in vids)
                AddOrUpdateVideos(l);
        }

        public void AddOrUpdateVideos(Videos vid)
        {
            lock (dbLock)
            {
                using (var sqlcon = new SQLiteConnection(ConnectionString))
                {
                    sqlcon.Execute(DBConstants.DBClauseSyncOff);
                    sqlcon.BeginTransaction();
                    try
                    {
                        if (sqlcon.Execute("UPDATE Videos SET id=?, " +
                                "humanid=?, recordedon=?, videoname=? WHERE id=?",
                                vid.id, vid.humanid, vid.recordedon, vid.videoname, vid.id) == 0)
                            sqlcon.Insert(vid, typeof(Videos));
                        sqlcon.Commit(); 
                    }
                    catch (Exception ex)
                    {
                        #if DEBUG
                        Console.WriteLine("Error in Videos - {0}--{1}", ex.Message, ex.StackTrace);
                        #endif
                        sqlcon.Rollback();
                    }
                }
            }
        }

        #endregion

        #region TestTable

        public void AddOrUpdateTestTable(List<TestTable> test)
        {
            foreach (var c in test)
                AddOrUpdateTestTable(c);
        }

        public void AddOrUpdateTestTable(TestTable test)
        {
            lock (dbLock)
            {
                using (var sqlcon = new SQLiteConnection(ConnectionString))
                {
                    sqlcon.Execute(DBConstants.DBClauseSyncOff);
                    sqlcon.BeginTransaction();
                    try
                    {
                        if (sqlcon.Execute("UPDATE Chemicals SET id=?, " +
                                "abool=?, number=?, somename=?, today=? WHERE id=?",
                                test.id, test.abool, test.number, test.somename, test.today, test.id) == 0)
                            sqlcon.Insert(test, typeof(TestTable));
                        sqlcon.Commit(); 
                    }
                    catch (Exception ex)
                    {
                        #if DEBUG
                        Console.WriteLine("Error in AddOrUpdateTestTable - {0}--{1}", ex.Message, ex.StackTrace);
                        #endif
                        sqlcon.Rollback();
                    }
                }
            }
        }

        #endregion

        #endregion

        #region Getters

        #region PrivateNameConv

        private string GetName(string name)
        {
            var list = name.Split('.').ToList();
            if (list.Count == 1)
                return list[0];
            var last = list[list.Count - 1];
            return last;
        }

        #endregion

        public List<T> GetListOfObjects<T>(string id) where T:IIdentity, new()
        {
            lock (dbLock)
            {
                using (var sqlCon = new SQLiteConnection(ConnectionString))
                {
                    sqlCon.Execute(DBConstants.DBClauseSyncOff);
                    sqlCon.BeginTransaction();
                    string sql = string.Format("SELECT * FROM {0} WHERE id=\"{1}\"", GetName(typeof(T).ToString()), id);
                    var data = sqlCon.Query<T>(sql);
                    return data;
                }
            }
        }

        public List<T> GetListOfObjects<T>(string para, string val) where T:IIdentity, new()
        {
            lock (dbLock)
            {
                using (var sqlCon = new SQLiteConnection(ConnectionString))
                {
                    sqlCon.Execute(DBConstants.DBClauseSyncOff);
                    sqlCon.BeginTransaction();
                    string sql = string.Format("SELECT * FROM {0} WHERE {1}=\"{2}\"", GetName(typeof(T).ToString()), para, val);
                    var data = sqlCon.Query<T>(sql).ToList();
                    return data;
                }
            }
        }

        public List<T> GetListOfObjects<T>(string para1, string val1, string para2, string val2, bool ne = false) where T:IIdentity, new()
        {
            lock (dbLock)
            {
                using (var sqlCon = new SQLiteConnection(ConnectionString))
                {
                    sqlCon.Execute(DBConstants.DBClauseSyncOff);
                    sqlCon.BeginTransaction();
                    string sign = ne ? "!=" : "=";
                    string sql = string.Format("SELECT * FROM {0} WHERE {1}{2}\"{3}\" AND {4}{5}\"{6}\"", GetName(typeof(T).ToString()), para1, sign, val1, para2, sign, val2);
                    var data = sqlCon.Query<T>(sql).ToList();
                    return data;
                }
            }
        }

        public List<T> GetListOfObjects<T>() where T:IIdentity, new()
        {
            lock (dbLock)
            {
                using (var sqlCon = new SQLiteConnection(ConnectionString))
                {
                    sqlCon.Execute(DBConstants.DBClauseSyncOff);
                    sqlCon.BeginTransaction();
                    string sql = string.Format("SELECT * FROM {0}", GetName(typeof(T).ToString()));
                    var data = sqlCon.Query<T>(sql).ToList();
                    return data;
                }
            }
        }

        public T GetSingleObject<T>(string id) where T:IIdentity, new()
        {
            lock (dbLock)
            {
                using (var sqlCon = new SQLiteConnection(ConnectionString))
                {
                    sqlCon.Execute(DBConstants.DBClauseSyncOff);
                    sqlCon.BeginTransaction();
                    string sql = string.Format("SELECT * FROM {0} WHERE id=\"{1}\"", GetName(typeof(T).ToString()), id);
                    var data = sqlCon.Query<T>(sql).ToList();
                    return data[0];
                }
            }
        }

        public T GetSingleObject<T>() where T:IIdentity, new()
        {
            lock (dbLock)
            {
                using (var sqlCon = new SQLiteConnection(ConnectionString))
                {
                    sqlCon.Execute(DBConstants.DBClauseSyncOff);
                    sqlCon.BeginTransaction();
                    string sql = string.Format("SELECT * FROM {0}", GetName(typeof(T).ToString()));
                    var data = sqlCon.Query<T>(sql).FirstOrDefault();
                    return data;
                }
            }
        }

        public T GetSingleObject<T>(string para, string val) where T:IIdentity, new()
        {
            lock (dbLock)
            {
                using (var sqlCon = new SQLiteConnection(ConnectionString))
                {
                    sqlCon.Execute(DBConstants.DBClauseSyncOff);
                    sqlCon.BeginTransaction();
                    string sql = string.Format("SELECT * FROM {0} WHERE {1}=\"{2}\"", GetName(typeof(T).ToString()), para, val);
                    var data = sqlCon.Query<T>(sql).FirstOrDefault();
                    return data;
                }
            }
        }

        public T GetSingleObject<T>(string para1, string val1, string para2, string val2, bool ne = false) where T:IIdentity, new()
        {
            lock (dbLock)
            {
                using (var sqlCon = new SQLiteConnection(ConnectionString))
                {
                    sqlCon.Execute(DBConstants.DBClauseSyncOff);
                    sqlCon.BeginTransaction();
                    string sign = ne ? "!=" : "=";
                    string sql = string.Format("SELECT * FROM {0} WHERE {1}{2}\"{3}\" AND {4}{5}\"{6}\"", GetName(typeof(T).ToString()), para1, sign, val1, para2, sign, val2);
                    var data = sqlCon.Query<T>(sql).FirstOrDefault();
                    return data;
                }
            }
        }

        public T GetSingleObject<T>(string para1, string val1, string para2, double val2) where T:IIdentity, new()
        {
            lock (dbLock)
            {
                using (var sqlCon = new SQLiteConnection(ConnectionString))
                {
                    sqlCon.Execute(DBConstants.DBClauseSyncOff);
                    sqlCon.BeginTransaction();
                    string sql = string.Format("SELECT * FROM {0} WHERE {1}=\"{2}\" AND {3}={4}", GetName(typeof(T).ToString()), para1, val1, para2, val2);
                    var data = sqlCon.Query<T>(sql).FirstOrDefault();
                    return data;
                }
            }
        }

        public List<T> GetObjectForUpdate<T>() where T:IIdentity, new()
        {
            lock (dbLock)
            {
                using (var sqlCon = new SQLiteConnection(ConnectionString))
                {
                    sqlCon.Execute(DBConstants.DBClauseSyncOff);
                    sqlCon.BeginTransaction();
                    string sql = string.Format("SELECT * FROM {0}", GetName(typeof(T).ToString()));
                    var data = sqlCon.Query <T>(sql);
                    return data;
                }
            }
        }

        #endregion
    }
}
