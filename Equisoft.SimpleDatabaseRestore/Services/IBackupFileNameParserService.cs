using System.Collections.Generic;

namespace Equisoft.SimpleDatabaseRestore.Services
{
    public interface IBackupFileNameParserService
    {
        IDictionary<string, string> Parse(string root, string directory, string filename, string extension);
    }
}