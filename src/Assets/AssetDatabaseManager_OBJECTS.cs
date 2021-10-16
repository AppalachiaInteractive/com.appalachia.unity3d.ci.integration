using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Profiling;

namespace Appalachia.CI.Integration.Assets
{
    public static partial class AssetDatabaseManager
    {
        
        private static Dictionary<string, string[]> _guidsByTypeName;
        private static Dictionary<string, string[]> _pathsByTypeName;
        private static Dictionary<string, Type[]> _typesByTypeName;

        private const string _PRF_PFX = nameof(AssetDatabaseManager) + ".";
        
        private static readonly ProfilerMarker _PRF_GetAllMonoScripts = new ProfilerMarker(_PRF_PFX + nameof(GetAllMonoScripts));

        private static readonly ProfilerMarker _PRF_InitializeTypeData = new ProfilerMarker(_PRF_PFX + nameof(InitializeTypeData));
        
        private static void InitializeTypeData(string typeName)
        {
            using (_PRF_InitializeTypeData.Auto())
            {
                _typesByTypeName ??= new Dictionary<string, Type[]>();
                _pathsByTypeName ??= new Dictionary<string, string[]>();
                _guidsByTypeName ??= new Dictionary<string, string[]>();

                if (!_typesByTypeName.ContainsKey(typeName))
                {
                    var guids = FindAssets($"t: {typeName}");

                    var sglength = guids.Length;
                
                    var paths = new string[sglength];
                    var types = new Type[sglength];

                    for (var i = 0; i < sglength; i++)
                    {
                        var guid = guids[i];
                        var path = GUIDToAssetPath(guid);
                        var type = GetMainAssetTypeAtPath(path);
                    
                        paths[i] = path;
                        types[i] = type;
                    }
                    
                    _typesByTypeName.Add(typeName, types);
                    _pathsByTypeName.Add(typeName, paths);
                    _guidsByTypeName.Add(typeName, guids);
                }
            }
        }
        
  
        private static readonly ProfilerMarker _PRF_GetAllAssetGuids = new ProfilerMarker(_PRF_PFX + nameof(GetAllAssetGuids));
        public static string[] GetAllAssetGuids(Type type = null)
        {
            using (_PRF_GetAllAssetGuids.Auto())
            {
                var typeName = type?.Name ?? nameof(UnityEngine.Object);
                InitializeTypeData(typeName);

                return _guidsByTypeName[typeName];
            }
        }

        private static readonly ProfilerMarker _PRF_GetAllAssetPaths = new ProfilerMarker(_PRF_PFX + nameof(GetAllAssetPaths));
        public static string[] GetAllAssetPaths(Type type)
        {
            using (_PRF_GetAllAssetPaths.Auto())
            {
                var typeName = type?.Name ?? nameof(UnityEngine.Object);
                InitializeTypeData(typeName);

                return _pathsByTypeName[typeName];
            }
        }

        private static readonly ProfilerMarker _PRF_GetProjectAssetPaths = new ProfilerMarker(_PRF_PFX + nameof(GetProjectAssetPaths));
        public static string[] GetProjectAssetPaths(Type type = null)
        {
            using (_PRF_GetProjectAssetPaths.Auto())
            {
                var assetPaths = GetAllAssetPaths(type);

                var filteredAssetPaths = assetPaths
                                        .Where(
                                             p => p.StartsWith("Assets/") &&
                                                  p.Contains("Appalachia")
                                         )
                                        .ToArray();

                Array.Sort(filteredAssetPaths);

                return filteredAssetPaths;
            }
        }
        
        

        private static readonly ProfilerMarker _PRF_GetAllAssetTypes = new ProfilerMarker(_PRF_PFX + nameof(GetAllAssetTypes));
        public static Type[] GetAllAssetTypes(Type type = null)
        {
            using (_PRF_GetAllAssetTypes.Auto())
            {
                var typeName = type?.Name ?? nameof(UnityEngine.Object);
                InitializeTypeData(typeName);

                return _typesByTypeName[typeName];
            }
        }

        
        
    }
}
