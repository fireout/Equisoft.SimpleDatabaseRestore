using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Equisoft.SimpleDatabaseRestore.Models
{
    public class DatabaseFile
    {
        public string LogicalName { get; set; }
        public string PhysicalPath { get; set; }
    }
}