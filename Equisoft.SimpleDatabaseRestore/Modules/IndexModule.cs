using System.Collections.Generic;
using Equisoft.SimpleDatabaseRestore.Extensions;
using Equisoft.SimpleDatabaseRestore.Models;
using Equisoft.SimpleDatabaseRestore.Repositories;
using Nancy;

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
            IList<DatabaseServer> foundFiles = sharedBackupsRepository.GetAllServers();
            IList<DatabaseServer> targetInstances = targetDatabaseServerRepositoty.GetAll();
            dynamic model = new
                {
                    FoundFiles = foundFiles,
                    TargertInstances = targetInstances
                };

            return View["index", model];
        }
    }
}