using System.Collections.Generic;
using System.IO;
using Equisoft.SimpleDatabaseRestore.Models;

namespace Equisoft.SimpleDatabaseRestore.Commands
{
    public class DatabaseRestoreRequest
    {
        public string FullBackupFile { get; set; }
        public string BackupFile { get { return Path.GetFileName(FullBackupFile); } }
        public string TargetInstance { get; set; }
        public string TargetDatabase { get; set; }
        public IList<FileToRestore> FilesLists { get; set; }
        
    }
}