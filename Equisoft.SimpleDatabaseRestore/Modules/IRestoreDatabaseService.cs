using Equisoft.SimpleDatabaseRestore.Commands;

namespace Equisoft.SimpleDatabaseRestore.Modules
{
    public interface IRestoreDatabaseService
    {
        DatabaseRestoreRequest GenerateRequest(string backupFileName,
                                                                                     string targetInstanceName,
                                                                                     string targetDatabase);
    }
}