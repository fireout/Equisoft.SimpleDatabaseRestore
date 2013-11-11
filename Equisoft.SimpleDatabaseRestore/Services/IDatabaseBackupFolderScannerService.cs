using System.Collections.Generic;
using System.IO;
using Equisoft.SimpleDatabaseRestore.Models;

namespace Equisoft.SimpleDatabaseRestore.Services
{
    public interface IDatabaseBackupFolderScannerService
    {
        IList<BackupFile> ScanFolderAndSubdirectory(string path, string searchPattern);
    }
}