using System;
using SQLite.Net.Attributes;
using SQLForms.Interfaces;

namespace SQLForms.SQL
{
    public class Event : IDatabase
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }

        public string event_name { get; set; }

        public string event_details { get; set; }

        public string event_address { get; set; }

        public string event_postcode { get; set; }

        public DateTime __updatedAt { get; set; }

    }
}

