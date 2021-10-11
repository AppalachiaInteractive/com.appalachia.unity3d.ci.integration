using System.Collections.Generic;
using System.IO;

namespace Appalachia.CI.Integration.Extensions
{
    public static class StringExtensions
    {
        private static Dictionary<string, string> _relativePathLookup;
        private static Dictionary<string, string> _absolutePathLookup;

        public static string CleanFullPath(this string path)
        {
            return path.Replace("\\", "/")
                       .Replace("//", "/")
                       .Replace("//", "/")
                       .Trim('/')
                       .Trim(' ')
                       .Trim('.')
                       .Trim();
        }

        public static string ToAbsolutePath(this string relativePath)
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

            var result = Path.Combine(basePath, relativePathSubstring).Trim('.', '/', '\\', ' ');

            _relativePathLookup.Add(relativePath, result);

            return result;
        }

        public static string ToRelativePath(this string absolutePath)
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
