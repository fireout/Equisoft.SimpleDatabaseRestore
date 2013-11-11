using System.Collections.Generic;
using Equisoft.SimpleDatabaseRestore.Commands;
using Nancy;

namespace Equisoft.SimpleDatabaseRestore.Modules
{
    public class ResquestRestoreModule:NancyModule
    {
        private readonly IRestoreDatabaseService restoreDatabaseService;

        public ResquestRestoreModule(IRestoreDatabaseService restoreDatabaseService)
        {
            this.restoreDatabaseService = restoreDatabaseService;
            Get["/RequestRestore"] = ConfirmRestoreRequest;

            Post["/Restore"] = Restore;
        }

        private dynamic Restore(dynamic parameters)
        {
            return View["requestRestore"];
        }

        private dynamic ConfirmRestoreRequest(dynamic parameters)
        {
            if (!Request.Query.targetDatabase.HasValue || !Request.Query.backupFileName.HasValue ||
                string.IsNullOrWhiteSpace(Request.Query.targetDatabase) || string.IsNullOrWhiteSpace(Request.Query.backupFileName))
            {
                Session["Errors"] = "Please select the backup file and the target database before submitting a request.";
                    //new List<string>
                    //{
                    //    "Please select the backup file and the target database before submitting a request."
                    //};
                return new Nancy.Responses.RedirectResponse("/");
            }


            string[] targetDatabaseParams = ((string) Request.Query.targetDatabase).Split('.');

            string targetInstanceName = targetDatabaseParams[0];
            string targetDatabase = targetDatabaseParams[1];

            string backupFileName = Request.Query.backupFileName;

            DatabaseRestoreRequest model = restoreDatabaseService.GenerateRequest(backupFileName, targetInstanceName, targetDatabase);

            return View["requestRestore", model];
        }
    }
}