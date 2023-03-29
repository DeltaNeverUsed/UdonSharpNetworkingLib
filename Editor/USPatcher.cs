using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CSharp;
using UdonSharp.Compiler;
using UnityEditor;
using UnityEngine;

namespace USPPNet
{
    [InitializeOnLoad]
    public static class USPatcher
    {
        private static string part1 = @"
    public static class PublicProgramSource
    {
        public delegate string ProgramSourceDelegate(string prog); 
        public static event ProgramSourceDelegate ProgramSourceEvent;

        public static string Parse(string prog)
        {
            return ProgramSourceEvent?.Invoke(prog);
        }
    }";

        private static string part2 = @"
    programSource = PublicProgramSource.Parse(programSource);";

        static USPatcher()
        {

            var path = Application.dataPath.Replace("/Assets",
                "/Packages/com.vrchat.udonsharp/Editor/Compiler/CompilationContext.cs");

            if (!File.Exists(path))
            {
                Debug.LogError("CompilationContext.cs Does not exist");
                return;
            }

            var compContextSource = File.ReadAllText(path);

            var needToPatch = !compContextSource.Contains("public static class PublicProgramSource");
            if (needToPatch)
            {
                Debug.Log("Patching UdonSharp");

                // Part one of patch
                var patchP1Index = compContextSource.IndexOf("using SyntaxTree", StringComparison.Ordinal);
                patchP1Index = compContextSource.IndexOf("\n", patchP1Index, StringComparison.Ordinal);

                compContextSource = compContextSource.Insert(patchP1Index, "\n" + part1);

                // Part two of patch
                var patchP2Index = compContextSource.IndexOf(
                    "string programSource = UdonSharpUtils.ReadFileTextSync(currentSource);", StringComparison.Ordinal);
                patchP2Index = compContextSource.IndexOf("\n", patchP2Index, StringComparison.Ordinal);

                compContextSource = compContextSource.Insert(patchP2Index, "\n" + part2);


                compContextSource = compContextSource.Replace("\r\n", "\n").Replace("\n", "\r\n");
                File.WriteAllText(path, compContextSource);
            }
            else
            {
                Debug.Log("UdonSharp Already Patched, Subbing");

                // Define the code string to compile
                string code = @"
                    using UdonSharp.Compiler;
                    using USPPNet;
                    public class MyDynamicClass 
                    {
                        public void MyDynamicMethod()
                        {
                            PublicProgramSource.ProgramSourceEvent -= PreProcessor.Parse;
                            PublicProgramSource.ProgramSourceEvent += PreProcessor.Parse;
                        }
                    }
                ";

                // Create a CSharpCodeProvider instance
                CSharpCodeProvider provider = new CSharpCodeProvider();

                // Set up the compiler parameters
                CompilerParameters parameters = new CompilerParameters();
                parameters.GenerateExecutable = false;
                parameters.GenerateInMemory = true;
                parameters.ReferencedAssemblies.Add(Assembly.GetAssembly(typeof(UdonSharpCompilerV1)).Location);
                parameters.ReferencedAssemblies.Add(Assembly.GetAssembly(typeof(PreProcessor)).Location);
                

                // Compile the code string into a .NET assembly
                CompilerResults results = provider.CompileAssemblyFromSource(parameters, code);

                // Check for errors during compilation
                if (results.Errors.Count > 0)
                {
                    Debug.LogError("Compilation failed with errors:");
                    foreach (CompilerError error in results.Errors)
                    {
                        Debug.LogError("\t" + error.ErrorText);
                    }
                }
                else
                {
                    // Get the type of the compiled class
                    Type dynamicType = results.CompiledAssembly.GetType("MyDynamicClass");

                    // Create an instance of the dynamic class
                    object instance = Activator.CreateInstance(dynamicType);

                    // Get the method to invoke
                    MethodInfo dynamicMethod = dynamicType.GetMethod("MyDynamicMethod");

                    // Invoke the dynamic method
                    dynamicMethod.Invoke(instance, null);
                }
            }
        }
    }
}
