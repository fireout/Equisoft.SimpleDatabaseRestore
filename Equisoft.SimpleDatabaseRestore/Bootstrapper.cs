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
using log4net.Config;
using log4net;
using NewRelicAgent = NewRelic.Api.Agent.NewRelic;

namespace Equisoft.SimpleDatabaseRestore
{
    public class Bootstrapper : NinjectNancyBootstrapper
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(Bootstrapper));
        protected override void ApplicationStartup(IKernel container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            // configure log4net
            XmlConfigurator.Configure();

            // Log all errors with log4net and notify New Relic.
            pipelines.OnError.AddItemToStartOfPipeline(
                            (context, ex) =>
                            {
                                log.Error(ex);
                                NewRelicAgent.NoticeError(ex);
                                return null;
                            });

            // Leave this on always...no point in not showing the full error since this is used internally only.
            StaticConfiguration.DisableErrorTraces = false;

            //Activate cookie-based Session variables 
            CookieBasedSessions.Enable(pipelines);

            // Transform messages session variables in ViewBag variables.
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

            // Configure SignalR with Ninject Kernel.
            var resolver = new NinjectSignalRDependencyResolver(this.ApplicationContainer);
            var config = new HubConfiguration()
            {
                Resolver = resolver
            };

            // Register SignalR routes
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
}