using System.Collections.Generic;

namespace Equisoft.SimpleDatabaseRestore.Repositories
{
    public interface IScriptsRepository
    {
        IList<string> GetScripts(string instance, string database);
        string RoothPath { get; }
    }
}