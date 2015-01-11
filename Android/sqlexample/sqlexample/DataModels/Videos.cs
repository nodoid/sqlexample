using System;
using SQLite;

namespace sqlexample
{
    public class Videos : IIdentity
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; private set; }

        public string videoname { get; set; }

        public int humanid { get; set; }

        public DateTime recordedon { get; set; }
    }
}

