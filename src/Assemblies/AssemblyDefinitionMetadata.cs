using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Appalachia.CI.Integration.Repositories;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditorInternal;

namespace Appalachia.CI.Integration.Assemblies
{
    [Serializable]
    public class AssemblyDefinitionMetadata : IComparable<AssemblyDefinitionMetadata>, IComparable
    {
        private static Dictionary<string, AssemblyDefinitionMetadata> _instances;
        
            
        private AssemblyDefinitionMetadata()
        {
        }

        
        private static StringBuilder _asmdefNameBuilder;
        private static string[] _partExclusions;

        public RepositoryDirectoryMetadata repository;
        
        public bool readOnly;
        public AssemblyDefinitionImporter importer;
        public AssemblyDefinitionAsset asset;
        public AssemblyDefinitionModel assetModel;
        public List<string> referenceStrings;
        public List<AssemblyDefinitionReferenceMetadata> references;
        public string path;
        public string guid;
        public string assembly_current;
        public string filename_current;
        public string root_namespace_current;
        public string assembly_ideal;
        public string filename_ideal;
        public string root_namespace_ideal;

        private bool? _referenceIssues;
        private bool? _shouldSortReferences;
        public bool? _doesUseGuidReferences;

        public bool IsPackage => path.StartsWith("Package");
        public bool IsLibrary => path.StartsWith("Library");
        public bool IsAsset => path.StartsWith("Asset");
        
        public bool DoesFileNameMatch => !readOnly && (filename_current == filename_ideal);
        public bool DoesAssemblyMatch => !readOnly && (assembly_current == assembly_ideal);

        public bool DoesNamespaceMatch =>
            !readOnly &&
            ((string.IsNullOrWhiteSpace(root_namespace_current) &&
              string.IsNullOrWhiteSpace(root_namespace_ideal)) ||
             (root_namespace_current == root_namespace_ideal));
        public bool DoAllNamesMatch => !readOnly && DoesFileNameMatch && DoesAssemblyMatch && DoesNamespaceMatch;

        public bool AnyIssues =>
            !DoAllNamesMatch ||
            ShouldSortReferences ||
            HasInvalidAssemblies ||
            !DoesUseGuidReferences;
        
        public bool DoesUseGuidReferences
        {
            get
            {
                if (!_doesUseGuidReferences.HasValue)
                {
                    if (referenceStrings.Count > 0)
                    {
                        _doesUseGuidReferences = referenceStrings[0].StartsWith(AssemblyDefinitionModel.GUID_PREFIX);
                    }
                    else
                    {
                        _doesUseGuidReferences = true;
                    }
                }

                return _doesUseGuidReferences.Value;
            }
        }

        public bool HasInvalidAssemblies
        {
            get
            {
                if (!_referenceIssues.HasValue)
                {
                    _referenceIssues = references?.Any(r => r.assembly == null) ?? false;
                }

                return _referenceIssues.Value;
            }
        }

        public bool ShouldSortReferences
        {
            get
            {
                if (!_shouldSortReferences.HasValue)
                {
                    if (references == null)
                    {
                        return false;
                    }
                    
                    for (var i = 0; i < references.Count - 1; i++)
                    {
                        var ref1 = references[i];
                        var ref2 = references[i + 1];

                        if (ref1 > ref2)
                        {
                            ref1.outOfSorts = true;
                            ref2.outOfSorts = true;
                            _shouldSortReferences = true;
                            return _shouldSortReferences.Value;
                        }
                    }

                    _shouldSortReferences = false;
                    return _shouldSortReferences.Value;
                }

                return _shouldSortReferences.Value;
            }
        }


        public int CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return 1;
            }

            if (ReferenceEquals(this, obj))
            {
                return 0;
            }

