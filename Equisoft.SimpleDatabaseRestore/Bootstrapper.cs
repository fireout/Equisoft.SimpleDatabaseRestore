using System.Configuration;
using Equisoft.SimpleDatabaseRestore.Modules;
using Equisoft.SimpleDatabaseRestore.Repositories;
using Equisoft.SimpleDatabaseRestore.Services;
using Nancy.Bootstrappers.Ninject;
using Nancy.Session;
using Ninject;

namespace Equisoft.SimpleDatabaseRestore
{

    public class Bootstrapper : NinjectNancyBootstrapper 
    {
        protected override void ApplicationStartup(IKernel container, Nancy.Bootstrapper.IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            Nancy.Session.CookieBasedSessions.Enable(pipelines);
            pipelines.BeforeRequest += (ctx) =>
            {
                if (ctx.Request.Session["Errors"] != null)
                {
                    ctx.ViewBag.Errors = ctx.Request.Session["Errors"];
                    ctx.Request.Session.Delete("Errors");
                }
                return null;
            };
            
        }


        protected override void ConfigureApplicationContainer(IKernel existingContainer)
        {
            existingContainer.Bind<IBackupFileNameParserService>()
                             .To<SharedBackupFileNameParserService>()
                             .InSingletonScope();

            existingContainer.Bind<IRestoreDatabaseService>()
                             .To<RestoreDatabaseService>()
                             .InSingletonScope();

            existingContainer.Bind<IBackupFileReaderService>()
                             .To<BackupFileReaderService>()
                             .InSingletonScope();

            existingContainer.Bind<IDatabaseBackupFolderScannerService>()
                             .To<DatabaseBackupFolderScannerService>()
                             .InSingletonScope();

            existingContainer.Bind<ISharedBackupsRepository>()
                             .To<SharedBackupsRepository>().InSingletonScope()
                             .WithConstructorArgument("sharedBackupsPath", ConfigurationManager.AppSettings["sharedBackupsPath"])
                             .WithConstructorArgument("searchPattern", ConfigurationManager.AppSettings["searchPattern"]);

            existingContainer.Bind<ITargetDatabaseServerRepositoty>()
                            .To<TargetDatabaseServerRepositoty>()
                            .InSingletonScope()
                            .WithConstructorArgument("serversToExclude", ConfigurationManager.AppSettings["destinationServersToExlude"]
                                                                                             .Replace(" ","")
                                                                                             .Split(','));

            base.ConfigureApplicationContainer(existingContainer);
        }

   }
}