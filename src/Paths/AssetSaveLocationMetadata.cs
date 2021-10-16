using System;

namespace Appalachia.CI.Integration.Paths
{
    [Serializable]
    public class AssetSaveLocationMetadata
    {
        public AssetPathMetadata typeMetadata;
        public AssetPathMetadata saveLocationMetadata;
    }
}
