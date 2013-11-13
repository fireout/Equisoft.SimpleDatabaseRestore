using System.Collections.Generic;
using Equisoft.SimpleDatabaseRestore.Extensions;
using Equisoft.SimpleDatabaseRestore.Models;
using Equisoft.SimpleDatabaseRestore.Repositories;
using Nancy;
using System.Threading.Tasks;
using System;

namespace Equisoft.SimpleDatabaseRestore.Modules
{
    public class IndexModule : NancyModule
    {
        private readonly ISharedBackupsRepository sharedBackupsRepository;
        private readonly ITargetDatabaseServerRepositoty targetDatabaseServerRepositoty;


        public IndexModule(ISharedBackupsRepository sharedBackupsRepository,
                           ITargetDatabaseServerRepositoty targetDatabaseServerRepositoty)
        {
            this.sharedBackupsRepository = sharedBackupsRepository;
            this.targetDatabaseServerRepositoty = targetDatabaseServerRepositoty;

            this.RequiresWindowsAuthentication();

            Get["/"] = Index;
        }

        private dynamic Index(dynamic parameters)
        {

            var foundFilesTask = Task.Factory.StartNew(()=>sharedBackupsRepository.GetAllServers());
            var targetInstancesTask = Task.Factory.StartNew(()=> targetDatabaseServerRepositoty.GetAll());

            Task.WaitAll(foundFilesTask, targetInstancesTask);
            IList<DatabaseServer> foundFiles = foundFilesTask.Result;
            IList<DatabaseServer> targetInstances = targetInstancesTask.Result;

            dynamic model = new
                {
                    FoundFiles = foundFiles,
                    TargertInstances = targetInstances
                };

            return View["index", model];
        }
    }
}