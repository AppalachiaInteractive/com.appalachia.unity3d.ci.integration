using System;
using System.Collections.Generic;

namespace Appalachia.CI.Integration.Repositories
{
    [Serializable]
    public class RepositoryAssetSaveDirectories
    {
        public RepositoryDirectoryMetadata repository;
        public List<RepositorySubdirectory> locations;
    }
}
