using System.Collections.Generic;
using Equisoft.SimpleDatabaseRestore.Models;
using Database = Microsoft.SqlServer.Management.Smo.Database;

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