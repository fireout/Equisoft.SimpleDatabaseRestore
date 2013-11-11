using System.Collections.Generic;
using Equisoft.SimpleDatabaseRestore.Models;

namespace Equisoft.SimpleDatabaseRestore.Repositories
{
    public interface ISharedBackupsRepository
    {
        IList<DatabaseServer> GetAllServers();
    }
}