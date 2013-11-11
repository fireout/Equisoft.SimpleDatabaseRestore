using System.Collections.Generic;
using System.Linq;
using Equisoft.SimpleDatabaseRestore.Models;
using Equisoft.SimpleDatabaseRestore.Services;

namespace Equisoft.SimpleDatabaseRestore.Repositories
{
    public class SharedBackupsRepository : ISharedBackupsRepository
    {
        private readonly IBackupFileNameParserService fileNameParserService;
        private readonly IDatabaseBackupFolderScannerService scannerService;
        private readonly string searchPattern;
        private readonly string sharedBackupsPath;

        public SharedBackupsRepository(IDatabaseBackupFolderScannerService scannerService,
                                       IBackupFileNameParserService fileNameParserService, string sharedBackupsPath,
                                       string searchPattern)
        {
            this.scannerService = scannerService;
            this.fileNameParserService = fileNameParserService;
            this.sharedBackupsPath = sharedBackupsPath;
            this.searchPattern = searchPattern;
        }

        public IList<DatabaseServer> GetAllServers()
        {
            IList<BackupFile> foundFiles = scannerService.ScanFolderAndSubdirectory(sharedBackupsPath, searchPattern);
            var servers = new List<DatabaseServer>();

            foreach (BackupFile file in foundFiles)
            {
                IDictionary<string, string> parseResult = fileNameParserService.Parse(sharedBackupsPath,
                                                                                      file.DirectoryFullName,
                                                                                      file.FileName, file.Extension);

                DatabaseServer server = servers.FirstOrDefault(s => s.Name == parseResult["server"]);

                if (server == null)
                {
                    server = new DatabaseServer();
                    server.Name = parseResult["server"];
                    servers.Add(server);
                }

                DatabaseInstance instance = server.Instances.FirstOrDefault(s => s.Name == parseResult["instance"]);

                if (instance == null)
                {
                    instance = new DatabaseInstance();
                    instance.Name = parseResult["instance"];
                    server.Instances.Add(instance);
                }

                Database database = instance.Databases.FirstOrDefault(s => s.Name == parseResult["database"]);

                if (database == null)
                {
                    database = new Database();
                    database.Name = parseResult["database"];
                    instance.Databases.Add(database);
                }

                database.Backups.Add(new DatabaseBackup {FilePath = file.FileName, Name = file.Name});
            }

            return servers.OrderBy(s => s.Name).ToList();
        }
    }
}