using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using BlazorBindGen.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BlazorBindGenerator;

[Generator]
public class JSBindingGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not JSBindingSyntaxReciever reciever)
            return;

        if(reciever.ClassDeclarations.Count == 0) return;

        var wwwRootPath = GetPathOfWwwrootFolder(reciever,context);

        foreach(var classPair in reciever.ClassDeclarations)
        {
            if (!HandleClassValidation(classPair.Class, context))
                continue;

            ProcessModuleClass(classPair,context,wwwRootPath);
        }

    }

    private void ProcessModuleClass((ClassDeclarationSyntax Class, AttributeSyntax Attribute) classItem,
        GeneratorExecutionContext context,
        string wwwrootPath)
    {
        var moduleFunctions = GetModuleDeclaredMethods(classItem.Class, context);
        GenerateJSModuleClass( context,moduleFunctions, classItem, wwwrootPath);
    }

    private void GenerateJSModuleClass(GeneratorExecutionContext context, MethodDeclarationSyntax[] moduleFunctions,
        (ClassDeclarationSyntax Class, AttributeSyntax Attribute) classPair, string wwwrootPath)
    {
        var filePath = Path.Combine(wwwrootPath,"js",GetJsFileName(classPair.Class));

        if (!Directory.Exists(Path.GetDirectoryName(filePath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        }
        using StringWriter ss = new();
        using IndentedTextWriter writer = new(ss);


        writer.Write("import {");
        ///write import 
        var methodsToImport = GetJSModuleMethodsToImport(moduleFunctions);
        var importUrl = GetValueOfAttributeArgument(classPair.Attribute,0);

        writer.Write(string.Join(",",methodsToImport));
        writer.Write("} from ");
        writer.WriteLine($"\"{importUrl}\";");

        File.WriteAllText(filePath, ss.ToString());
    }

    private string[] GetJSModuleMethodsToImport(MethodDeclarationSyntax[] methods)
    {
        return methods.Select(GetAttributeForMethodDelc).Select(x=>GetValueOfAttributeArgument(x,0)).ToArray();
    }

    private AttributeSyntax GetAttributeForMethodDelc(MethodDeclarationSyntax method)
    {
        return method.DescendantNodes().OfType<AttributeSyntax>()
            .First(y => y.Name.ToString().EndsWith(GetAttributeShortName<ModuleFunctionAttribute>()));
    }

    private string GetValueOfAttributeArgument(AttributeSyntax attributeSyntax,int index,bool trimDoubleQuote= true)
    {
        if (attributeSyntax == null || attributeSyntax.ArgumentList==null || attributeSyntax.ArgumentList.Arguments==null)
            return null;

        var args = attributeSyntax.ArgumentList.Arguments;

        if (args.Count <= index)
            return null;

        if(args[index].Expression is InvocationExpressionSyntax invocation && invocation.Expression is IdentifierNameSyntax identifierName
            && identifierName.Identifier.Text=="nameof")
        {

            var argRaw = invocation.ArgumentList.Arguments.First();
            // if it is without quote aka nameof(M)
            if(argRaw.Expression is IdentifierNameSyntax id)
                return id.Identifier.Text;

            // otherwise form is nameof("M")
            var value = argRaw.Expression.ToString();
            if(trimDoubleQuote)
                return value.Trim('"');
            return value;
        }

        var val = args[index].ToString().Trim('"');

        if (trimDoubleQuote)
            return val.Trim('"');

        return val;
    }

    private string GetJsFileName(ClassDeclarationSyntax @class)
    {
        return @class.Identifier.Value.ToString()+$"_bindgen.js";
    }

    private string GetSymbolName(SyntaxNode syntaxNode, GeneratorExecutionContext context)
    {
        ISymbol symbol = context.Compilation
               .GetSemanticModel(syntaxNode.SyntaxTree).GetDeclaredSymbol(syntaxNode);

        return symbol.Name;

    }
    private bool HandleClassValidation(ClassDeclarationSyntax classDeclaration, GeneratorExecutionContext context)
    {
        if (!CheckIfPartialClass(classDeclaration, context))
        {
            ReportDiagonostics("No partial attribute",
                $"partial attribute is missing from the class {classDeclaration.Identifier.Text}", classDeclaration,context);
            return false;
        }

        return true;
    }

    private string GetPathOfWwwrootFolder(JSBindingSyntaxReciever reciever, GeneratorExecutionContext context)
    {
        var @class = reciever.ClassDeclarations.First();
        var fileName = @class.Class.SyntaxTree.FilePath;

        var dir = new DirectoryInfo(Path.GetDirectoryName(fileName));
        string path = null;
        do
        {
            var proj = Directory.GetFiles(dir.FullName, "*.csproj");

            if (proj.Length > 0)
            {
                path = proj[0];
                break;
            }

            dir = dir.Parent;

        } while (dir != null);

        if (path == null)
        {
            ReportDiagonostics("No wwwroot folder found",
               $"Can't find  csproj Path, cant generate JS binding files without knowing path.", null, context);
            return null;
        }

        path = Path.Combine(Path.GetDirectoryName(path), "wwwroot");
        if (!Directory.Exists(path))
        {
            ReportDiagonostics("No wwwroot folder found",
               $"Can't find  wwwroot folder, cant generate JS binding files without knowing path.", null, context);
            return null;
        }

        return path;
    }

    private bool CheckIfPartialClass(ClassDeclarationSyntax classDeclaration,GeneratorExecutionContext context)
    {
        return classDeclaration.Modifiers.Any(x => x.IsKind(SyntaxKind.PartialKeyword));
    }

    private void ReportDiagonostics(string title,string Msg, SyntaxNode dataType, GeneratorExecutionContext context)
    {
        if (dataType != null)
        {
            ISymbol symbol = context.Compilation
                .GetSemanticModel(dataType.SyntaxTree).GetDeclaredSymbol(dataType);

            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor(
                    "SG0001",
                    title,
                    $"{Msg} -> {symbol.Name}",
                    "Error",
                    DiagnosticSeverity.Error,
                    true), symbol.Locations.FirstOrDefault(),
                symbol.Name, symbol.Name));
        }
        else
        {
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor(
                    "SG0001",
                    title,
                    Msg,
                    "Error",
                    DiagnosticSeverity.Error,
                    true), null));
        }
    }

    private MethodDeclarationSyntax[] GetModuleDeclaredMethods(ClassDeclarationSyntax @class,GeneratorExecutionContext context)
    {
        var methods = @class.DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Where(x=>x.DescendantNodes().OfType<AttributeSyntax>()
            .Any(y=>y.Name.ToString().EndsWith(GetAttributeShortName<ModuleFunctionAttribute>())));

        return methods.ToArray();
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new JSBindingSyntaxReciever());
    }

    internal static string GetAttributeShortName<T>() where T : Attribute =>
         typeof(T).Name.Replace("Attribute", string.Empty);
}
