namespace SQLForms.Droid
{
    public class SQLFormsDroid
    {
        public SQLFormsDroid()
        {
            Singleton = this;
            ContentDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            DBManager = new DBManager();
        }

        public static SQLFormsDroid Singleton { get; private set; }

        public string ContentDirectory { get; set; }

        public DBManager DBManager { get; private set; }
    
    }
}