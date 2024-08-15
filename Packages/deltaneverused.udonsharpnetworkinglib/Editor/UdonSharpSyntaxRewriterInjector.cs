using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityEngine;

namespace UdonSharpNetworkingLib {
    public class UdonSharpSyntaxRewriterInjector : CSharpSyntaxRewriter {
        private bool _isRoot = true;
        private CompilationUnitSyntax _root;

        private FieldDeclarationSyntax CreateStringArray(string varName, string[] content) {
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
        
        private FieldDeclarationSyntax CreateByteArray(string varName, byte[] content) {
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

        public override SyntaxNode Visit(SyntaxNode node) {
            if (_isRoot) {
                _isRoot = false;
                _root = node as CompilationUnitSyntax;

                if (_root != null) {
                    UsingDirectiveSyntax dataDictAlias = SyntaxFactory.UsingDirective(
                            SyntaxFactory.NameEquals(SyntaxFactory.IdentifierName("Profiler_Data")),
                            SyntaxFactory.ParseName("VRC.SDK3.Data"))
                        .WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed);

                    UsingDirectiveSyntax utilitiesAlias = SyntaxFactory.UsingDirective(
                            SyntaxFactory.NameEquals(SyntaxFactory.IdentifierName("Profiler_Utilities")),
                            SyntaxFactory.ParseName("VRC.SDKBase.Utilities"))
                        .WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed);

                    _root = _root.AddUsings(dataDictAlias, utilitiesAlias);

                    // Inject timing functions into the base classes
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
                            
                        foreach (var method in networkedMethods)
                        {
                            var attributes = method.AttributeLists
                                .SelectMany(al => al.Attributes)
                                .Where(attr => attr.Name.ToString() == "NetworkingTarget");

                            foreach (var attribute in attributes)
                            {
                                var targetType = attribute.ArgumentList?.Arguments.FirstOrDefault();

                                if (targetType != null) {
                                    Enum.TryParse<NetworkingTargetType>(targetType.Expression.ToString(), out var result);
                                    networkTypes.Add((byte)result);
                                }
                            }
                        }

                        var methodAndTypeNames = networkedMethods.Select(m => m.Identifier + "_" + m.ParameterList
                            .Parameters.Select(p => p.Type?.ToString())
                            .Aggregate((current, next) => current + next)).ToArray();

                        var newClassDeclaration = classDeclaration
                            .AddMembers(
                                CreateStringArray(UdonSharpNetworkingLibConsts.FunctionListKey, methodAndTypeNames),
                                CreateByteArray(UdonSharpNetworkingLibConsts.NetworkingTypeKey, networkTypes.ToArray())
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