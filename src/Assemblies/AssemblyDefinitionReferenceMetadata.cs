using System;
using System.Collections.Generic;

namespace Appalachia.CI.Integration.Assemblies
{
    [Serializable]
    public class AssemblyDefinitionReferenceMetadata : IComparable<AssemblyDefinitionReferenceMetadata>, IComparable
    {
        public string guid;
        public AssemblyDefinitionMetadata assembly;
        public bool outOfSorts;

        public bool IsGuidReference => guid.StartsWith(AssemblyDefinitionModel.GUID_PREFIX);
        
        public int CompareTo(AssemblyDefinitionReferenceMetadata other)
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }

            if (ReferenceEquals(null, other))
            {
                return 1;
            }
            
            return Comparer<AssemblyDefinitionMetadata>.Default.Compare(assembly, other.assembly);
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

            return obj is AssemblyDefinitionReferenceMetadata other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(AssemblyDefinitionReferenceMetadata)}");
        }

        public static bool operator <(AssemblyDefinitionReferenceMetadata left, AssemblyDefinitionReferenceMetadata right)
        {
            return Comparer<AssemblyDefinitionReferenceMetadata>.Default.Compare(left, right) < 0;
        }

        public static bool operator >(AssemblyDefinitionReferenceMetadata left, AssemblyDefinitionReferenceMetadata right)
        {
            return Comparer<AssemblyDefinitionReferenceMetadata>.Default.Compare(left, right) > 0;
        }

        public static bool operator <=(AssemblyDefinitionReferenceMetadata left, AssemblyDefinitionReferenceMetadata right)
        {
            return Comparer<AssemblyDefinitionReferenceMetadata>.Default.Compare(left, right) <= 0;
        }

        public static bool operator >=(AssemblyDefinitionReferenceMetadata left, AssemblyDefinitionReferenceMetadata right)
        {
            return Comparer<AssemblyDefinitionReferenceMetadata>.Default.Compare(left, right) >= 0;
        }
    }
}
