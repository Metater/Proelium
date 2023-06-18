//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using System.Collections.Generic;
//using System.Linq;

//namespace Proelium.Server.Generators;

//[Generator]
//public class ContextGenerator : ISourceGenerator
//{
//    public void Initialize(GeneratorInitializationContext context)
//    {
//        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
//    }

//    public void Execute(GeneratorExecutionContext context)
//    {
//        SyntaxReceiver receiver = (SyntaxReceiver)context.SyntaxReceiver;

//        foreach (var classDeclaration in receiver.classDeclarations)
//        {
//            var semanticModel = context.Compilation.GetSemanticModel(classDeclaration.SyntaxTree);
//            var symbol = semanticModel.GetDeclaredSymbol(classDeclaration);
//            var allInterfaces = symbol.AllInterfaces;

//            if (allInterfaces.Any(i => i.Name == "IRequireContext"))
//            {
//                context.AddSource($"{classDeclaration.Identifier.ValueText}.g.cs",
//$@"
//namespace {symbol.ContainingNamespace.ToDisplayString()};

//public partial class {classDeclaration.Identifier.ValueText} : IRequireContext
//{{
//    public required Context Ctx {{ get; init; }}
//}}"
//                );
//            }
//        }
//    }

//    private class SyntaxReceiver : ISyntaxReceiver
//    {
//        public readonly List<ClassDeclarationSyntax> classDeclarations = new();
        
//        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
//        {
//            if (syntaxNode is ClassDeclarationSyntax cds)
//            {
//                classDeclarations.Add(cds);
//            }
//        }
//    }
//}