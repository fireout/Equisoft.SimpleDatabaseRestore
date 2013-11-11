using Equisoft.SimpleDatabaseRestore.Commands;

namespace Equisoft.SimpleDatabaseRestore.Services
{
    public interface IRestoreDatabaseService
    {
        DatabaseRestoreRequest GenerateRequest(string backupFileName,
                                               string targetInstanceName,
                                               string targetDatabase);

        void Restore(DatabaseRestoreRequest request);
    }
}