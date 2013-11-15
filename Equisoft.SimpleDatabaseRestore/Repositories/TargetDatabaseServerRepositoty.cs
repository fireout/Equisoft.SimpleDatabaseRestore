using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Sql;
using System.Globalization;
using System.Linq;
using Equisoft.SimpleDatabaseRestore.Models;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Database = Microsoft.SqlServer.Management.Smo.Database;
using DatabaseFile = Equisoft.SimpleDatabaseRestore.Models.DatabaseFile;

namespace Equisoft.SimpleDatabaseRestore.Repositories
{
    public class TargetDatabaseServerRepositoty : ITargetDatabaseServerRepositoty
    {
        private readonly string[] serversToExclude;

        public TargetDatabaseServerRepositoty(params string[] serversToExclude)
        {
            this.serversToExclude = serversToExclude;
        }


        public IList<DatabaseServer> GetAll()
        {
            SqlDataSourceEnumerator instance = SqlDataSourceEnumerator.Instance;
            DataTable table = instance.GetDataSources();
            var servers = new List<DatabaseServer>();


            var foundInstances = table.AsEnumerable()
                                      .Where(row => IsAllowed(row.Field<string>("ServerName")))
                                      .Select(
                                          row =>
                                          new
                                              {
                                                  Server = row.Field<string>("ServerName"),
                                                  InstanceName = row.Field<string>("InstanceName")
                                              });


            foreach (var foundInstance in foundInstances)
            {
                DatabaseServer server = servers.FirstOrDefault(s => s.Name == foundInstance.Server);
                if (server == null)
                {
                    server = new DatabaseServer {Name = foundInstance.Server};
                    servers.Add(server);
                }

                var dbInstance = new DatabaseInstance {Name = foundInstance.InstanceName};


                try
                {
                    var serverInfo = new Server(foundInstance.Server + "\\" + foundInstance.InstanceName);

                    // Only allowed Online database (Status == DatabaseStatus.Normal) and non system database (!database.IsSystemObject)
                    var allowedDatabase = serverInfo.Databases.Cast<Database>()
                                                    .Where(database => database.Status == DatabaseStatus.Normal && !database.IsSystemObject);

                    foreach (Database database in allowedDatabase)
                    {
                        dbInstance.Databases.Add(new Models.Database {Name = database.Name});
                    }
                }
                catch (ConnectionFailureException)
                {
                }

                if (dbInstance.Databases.Any())
                {
                    server.Instances.Add(dbInstance);
                }
            }

            return servers.OrderBy(s => s.Name).ToList();
        }

        public ServerInfo GetServerInfo(string targetInstance, string targetDatabase)
        {
            var server = new Server(targetInstance);
            Database database = server.Databases[targetDatabase];

            return GetServerInfo(database);
        }


        public ServerInfo GetServerInfo(Database database)
        {
            var info = new ServerInfo();
            DataFile dataFile = database.FileGroups[0].Files[0];

            info.DataFile = new DatabaseFile {LogicalName = dataFile.Name, PhysicalPath = dataFile.FileName};

            foreach (LogFile logFile in database.LogFiles)
            {
                info.LogsFiles.Add(new DatabaseFile {LogicalName = logFile.Name, PhysicalPath = logFile.FileName});
            }

            return info;
        }

        private bool IsAllowed(string serverName)
        {
            if (serversToExclude.Contains(serverName))
            {
                return false;
            }

            if (serverName.StartsWith("equi", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            int result;
            return int.TryParse(serverName[1].ToString(CultureInfo.InvariantCulture), out result);
        }
    }
}