using System.Data;
using Equisoft.SimpleDatabaseRestore.Models;
using Microsoft.SqlServer.Management.Smo;
using DatabaseFile = Equisoft.SimpleDatabaseRestore.Models.DatabaseFile;

namespace Equisoft.SimpleDatabaseRestore.Services
{
    public class BackupFileReaderService : IBackupFileReaderService
    {

        public BackupInfo GetBackupInfo(string backupFileName, string targetInstanceName)
        {
            var server = new Server(targetInstanceName);

            return GetBackupInfo(backupFileName, server);
        }

        public BackupInfo GetBackupInfo(string backupFileName, Server server)
        {
            var restoreDb = new Restore();
            var info = new BackupInfo();
            restoreDb.Action = RestoreActionType.Database;
            restoreDb.Devices.AddDevice(backupFileName, DeviceType.File);
            var fileList = restoreDb.ReadFileList(server);

            foreach (DataRow row in fileList.Rows)
            {
                if (row.Field<string>("Type").Equals("D"))
                {
                    info.DataLogicalName = row.Field<string>("LogicalName");
                }
                else if (row.Field<string>("Type").Equals("L"))
                {
                    info.LogFiles.Add(new DatabaseFile { LogicalName = row.Field<string>("LogicalName") });
                }
            }

            return info;
        }
    }
}