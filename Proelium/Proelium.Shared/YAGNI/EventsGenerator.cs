//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using System.IO;
//using System.Text;

//namespace Proelium.Server.Generators;

//[Generator]
//public class EventsGenerator : ISourceGenerator
//{
//    public void Initialize(GeneratorInitializationContext context)
//    {
//        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
//    }

//    public void Execute(GeneratorExecutionContext context)
//    {
//        SyntaxReceiver receiver = (SyntaxReceiver)context.SyntaxReceiver;

//        if (receiver.EventsCds == null)
//        {
//            return;
//        }

//        var eventsSemanticModel = context.Compilation.GetSemanticModel(receiver.EventsCds.SyntaxTree);
//        var eventsSymbol = eventsSemanticModel.GetDeclaredSymbol(receiver.EventsCds);

//        StringBuilder code = new();

//        code.AppendLine("using Proelium.Server.Collections;");
//        code.AppendLine();
//        code.AppendLine($"namespace {eventsSymbol.ContainingNamespace.ToDisplayString()};");
//        code.AppendLine();
//        code.AppendLine($"public partial class {receiver.EventsCds.Identifier.ValueText}");
//        code.AppendLine("{");

//        foreach (var member in receiver.EventsCds.Members)
//        {
//            if (member is RecordDeclarationSyntax rds)
//            {
//                string name = rds.Identifier.ValueText;
//                string variable = char.ToLowerInvariant(name[0]) + name.Substring(1);
//                code.AppendLine($"\tpublic readonly StructEvent<Events.{name}> {variable} = new();");
//            }
//        }

//        code.AppendLine("}");

//        #pragma warning disable RS1035 // Do not use APIs banned for analyzers
//        Directory.Delete(@"E:\Projects\Proelium\Proelium\Proelium.Server\Generated", recursive: true);
//        Directory.CreateDirectory(@"E:\Projects\Proelium\Proelium\Proelium.Server\Generated");
//        File.WriteAllText(@"E:\Projects\Proelium\Proelium\Proelium.Server\Generated\Events.g.cs", code.ToString());
//        #pragma warning restore RS1035 // Do not use APIs banned for analyzers
//    }

//    private class SyntaxReceiver : ISyntaxReceiver
//    {
//        public ClassDeclarationSyntax EventsCds { get; private set; } = null;

//        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
//        {
//            if (syntaxNode is ClassDeclarationSyntax cds && cds.Identifier.ValueText == "Events")
//            {
//                foreach (var member in cds.Members)
//                {
//                    if (member is RecordDeclarationSyntax)
//                    {
//                        EventsCds = cds;
//                        break;
//                    }
//                }
//            }
//        }
//    }
//}