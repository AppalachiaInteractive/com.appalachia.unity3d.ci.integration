using System.Collections.Generic;
using Appalachia.CI.Integration.FileSystem;
using Unity.Profiling;

namespace Appalachia.CI.Integration.Extensions
{
    public static class StringExtensions
    {
        private const string _PRF_PFX = nameof(StringExtensions) + ".";
        private static Dictionary<string, string> _relativePathLookup;
        private static Dictionary<string, string> _absolutePathLookup;

        private static readonly ProfilerMarker _PRF_CleanFullPath = new ProfilerMarker(_PRF_PFX + nameof(CleanFullPath));
        public static string CleanFullPath(this string path)
        {
            using (_PRF_CleanFullPath.Auto())
            {
                return path.Replace("\\", "/")
                           .Replace("//", "/")
                           .Replace("//", "/")
                           .Trim('/')
                           .Trim(' ')
                           .Trim('.')
                           .Trim();
            }
        }

        private static readonly ProfilerMarker _PRF_ToAbsolutePath = new ProfilerMarker(_PRF_PFX + nameof(ToAbsolutePath));
        public static string ToAbsolutePath(this string relativePath)
        {
            using (_PRF_ToAbsolutePath.Auto())
            {
                if (_absolutePathLookup == null)
                {
                    _absolutePathLookup = new Dictionary<string, string>();
                }

                if (_absolutePathLookup.ContainsKey(relativePath))
                {
                    return _absolutePathLookup[relativePath];
                }

                var cleanRelativePath = relativePath.CleanFullPath();

                var basePath = ProjectLocations.GetProjectDirectoryPath();

                var firstSubfolder = cleanRelativePath.IndexOf('/');
                var relativePathSubstring = cleanRelativePath.Substring(firstSubfolder + 1);

                var result = AppaPath.Combine(basePath, relativePathSubstring).Trim('.', '/', '\\', ' ');

                _relativePathLookup.Add(relativePath, result);

                return result;
            }
        }


        private static readonly ProfilerMarker _PRF_ToRelativePath = new ProfilerMarker(_PRF_PFX + nameof(ToRelativePath));
        public static string ToRelativePath(this string absolutePath)
        {
            using (_PRF_ToRelativePath.Auto())
            {
                if (_relativePathLookup == null)
                {
                    _relativePathLookup = new Dictionary<string, string>();
                }

                if (_relativePathLookup.ContainsKey(absolutePath))
                {
                    return _relativePathLookup[absolutePath];
                }

                var cleanAbsolutePath = absolutePath.CleanFullPath();

                var basePath = ProjectLocations.GetProjectDirectoryPath();

                var result = cleanAbsolutePath.Replace(basePath, string.Empty)
                                              .Trim('.', '/', '\\', ' ');

                _relativePathLookup.Add(absolutePath, result);

                return result;
            }
        }
    }
}
