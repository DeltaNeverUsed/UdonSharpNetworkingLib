using System;
using System.IO;
using System.Reflection;
using UdonSharp.Compiler;
using UnityEditor;
using UnityEngine;

namespace USPPNet
{
    public static class PreProcessor
    {
        private static string USPPNetVars = @"
";
        
        public static string Parse(string prog)
        {
            prog = prog.Replace("It worked 2!", "SomeWrong");
            prog = prog.Replace("This is a test", "It worked 2!");
            return prog;
        }
    }
}