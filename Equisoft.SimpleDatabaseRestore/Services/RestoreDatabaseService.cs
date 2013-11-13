using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using System.Web.Management;
using Equisoft.SimpleDatabaseRestore.Commands;
using Equisoft.SimpleDatabaseRestore.Models;
using Equisoft.SimpleDatabaseRestore.Repositories;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Database = Microsoft.SqlServer.Management.Smo.Database;
using DatabaseFile = Equisoft.SimpleDatabaseRestore.Models.DatabaseFile;

namespace Equisoft.SimpleDatabaseRestore.Services
{
    public class RestoreDatabaseService : IRestoreDatabaseService
    {
        private readonly IBackupFileReaderService backupFileReaderService;
        private readonly IScriptsRepository scriptsRepository;
        private readonly ITargetDatabaseServerRepositoty targetDatabaseServerRepositoty;

        public RestoreDatabaseService(ITargetDatabaseServerRepositoty targetDatabaseServerRepositoty,
                                      IBackupFileReaderService backupFileReaderService,
                                      IScriptsRepository scriptsRepository)
        {
            this.targetDatabaseServerRepositoty = targetDatabaseServerRepositoty;
            this.backupFileReaderService = backupFileReaderService;
            this.scriptsRepository = scriptsRepository;
        }


        public DatabaseRestoreRequest GenerateRequest(string backupFileName, string targetInstanceName,
                                                      string targetDatabase)
        {
            BackupInfo backupInfo = backupFileReaderService.GetBackupInfo(backupFileName, targetInstanceName);

            ServerInfo targetServerInfo = targetDatabaseServerRepositoty.GetServerInfo(targetInstanceName,
                                                                                       targetDatabase);

            IList<FileToRestore> filesList = MergeFilesLists(backupInfo, targetServerInfo);

            return new DatabaseRestoreRequest
                {
                    FullBackupFile = backupFileName,
                    TargetInstance = targetInstanceName,
                    TargetDatabase = targetDatabase,
                    FilesLists = filesList,
                    ScriptsToExecute = scriptsRepository.GetScripts(targetInstanceName, targetDatabase),
                    ScriptsRootPath = scriptsRepository.RoothPath
                };
        }

        public void Restore(DatabaseRestoreRequest request, PercentCompleteEventHandler percentCompleteDelegate)
        {
            var server = new Server(request.TargetInstance);
            Database database = server.Databases[request.TargetDatabase];

            // Start by locking down the database
            database.DatabaseOptions.UserAccess = DatabaseUserAccess.Single;
            database.Alter(TerminationClause.RollbackTransactionsImmediately);

            var restoreDb = new Restore { Database = database.Name, Action = RestoreActionType.Database };

            //Specify whether you want to restore database or files or log etc
            restoreDb.Devices.AddDevice(request.FullBackupFile, DeviceType.File);

            // For now we only support database replacement.
            restoreDb.ReplaceDatabase = true;

            // For full backup no recovery is not usefull. Will need to to change this if support for restoring transactional backup file happens.
            restoreDb.NoRecovery = false;

            // Associate the correct physical path for each file to be restored
            foreach (FileToRestore fileToRestore in request.FilesLists)
            {
                restoreDb.RelocateFiles.Add(new RelocateFile(fileToRestore.BackupLogicalName,
                                                             fileToRestore.TargetPhysicalPath));
            }

            if (percentCompleteDelegate != null)
            {
                restoreDb.PercentComplete += percentCompleteDelegate;
            }

            // Magic!
            restoreDb.SqlRestore(server);

            restoreDb.SqlRestoreAsync(server);

            // After the restore, ensure the recovery model is set to simple.
            // Since we only support DEV/TEST/DEMO, we dont want the overhead of the other recovery models.
            database.RecoveryModel = RecoveryModel.Simple;
            database.Alter();

            string sqlConnectionString = string.Format("Integrated Security=SSPI;Persist Security Info=True;Initial Catalog={1};Data Source={0}", server.Name, database.Name);

            foreach (var script in request.ScriptsToExecute)
            {
                var fileInfo = new FileInfo(script);


                string sql;

                using (var text = fileInfo.OpenText())
                {
                    sql = text.ReadToEnd();
                    text.Close();
                }


                SqlConnection connection = new SqlConnection(sqlConnectionString);
                Server srv = new Server(new ServerConnection(connection));
                srv.ConnectionContext.SqlExecutionModes = SqlExecutionModes.ExecuteAndCaptureSql;
                srv.ConnectionContext.ExecuteNonQuery(sql);
            }

        }



