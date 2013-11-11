using System.Collections.Generic;

namespace Equisoft.SimpleDatabaseRestore.Services
{
    public class SharedBackupFileNameParserService : IBackupFileNameParserService
    {
        public IDictionary<string, string> Parse(string root, string directory, string filename, string extension)
        {
            int timestampPosition = filename.LastIndexOf('_');
            string filenameWithoutExtension = filename.Substring(0, filename.Length - extension.Length);

            var values = new Dictionary<string, string>();
            values.Add("database", filenameWithoutExtension.Substring(0, timestampPosition));
            values.Add("timestamp", filenameWithoutExtension.Substring(timestampPosition + 1));


            string directoryWithoutRoot = directory.Replace(root, "").Trim('\\');

            int serverPosition = directoryWithoutRoot.IndexOf('\\');
            values.Add("server", directoryWithoutRoot.Substring(0, serverPosition));
            string instance = directoryWithoutRoot.Substring(serverPosition + 1);

            if (instance.EndsWith("\\"))
            {
                instance = instance.Trim('\\');
            }

            values.Add("instance", instance);

            return values;
        }
    }
}