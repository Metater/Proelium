using Microsoft.CodeAnalysis;
using System;
using System.IO;

namespace Proelium.Server.Generators
{
    [Generator]
    public class TestGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            context.AddSource("TestGeneratedCode.g.cs", "public record TestGeneratedCode();");
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            
        }
    }
}