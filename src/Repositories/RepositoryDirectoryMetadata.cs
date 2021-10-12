using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Appalachia.CI.Integration.Assemblies;
using Appalachia.CI.Integration.Extensions;
using Newtonsoft.Json.Linq;

namespace Appalachia.CI.Integration.Repositories
{
    [Serializable]
    public class RepositoryDirectoryMetadata : IEquatable<RepositoryDirectoryMetadata>
    {
        private static Dictionary<string, RepositoryDirectoryMetadata> _instances;
        
        private const string GIT = ".git";
        private const string DATA = "data";
        private const string SRC = "src";
        private const string ASSET = "asset";
        private const string CONFIG = ".config";
        private const string VERSION = "version";
        private const string URL = "url";
        private const string PACKAGEJSON = "package.json";

        public List<AssemblyDefinitionMetadata> assemblies;

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
            assemblies = new List<AssemblyDefinitionMetadata>();
        }

        private RepositoryDirectoryMetadata(
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
            assemblies = new List<AssemblyDefinitionMetadata>();
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

                    _gitDirectory = new DirectoryInfo(Path.Combine(root.FullName, GIT));
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

                    _assetsDirectory = new DirectoryInfo(Path.Combine(root.FullName, ASSET));
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

                    _dataDirectory = new DirectoryInfo(Path.Combine(root.FullName, DATA));
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

                    _srcDirectory = new DirectoryInfo(Path.Combine(root.FullName, SRC));
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
       

        public static RepositoryDirectoryMetadata Create(
            DirectoryInfo root,
            DirectoryInfo gitDirectory,
            DirectoryInfo assetsDirectory,
            DirectoryInfo dataDirectory,
            DirectoryInfo srcDirectory,
            JObject packageJson)
        {
            if (_instances == null)
            {
                _instances = new Dictionary<string, RepositoryDirectoryMetadata>();
            }

            if (_instances.ContainsKey(root.FullName))
            {
                return _instances[root.FullName];
            }

            var newInstance = new RepositoryDirectoryMetadata(
                root,
                gitDirectory,
                assetsDirectory,
                dataDirectory,
                srcDirectory,
                packageJson
            );

            _instances.Add(root.FullName, newInstance);

            return newInstance;
        }

        
        public static RepositoryDirectoryMetadata FromRootPath(string path)
        {
            var directory = new DirectoryInfo(path);
            return FromRoot(directory);
        }

        public static RepositoryDirectoryMetadata FromRoot(DirectoryInfo directory)
        {
            if (_instances == null)
            {
                _instances = new Dictionary<string, RepositoryDirectoryMetadata>();
            }

            if (_instances.ContainsKey(directory.FullName))
            {
                return _instances[directory.FullName];
            }
            
            var childDirectories = directory.GetDirectories();

            var foundGit = false;
            DirectoryInfo git = null;

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

            DirectoryInfo asset;
            DirectoryInfo data;
            DirectoryInfo src;

            asset = new DirectoryInfo(Path.Combine(directory.FullName, ASSET));
            data = new DirectoryInfo(Path.Combine(directory.FullName,  DATA));
            src = new DirectoryInfo(Path.Combine(directory.FullName,   SRC));

            JObject package = null;

            var childFiles = directory.GetFiles(PACKAGEJSON);

            for (var childFileIndex = 0; childFileIndex < childFiles.Length; childFileIndex++)
            {
                var childFile = childFiles[childFileIndex];

                if (childFile.Name.ToLowerInvariant() == PACKAGEJSON)
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

            var result =  Create(directory, git, asset, data, src, package);

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
