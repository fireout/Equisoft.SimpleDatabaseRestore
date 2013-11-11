using System.IO;

namespace Equisoft.SimpleDatabaseRestore.Models
{
    public class BackupFile
    {
        public BackupFile()
        {
        }

        public BackupFile(string fileName)
        {
            FileName = fileName;
            var file = new FileInfo(fileName);
            Extension = file.Extension;
            Name = file.Name;
            DirectoryFullName = file.Directory.FullName;
            DirectoryName = file.Directory.Name;
        }

        public string FileName { get; set; }
        public string DirectoryName { get; set; }
        public string DirectoryFullName { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
    }
}