using System.Configuration;
using Equisoft.SimpleDatabaseRestore.Repositories;
using Equisoft.SimpleDatabaseRestore.Services;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Ninject;
using Nancy.Session;
using Ninject;

namespace Equisoft.SimpleDatabaseRestore
{
    public class Bootstrapper : NinjectNancyBootstrapper
    {
        protected override void ApplicationStartup(IKernel container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            CookieBasedSessions.Enable(pipelines);
            pipelines.BeforeRequest += ctx =>
                {
                    if (ctx.Request.Session["Errors"] != null)
                    {
                        ctx.ViewBag.Errors = ctx.Request.Session["Errors"];
                        ctx.Request.Session.Delete("Errors");
                    }
                    if (ctx.Request.Session["Success"] != null)
                    {
                        ctx.ViewBag.Success = ctx.Request.Session["Success"];
                        ctx.Request.Session.Delete("Success");
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
                             .WithConstructorArgument("sharedBackupsPath",
                                                      ConfigurationManager.AppSettings["sharedBackupsPath"])
                             .WithConstructorArgument("searchPattern", ConfigurationManager.AppSettings["searchPattern"]);

            existingContainer.Bind<ITargetDatabaseServerRepositoty>()
                             .To<TargetDatabaseServerRepositoty>()
                             .InSingletonScope()
                             .WithConstructorArgument("serversToExclude",
                                                      ConfigurationManager.AppSettings["destinationServersToExlude"]
                                                          .Replace(" ", "")
                                                          .Split(','));


            existingContainer.Bind<IScriptsRepository>()
                             .To<ScriptsRepository>()
                             .InSingletonScope()
                             .WithConstructorArgument("rootPath",
                                                      ConfigurationManager.AppSettings["sqlScriptsPath"]);

            base.ConfigureApplicationContainer(existingContainer);
        }
    }
}