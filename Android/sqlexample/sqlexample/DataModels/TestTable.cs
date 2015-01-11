using System;
using SQLite;

namespace sqlexample
{
    public class TestTable : IIdentity
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; private set; }

        public string somename { get; set; }

        public double number { get; set; }

        public bool abool { get; set; }

        public DateTime today { get; set; }
    }
}

