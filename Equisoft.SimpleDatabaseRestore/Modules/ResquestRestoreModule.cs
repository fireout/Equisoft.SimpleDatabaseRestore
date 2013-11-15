using Equisoft.SimpleDatabaseRestore.Commands;
using Equisoft.SimpleDatabaseRestore.Extensions;
using Equisoft.SimpleDatabaseRestore.Services;
using Nancy;
using Nancy.Responses;
using Nancy.ModelBinding;

namespace Equisoft.SimpleDatabaseRestore.Modules
{
    public class ResquestRestoreModule : NancyModule
    {
        private readonly IRestoreDatabaseService restoreDatabaseService;

        public ResquestRestoreModule(IRestoreDatabaseService restoreDatabaseService)
        {
            this.restoreDatabaseService = restoreDatabaseService;

            this.RequiresWindowsAuthentication();
            
            Get["/RequestRestore"] = ConfirmRestoreRequest;

        //    Post["/Restore"] = Restore;
        }



        //private dynamic Restore(dynamic parameters)
        //{
        //    string targetInstanceName = Request.Form.TargetInstance;
        //    string targetDatabase = Request.Form.TargetDatabase;
        //    string backupFileName = Request.Form.BackupFile;

        //    DatabaseRestoreRequest request = restoreDatabaseService.GenerateRequest(backupFileName, targetInstanceName,
        //                                                                            targetDatabase);

        //    restoreDatabaseService.Restore(request, null);

        //    Session["Success"] = string.Format("Database {0} was successfuly restored on {1}", targetDatabase,
        //                                       targetInstanceName);

        //    return new RedirectResponse("/");
        //}

        private dynamic ConfirmRestoreRequest(dynamic parameters)
        {
            if (!Request.Query.targetDatabase.HasValue || !Request.Query.backupFileName.HasValue ||
                string.IsNullOrWhiteSpace(Request.Query.targetDatabase) ||
                string.IsNullOrWhiteSpace(Request.Query.backupFileName))
            {
                Session["Errors"] = "Please select the backup file and the target database before submitting a request.";
               
                return new RedirectResponse("/");
            }

            string[] targetDatabaseParams = ((string) Request.Query.targetDatabase).Split('.');

            string targetInstanceName = targetDatabaseParams[0];
            string targetDatabase = targetDatabaseParams[1];

            string backupFileName = Request.Query.backupFileName;

            DatabaseRestoreRequest model = restoreDatabaseService.GenerateRequest(backupFileName, targetInstanceName,
                                                                                  targetDatabase);

            return View["requestRestore", model];
        }
    }
}