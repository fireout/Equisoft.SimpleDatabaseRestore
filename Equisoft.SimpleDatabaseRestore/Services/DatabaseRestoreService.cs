using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Equisoft.SimpleDatabaseRestore.Modules;
using Equisoft.SimpleDatabaseRestore.Repositories;
using Microsoft.SqlServer.Management.Smo;

namespace Equisoft.SimpleDatabaseRestore.Services
{
    public class DatabaseRestoreService
    {
        private readonly ITargetDatabaseServerRepositoty targetDatabaseServerRepositoty;
        private readonly IBackupFileReaderService backupFileReaderService;

        public DatabaseRestoreService(ITargetDatabaseServerRepositoty targetDatabaseServerRepositoty, IBackupFileReaderService backupFileReaderService)
        {
            this.targetDatabaseServerRepositoty = targetDatabaseServerRepositoty;
            this.backupFileReaderService = backupFileReaderService;
        }

        public void Restore(string backupLocation, string targetInstanceName, string targetDatabaseName)
       {

           Server server = new Server(targetInstanceName);
           Restore restoreDb = new Restore();
           restoreDb.Database = targetDatabaseName;

           /* Specify whether you want to restore database or files or log etc */
           restoreDb.Action = RestoreActionType.Database;
           restoreDb.Devices.AddDevice(backupLocation, DeviceType.File);

           var fileList = restoreDb.ReadFileList(server);

           var database = server.Databases[targetDatabaseName];


           database.DatabaseOptions.UserAccess = DatabaseUserAccess.Single;
           database.Alter();

           /* You can specify ReplaceDatabase = false (default) to not create a new
            * database, the specified database must exist on SQL Server instance.
            * You can specify ReplaceDatabase = true to create new database 
            * regardless of the existence of specified database */
           restoreDb.ReplaceDatabase = true;

           /* If you have a differential or log restore to be followed, you would 
            * specify NoRecovery = true, this will ensure no recovery is done
            * after the restore and subsequent restores are completed. The database
            * would be in a recovered state. */
           restoreDb.NoRecovery = false;

           

           /* RelocateFiles collection allows you to specify the logical file names
            * and physical file names (new locations) if you want to restore to a
            * different location.*/
           restoreDb.RelocateFiles.Add(new RelocateFile("AdventureWorks_Data", @"D:\AdventureWorksNew_Data.mdf"));
           restoreDb.RelocateFiles.Add(new RelocateFile("AdventureWorks_Log", @"D:\AdventureWorksNew_Log.ldf"));

           /* Wiring up events for progress monitoring */
           //restoreDB.PercentComplete += CompletionStatusInPercent;
           //restoreDB.Complete += Restore_Completed;

           /* SqlRestore method starts to restore database
            * You can also use SqlRestoreAsync method to perform restore 
            * operation asynchronously */
           restoreDb.SqlRestore(server);
           
           database.RecoveryModel = RecoveryModel.Simple;

           database.Alter();
       }

    }
}