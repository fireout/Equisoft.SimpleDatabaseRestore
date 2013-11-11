using System.Collections.Generic;
using Equisoft.SimpleDatabaseRestore.Models;
using Microsoft.SqlServer.Management.Smo;
using Database = Microsoft.SqlServer.Management.Smo.Database;
using DatabaseFile = Equisoft.SimpleDatabaseRestore.Models.DatabaseFile;

namespace Equisoft.SimpleDatabaseRestore.Repositories
{
    public interface ITargetDatabaseServerRepositoty
    {
        IList<DatabaseServer> GetAll();
        ServerInfo GetServerInfo(string targetInstance, string targetDatabase);
        ServerInfo GetServerInfo(Database database);
    }

    public class ServerInfo
    {
        public ServerInfo()
        {
            LogsFiles = new List<DatabaseFile>();
        }

        public DatabaseFile DataFile { get; set; }
        public IList<DatabaseFile> LogsFiles { get; set; }

    }
}