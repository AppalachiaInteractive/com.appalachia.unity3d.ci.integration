using System;
using System.Collections.Generic;
using System.Linq;
using Appalachia.CI.Integration.Assemblies;
using Appalachia.CI.Integration.FileSystem;
using Newtonsoft.Json.Linq;

namespace Appalachia.CI.Integration.Repositories
{
    [Serializable]
    public class RepositoryDirectoryMetadata : IEquatable<RepositoryDirectoryMetadata>
    {
        private const string GIT = ".git";
        private const string DATA = "data";
        private const string SRC = "src";
        private const string ASSET = "asset";
        private const string CONFIG = "config";
        private const string VERSION = "version";
        private const string URL = "url";
        private const string PACKAGEJSON = "package.json";
        private static Dictionary<string, RepositoryDirectoryMetadata> _instances;

        public List<AssemblyDefinitionMetadata> assemblies;

        private AppaDirectoryInfo _assetsDirectory;

        private AppaDirectoryInfo _dataDirectory;

        private AppaDirectoryInfo _gitDirectory;
        private string _packageVersion;

        private string _repoName;

        private AppaDirectoryInfo _srcDirectory;
        public JObject packageJson;
        public AppaDirectoryInfo root;

        private RepositoryDirectoryMetadata()
        {
            assemblies = new List<AssemblyDefinitionMetadata>();
        }

        private RepositoryDirectoryMetadata(
            AppaDirectoryInfo root,
            AppaDirectoryInfo gitDirectory,
            AppaDirectoryInfo assetsDirectory,
            AppaDirectoryInfo dataDirectory,
            AppaDirectoryInfo srcDirectory,
            JObject packageJson)
        {
            this.root = root;
            this.gitDirectory = gitDirectory;
            this.assetsDirectory = assetsDirectory;
            this.dataDirectory = dataDirectory;
            this.srcDirectory = srcDirectory;
            this.packageJson = packageJson;
            assemblies = new List<AssemblyDefinitionMetadata>();
        }

        public AppaDirectoryInfo gitDirectory
        {
            get
            {
                if (_gitDirectory == null)
                {
                    if (root == null)
                    {
                        return null;
                    }

                    _gitDirectory = new AppaDirectoryInfo(AppaPath.Combine(root.FullPath, GIT));
                }

                return _gitDirectory;
            }
            set => _gitDirectory = value;
        }

        public AppaDirectoryInfo assetsDirectory
        {
            get
            {
                if (_assetsDirectory == null)
                {
                    if (root == null)
                    {
                        return null;
                    }

                    _assetsDirectory =
                        new AppaDirectoryInfo(AppaPath.Combine(root.FullPath, ASSET));
                }

                return _assetsDirectory;
            }
            set => _assetsDirectory = value;
        }

        public AppaDirectoryInfo dataDirectory
        {
            get
            {
                if (_dataDirectory == null)
                {
                    if (root == null)
                    {
                        return null;
                    }

                    _dataDirectory = new AppaDirectoryInfo(AppaPath.Combine(root.FullPath, DATA));
                }

                return _dataDirectory;
            }
            set => _dataDirectory = value;
        }

        public AppaDirectoryInfo srcDirectory
        {
            get
            {
                if (_srcDirectory == null)
                {
                    if (root == null)
                    {
                        return null;
                    }

                    _srcDirectory = new AppaDirectoryInfo(AppaPath.Combine(root.FullPath, SRC));
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
                    var repoConfig = repoFiles.First(f => f.Name == CONFIG);

                    var repoConfigStrings = new List<string>();

                    repoConfigStrings.AddRange(repoConfig.ReadAllLines());

                    foreach (var repoConfigString in repoConfigStrings)
                    {
                        //url = https://github.com/AppalachiaInteractive/com.appalachia.unity3d.audio.git
                        if (!repoConfigString.Contains(URL))
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
                    _packageVersion = packageJson[VERSION]?.ToString();
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

            return Equals(root?.FullPath,         other.root?.FullPath) &&
                   Equals(gitDirectory?.FullPath, other.gitDirectory?.FullPath) &&
                   Equals(srcDirectory?.FullPath, other.srcDirectory?.FullPath);
        }

        public override string ToString()
        {
            if (!HasPackage)
            {
                return root.FullPath;
            }

            return $"{root.FullPath}: {PackageVersion}";
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
                var hashCode = root != null ? root.FullPath.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^
                           (gitDirectory != null ? gitDirectory.FullPath.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^
                           (srcDirectory != null ? srcDirectory.FullPath.GetHashCode() : 0);
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

        public static RepositoryDirectoryMetadata Create(
            AppaDirectoryInfo root,
            AppaDirectoryInfo gitDirectory,
            AppaDirectoryInfo assetsDirectory,
            AppaDirectoryInfo dataDirectory,
            AppaDirectoryInfo srcDirectory,
            JObject packageJson)
        {
            if (_instances == null)
            {
                _instances = new Dictionary<string, RepositoryDirectoryMetadata>();
            }

            if (_instances.ContainsKey(root.FullPath))
            {
                return _instances[root.FullPath];
            }

            var newInstance = new RepositoryDirectoryMetadata(
                root,
                gitDirectory,
                assetsDirectory,
                dataDirectory,
                srcDirectory,
                packageJson
            );

            _instances.Add(root.FullPath, newInstance);

            return newInstance;
        }

        public static RepositoryDirectoryMetadata FromRootPath(string path)
        {
            var directory = new AppaDirectoryInfo(path);
            return FromRoot(directory);
        }

        public static RepositoryDirectoryMetadata FromRoot(AppaDirectoryInfo directory)
        {
            if (_instances == null)
            {
                _instances = new Dictionary<string, RepositoryDirectoryMetadata>();
            }

            if (_instances.ContainsKey(directory.FullPath))
            {
                return _instances[directory.FullPath];
            }

            var childDirectories = directory.GetDirectories();

            var foundGit = false;
            AppaDirectoryInfo git = null;

            for (var childIndex = 0; childIndex < childDirectories.Length; childIndex++)
            {
                var childDirectory = childDirectories[childIndex];

                if (childDirectory.Name == GIT)
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

            AppaDirectoryInfo asset;
            AppaDirectoryInfo data;
            AppaDirectoryInfo src;

            asset = new AppaDirectoryInfo(AppaPath.Combine(directory.FullPath, ASSET));
            data = new AppaDirectoryInfo(AppaPath.Combine(directory.FullPath,  DATA));
            src = new AppaDirectoryInfo(AppaPath.Combine(directory.FullPath,   SRC));

            JObject package = null;

            var childFiles = directory.GetFiles(PACKAGEJSON);

            for (var childFileIndex = 0; childFileIndex < childFiles.Length; childFileIndex++)
            {
                var childFile = childFiles[childFileIndex];

                if (childFile.Name.ToLowerInvariant() == PACKAGEJSON)
                {
                    var text = AppaFile.ReadAllText(childFile.FullPath);
                    package = JObject.Parse(text);
                    break;
                }
            }

            var result = Create(directory, git, asset, data, src, package);

            if (result.assemblies == null)
            {
                result.assemblies = new List<AssemblyDefinitionMetadata>();
            }

            return result;
        }

        public static RepositoryDirectoryMetadata Empty()
        {
            return new();
        }
    }
}
