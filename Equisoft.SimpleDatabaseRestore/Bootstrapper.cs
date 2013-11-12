using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Routing;
using Equisoft.SimpleDatabaseRestore.Repositories;
using Equisoft.SimpleDatabaseRestore.Services;
using Microsoft.AspNet.SignalR;
using Nancy;
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

            StaticConfiguration.DisableErrorTraces = false;

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

            var resolver = new NinjectSignalRDependencyResolver(this.ApplicationContainer);

            var config = new HubConfiguration()
            {
                Resolver = resolver
            };

            RouteTable.Routes.MapHubs(config);
        }


        protected override void ConfigureApplicationContainer(IKernel existingContainer)
        {
            RegisterServices(existingContainer);

            base.ConfigureApplicationContainer(existingContainer);
        }

        private static void RegisterServices(IKernel existingContainer)
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
        }
    }

    internal class NinjectSignalRDependencyResolver : DefaultDependencyResolver
    {
        private readonly IKernel _kernel;
        public NinjectSignalRDependencyResolver(IKernel kernel)
        {
            _kernel = kernel;
        }

        public override object GetService(Type serviceType)
        {
            return _kernel.TryGet(serviceType) ?? base.GetService(serviceType);
        }

        public override IEnumerable<object> GetServices(Type serviceType)
        {
            return _kernel.GetAll(serviceType).Concat(base.GetServices(serviceType));
        }
    }
}