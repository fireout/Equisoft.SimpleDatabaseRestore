using System.Collections.Generic;

namespace Equisoft.SimpleDatabaseRestore.Models
{
    public class DatabaseServer
    {
        public DatabaseServer()
        {
            Instances = new List<DatabaseInstance>();
        }

        public string Name { get; set; }

        public IList<DatabaseInstance> Instances { get; set; }
    }
}