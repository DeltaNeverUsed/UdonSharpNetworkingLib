using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using UdonSharp.Compiler;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace UdonSharpNetworkingLib {
    [InitializeOnLoad]
    public static class Injections {
        internal static Harmony _harmony;
        
        static Injections() {
            Patch();
        }

        private static void Unpatch() {
            _harmony.UnpatchAll("DeltaNeverUsed.UdonSharpNetworkingLib.patch");
        }

        private static void Patch() {
            _harmony ??= new Harmony("DeltaNeverUsed.UdonSharpNetworkingLib.patch");
            Unpatch();
            _harmony.PatchAll();

            var compileMethodParameters = new Type[] {
                ReflectionHelper.ByName("UdonSharp.Compiler.CompilationContext"),
                typeof(IReadOnlyDictionary<,>).MakeGenericType(typeof(string),
                    ReflectionHelper.ByName("UdonSharp.Compiler.UdonSharpCompilerV1/ProgramAssetInfo")),
                typeof(IEnumerable<string>),
                typeof(string[])
            };

            var compileMethod = typeof(UdonSharpCompilerV1).GetMethod("Compile",
                BindingFlags.NonPublic | BindingFlags.Static, null, compileMethodParameters, null);
            if (compileMethod != null) {
                var transpilerMethod = typeof(CompilePatch).GetMethod(nameof(CompilePatch.Transpiler),
                    BindingFlags.Static | BindingFlags.Public);
                _harmony.Patch(compileMethod, transpiler: new HarmonyMethod(transpilerMethod));
            }
        }

        public static void PrintError(object message) {
            Debug.LogError($"<color=red>{new StackFrame(1, true).GetMethod().Name}</color>: {message}");
        }
    }
}