        public Task<string> RestoreAsync(DatabaseRestoreRequest request, PercentCompleteEventHandler percentCompleteDelegate)
        {



            var server = new Server(request.TargetInstance);
            Database database = server.Databases[request.TargetDatabase];

            // Start by locking down the database
            database.DatabaseOptions.UserAccess = DatabaseUserAccess.Single;
            database.Alter(TerminationClause.RollbackTransactionsImmediately);

            var restoreDb = new Restore { Database = database.Name, Action = RestoreActionType.Database };

            //Specify whether you want to restore database or files or log etc
            restoreDb.Devices.AddDevice(request.FullBackupFile, DeviceType.File);

            // For now we only support database replacement.
            restoreDb.ReplaceDatabase = true;

            // For full backup no recovery is not usefull. Will need to to change this if support for restoring transactional backup file happens.
            restoreDb.NoRecovery = false;

            // Associate the correct physical path for each file to be restored
            foreach (FileToRestore fileToRestore in request.FilesLists)
            {
                restoreDb.RelocateFiles.Add(new RelocateFile(fileToRestore.BackupLogicalName,
                                                             fileToRestore.TargetPhysicalPath));
            }

            if (percentCompleteDelegate != null)
            {
                restoreDb.PercentComplete += percentCompleteDelegate;
            }

            // Magic!
            //restoreDb.SqlRestore(server);

            var tcs = new TaskCompletionSource<string>();

            restoreDb.Complete += (sender, e) => TransferCompletion<string>(tcs, e, e.ToString, null, request, database, server);

            try
            {
                restoreDb.SqlRestoreAsync(server);
            }
            catch (Exception ex)
            {

                tcs.TrySetException(ex);
            }
            //tcs.Task.ContinueWith(task => FinishRestore(request, database, server));

            return tcs.Task;
        }

        private static void FinishRestore(DatabaseRestoreRequest request, Database database, Server server)
        {

            // After the restore, ensure the recovery model is set to simple.
            // Since we only support DEV/TEST/DEMO, we dont want the overhead of the other recovery models.
            database.RecoveryModel = RecoveryModel.Simple;
            database.Alter();

            string sqlConnectionString =
                string.Format("Integrated Security=SSPI;Persist Security Info=True;Initial Catalog={1};Data Source={0}",
                              server.Name, database.Name);

            foreach (var script in request.ScriptsToExecute)
            {
                var fileInfo = new FileInfo(script);


                string sql;

                using (var text = fileInfo.OpenText())
                {
                    sql = text.ReadToEnd();
                    text.Close();
                }


                SqlConnection connection = new SqlConnection(sqlConnectionString);
                Server srv = new Server(new ServerConnection(connection));
                srv.ConnectionContext.SqlExecutionModes = SqlExecutionModes.ExecuteAndCaptureSql;
                srv.ConnectionContext.ExecuteNonQuery(sql);
            }
        }

        private static void TransferCompletion<T>(
            TaskCompletionSource<string> tcs, ServerMessageEventArgs args,
            Func<string> getResult, Action unregisterHandler, DatabaseRestoreRequest request, Database database, Server server)
        {
            try
            {


                // God knows why if a backup successfuly completed the SqlError object is set with a success message and a code 3014.
                if (args.Error != null && args.Error.Number != 3014)
                {
                    tcs.TrySetException(new SqlExecutionException(args.Error.Message));
                }
                else
                {
                    FinishRestore(request, database, server);
                    tcs.TrySetResult(args.ToString());

                }
                if (unregisterHandler != null)
                    unregisterHandler();

            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        }


        public DatabaseRestoreRequest GenerateDatabaseRestoreRequestService(string backupFileName,
                                                                            Server server, Database database)
        {
            BackupInfo backupInfo = backupFileReaderService.GetBackupInfo(backupFileName, server);
            ServerInfo targetServerInfo = targetDatabaseServerRepositoty.GetServerInfo(database);

            IList<FileToRestore> filesList = MergeFilesLists(backupInfo, targetServerInfo);

            return new DatabaseRestoreRequest
                {
                    FullBackupFile = backupFileName,
                    TargetInstance = server.Name,
                    TargetDatabase = database.Name,
                    FilesLists = filesList
                };
        }


        private static IList<FileToRestore> MergeFilesLists(BackupInfo backupInfo, ServerInfo targetServerInfo)
        {
            var filesList = new List<FileToRestore>();

            var dataFile = new FileToRestore
                {
                    BackupLogicalName = backupInfo.DataLogicalName,
                    TargetLogicalName = targetServerInfo.DataFile.LogicalName,
                    TargetPhysicalPath = targetServerInfo.DataFile.PhysicalPath
                };

            filesList.Add(dataFile);

            int index = 0;
            foreach (DatabaseFile logfile in backupInfo.LogFiles)
            {
                string targetLogicalName = null;
                string targetPhysicalPath = null;

                if (targetServerInfo.LogsFiles.Count - 1 <= index)
                {
                    DatabaseFile targetFile = targetServerInfo.LogsFiles[index];
                    targetLogicalName = targetFile.LogicalName;
                    targetPhysicalPath = targetFile.PhysicalPath;
                }

                filesList.Add(new FileToRestore
                    {
                        BackupLogicalName = logfile.LogicalName,
                        TargetLogicalName = targetLogicalName,
                        TargetPhysicalPath = targetPhysicalPath
                    });


                index++;
            }
            return filesList;
        }
    }
}