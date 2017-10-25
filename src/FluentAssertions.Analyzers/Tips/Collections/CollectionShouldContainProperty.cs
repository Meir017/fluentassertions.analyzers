﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;

namespace FluentAssertions.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CollectionShouldContainPropertyAnalyzer : FluentAssertionsAnalyzer
    {
        public const string DiagnosticId = Constants.Tips.Collections.CollectionShouldContainProperty;
        public const string Category = Constants.Tips.Category;

        public const string Message = "Use .Should().Contain() instead.";

        protected override DiagnosticDescriptor Rule => new DiagnosticDescriptor(DiagnosticId, Title, Message, Category, DiagnosticSeverity.Info, true);
        protected override IEnumerable<FluentAssertionsCSharpSyntaxVisitor> Visitors
        {
            get
            {
                yield return new AnyShouldBeTrueSyntaxVisitor();
                yield return new WhereShouldNotBeEmptySyntaxVisitor();
            }
        }

        public class AnyShouldBeTrueSyntaxVisitor : FluentAssertionsCSharpSyntaxVisitor
        {
            public AnyShouldBeTrueSyntaxVisitor() : base(MemberValidator.MathodContainingLambda("Any"), MemberValidator.Should, new MemberValidator("BeTrue"))
            {
            }
        }

        public class WhereShouldNotBeEmptySyntaxVisitor : FluentAssertionsCSharpSyntaxVisitor
        {
            public WhereShouldNotBeEmptySyntaxVisitor() : base(MemberValidator.MathodContainingLambda("Where"), MemberValidator.Should, new MemberValidator("NotBeEmpty"))
            {
            }
        }
    }

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CollectionShouldContainPropertyCodeFix)), Shared]
    public class CollectionShouldContainPropertyCodeFix : FluentAssertionsCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(CollectionShouldContainPropertyAnalyzer.DiagnosticId);
        
        protected override ExpressionSyntax GetNewExpression(ExpressionSyntax expression, FluentAssertionsDiagnosticProperties properties)
        {
            if (properties.VisitorName == nameof(CollectionShouldContainPropertyAnalyzer.AnyShouldBeTrueSyntaxVisitor))
            {
                var remove = new NodeReplacement.RemoveAndExtractArgumentsNodeReplacement("Any");
                var newExpression = GetNewExpression(expression, remove);

                return GetNewExpression(newExpression, new NodeReplacement.RenameAndPrependArgumentsNodeReplacement("BeTrue", "Contain", remove.Arguments));
            }
            else if (properties.VisitorName == nameof(CollectionShouldContainPropertyAnalyzer.WhereShouldNotBeEmptySyntaxVisitor))
            {
                var remove = new NodeReplacement.RemoveAndExtractArgumentsNodeReplacement("Where");
                var newExpression = GetNewExpression(expression, remove);

                return GetNewExpression(newExpression, new NodeReplacement.RenameAndPrependArgumentsNodeReplacement("NotBeEmpty", "Contain", remove.Arguments));
            }
            throw new System.InvalidOperationException($"Invalid visitor name - {properties.VisitorName}");
        }
    }
}