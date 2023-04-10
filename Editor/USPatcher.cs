using HarmonyLib;
using UdonSharp.Compiler;
using UnityEditor;

namespace USPPNet
{
    public class UdonSharpReadFilePatch
    {
        public static void Postfix(string filePath, float timeoutSeconds, ref string __result)
        {
            if (__result == "")
                return;

            __result = PreProcessor.Parse(__result);
        }
    }

    [InitializeOnLoad]
    public static class TestPatch
    {
        static TestPatch() {
            var assembly = typeof(UdonSharpCompilerV1).Assembly;
            
            var ReadMethod = assembly.GetType("UdonSharp.UdonSharpUtils").GetMethod("ReadFileTextSync");
            var harmony = new Harmony("USPPs.DeltaNeverUsed.patch");
            harmony.Patch(ReadMethod, null, new HarmonyMethod(typeof(UdonSharpReadFilePatch), "Postfix"));
            //test
        }
    }
}

