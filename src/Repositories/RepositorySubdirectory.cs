using System;
using System.Collections.Generic;
using Appalachia.CI.Integration.Assets;
using Appalachia.CI.Integration.Extensions;
using Appalachia.CI.Integration.FileSystem;
using Appalachia.CI.Integration.Paths;
using Object = UnityEngine.Object;

namespace Appalachia.CI.Integration.Repositories
{
    [Serializable]
    public class RepositorySubdirectory
    {
        public bool isConventional;
        public string relativePath;
        public Object directory;
        public List<Object> instances;
        public RepositoryDirectoryMetadata repository;
        public AssetPathMetadata correctionPath;

        public bool showInstances;
        public AppaDirectoryInfo directoryInfo;
        public HashSet<Object> lookup;

        public RepositorySubdirectory(RepositoryDirectoryMetadata repository, string relativePath)
        {
            this.relativePath = relativePath.CleanFullPath();
            var type = AssetDatabaseManager.GetMainAssetTypeAtPath(this.relativePath);
            directory = AssetDatabaseManager.LoadAssetAtPath(this.relativePath, type);
            directoryInfo = new AppaDirectoryInfo(this.relativePath);
            this.repository = repository;
        }
    }
}
