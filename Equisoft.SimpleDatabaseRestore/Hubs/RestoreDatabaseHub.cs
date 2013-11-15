using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Equisoft.SimpleDatabaseRestore.Commands;
using Equisoft.SimpleDatabaseRestore.Services;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Ninject.Extensions.Logging;

namespace Equisoft.SimpleDatabaseRestore.Hubs
{
    [HubName("restoreDatabase")]
    public class RestoreDatabaseHub : Hub
    {
        private readonly IRestoreDatabaseService restoreDatabaseService;
        private readonly ILogger log;

        public RestoreDatabaseHub(IRestoreDatabaseService restoreDatabaseService, ILogger log)
        {
            this.restoreDatabaseService = restoreDatabaseService;
            this.log = log;
        }

        public async Task Restore(string backupFileName, string targetInstanceName, string targetDatabase)
        {

            var watch = new Stopwatch();
            string username = Context.User.Identity.Name;

            log.Info("Starting to restore {0} on {1}.{2} - The restore was requested by {3}", backupFileName, targetInstanceName, targetDatabase, username);

            Clients.All.showInfo(string.Format("<strong>{0:HH:mm:ss}</strong> - Restore was requested for <strong>{1}</strong> on <strong>{2}</strong><small>by {3}</small>", DateTime.Now, targetDatabase,
                                      targetInstanceName, username));

            watch.Start();
            DatabaseRestoreRequest request = restoreDatabaseService.GenerateRequest(backupFileName, targetInstanceName, targetDatabase);

            // Start the DB restore asynchrounosly. 
            // We add a progress handler to notify the caller of any progress reported by the service.
            var restoreTask = restoreDatabaseService.RestoreAsync(request, (sender, args) => Clients.Caller.showProgress(string.Format("<strong>{0:HH:mm:ss}</strong> - {1}", DateTime.Now, args.Message)));

            try
            {
                // Wait for the task to complete. If every thing is ok, send the confirmation message.
                await restoreTask.ContinueWith(task =>
                    {
                        watch.Stop();
                        Clients.All.showSuccess(string.Format("<strong>{0:HH:mm:ss}</strong> -  Database <strong>{1}</strong> was successfuly restored on <strong>{2}</strong><small> in {3} seconds by {4}</small>",
                                                               DateTime.Now, targetDatabase, targetInstanceName, watch.Elapsed.TotalSeconds, username));
                        log.Info("Restore completed of {0} on {1}.{2} - The restore was requested by {3}", backupFileName, targetInstanceName, targetDatabase, username);
                    }, TaskContinuationOptions.OnlyOnRanToCompletion);

            }
            catch (Exception exception)
            {
                // If the task is faulted (an error occured within the task). Extract the information.
                if (restoreTask.IsFaulted)
                {
                    foreach (var ex in restoreTask.Exception.Flatten().InnerExceptions)
                    {
                        log.ErrorException(string.Format("Restore failed of {0} on {1}.{2} - The restore was requested by {3} ", backupFileName, targetInstanceName, targetDatabase, username), ex);
                        Clients.Caller.showError(string.Format("<strong>{0:HH:mm:ss}</strong> - Restore failed for <strong>{1}</strong> on <strong>{2}</strong><small>by {3}</small><br/>{4}",
                            DateTime.Now, targetDatabase, targetInstanceName, username, ex.Message));
                    }


                }
                else
                {
                    // Otherwise send the exception directly.
                    log.ErrorException("A problem occured.", exception);
                    Clients.Caller.showError(string.Format("<strong>{0:HH:mm:ss}</strong> - Restore failed for <strong>{1}</strong> on <strong>{2}</strong><small>by {3}</small><br/>{4}",
                            DateTime.Now, targetDatabase, targetInstanceName, username, exception.Message));

                }

            }
        }
    }
}