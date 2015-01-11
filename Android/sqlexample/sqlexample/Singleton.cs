using System.IO;
using System;
using Android.Content;

namespace sqlexample
{
    public class sql
    {
        public static sql Singleton;

        public DBManager DBManager { get; private set; }

        public sql()
        {
            DBManager = new DBManager();
            DBManager.SetupDB();
        }
    }
}

