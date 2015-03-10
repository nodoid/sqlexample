using System;
using System.IO;
using SQLite.Net;
using SQLForms.SQLite;

namespace SQLForms.iOS
{
    [assembly: Xamarin.Forms.Dependency(typeof(DBManager))]
    public class DBManager :IDatabaseConnection
    {
        #region IDatabaseConnection implementation

        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        const string sqliteFilename = "testdb.db3";

        public SQLiteConnection Connection
        {
            get
            {
                string libraryPath = Path.Combine(documentsPath, "..", "Library");         
                var path = Path.Combine(libraryPath, sqliteFilename);
                var plat = new SQLite.Net.Platform.XamarinIOS.SQLitePlatformIOS();
                var conn = new SQLiteConnection(plat, path);
                return conn;
            }
        }

        public string ConnectionString
        {
            get
            {
                var pDocs = Path.Combine(documentsPath, "groupshoot.db3");
                return string.Format("{0}; New=true; Version=3;PRAGMA locking_mode=EXCLUSIVE; PRAGMA journal_mode=WAL; PRAGMA cache_size=20000; PRAGMA page_size=32768; PRAGMA synchronous=off", pDocs);
            }
        }

        #endregion
    }
}
