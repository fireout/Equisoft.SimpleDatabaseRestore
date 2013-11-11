using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Equisoft.SimpleDatabaseRestore.Services;
using Xunit;
using Xunit.Extensions;

namespace Equisoft.SimpleDatabaseRestore.Tests.Services
{
    public class SharedBackupFileParserServiceTests
    {
        [Theory]
        [InlineData(@"\\test\", @"\\test\server\instance\","Test_Test__Test_timestamp.bak","server","instance", "Test_Test__Test", "timestamp")]
        [InlineData(@"\\test", @"\\test\server\instance\", "Test_Test_timestamp.bak", "server", "instance", "Test_Test", "timestamp")]
        [InlineData(@"\\test\", @"\\test\server\instance", "Test_timestamp.bak", "server", "instance", "Test", "timestamp")]
        public void Standard_FileName_Is_Parse_Returns_Database_Name(string root, string directory, string filename, 
                                                                    string server, string instance, string databaseName, string timestamp)
        {
            var parser = new SharedBackupFileNameParserService();

            var result = parser.Parse(root,directory, filename,".bak");

            Assert.Equal(4,result.Count);
            Assert.Equal(databaseName, result["database"]);
            Assert.Equal(timestamp, result["timestamp"]);
            Assert.Equal(instance, result["instance"]);
            Assert.Equal(server, result["server"]);
        }
    }
}
