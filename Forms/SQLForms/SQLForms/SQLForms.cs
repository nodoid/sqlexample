using System;

using Xamarin.Forms;
using SQLForms.SQL;

namespace SQLForms
{
    public class App : Application
    {
        public static App Singleton { get; private set; }

        public DBManager DBManager { get; private set; }

        public App()
        {
            Singleton = this;
            DBManager = new DBManager();
            MainPage = new SQLExample();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}

