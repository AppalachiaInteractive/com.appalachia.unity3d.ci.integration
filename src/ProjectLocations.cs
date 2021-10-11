using System.Collections.Generic;
using System.IO;
using Appalachia.CI.Integration.Extensions;
using Appalachia.CI.Integration.Repositories;
using UnityEngine;

namespace Appalachia.CI.Integration
{
    public static class ProjectLocations
    {
        private static Dictionary<string, RepositoryDirectoryMetadata> _assetRepoLookup;
        private static Dictionary<DirectoryInfo, RepositoryDirectoryMetadata> _repoDirectoryLookup;

        private static string _assetDirectoryPath;
        private static DirectoryInfo _assetDirectoryInfo;
        private static Dictionary<string, string> _thirdPartyAssetDirectoryPath;
        private static Dictionary<string, DirectoryInfo> _thirdPartyAssetDirectoryInfo;
        private static string _projectDirectoryPath;
        private static DirectoryInfo _projectDirectoryInfo;

        public static RepositoryDirectoryMetadata GetAssetRepository(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return null;
            }

            if (_assetRepoLookup == null)
            {
                _assetRepoLookup = new Dictionary<string, RepositoryDirectoryMetadata>();
            }

            if (_assetRepoLookup.ContainsKey(assetPath))
            {
                return _assetRepoLookup[assetPath];
            }

            var directoryInfo = new DirectoryInfo(assetPath);

            while (!directoryInfo.HasSiblingDirectory(".git", out _))
            {
                directoryInfo = directoryInfo.Parent;

                if (directoryInfo == null)
                {
                    return null;
                }
            }

            if (_repoDirectoryLookup == null)
            {
                _repoDirectoryLookup = new Dictionary<DirectoryInfo, RepositoryDirectoryMetadata>();
            }

            var rootDirectory = directoryInfo.Parent;

            if (_repoDirectoryLookup.ContainsKey(rootDirectory))
            {
                return _repoDirectoryLookup[rootDirectory];
            }

            var repo = RepositoryDirectoryMetadata.FromRoot(rootDirectory);

            _assetRepoLookup.Add(assetPath, repo);
            _repoDirectoryLookup.Add(rootDirectory, repo);

            return repo;
        }

        public static string GetAssetsDirectoryPath()
        {
            if (_assetDirectoryPath != null)
            {
                return _assetDirectoryPath;
            }

            _assetDirectoryPath = Application.dataPath.CleanFullPath();
            return _assetDirectoryPath;
        }

        public static DirectoryInfo GetAssetsDirectoryInfo()
        {
            if (_assetDirectoryInfo != null)
            {
                return _assetDirectoryInfo;
            }

            _assetDirectoryInfo = new DirectoryInfo(GetAssetsDirectoryPath());
            return _assetDirectoryInfo;
        }

        public static string GetThirdPartyAssetsDirectoryPath(string partyName)
        {
            if (_thirdPartyAssetDirectoryPath == null)
            {
                _thirdPartyAssetDirectoryPath = new Dictionary<string, string>();
            }
            
            if (_thirdPartyAssetDirectoryPath.ContainsKey(partyName))
            {
                return _thirdPartyAssetDirectoryPath[partyName];
            }

            var basePath = GetAssetsDirectoryPath();
            var thirdPartyPath = Path.Combine(basePath, "Third-Party", partyName).CleanFullPath();
            var thirdPartyInfo = new DirectoryInfo(thirdPartyPath);
            if (!thirdPartyInfo.Exists)
            {
                thirdPartyInfo.Create();
            }
            
            _thirdPartyAssetDirectoryPath.Add(partyName, thirdPartyPath);
            
            return thirdPartyPath;
        }

        public static DirectoryInfo GetThirdPartyAssetsDirectoryInfo(string partyName)
        {
            if (_thirdPartyAssetDirectoryInfo == null)
            {
                _thirdPartyAssetDirectoryInfo = new Dictionary<string, DirectoryInfo>();
            }
            
            if (_thirdPartyAssetDirectoryInfo.ContainsKey(partyName))
            {
                return _thirdPartyAssetDirectoryInfo[partyName];
            }

            var thirdParty = GetThirdPartyAssetsDirectoryPath(partyName);

            var thirdPartyInfo = new DirectoryInfo(thirdParty);
            _thirdPartyAssetDirectoryInfo.Add(partyName, thirdPartyInfo);

            if (!thirdPartyInfo.Exists)
            {
                thirdPartyInfo.Create();
            }
            
            return thirdPartyInfo;
        }

        public static string GetProjectDirectoryPath()
        {
            if (_projectDirectoryPath != null)
            {
                return _projectDirectoryPath;
            }

            _projectDirectoryPath = GetAssetsDirectoryPath().Replace("/Assets", string.Empty);
            return _projectDirectoryPath;
        }

        public static DirectoryInfo GetProjectDirectoryInfo()
        {
            if (_projectDirectoryInfo != null)
            {
                return _projectDirectoryInfo;
            }

            _projectDirectoryInfo = new DirectoryInfo(GetProjectDirectoryPath());
            return _projectDirectoryInfo;
        }


        public static bool HasSiblingDirectory(
            this DirectoryInfo thisdir,
            string name,
            out DirectoryInfo siblingDirectory)
        {
            foreach (var sibling in thisdir.Parent.GetDirectories())
            {
                if (sibling.Name == name)
                {
                    siblingDirectory = sibling;
                    return true;
                }
            }

            siblingDirectory = null;
            return false;
        }

        public static bool HasSubDirectory(this DirectoryInfo parentDir, string name)
        {
            foreach (var child in parentDir.GetDirectories())
            {
                if (child.Name == name)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
