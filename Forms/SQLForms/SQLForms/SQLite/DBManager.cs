using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SQLite.Net;
using Xamarin.Forms;
using SQLForms.Interfaces;

namespace SQLForms.SQL
{
    public class DBManager
    {
        object dbLock = new object();

        #region SetupAndDelete

        public bool SetupDB()
        {
            lock (dbLock)
            {
                try
                {
                    using (var sqlCon = DependencyService.Get<IDatabaseConnection>().Connection)
                    {
                        sqlCon.CreateTable<Event>();
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
            lock (dbLock)
            {
                using (var sqlCon = DependencyService.Get<IDatabaseConnection>().Connection)
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
                        Debug.WriteLine("Error in DropATable! {0}--{1}", ex.Message, ex.StackTrace);
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
                using (var sqlCon = DependencyService.Get<IDatabaseConnection>().Connection)
                {		
                    sqlCon.Execute(DBConstants.DBClauseSyncOff);
                    sqlCon.BeginTransaction();
                    try
                    {
                        sqlCon.Execute("DELETE FROM Event");
                        sqlCon.Commit();
                        sqlCon.Execute(DBConstants.DBClauseVacuum);
                    }
                    catch (Exception ex)
                    {
                        #if(DEBUG)
                        Debug.WriteLine("Error in CleanUpDB! {0}--{1}", ex.Message, ex.StackTrace);
                        #endif
                        sqlCon.Rollback();
                    }
                }
            }
        }

        #endregion

        #region Setters

        public void AddOrUpdateEvents(List<Event> events)
        {
            foreach (var ev in events)
                AddOrUpdateEvent(ev);
        }

        public void AddOrUpdateEvent(Event ev)
        {
            lock (dbLock)
            {
                using (var sqlcon = DependencyService.Get<IDatabaseConnection>().Connection)
                {
                    sqlcon.Execute(DBConstants.DBClauseSyncOff);
                    sqlcon.BeginTransaction();
                    try
                    {
                        if (sqlcon.Execute("UPDATE Event SET id=?, " +
                                "event_name=?, event_details=?, event_address=?, event_postcode=?, __updatedAt=? WHERE id=?",
                                ev.id, ev.event_name, ev.event_details, ev.event_address, ev.event_postcode, ev.__updatedAt, ev.id) == 0)
                            sqlcon.Insert(ev, typeof(Event));
                        sqlcon.Commit(); 
                    }
                    catch (Exception ex)
                    {
                        #if DEBUG
                        Debug.WriteLine("Error in AddOrUpdateEvent - {0}--{1}", ex.Message, ex.StackTrace);
                        #endif
                        sqlcon.Rollback();
                    }
                }
            }
        }

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

        public List<T> GetListOfObjects<T>(string id) where T:IDatabase, new()
        {
            lock (dbLock)
            {
                using (var sqlCon = DependencyService.Get<IDatabaseConnection>().Connection)
                {
                    sqlCon.Execute(DBConstants.DBClauseSyncOff);
                    sqlCon.BeginTransaction();
                    string sql = string.Format("SELECT * FROM {0} WHERE id=\"{1}\"", GetName(typeof(T).ToString()), id);
                    var data = sqlCon.Query<T>(sql);
                    return data;
                }
            }
        }

        public List<T> GetListOfObjects<T>(string para, string val) where T:IDatabase, new()
        {
            lock (dbLock)
            {
                using (var sqlCon = DependencyService.Get<IDatabaseConnection>().Connection)
                {
                    sqlCon.Execute(DBConstants.DBClauseSyncOff);
                    sqlCon.BeginTransaction();
                    string sql = string.Format("SELECT * FROM {0} WHERE {1}=\"{2}\"", GetName(typeof(T).ToString()), para, val);
                    var data = sqlCon.Query<T>(sql);
                    return data;
                }
            }
        }

        public List<T> GetListOfObjects<T>(string para1, string val1, string para2, string val2, bool ne = false) where T:IDatabase, new()
        {
            lock (dbLock)
            {
                using (var sqlCon = DependencyService.Get<IDatabaseConnection>().Connection)
                {
                    sqlCon.Execute(DBConstants.DBClauseSyncOff);
                    sqlCon.BeginTransaction();
                    string sign = ne ? "!=" : "=";
                    string sql = string.Format("SELECT * FROM {0} WHERE {1}{2}\"{3}\" AND {4}{5}\"{6}\"", GetName(typeof(T).ToString()), para1, sign, val1, para2, sign, val2);
                    var data = sqlCon.Query<T>(sql);
                    return data;
                }
            }
        }

        public List<T> GetListOfObjects<T>() where T:IDatabase, new()
        {
            lock (dbLock)
            {
                using (var sqlCon = DependencyService.Get<IDatabaseConnection>().Connection)
                {
                    sqlCon.Execute(DBConstants.DBClauseSyncOff);
                    sqlCon.BeginTransaction();
                    string sql = string.Format("SELECT * FROM {0}", GetName(typeof(T).ToString()));
                    var data = sqlCon.Query<T>(sql);
                    return data;
                }
            }
        }

        public T GetSingleObject<T>() where T:IDatabase, new()
        {
            lock (dbLock)
            {
                using (var sqlCon = DependencyService.Get<IDatabaseConnection>().Connection)
                {
                    sqlCon.Execute(DBConstants.DBClauseSyncOff);
                    sqlCon.BeginTransaction();
                    string sql = string.Format("SELECT * FROM {0}", GetName(typeof(T).ToString()));
                    var data = sqlCon.Query<T>(sql).FirstOrDefault();
                    return data;
                }
            }
        }

        public T GetSingleObject<T>(string para, string val) where T:IDatabase, new()
        {
            lock (dbLock)
            {
                using (var sqlCon = DependencyService.Get<IDatabaseConnection>().Connection)
                {
                    sqlCon.Execute(DBConstants.DBClauseSyncOff);
                    sqlCon.BeginTransaction();
                    string sql = string.Format("SELECT * FROM {0} WHERE {1}=\"{2}\"", GetName(typeof(T).ToString()), para, val);
                    var data = sqlCon.Query<T>(sql).FirstOrDefault();
                    return data;
                }
            }
        }

        public T GetSingleObject<T>(string para1, string val1, string para2, string val2, bool ne = false) where T:IDatabase, new()
        {
            lock (dbLock)
            {
                using (var sqlCon = DependencyService.Get<IDatabaseConnection>().Connection)
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

        public T GetSingleObject<T>(string para1, string val1, string para2, double val2) where T:IDatabase, new()
        {
            lock (dbLock)
            {
                using (var sqlCon = DependencyService.Get<IDatabaseConnection>().Connection)
                {
                    sqlCon.Execute(DBConstants.DBClauseSyncOff);
                    sqlCon.BeginTransaction();
                    string sql = string.Format("SELECT * FROM {0} WHERE {1}=\"{2}\" AND {3}={4}", GetName(typeof(T).ToString()), para1, val1, para2, val2);
                    var data = sqlCon.Query<T>(sql).FirstOrDefault();
                    return data;
                }
            }
        }

        public DateTime GetLastSync
        {
            get
            {
                lock (dbLock)
                {
                    using (var sqlCon = DependencyService.Get<IDatabaseConnection>().Connection)
                    {
                        sqlCon.Execute(DBConstants.DBClauseSyncOff);
                        sqlCon.BeginTransaction();
                        var data = sqlCon.ExecuteScalar<DateTime>("SELECT lastSync FROM Services");
                        return data;
                    }
                }
            }
        }

        public List<T> GetObjectForUpdate<T>() where T:IDatabase, new()
        {
            lock (dbLock)
            {
                using (var sqlCon = DependencyService.Get<IDatabaseConnection>().Connection)
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
