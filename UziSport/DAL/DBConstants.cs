using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UziSport.DAL
{
    public static class DBConstants
    {
        public const string DatabaseFilename = "UziSportDatabase.db3";

        public const string DatabaseFolder = @"D:\Uzi Sport Database";

        public const SQLite.SQLiteOpenFlags Flags =
            // open the database in read/write mode
            SQLite.SQLiteOpenFlags.ReadWrite |
            // create the database if it doesn't exist
            SQLite.SQLiteOpenFlags.Create |
            // enable multi-threaded database access
            SQLite.SQLiteOpenFlags.SharedCache;

#if DEBUG
        public static string DatabasePath =>
            Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);
#else
            public static string DatabasePath =>
                Path.Combine(DatabaseFolder, DatabaseFilename);
#endif

    }
}
