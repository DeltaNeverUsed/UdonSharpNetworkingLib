#if UNITY_EDITOR && !COMPILER_UDONSHARP
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using USPPPatcher;
using UnityEngine;

namespace USPPNet {
    public class PreProcessorInstance {
        public Dictionary<string, string[]> functions;
    }

    public static class PreProcessorSnippits {
        #region Init

        public const string USPPNetInit = @"
        private object[] USPPNet_toBeSerialized = System.Array.Empty<object>();
        private ushort USPPNet_calls = 0;
        private byte USPPNet_updateIndexLast = 0;
        // USPPNet TEMP REPLACE MethodIndex
        [UdonSynced] private byte USPPNet_updateIndex = 0;
        [UdonSynced] private byte[] USPPNet_bytes = System.Array.Empty<byte>();

        
        private byte[] USPPNet_bytes_empty = System.Array.Empty<byte>();
        private object[] USPPNet_toBeSerialized_empty = System.Array.Empty<object>();

        public override void USPPNet_RPC(string method, params object[] args) {   
            if(USPPNet_toBeSerialized.Length == 0) {
                USPPNet_toBeSerialized = USPPNet_toBeSerialized.USPPNet_AppendArray((byte)0);
                USPPNet_updateIndexLast = USPPNet_updateIndex;
                USPPNet_updateIndex = (byte)((USPPNet_updateIndex + 1) % 254);
            }
            
            var callIndex = USPPNet_methodNames.USPPNet_IndexOf(method);
            if (callIndex == -1) {
                return;
            }

            USPPNet_calls++;
            USPPNet_toBeSerialized = USPPNet_toBeSerialized.USPPNet_AppendArray((ushort)callIndex);
            foreach (var arg in args) {
                USPPNet_toBeSerialized = USPPNet_toBeSerialized.USPPNet_AppendArray(arg);
            }
        }
";

        #endregion

        #region OnPreSerialization

        public static string USPPNetOnPreSerialization = @"
            if (USPPNet_updateIndexLast != USPPNet_updateIndex) {
                USPPNet_toBeSerialized[0] = USPPNet_calls;
                USPPNet_bytes = Serializer.Serialize(USPPNet_toBeSerialized);
                USPPNet_toBeSerialized = USPPNet_toBeSerialized_empty;
                USPPNet_calls = 0;
            }";

        #endregion

        #region OnPostSerialization

        public const string USPPNetOnPostSerialization = @"
            if (result.success) {
                bytesSent = result.byteCount;
                USPPNet_bytes = USPPNet_bytes_empty;
            }";

        #endregion

        #region OnDeserialization

        public static string USPPNetOnDeserialization = @"
            if (USPPNet_updateIndexLast != USPPNet_updateIndex) {
                
                int USPPNet_offset = 1;
                var desil = Serializer.Deserialize(USPPNet_bytes);
                var calls = (int)desil[0];
                
                for (var call = 0; call < calls; call++) {
                    var method = (ushort)desil[USPPNet_offset];
                    USPPNet_offset++;

                    // USPPNet TEMP REPLACE ME!!!
                }
                USPPNet_updateIndexLast = USPPNet_updateIndex;
            }";

        #endregion
    }

    public static class PreProcessor {
        private static string RemoveNewLines(string code) => code.Replace('\n', ' ').Replace('\r', ' ');

        private static string StripComments(string code) {
            var re = @"(@(?:""[^""]*"")+|""(?:[^""\n\\]+|\\.)*""|'(?:[^'\n\\]+|\\.)*')|//.*|/\*(?s:.*?)\*/";
            return Regex.Replace(code, re, "$1");
        }

        private static string[] StripComments(string[] code) {
            for (var i = 0; i < code.Length; i++)
                code[i] = StripComments(code[i]);
            return code;
        }

        private static void get_USPPNet_Functions(string[] lines, ref Dictionary<string, string[]> functions) {
            functions = new Dictionary<string, string[]>();

            foreach (var line in lines) {
                var l = StripComments(line);
                var start = l.IndexOf("void USPPNET_", StringComparison.Ordinal);
                if (start == -1) // check if line is USPPNET function
                    continue;
                
                var argsStart = l.IndexOf("(", start + 5, StringComparison.Ordinal) + 1;
                var argsEnd = l.IndexOf(")", argsStart, StringComparison.Ordinal);

                var functionName = l.Substring(start + 5, argsStart - (start + 6));
                
                var argLength = argsEnd - argsStart;
                if (argLength < 2) {
                    functions.Add(functionName, Array.Empty<string>());
                    continue;
                }

                var argsSubstring = l.Substring(argsStart, argLength);
                var args = argsSubstring.Split(',');
                
                var functionArgs = new string[args.Length];
            
                for (var index = 0; index < args.Length; index++) // adds function argument types to dict
                {
                    var split = args[index].Split(' ');
                    split = split.Where(x => !string.IsNullOrEmpty(x)).ToArray(); // remove blank strings
                    functionArgs[index] = split[0];
                }
                
                //Debug.Log($"Function: {functionName}, arg types: {ObjectDumper.Dump(functionArgs)}");

                functions.Add(functionName, functionArgs);
            }
        }

