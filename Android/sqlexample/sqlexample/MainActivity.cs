using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using sqlexample;

using Android.OS;

namespace sqlexample
{
    [Activity(Label = "sqlexample", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        TextView txtTable, txtVideo;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            sql l = new sql();

            var btnTable = FindViewById<Button>(Resource.Id.btnTestTable);
            var btnVideo = FindViewById<Button>(Resource.Id.btnAddVideo);
            txtTable = FindViewById<TextView>(Resource.Id.txtRecTable);
            txtVideo = FindViewById<TextView>(Resource.Id.txtRecVideo);

            btnTable.Click += delegate
            {
                AddToTable();
            };

            btnVideo.Click += delegate
            {
                AddToVideo();
            };
        }

        void AddToTable()
        {
            var tables = new List<TestTable>
            {
                new TestTable(){ number = 4.13, abool = false, somename = "algy", today = DateTime.Now },
                new TestTable(){ number = 3.333, abool = true, somename = "biggles", today = DateTime.Now.AddDays(3) },
                new TestTable(){ number = 312, abool = false, somename = "bertie", today = DateTime.Now.AddDays(-3) }
            };
            sql.Singleton.DBManager.AddOrUpdateTestTable(tables);

            var count = sql.Singleton.DBManager.GetListOfObjects<TestTable>().Count;
            txtTable.Text = count.ToString();
        }

        void AddToVideo()
        {
            var vids = new Videos(){ humanid = 21, videoname = "rocky", recordedon = DateTime.Now.AddDays(new Random(10).NextDouble()) };
            sql.Singleton.DBManager.AddOrUpdateVideos(vids);

            var count = sql.Singleton.DBManager.GetListOfObjects<Videos>().Count;
            txtVideo.Text = count.ToString();
        }
    }
}


