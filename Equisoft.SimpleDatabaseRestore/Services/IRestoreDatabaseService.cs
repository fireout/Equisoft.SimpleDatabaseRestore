using System.Threading.Tasks;
using Equisoft.SimpleDatabaseRestore.Commands;
using Microsoft.SqlServer.Management.Smo;

namespace Equisoft.SimpleDatabaseRestore.Services
{
    public interface IRestoreDatabaseService
    {
        DatabaseRestoreRequest GenerateRequest(string backupFileName,
                                               string targetInstanceName,
                                               string targetDatabase);

        void Restore(DatabaseRestoreRequest request, PercentCompleteEventHandler percentCompleteDelegate);
        Task<string> RestoreAsync(DatabaseRestoreRequest request, PercentCompleteEventHandler percentCompleteDelegate);
    }
}