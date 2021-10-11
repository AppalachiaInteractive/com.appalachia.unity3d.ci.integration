using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Appalachia.CI.Integration.Repositories
{
    [Serializable]
    public class RepositoryDirectoryMetadata : IEquatable<RepositoryDirectoryMetadata>
    {
        private DirectoryInfo _assetsDirectory;

        private DirectoryInfo _dataDirectory;

        private DirectoryInfo _gitDirectory;
        private string _packageVersion;

        private string _repoName;

        private DirectoryInfo _srcDirectory;
        public JObject packageJson;
        public DirectoryInfo root;

        private RepositoryDirectoryMetadata()
        {
        }

        public RepositoryDirectoryMetadata(
            DirectoryInfo root,
            DirectoryInfo gitDirectory,
            DirectoryInfo assetsDirectory,
            DirectoryInfo dataDirectory,
            DirectoryInfo srcDirectory,
            JObject packageJson)
        {
            this.root = root;
            this.gitDirectory = gitDirectory;
            this.assetsDirectory = assetsDirectory;
            this.dataDirectory = dataDirectory;
            this.srcDirectory = srcDirectory;
            this.packageJson = packageJson;
        }

        public DirectoryInfo gitDirectory
        {
            get
            {
                if (_gitDirectory == null)
                {
                    if (root == null)
                    {
                        return null;
                    }

                    _gitDirectory = new DirectoryInfo(Path.Combine(root.FullName, ".git"));
                }

                return _gitDirectory;
            }
            set => _gitDirectory = value;
        }

        public DirectoryInfo assetsDirectory
        {
            get
            {
                if (_assetsDirectory == null)
                {
                    if (root == null)
                    {
                        return null;
                    }

                    _assetsDirectory = new DirectoryInfo(Path.Combine(root.FullName, "assets"));
                }

                return _assetsDirectory;
            }
            set => _assetsDirectory = value;
        }

        public DirectoryInfo dataDirectory
        {
            get
            {
                if (_dataDirectory == null)
                {
                    if (root == null)
                    {
                        return null;
                    }

                    _dataDirectory = new DirectoryInfo(Path.Combine(root.FullName, "data"));
                }

                return _dataDirectory;
            }
            set => _dataDirectory = value;
        }

        public DirectoryInfo srcDirectory
        {
            get
            {
                if (_srcDirectory == null)
                {
                    if (root == null)
                    {
                        return null;
                    }

                    _srcDirectory = new DirectoryInfo(Path.Combine(root.FullName, "src"));
                }

                return _srcDirectory;
            }
            set => _srcDirectory = value;
        }

        public string repoName
        {
            get
            {
                if (_repoName == null)
                {
                    if (gitDirectory == null)
                    {
                        return null;
                    }

                    var repoFiles = gitDirectory.GetFiles();
                    var repoConfig = repoFiles.First(f => f.Name == "config");

                    var repoConfigStrings = new List<string>();

                    using (var fs = repoConfig.OpenRead())
                    using (var sr = new StreamReader(fs))
                    {
                        while (!sr.EndOfStream)
                        {
                            repoConfigStrings.Add(sr.ReadLine());
                        }
                    }

                    foreach (var repoConfigString in repoConfigStrings)
                    {
                        //url = https://github.com/AppalachiaInteractive/com.appalachia.unity3d.audio.git
                        if (!repoConfigString.Contains("url"))
                        {
                            continue;
                        }

                        var clean = repoConfigString.Trim();

                        var lastSlash = clean.LastIndexOf('/');
                        var subset = clean.Substring(lastSlash + 1);
                        subset = subset.Substring(0, subset.Length - 4);

                        _repoName = subset;
                        break;
                    }

                    if (_repoName == null)
                    {
                        _repoName = string.Empty;
                    }
                }

                return _repoName;
            }
        }

        public bool HasPackage => packageJson != null;

        public string PackageVersion
        {
            get
            {
                if (packageJson == null)
                {
                    return null;
                }

                if (_packageVersion == null)
                {
                    _packageVersion = packageJson["version"]?.ToString();
                }

                return _packageVersion;
            }
        }

        public bool Equals(RepositoryDirectoryMetadata other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(root?.FullName,         other.root?.FullName) &&
                   Equals(gitDirectory?.FullName, other.gitDirectory?.FullName) &&
                   Equals(srcDirectory?.FullName, other.srcDirectory?.FullName);
        }

        public override string ToString()
        {
            if (!HasPackage)
            {
                return root.FullName;
            }

            return $"{root.FullName}: {PackageVersion}";
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((RepositoryDirectoryMetadata) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = root != null ? root.FullName.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^
                           (gitDirectory != null ? gitDirectory.FullName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^
                           (srcDirectory != null ? srcDirectory.FullName.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(
            RepositoryDirectoryMetadata left,
            RepositoryDirectoryMetadata right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(
            RepositoryDirectoryMetadata left,
            RepositoryDirectoryMetadata right)
        {
            return !Equals(left, right);
        }

        public static RepositoryDirectoryMetadata FromRootPath(string path)
        {
            var directory = new DirectoryInfo(path);
            return FromRoot(directory);
        }

        public static RepositoryDirectoryMetadata FromRoot(DirectoryInfo directory)
        {
            var childDirectories = directory.GetDirectories();

            var foundGit = false;
            DirectoryInfo git = null;

            for (var childIndex = 0; childIndex < childDirectories.Length; childIndex++)
            {
                var childDirectory = childDirectories[childIndex];

                if (childDirectory.Name == ".git")
                {
                    foundGit = true;
                    git = childDirectory;
                    break;
                }
            }

            if (!foundGit)
            {
                return Empty();
            }

            DirectoryInfo assets;
            DirectoryInfo data;
            DirectoryInfo src;

            assets = new DirectoryInfo(Path.Combine(directory.FullName, "assets"));
            data = new DirectoryInfo(Path.Combine(directory.FullName,   "data"));
            src = new DirectoryInfo(Path.Combine(directory.FullName,    "src"));

            JObject package = null;

            var childFiles = directory.GetFiles("package.json");

            for (var childFileIndex = 0; childFileIndex < childFiles.Length; childFileIndex++)
            {
                var childFile = childFiles[childFileIndex];

                if (childFile.Name == "package.json")
                {
                    using (var fs = childFile.OpenRead())
                    using (var sr = new StreamReader(fs))
                    {
                        var text = sr.ReadToEnd();

                        package = JObject.Parse(text);
                        break;
                    }
                }
            }

            return new RepositoryDirectoryMetadata(directory, git, assets, data, src, package);
        }

        public static RepositoryDirectoryMetadata Empty()
        {
            return new();
        }
    }
}
