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
    public class CollectionShouldNotContainPropertyAnalyzer : FluentAssertionsAnalyzer
    {
        public const string DiagnosticId = Constants.Tips.Collections.CollectionShouldNotContainProperty;
        public const string Category = Constants.Tips.Category;

        public const string Message = "Use .Should().NotContain() instead.";

        protected override DiagnosticDescriptor Rule => new DiagnosticDescriptor(DiagnosticId, Title, Message, Category, DiagnosticSeverity.Info, true);
        protected override IEnumerable<FluentAssertionsCSharpSyntaxVisitor> Visitors
        {
            get
            {
                yield return new AnyShouldBeFalseSyntaxVisitor();
                yield return new WhereShouldBeEmptySyntaxVisitor();
                yield return new ShouldOnlyContainNotSyntaxVisitor();
            }
        }

        public class AnyShouldBeFalseSyntaxVisitor : FluentAssertionsCSharpSyntaxVisitor
        {
            public AnyShouldBeFalseSyntaxVisitor() : base(MemberValidator.MathodContainingLambda("Any"), MemberValidator.Should, new MemberValidator("BeFalse"))
            {
            }
        }

        public class WhereShouldBeEmptySyntaxVisitor : FluentAssertionsCSharpSyntaxVisitor
        {
            public WhereShouldBeEmptySyntaxVisitor() : base(MemberValidator.MathodContainingLambda("Where"), MemberValidator.Should, new MemberValidator("BeEmpty"))
            {
            }
        }

        public class ShouldOnlyContainNotSyntaxVisitor : FluentAssertionsCSharpSyntaxVisitor
        {
            public ShouldOnlyContainNotSyntaxVisitor() : base(MemberValidator.Should, MemberValidator.MathodContainingLambda("OnlyContain"))
            {
            }
        }
    }

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CollectionShouldNotContainPropertyCodeFix)), Shared]
    public class CollectionShouldNotContainPropertyCodeFix : FluentAssertionsCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(CollectionShouldNotContainPropertyAnalyzer.DiagnosticId);
        
        protected override ExpressionSyntax GetNewExpression(ExpressionSyntax expression, FluentAssertionsDiagnosticProperties properties)
        {
            if (properties.VisitorName == nameof(CollectionShouldNotContainPropertyAnalyzer.AnyShouldBeFalseSyntaxVisitor))
            {
                var remove = new NodeReplacement.RemoveAndExtractArgumentsNodeReplacement("Any");
                var newExpression = GetNewExpression(expression, remove);

                return GetNewExpression(newExpression, NodeReplacement.RenameAndPrependArguments("BeFalse", "NotContain", remove.Arguments));
            }
            else if (properties.VisitorName == nameof(CollectionShouldNotContainPropertyAnalyzer.WhereShouldBeEmptySyntaxVisitor))
            {
                var remove = new NodeReplacement.RemoveAndExtractArgumentsNodeReplacement("Where");
                var newExpression = GetNewExpression(expression, remove);

                return GetNewExpression(newExpression, NodeReplacement.RenameAndPrependArguments("BeEmpty", "NotContain", remove.Arguments));
            }
            else if (properties.VisitorName == nameof(CollectionShouldNotContainPropertyAnalyzer.ShouldOnlyContainNotSyntaxVisitor))
            {
                return GetNewExpression(expression, NodeReplacement.RenameAndNegateLambda("OnlyContain", "NotContain"));
            }
            throw new System.InvalidOperationException($"Invalid visitor name - {properties.VisitorName}");
        }
    }
}