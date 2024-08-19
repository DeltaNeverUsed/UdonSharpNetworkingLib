using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityEngine;

namespace UdonSharpNetworkingLib {
    public class UdonSharpSyntaxRewriterInjector : CSharpSyntaxRewriter {
        private bool _isRoot = true;
        private CompilationUnitSyntax _root;

        private static FieldDeclarationSyntax CreateStringArray(string varName, string[] content) {
            var initializerVars = new List<SyntaxNodeOrToken>();
            foreach (var str in content) {
                initializerVars.Add(SyntaxFactory.LiteralExpression(
                    SyntaxKind.StringLiteralExpression,
                    SyntaxFactory.Literal(str)));
                initializerVars.Add(SyntaxFactory.Token(SyntaxKind.CommaToken));
            }

            initializerVars.RemoveAt(initializerVars.Count - 1);

            var initializer = SyntaxFactory.EqualsValueClause(
                SyntaxFactory.InitializerExpression(
                    SyntaxKind.ArrayInitializerExpression,
                    SyntaxFactory.SeparatedList<ExpressionSyntax>(
                        initializerVars.ToArray()
                    )));

            var variableDeclarator = SyntaxFactory
                .VariableDeclarator(varName)
                .WithInitializer(initializer);

            var variableDeclaration = SyntaxFactory.VariableDeclaration(
                    SyntaxFactory.ArrayType(
                            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)))
                        .WithRankSpecifiers(
                            SyntaxFactory.SingletonList(
                                SyntaxFactory.ArrayRankSpecifier(
                                    SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                        SyntaxFactory.OmittedArraySizeExpression())))))
                .WithVariables(SyntaxFactory.SingletonSeparatedList(variableDeclarator));
            var declaration = SyntaxFactory.FieldDeclaration(variableDeclaration)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));

            return declaration;
        }

        private static FieldDeclarationSyntax CreateByteArray(string varName, byte[] content) {
            var initializerVars = new List<SyntaxNodeOrToken>();
            foreach (var b in content) {
                initializerVars.Add(SyntaxFactory.LiteralExpression(
                    SyntaxKind.NumericLiteralExpression,
                    SyntaxFactory.Literal(b)));
                initializerVars.Add(SyntaxFactory.Token(SyntaxKind.CommaToken));
            }

            initializerVars.RemoveAt(initializerVars.Count - 1);

            var initializer = SyntaxFactory.EqualsValueClause(
                SyntaxFactory.InitializerExpression(
                    SyntaxKind.ArrayInitializerExpression,
                    SyntaxFactory.SeparatedList<ExpressionSyntax>(
                        initializerVars.ToArray()
                    )));

            var variableDeclarator = SyntaxFactory
                .VariableDeclarator(varName)
                .WithInitializer(initializer);

            var variableDeclaration = SyntaxFactory.VariableDeclaration(
                    SyntaxFactory.ArrayType(
                            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ByteKeyword)))
                        .WithRankSpecifiers(
                            SyntaxFactory.SingletonList(
                                SyntaxFactory.ArrayRankSpecifier(
                                    SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                        SyntaxFactory.OmittedArraySizeExpression())))))
                .WithVariables(SyntaxFactory.SingletonSeparatedList(variableDeclarator));
            var declaration = SyntaxFactory.FieldDeclaration(variableDeclaration)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));

            return declaration;
        }

        private static ParameterListSyntax ParseParameters(string parametersString) {
            var parameters = parametersString.Split(',')
                .Select(p => {
                    var parts = p.Trim().Split(' ');
                    if (parts.Length == 2) {
                        return SyntaxFactory.Parameter(SyntaxFactory.Identifier(parts[1]))
                            .WithType(SyntaxFactory.ParseTypeName(parts[0]));
                    }

                    return SyntaxFactory.Parameter(SyntaxFactory.Identifier(parts[0]));
                })
                .ToArray();

            return SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters));
        }

        private static MethodDeclarationSyntax CreateFunctionCallMethod(IEnumerable<MethodDeclarationSyntax> methods) {
            var methodsList = methods.ToList();

            var sb = new StringBuilder();
            sb.AppendLine("{");

            sb.AppendLine("switch (functionId) {");

            for (var methodIndex = 0; methodIndex < methodsList.Count; methodIndex++) {
                var method = methodsList[methodIndex];
                var paramTypes = method.ParameterList.Parameters.Select(p => p.Type?.ToString()).ToList();

                sb.Append("case ");
                sb.Append(methodIndex.ToString());
                sb.AppendLine(":");

                var methodName = method.Identifier.ToString();
                sb.Append(methodName);
                sb.Append('(');

                for (var paramIndex = 0; paramIndex < method.ParameterList.Parameters.Count; paramIndex++) {
                    if (paramIndex != 0)
                        sb.Append(',');
                    
                    var currentParam = paramTypes[paramIndex];
                    var isArray = currentParam.Contains("[]");

                    if (isArray) {
                        // If it's an array type we want to make sure it's the right type when we're calling.
                        sb.Append("UdonSharpNetworkingLib.Serializer.ConvertAll<");
                        sb.Append(currentParam.Remove(currentParam.Length - 2, 2));
                        sb.Append(", object>((object[])");
                    }
                    else {
                        sb.Append('(');
                        sb.Append(currentParam);
                        sb.Append(')');
                    }

                    sb.Append("parameters[");
                    sb.Append(paramIndex.ToString());
                    sb.Append(']');

                    if (isArray)
                        sb.Append(')');
                }

                sb.AppendLine(");");
                sb.AppendLine("break;");
            }

            sb.AppendLine("default:");
            sb.AppendLine("break;");

            sb.AppendLine("}");
            sb.AppendLine("}");
            
            return SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                    SyntaxFactory.Identifier("NetworkingLib_FunctionCall"))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword),
                    SyntaxFactory.Token(SyntaxKind.OverrideKeyword)))
                .WithParameterList(ParseParameters("ushort functionId, object[] parameters"))
                .WithBody(SyntaxFactory.Block(
                    SyntaxFactory.ParseStatement(sb.ToString())
                ));
        }

        private static string GetRuntimeTypeName(TypeSyntax typeSyntax) {
            switch (typeSyntax) {
                case PredefinedTypeSyntax predefinedType:
                    return GetFrameworkTypeName(predefinedType.Keyword.ValueText); // Map to .NET type names
                case IdentifierNameSyntax identifierName:
                    return identifierName.Identifier.ValueText; // e.g., "MyClass"
                case GenericNameSyntax genericName:
                    var typeArguments = string.Join(", ",
                        genericName.TypeArgumentList.Arguments.Select(GetRuntimeTypeName));
                    return $"{genericName.Identifier.ValueText}<{typeArguments}>"; // e.g., "List<string>"
                case ArrayTypeSyntax arrayType:
                    var elementType = GetRuntimeTypeName(arrayType.ElementType);
                    return $"{elementType}[]"; // e.g., "string[]"
                case QualifiedNameSyntax qualifiedName:
                    return
                        $"{GetRuntimeTypeName(qualifiedName.Left)}.{GetRuntimeTypeName(qualifiedName.Right)}"; // e.g., "System.Collections.Generic.List"
                default:
                    return typeSyntax.ToString(); // Fallback for more complex cases
            }
        }

        private static string GetFrameworkTypeName(string keyword) {
            return keyword switch {
                "bool" => "Boolean",
                "byte" => "Byte",
                "sbyte" => "SByte",
                "char" => "Char",
                "decimal" => "Decimal",
                "double" => "Double",
                "float" => "Single",
                "int" => "Int32",
                "uint" => "UInt32",
                "long" => "Int64",
                "ulong" => "UInt64",
                "object" => "Object",
                "short" => "Int16",
                "ushort" => "UInt16",
                "string" => "String",
                _ => keyword, // For non-primitive types, return the original keyword
            };
        }

        // Probably shouldn't be using a syntax walker for this..
        public override SyntaxNode Visit(SyntaxNode node) {
            if (_isRoot) {
                _isRoot = false;
                _root = node as CompilationUnitSyntax;

                if (_root != null) {
                    var classDeclarations = _root.DescendantNodes().OfType<ClassDeclarationSyntax>();

                    foreach (var classDeclaration in classDeclarations) {
                        var inheritsFromBaseClass = classDeclaration.BaseList?.Types
                            .Any(baseType => baseType.ToString() == "NetworkingLibUdonSharpBehaviour") ?? false;
                        if (!inheritsFromBaseClass) continue; // Skip if it's not a NetworkingLibUdonSharpBehaviour

                        var networkedMethods = _root
                            .DescendantNodes()
                            .OfType<MethodDeclarationSyntax>()
                            .Where(method => method.AttributeLists
                                .SelectMany(a => a.Attributes)
                                .Any(attr => attr.Name.ToString() == "NetworkingTarget"))
                            .ToList();

                        var networkTypes = new List<byte>();

                        // Loop through all the methods and get the networking type
                        foreach (var method in networkedMethods) {
                            var attributes = method.AttributeLists
                                .SelectMany(al => al.Attributes)
                                .Where(attr => attr.Name.ToString() == "NetworkingTarget");

                            foreach (var attribute in attributes) {
                                var targetType = attribute.ArgumentList?.Arguments.FirstOrDefault();

                                if (targetType != null) {
                                    var typeString = targetType.Expression.ToString().Split('.').Last();
                                    Enum.TryParse<NetworkingTargetType>(typeString,
                                        out var result);

                                    if (!result.ToString().Contains(typeString))
                                        Debug.LogError($"Failed to parse {typeString}, falling back to {result}");
                                    networkTypes.Add((byte)result);
                                }
                            }
                        }

                        var methodAndTypeNames = networkedMethods.Select(m => m.Identifier + "_" + m.ParameterList
                            .Parameters.Select(p => GetRuntimeTypeName(p.Type)?.ToString())
                            .Aggregate((current, next) => current + next)).ToArray();

                        var newClassDeclaration = classDeclaration
                            .AddMembers(
                                CreateStringArray(UdonSharpNetworkingLibConsts.FunctionListKey, methodAndTypeNames),
                                CreateByteArray(UdonSharpNetworkingLibConsts.NetworkingTypeKey, networkTypes.ToArray()),
                                CreateFunctionCallMethod(networkedMethods)
                            );
                        node = _root.ReplaceNode(classDeclaration, newClassDeclaration);
                    }
                }
                else {
                    Injections.PrintError("Root was null?");
                }
            }

            return base.Visit(node);
        }
    }
}