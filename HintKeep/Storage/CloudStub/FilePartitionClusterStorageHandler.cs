using System.IO;
using CloudStub.Core.StorageHandlers;

namespace HintKeep.Storage.CloudStub
{
    public class FilePartitionClusterStorageHandler : IPartitionClusterStorageHandler
    {
        private readonly FileInfo _tableFile;

        public FilePartitionClusterStorageHandler(FileInfo tableFile)
            => _tableFile = tableFile;

        public string Key
            => _tableFile.Name;

        public TextReader OpenRead()
            => new StreamReader(new FileStream(_tableFile.FullName, FileMode.Open, FileAccess.Read, FileShare.Read));

        public TextWriter OpenWrite()
            => new StreamWriter(new FileStream(_tableFile.FullName, FileMode.Create, FileAccess.Write, FileShare.Read));
    }
}