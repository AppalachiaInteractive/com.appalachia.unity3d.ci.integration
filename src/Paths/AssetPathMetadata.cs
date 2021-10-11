using System;
using System.IO;
using Appalachia.CI.Integration.Extensions;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Appalachia.CI.Integration.Paths
{
    [Serializable]
    public class AssetPathMetadata
    {
        public string name;
        public bool isDirectory;
        public bool doesExist;
        public string absolutePath;
        public string relativePath;
        public Object asset;
        public Object parentDirectory;
        public string parentDirectoryRelative;
        public AssetPathMetadataType pathType;
        public FileSystemInfo assetInfo;
        public DirectoryInfo parentDirectoryInfo;

        public AssetPathMetadata(
            string rp,
            bool isDirectory,
            AssetPathMetadataType pathType = AssetPathMetadataType.None)
        {
            this.isDirectory = isDirectory;

            if (this.isDirectory)
            {
                var ai = new DirectoryInfo(rp);
                parentDirectoryInfo = ai.Parent;
                assetInfo = ai;
            }
            else
            {
                var ai = new FileInfo(rp);
                parentDirectoryInfo = ai.Directory;
                assetInfo = ai;
            }

            absolutePath = assetInfo.FullName;
            name = Path.GetFileName(absolutePath);

            doesExist = this.isDirectory
                ? Directory.Exists(absolutePath)
                : File.Exists(absolutePath);

            var parentFullPath = parentDirectoryInfo.FullName;
            var cleanParentFullPath = parentFullPath.CleanFullPath();

            var cleanDataPath = ProjectLocations.GetAssetsDirectoryPath().CleanFullPath();
            var cleanProjectPath = cleanDataPath.Replace("/Assets", string.Empty);

            parentDirectoryRelative = cleanParentFullPath.Replace(cleanProjectPath, string.Empty);

            parentDirectoryRelative = parentDirectoryRelative.CleanFullPath();

            if (AssetDatabase.IsValidFolder(parentDirectoryRelative))
            {
                var type = AssetDatabase.GetMainAssetTypeAtPath(parentDirectoryRelative);
                parentDirectory = AssetDatabase.LoadAssetAtPath(parentDirectoryRelative, type);
            }

            relativePath = absolutePath.CleanFullPath();

            if (relativePath.Contains(cleanProjectPath))
            {
                relativePath = relativePath.Replace(cleanProjectPath, string.Empty);
            }

            relativePath = relativePath.CleanFullPath();

            if (parentDirectory != null)
            {
                var type = AssetDatabase.GetMainAssetTypeAtPath(relativePath);
                asset = AssetDatabase.LoadAssetAtPath(relativePath, type);
            }

            if (pathType == AssetPathMetadataType.None)
            {
                if (relativePath.StartsWith("Packages"))
                {
                    this.pathType = AssetPathMetadataType.PackageResource;
                }
                else if (relativePath.StartsWith("Library"))
                {
                    this.pathType = AssetPathMetadataType.LibraryResource;
                }
            }
            else
            {
                this.pathType = pathType;
            }
        }

        public static AssetPathMetadata ForScript(MonoScript script)
        {
            if (script == null)
            {
                return null;
            }

            var path = AssetDatabase.GetAssetPath(script);

            var result = new AssetPathMetadata(path, false, AssetPathMetadataType.Script);
            return result;
        }

        public void CreateDirectoryStructure()
        {
            var di = new DirectoryInfo(absolutePath);

            if (di.Exists)
            {
                doesExist = true;
                return;
            }

            di.Create();

            AssetDatabase.ImportAsset(relativePath);
            doesExist = true;
        }

        public string[] GetFileLines()
        {
            return File.ReadAllLines(absolutePath);
        }

        public void WriteFileLines(string[] lines)
        {
            File.WriteAllLines(absolutePath, lines);
        }
    }
}