            return obj is AssemblyDefinitionMetadata other
                ? CompareTo(other)
                : throw new ArgumentException(
                    $"Object must be of type {nameof(AssemblyDefinitionMetadata)}"
                );
        }

        public int CompareTo(AssemblyDefinitionMetadata other)
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }

            if (ReferenceEquals(null, other))
            {
                return 1;
            }

            var assemblyCurrentComparison = string.Compare(
                assembly_current,
                other.assembly_current,
                StringComparison.Ordinal
            );
            if (assemblyCurrentComparison != 0)
            {
                return assemblyCurrentComparison;
            }

            return string.Compare(path, other.path, StringComparison.Ordinal);
        }

        public static AssemblyDefinitionMetadata CreateNew(string path)
        {
            if (_instances == null)
            {
                _instances = new Dictionary<string, AssemblyDefinitionMetadata>();
            }

            if (_instances.ContainsKey(path))
            {
                return _instances[path];
            }

            var newInstance = new AssemblyDefinitionMetadata();

            newInstance.Initialize(path);

            _instances.Add(path, newInstance);

            return newInstance;
        }
        
        public void Initialize(string assemblyDefinitionPath)
        {
            if (_partExclusions == null)
            {
                _partExclusions = new[] {"src", "dist", "asmdef", "Runtime", "Scripts"};
            }

            readOnly = !assemblyDefinitionPath.StartsWith("Assets");

            path = assemblyDefinitionPath;
            guid = AssemblyDefinitionModel.GUID_PREFIX + AssetDatabase.AssetPathToGUID(path);
            filename_current = Path.GetFileName(assemblyDefinitionPath);

            importer = (AssemblyDefinitionImporter) AssetImporter.GetAtPath(assemblyDefinitionPath);
            asset = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(assemblyDefinitionPath);
            repository = ProjectLocations.GetAssetRepository(path);
            references = new List<AssemblyDefinitionReferenceMetadata>();
            referenceStrings = new List<string>();

            if (_asmdefNameBuilder == null)
            {
                _asmdefNameBuilder = new StringBuilder();
            }

            _asmdefNameBuilder.Clear();

            var asmDefDirectory = Path.GetDirectoryName(assemblyDefinitionPath).Replace("\\", "/");

            var parts = asmDefDirectory.Split('/');

            for (var partIndex = 1; partIndex < parts.Length; partIndex++)
            {
                var part = parts[partIndex];
                var exclude = false;
                for (var exclusionIndex = 0;
                    exclusionIndex < _partExclusions.Length;
                    exclusionIndex++)
                {
                    var partExclusion = _partExclusions[exclusionIndex];
                    if (partExclusion.ToLowerInvariant() == part.ToLowerInvariant())
                    {
                        exclude = true;
                    }
                }

                if (!exclude)
                {
                    _asmdefNameBuilder.Append(part + '.');
                }
            }

            assembly_ideal = _asmdefNameBuilder.ToString().Trim('.');
            filename_ideal = assembly_ideal + ".asmdef";
            root_namespace_ideal = null;

            assetModel = JsonConvert.DeserializeObject<AssemblyDefinitionModel>(asset.text);

            assembly_current = assetModel.name;
            root_namespace_current = assetModel.rootNamespace;

            referenceStrings = assetModel.references?.ToList() ?? new List<string>();

        }
        
        

        public void UpdateNames(bool testFile, bool reimport)
        {
            if (DoAllNamesMatch)
            {
                return;
            }

            if (!DoesFileNameMatch)
            {
                AssetDatabase.RenameAsset(path, filename_ideal);
                return;
            }
            
            assetModel.name = assembly_ideal;
            assetModel.rootNamespace = root_namespace_ideal;

            SaveFile(testFile, reimport);
        }

        public void SortReferences(bool testFile, bool reimport)
        {
            if (!ShouldSortReferences)
            {
                return;
            }

            references.Sort();
            
            WriteReferences();

            SaveFile(testFile, reimport);
        }

        public void RemoveInvalidAssemblies(bool testFile, bool reimport)
        {
            if (!HasInvalidAssemblies)
            {
                return;
            }

            for (int i = references.Count - 1; i >= 0; i--)
            {
                var reference = references[i];

                if (reference.assembly == null)
                {
                    references.RemoveAt(i);
                }
            }
            
            WriteReferences();

            SaveFile(testFile, reimport);
        }

        public void ConvertToGuidReferences(List<AssemblyDefinitionMetadata> allAssemblies, bool testFile, bool reimport)
        {
            var changed = false;

            for (int i = 0; i < references.Count; i++)
            {
                var reference = references[i];

                if (reference.IsGuidReference)
                {
                    continue;
                }

                changed = true;

                if (reference.assembly == null)
                {
                    for (var index = 0; index < allAssemblies.Count; index++)
                    {
                        var assemblyToCheck = allAssemblies[index];

                        if (assemblyToCheck.assembly_current == reference.guid)
                        {
                            reference.assembly = assemblyToCheck;
                            break;
                        }
                    }
                }
                
                reference.guid = reference.assembly.guid;
                referenceStrings[i] = reference.assembly.guid;
            }
            
            WriteReferences();
            
            if (changed)
            {
                SaveFile(testFile, reimport);
            }
        }

        public void SetReferences(Dictionary<string, AssemblyDefinitionMetadata> lookup)
        {
            references = new List<AssemblyDefinitionReferenceMetadata>();

            foreach (var referenceString in referenceStrings)
            {
                AssemblyDefinitionReferenceMetadata reference;                
                
                if (!lookup.ContainsKey(referenceString))
                {
                    reference = new AssemblyDefinitionReferenceMetadata
                    {
                        guid = referenceString
                    };
                }
                else
                {
                    reference = new AssemblyDefinitionReferenceMetadata
                    {
                        assembly = lookup[referenceString], guid = referenceString
                    };
                }

                references.Add(reference);
            }
        }

        private void WriteReferences()
        {
            referenceStrings.Clear();
            
            for (int i = 0; i < references.Count; i++)
            {
                var reference = references[i];

                referenceStrings.Add(reference.guid);
            }

            assetModel.references = referenceStrings.ToArray();
        }
        
        public void SaveFile(bool testFile, bool reimport)
        {
            var settings = new JsonSerializerSettings {Formatting = Formatting.Indented};
            settings.NullValueHandling = NullValueHandling.Include;

            var outputPath = path;
            var fileMode = FileMode.Truncate;
            
            if (testFile)
            {
                outputPath += ".test";
                fileMode = FileMode.Create;
            }

            assetModel.CheckBeforeWrite();
            
            using (var fs = File.Open(outputPath, fileMode))
            using (var sw = new StreamWriter(fs))
            {
                var text = JsonConvert.SerializeObject(assetModel, settings);
                
                sw.Write(text);
            }

            EditorUtility.SetDirty(asset);

            if (reimport)
            {
                AssetDatabase.ImportAsset(outputPath);
            }
        }
        
        public static bool operator <(
            AssemblyDefinitionMetadata left,
            AssemblyDefinitionMetadata right)
        {
            return Comparer<AssemblyDefinitionMetadata>.Default.Compare(left, right) < 0;
        }

        public static bool operator >(
            AssemblyDefinitionMetadata left,
            AssemblyDefinitionMetadata right)
        {
            return Comparer<AssemblyDefinitionMetadata>.Default.Compare(left, right) > 0;
        }

        public static bool operator <=(
            AssemblyDefinitionMetadata left,
            AssemblyDefinitionMetadata right)
        {
            return Comparer<AssemblyDefinitionMetadata>.Default.Compare(left, right) <= 0;
        }

        public static bool operator >=(
            AssemblyDefinitionMetadata left,
            AssemblyDefinitionMetadata right)
        {
            return Comparer<AssemblyDefinitionMetadata>.Default.Compare(left, right) >= 0;
        }
    }
}
