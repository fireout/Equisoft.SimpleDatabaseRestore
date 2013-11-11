namespace Equisoft.SimpleDatabaseRestore.Models
{
    public class FileToRestore
    {

        public string BackupLogicalName { get; set; }
        public string TargetLogicalName { get; set; }
        public string TargetPhysicalPath { get; set; }
    }
}