        /*
         * Turns USPPNET_Test("Hello there!", 69);
         * Into USPPNet_RPC("USPPNET_Test", "Hello there!", 69);
         */
        private static string[] replace_USPPNet_Calls(this string[] lines, ref Dictionary<string, string[]> functions) {
            var functionNames = functions.Keys.ToArray();

            var lineNum = 0;

            for (var index = 0; index < lines.Length; index++) {
                var line = lines[index];
                lineNum++;
                var l = StripComments(line);
                if (!functionNames.Any(c => l.TrimStart().StartsWith(c)))
                    continue;
                if (!l.Contains(");"))
                    continue;

                var namestart = l.IndexOf("USPPNET_", StringComparison.Ordinal);
                var nameEnd = l.IndexOf("(", namestart, StringComparison.Ordinal);
                var functionEnd = l.IndexOf(";", nameEnd, StringComparison.Ordinal);

                var functionName = l.Substring(namestart, nameEnd - namestart);
                var function = l.Substring(namestart, nameEnd - namestart + 1);

                if (functions[functionName].Length > 0)
                    lines[index] = l.Replace(function, $"USPPNet_RPC(\"{functionName}\", ");
                else
                    lines[index] = l.Replace(function, $"USPPNet_RPC(\"{functionName}\"");

                //Debug.Log($"Line: {lineNum}, Line: {lines[index]}");
                //break; // remember to remove
            }

            return lines;
        }

        private static string create_MethodIndexList(ref Dictionary<string, string[]> functions) {
            return functions.Aggregate("", (current, func) => current + $"\"{func.Key}\", ");
        }

        private static string create_OnDeserialization_MethodCall(ref Dictionary<string, string[]> functions) {
            var sb = new StringBuilder();
            var functionNames = functions.Keys.ToArray();


            foreach (var func in functionNames) {
                var tempArgs = "";
                var offset = 0;
                for (var index = 0; index < functions[func].Length; index++) {
                    var argType = functions[func][index];
                    if (index > 0)
                        tempArgs += ", ";

                    if (offset > 0)
                        tempArgs += $"({argType})desil[USPPNet_offset + {offset}]";
                    else
                        tempArgs += $"({argType})desil[USPPNet_offset]";
                    offset++;
                }

                sb.Append($@"
                if(method == {functions.Keys.ToArray().USPPNet_IndexOf(func)}) " + "{" + $@"
                    {func}({tempArgs});");

                sb.Append($"USPPNet_offset += {offset};");

                sb.Append("continue;");
                sb.Append("}\n");
            }

            return RemoveNewLines(sb.ToString());
        }

        private static bool Uses_USPPNet(ref string prog) =>
            prog.Contains("USPPNetUdonSharpBehaviour") && prog.Contains("// USPPNet OnPreSerialization") && prog.Contains("// USPPNet OnPostSerialization") && prog.Contains("// USPPNet OnDeserialization");

        private static string[] replace_Placeholder_Comments(this string[] lines,
                                                             ref Dictionary<string, string[]> functions) {
            var methcall = create_OnDeserialization_MethodCall(ref functions);
            var callIndex = create_MethodIndexList(ref functions);
            //Debug.Log("Goobed:"+methcall);

            for (var i = 0; i < lines.Length; i++) {
                lines[i] = lines[i].Replace("// USPPNet Init", RemoveNewLines(PreProcessorSnippits.USPPNetInit));
                lines[i] = lines[i].Replace("// USPPNet OnPreSerialization",
                    RemoveNewLines(PreProcessorSnippits.USPPNetOnPreSerialization));
                lines[i] = lines[i].Replace("// USPPNet OnPostSerialization",
                    RemoveNewLines(PreProcessorSnippits.USPPNetOnPostSerialization));
                lines[i] = lines[i].Replace("// USPPNet OnDeserialization",
                    RemoveNewLines(RemoveNewLines(PreProcessorSnippits.USPPNetOnDeserialization)));
                lines[i] = lines[i].Replace("// USPPNet TEMP REPLACE ME!!!", methcall);
                lines[i] = lines[i].Replace("// USPPNet TEMP REPLACE MethodIndex",
                    "private string[] USPPNet_methodNames = { " + callIndex + " };");
            }

            return lines;
        }

        private static string Parse(string prog, PPInfo info) {
            if (!Uses_USPPNet(ref prog))
                return prog;

            var inst = new PreProcessorInstance();
            var lines = prog.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            get_USPPNet_Functions(lines, ref inst.functions);

            lines = lines.replace_USPPNet_Calls(ref inst.functions).replace_Placeholder_Comments(ref inst.functions);

            prog = lines.Aggregate("", (current, line) => current + line + "\n");

            //Debug.Log(prog); // Uncomment to get program after parsing

            return prog;
        }

        [InitializeOnLoadMethod]
        private static void Subscribe() {
            PPHandler.Subscribe(Parse, 1, "USPPNet");
        }
    }
}
#endif