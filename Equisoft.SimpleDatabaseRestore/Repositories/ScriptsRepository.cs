using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Equisoft.SimpleDatabaseRestore.Repositories
{
    public class ScriptsRepository : IScriptsRepository
    {
        private readonly string rootPath;

        public ScriptsRepository(string rootPath)
        {
            this.rootPath = rootPath;
        }

        public string RoothPath { get { return rootPath; } }

        public IList<string> GetScripts(string instance, string database)
        {

            string[] instanceInfo = instance.Split('\\');
            string serverName = instanceInfo[0];
            string instanceName = instanceInfo[1];


            string[] pathsToExplore =
                {
                    rootPath, // Global path
                    Path.Combine(rootPath, serverName, instanceName, "_Scripts"), // Instance specific scripts
                    Path.Combine(rootPath, serverName, instanceName, database) // Database specific scripts
                };
            
            var scripts = new List<string>();

            foreach (var path in pathsToExplore)
            {
                if(!Directory.Exists(path))
                    continue;

                scripts.AddRange(Directory.GetFiles(path, "*.sql").OrderBy(file => file).ToList());
            }

            return scripts;
        }
    }
}