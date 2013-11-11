using System.Collections.Generic;

namespace Equisoft.SimpleDatabaseRestore.Models
{
    public class DatabaseInstance
    {
        public DatabaseInstance()
        {
            Databases = new List<Database>();
        }

        public string Name { get; set; }

        public IList<Database> Databases { get; set; }
    }
}