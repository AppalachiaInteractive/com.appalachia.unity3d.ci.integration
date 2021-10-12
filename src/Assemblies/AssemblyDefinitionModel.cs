using System;

namespace Appalachia.CI.Integration.Assemblies
{
    [Serializable]
    public class AssemblyDefinitionModel
    {
        public const string GUID_PREFIX = "GUID:";
        
        public string name;
        public string rootNamespace;
        public string[] references;
        public string[] includePlatforms;
        public string[] excludePlatforms;
        public bool allowUnsafeCode;
        public bool overrideReferences;
        public string[] precompiledReferences;
        public bool autoReferenced;
        public string[] defineConstraints;
        public AssemblyVersionDefineModel[] versionDefines;
        public bool noEngineReferences;
        public string[] optionalUnityReferences;

        public void CheckBeforeWrite()
        {
            name = string.IsNullOrWhiteSpace(name) ? string.Empty : name;
            rootNamespace = string.IsNullOrWhiteSpace(rootNamespace) ? string.Empty : rootNamespace;

            references ??= new string[0];
            includePlatforms ??= new string[0];
            excludePlatforms ??= new string[0];
            precompiledReferences ??= new string[0];
            defineConstraints ??= new string[0];
            optionalUnityReferences ??= new string[0];

            versionDefines ??= new AssemblyVersionDefineModel[0];
        }
    }
}