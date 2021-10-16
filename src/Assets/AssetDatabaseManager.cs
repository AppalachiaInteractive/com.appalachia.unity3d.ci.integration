using UnityEditor;
using UnityEditor.Compilation;

namespace Appalachia.CI.Integration.Assets
{
    public static partial class AssetDatabaseManager
    {
        [MenuItem("Assets/Force Recompile C# Project")]
        public static void ForceRecompile()
        {
            CompilationPipeline.RequestScriptCompilation(RequestScriptCompilationOptions.None);
        }
    }
}
