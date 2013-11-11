using System.Collections.Generic;
using Equisoft.SimpleDatabaseRestore.Models;
using Equisoft.SimpleDatabaseRestore.Repositories;
using Equisoft.SimpleDatabaseRestore.Services;
using Moq;
using Xunit;

namespace Equisoft.SimpleDatabaseRestore.Tests.Repositories
{
    public class SharedBackupRepositoryTests
    {
        [Fact]
        public void GetAllServers_Should_Backup_Within_A_Server_Hierarchy()
        {
            var scanner = new Mock<IDatabaseBackupFolderScannerService>();

            scanner.Setup(s => s.ScanFolderAndSubdirectory(@"\\MySharedPath", ".bak")).Returns(GetTestFiles());

            var repo = new SharedBackupsRepository(scanner.Object, new SharedBackupFileNameParserService(),
                                                   @"\\MySharedPath", ".bak");

            IList<DatabaseServer> servers = repo.GetAllServers();

            Assert.Equal(2, servers.Count);
            Assert.Equal(2, servers[0].Instances.Count);
            Assert.Equal(1, servers[1].Instances.Count);

            Assert.Equal(1, servers[0].Instances[0].Databases.Count);
            Assert.Equal(2, servers[0].Instances[1].Databases.Count);

            Assert.Equal(1, servers[0].Instances[0].Databases[0].Backups.Count);
            Assert.Equal(2, servers[0].Instances[1].Databases[1].Backups.Count);
        }

        private static BackupFile[] GetTestFiles()
        {
            var files = new[]
                {
                    new BackupFile
                        {
                            DirectoryFullName = @"\\MySharedPath\Server1\Instance1",
                            DirectoryName = "Instance1",
                            Extension = ".bak",
                            FileName = @"\\MySharedPath\Server1\Instance1\database1_ts.bak",
                            Name = "database1_ts.bak"
                        },
                    new BackupFile
                        {
                            DirectoryFullName = @"\\MySharedPath\Server1\Instance2",
                            DirectoryName = "Instance2",
                            Extension = ".bak",
                            FileName = @"\\MySharedPath\Server1\Instance2\database1_ts.bak",
                            Name = "database1_ts.bak"
                        },
                    new BackupFile
                        {
                            DirectoryFullName = @"\\MySharedPath\Server1\Instance2",
                            DirectoryName = "Instance2",
                            Extension = ".bak",
                            FileName = @"\\MySharedPath\Server1\Instance2\database2_ts.bak",
                            Name = "database2_ts.bak"
                        },
                    new BackupFile
                        {
                            DirectoryFullName = @"\\MySharedPath\Server1\Instance2",
                            DirectoryName = "Instance2",
                            Extension = ".bak",
                            FileName = @"\\MySharedPath\Server1\Instance2\database2_ts1.bak",
                            Name = "database2_ts1.bak"
                        },
                    new BackupFile
                        {
                            DirectoryFullName = @"\\MySharedPath\Server2\Instance1",
                            DirectoryName = "Instance1",
                            Extension = ".bak",
                            FileName = @"\\MySharedPath\Server2\Instance1\database1_ts.bak",
                            Name = "database1_ts.bak"
                        }
                };
            return files;
        }
    }
}