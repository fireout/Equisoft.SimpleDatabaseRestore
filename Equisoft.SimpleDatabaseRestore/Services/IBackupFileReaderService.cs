using Equisoft.SimpleDatabaseRestore.Models;
using Microsoft.SqlServer.Management.Smo;

namespace Equisoft.SimpleDatabaseRestore.Services
{
    public interface IBackupFileReaderService
    {
        BackupInfo GetBackupInfo(string backupFileName, string targetInstanceName);
        BackupInfo GetBackupInfo(string backupFileName, Server server);
    }
}