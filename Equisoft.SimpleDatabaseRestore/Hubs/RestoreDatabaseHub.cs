using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Equisoft.SimpleDatabaseRestore.Commands;
using Equisoft.SimpleDatabaseRestore.Services;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Equisoft.SimpleDatabaseRestore.Hubs
{
    [HubName("restoreDatabase")]
    public class RestoreDatabaseHub : Hub
    {
        private readonly IRestoreDatabaseService restoreDatabaseService;

        public RestoreDatabaseHub(IRestoreDatabaseService restoreDatabaseService)
        {
            this.restoreDatabaseService = restoreDatabaseService;
        }

        public async Task Restore(string backupFileName, string targetInstanceName, string targetDatabase)
        {
            var watch = new Stopwatch();

            Clients.All.showInfo(string.Format("<strong>{0:HH:mm:ss}</strong> - Restore was requested for <strong>{1}</strong> on <strong>{2}</strong>", DateTime.Now, targetDatabase,
                                      targetInstanceName));

            watch.Start();
            DatabaseRestoreRequest request = restoreDatabaseService.GenerateRequest(backupFileName, targetInstanceName,
                                                                                               targetDatabase);
            var restoreTask = restoreDatabaseService.RestoreAsync(request, (sender, args) => Clients.Caller.showProgress(string.Format("<strong>{0:HH:mm:ss}</strong> - {1}", DateTime.Now, args.Message)));

            await restoreTask.ContinueWith(task =>
                {
                    watch.Stop();
                    Clients.All.showSuccess(string.Format("<strong>{0:HH:mm:ss}</strong> -  Database <strong>{1}</strong> was successfuly restored on <strong>{2}</strong> in {3} seconds",
                                                           DateTime.Now, targetDatabase, targetInstanceName, watch.Elapsed.TotalSeconds));
                });
        }
    }
}