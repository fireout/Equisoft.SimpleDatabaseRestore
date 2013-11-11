using System.Collections.Generic;
using System.IO;
using System.Linq;
using Equisoft.SimpleDatabaseRestore.Models;

namespace Equisoft.SimpleDatabaseRestore.Services
{
    public class DatabaseBackupFolderScannerService : IDatabaseBackupFolderScannerService
    {
        public IList<BackupFile> ScanFolderAndSubdirectory(string path, string searchPattern)
        {
            return Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories)
                            .Select(x => new BackupFile(x))
                            .OrderBy(info => info.DirectoryFullName)
                            .ToList();
        }
    }
}