using System.Collections.Generic;

namespace Equisoft.SimpleDatabaseRestore.Models
{
    public class BackupInfo
    {
        public BackupInfo()
        {
            LogFiles = new List<DatabaseFile>();
        }

        public string DataLogicalName { get; set; }
        public IList<DatabaseFile> LogFiles { get; set; }
    }
}