using System.Collections.Generic;
using System.Linq;
using Equisoft.SimpleDatabaseRestore.Models;
using Equisoft.SimpleDatabaseRestore.Services;

namespace Equisoft.SimpleDatabaseRestore.Repositories
{
    public interface ISharedBackupsRepository
    {
        IList<DatabaseServer> GetAllServers();
    }

    public class SharedBackupsRepository : ISharedBackupsRepository
    {
        private readonly IDatabaseBackupFolderScannerService scannerService;
        private readonly IBackupFileNameParserService fileNameParserService;
        private readonly string sharedBackupsPath;
        private readonly string searchPattern;

        public SharedBackupsRepository(IDatabaseBackupFolderScannerService scannerService, IBackupFileNameParserService fileNameParserService, string sharedBackupsPath, string searchPattern)
        {
            this.scannerService = scannerService;
            this.fileNameParserService = fileNameParserService;
            this.sharedBackupsPath = sharedBackupsPath;
            this.searchPattern = searchPattern;
        }

        public IList<DatabaseServer> GetAllServers()
        {
            var foundFiles = scannerService.ScanFolderAndSubdirectory(sharedBackupsPath, searchPattern);
            var servers = new List<DatabaseServer>();

            foreach (var file in foundFiles)
            {
                var parseResult = fileNameParserService.Parse(sharedBackupsPath, file.DirectoryFullName, file.FileName, file.Extension);

                var server = servers.FirstOrDefault(s => s.Name == parseResult["server"]);

                if (server == null)
                {
                    server = new DatabaseServer();
                    server.Name = parseResult["server"];
                    servers.Add(server);
                }

                var instance = server.Instances.FirstOrDefault(s => s.Name == parseResult["instance"]);

                if (instance == null)
                {
                    instance = new DatabaseInstance();
                    instance.Name = parseResult["instance"];
                    server.Instances.Add(instance);
                }

                var database  = instance.Databases.FirstOrDefault(s => s.Name == parseResult["database"]);

                if (database == null)
                {
                    database = new Database();
                    database.Name = parseResult["database"];
                    instance.Databases.Add(database);
                }

                database.Backups.Add(new DatabaseBackup {FilePath = file.FileName,Name = file.Name});

            }

            return servers.OrderBy(s => s.Name).ToList();
        }
    }
}