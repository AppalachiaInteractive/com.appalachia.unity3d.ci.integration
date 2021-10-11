using System;
using System.Collections.Generic;
using System.IO;
using Appalachia.CI.Integration.Extensions;
using Appalachia.CI.Integration.Paths;
using UnityEditor;
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
        public DirectoryInfo directoryInfo;
        public HashSet<Object> lookup;

        public RepositorySubdirectory(RepositoryDirectoryMetadata repository, string relativePath)
        {
            this.relativePath = relativePath.CleanFullPath();
            var type = AssetDatabase.GetMainAssetTypeAtPath(this.relativePath);
            directory = AssetDatabase.LoadAssetAtPath(this.relativePath, type);
            directoryInfo = new DirectoryInfo(this.relativePath);
            this.repository = repository;
        }
    }
}
