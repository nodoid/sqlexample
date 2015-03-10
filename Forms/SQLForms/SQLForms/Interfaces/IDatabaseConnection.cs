using SQLite.Net;

namespace SQLForms.Interfaces
{
    public interface IDatabaseConnection
    {
        SQLiteConnection Connection { get; }

        string ConnectionString { get; }
    }
}

