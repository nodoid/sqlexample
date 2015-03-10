using System;

namespace SQLForms.Interfaces
{
    public interface IDatabase
    {
        int id { get; }

        DateTime __updatedAt { get; set; }
    }
}

