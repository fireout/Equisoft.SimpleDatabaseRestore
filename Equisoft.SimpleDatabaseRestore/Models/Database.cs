using System.Collections.Generic;

namespace Equisoft.SimpleDatabaseRestore.Models
{
    public class Database
    {
        public Database()
        {
            Backups = new List<DatabaseBackup>();
        }

        public string Name { get; set; }

        public bool CanRestore { get; set; }

        public IList<DatabaseBackup> Backups { get; set; }
    }
}