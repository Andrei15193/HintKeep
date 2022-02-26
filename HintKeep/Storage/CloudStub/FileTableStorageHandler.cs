using System.Collections.Generic;
using System.IO;
using CloudStub.Core.StorageHandlers;

namespace HintKeep.Storage.CloudStub
{
    public class FileTableStorageHandler : ITableStorageHandler
    {
        private readonly DirectoryInfo _root;

        public FileTableStorageHandler()
            => _root = new DirectoryInfo(Directory.GetCurrentDirectory()).CreateSubdirectory(".appData").CreateSubdirectory("TableStorage");

        public bool Create(string tableName)
        {
            if (Exists(tableName))
                return false;
            else
            {
                using (var textWriter = File.CreateText(Path.Combine(_root.FullName, tableName + ".json")))
                    textWriter.Write("[]");
                return true;
            }
        }

        public bool Delete(string tableName)
        {
            if (Exists(tableName))
            {
                File.Delete(Path.Combine(_root.FullName, tableName + ".json"));
                return true;
            }
            else
                return false;
        }

        public bool Exists(string tableName)
            => File.Exists(Path.Combine(_root.FullName, tableName + ".json"));

        public IPartitionClusterStorageHandler GetPartitionClusterStorageHandler(string tableName, string partitionKey)
            => new FilePartitionClusterStorageHandler(new FileInfo(Path.Combine(_root.FullName, tableName + ".json")));

        public IEnumerable<IPartitionClusterStorageHandler> GetPartitionClusterStorageHandlers(string tableName)
        {
            yield return new FilePartitionClusterStorageHandler(new FileInfo(Path.Combine(_root.FullName, tableName + ".json")));
        }
    }
}