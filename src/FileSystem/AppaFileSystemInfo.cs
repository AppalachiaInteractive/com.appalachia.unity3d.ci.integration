using System.IO;

namespace Appalachia.CI.Integration.FileSystem
{
    public abstract class AppaFileSystemInfo
    {
        public abstract bool Exists { get; }
        public abstract string Name { get; }
        public abstract string FullPath { get; }
        public abstract string RelativePath { get; }
        public abstract AppaDirectoryInfo Parent { get; }

        public abstract void Delete();


        protected abstract FileSystemInfo GetFileSystemInfo();

        public static implicit operator FileSystemInfo(AppaFileSystemInfo o) =>
            o.GetFileSystemInfo();

        public static implicit operator AppaFileSystemInfo(FileSystemInfo o) =>
            o is FileInfo fi
                ? (AppaFileSystemInfo) (AppaFileInfo) fi
                : (AppaFileSystemInfo) (AppaDirectoryInfo) (DirectoryInfo) o;

    }
}
