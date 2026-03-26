using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;


namespace ElShrine.Wpf.Generators;

[Generator]
public sealed class DPCliGenerator : IIncrementalGenerator
{
    private const string DeclaresAttributeName = nameof(GenerateDPCliDeclaresAttribute);//"GenerateDPCliDeclaresAttribute";
    private const string DeclaresAttributeFullName = $"ElShrine.Wpf.{DeclaresAttributeName}";
    private const string CliAttributeName = nameof(GenerateDPCliAttribute);//"GenerateDPCliAttribute";
    private const string CliAttributeFullName = $"ElShrine.Wpf.{CliAttributeName}";
    private static string BuildGeneratedFileName(string fileName) => $"{fileName}.g.cs";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        //var classNames = new HashSet<string>();
        //context.RegisterPostInitializationOutput(i => i.AddSource(BuildGeneratedFileName("DPCliAttributes"), AttributesSource));

        var targetInterfaces = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, ct) => s is InterfaceDeclarationSyntax ids && ids.AttributeLists.Any(),
                transform: static (ctx, ct) => GetInterfaceSymbolIfHasAttribute(ctx, DeclaresAttributeFullName, ct)
            )
            .Where(static s => s is not null)
            .Select(static (s, ct) => s!)
            .Collect();
        var implementingClasses = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, ct) => s is ClassDeclarationSyntax,
                transform: static (ctx, ct) => GetClassSymbolAndOptionalCliAttribute(ctx, CliAttributeFullName, ct)
            )
            .Where(static s => s.Class is not null)
            .Select(static (s, ct) => s!);
        /*var outputData = implementingClasses
            .Combine(targetInterfaces)
            .SelectMany(static (pair, ct) =>
            {
                var (Class, CliAttribute) = pair.Left;
                var classSymbol = Class;
                var classCliAttr = CliAttribute;
                var targetInterfaces = pair.Right;

                var interfacesToImplement = classSymbol?.AllInterfaces
                    .Where(implementedInterface => targetInterfaces.Contains(implementedInterface, SymbolEqualityComparer.Default))
                    .ToImmutableArray();

                if (interfacesToImplement?.IsEmpty ?? true || classSymbol is null) return [];
                else return ImmutableArray.Create<(INamedTypeSymbol Class, AttributeData? CliAttr, ImmutableArray<INamedTypeSymbol> Interfaces)>((classSymbol!, classCliAttr, interfacesToImplement ?? []));
            });*/
        var allRawData = implementingClasses 
            .Combine(targetInterfaces)
            .SelectMany(static (pair, ct) =>
            {
                var (Class, CliAttribute) = pair.Left;
                var classSymbol = Class;
                var classCliAttr = CliAttribute;
                var targetInterfaces = pair.Right;

                var interfacesToImplement = classSymbol?.AllInterfaces
                    .Where(implementedInterface => targetInterfaces.Contains(implementedInterface, SymbolEqualityComparer.Default))
                    .ToImmutableArray();
                if (interfacesToImplement?.IsEmpty ?? true || classSymbol is null) return [];
                var fullClassName = classSymbol!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                return ImmutableArray.Create<(string Key, INamedTypeSymbol Class, AttributeData? CliAttr, ImmutableArray<INamedTypeSymbol> Interfaces)>(
                    (fullClassName, classSymbol!, classCliAttr, interfacesToImplement ?? []));
            })
            .Collect();
        var outputData = allRawData
            .Select(static (rawItems, ct) =>
            {
                var distinctItems = new List<(INamedTypeSymbol Class, AttributeData? CliAttr, ImmutableArray<INamedTypeSymbol> Interfaces)>();
                var distinctKeys = new HashSet<string>(System.StringComparer.Ordinal);
                foreach (var (Key, Class, CliAttr, Interfaces) in rawItems)
                {
                    if (distinctKeys.Add(Key)) distinctItems.Add((Class, CliAttr, Interfaces));
                }
                return distinctItems.ToImmutableArray();
            })
            .SelectMany(static (items, ct) => items);
        context.RegisterSourceOutput(outputData,
            (spc, item) =>
            {
                var classSymbol = item.Class;
                var cliAttr = item.CliAttr;
                var interfaces = item.Interfaces;
                var sourceCode = GeneratePartialClass(classSymbol, cliAttr, interfaces);
                var fullClassName = classSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                /*if (classNames.Contains(fullClassName)) return;
                classNames.Add(fullClassName);*/
                spc.AddSource(hintName: BuildGeneratedFileName($"{fullClassName}.DPs"), sourceCode);
            });
    }

    /// <summary>检查接口是否有 GenerateDPCliDeclaresAttribute</summary>
    private static INamedTypeSymbol? GetInterfaceSymbolIfHasAttribute(GeneratorSyntaxContext context, string attributeFullName, CancellationToken ct)
    {
        var interfaceDecl = (InterfaceDeclarationSyntax)context.Node;
        if (context.SemanticModel.GetDeclaredSymbol(interfaceDecl, ct) is not INamedTypeSymbol interfaceSymbol) return null;

        return interfaceSymbol.GetAttributes().Any(attr =>
            Equals(attr.AttributeClass?.ToDisplayString(), attributeFullName) ||
            Equals(attr.AttributeClass?.Name, DeclaresAttributeName)
        ) ? interfaceSymbol : null;
    }

    /// <summary>检查类是否有 GenerateDPCliAttribute (可选)，并返回类符号和特性数据</summary>
    private static (INamedTypeSymbol? Class, AttributeData? CliAttribute) GetClassSymbolAndOptionalCliAttribute(GeneratorSyntaxContext context, string attributeFullName, CancellationToken ct)
    {
        var classDecl = (ClassDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(classDecl, ct) is not INamedTypeSymbol classSymbol) return (null, null);

        var attr = classSymbol.GetAttributes().FirstOrDefault(attr =>
            Equals(attr.AttributeClass?.ToDisplayString(), attributeFullName) ||
            Equals(attr.AttributeClass?.Name, CliAttributeName)
        );

        return (classSymbol, attr);
    }

    /// <summary>检查特性是否为指定的 DeclaresAttribute</summary>
    private static bool IsDeclaresAttr(AttributeData attrData)
    {
        var attrStr = attrData.AttributeClass?.ToDisplayString();
        return attrStr == DeclaresAttributeFullName || attrStr == DeclaresAttributeName;
    }

    private static string GeneratePartialClass(INamedTypeSymbol classSymbol, AttributeData? cliAttr, ImmutableArray<INamedTypeSymbol> interfaces)
    {
        var builder = new StringBuilder();
        var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
        var className = classSymbol.Name;
        var accessibility = classSymbol.DeclaredAccessibility.ToString().ToLower();

        ITypeSymbol? classDPOwnerTypeSymbol = null;
        var ignoreProps = new HashSet<string>();

        if (cliAttr is not null)
        {
            classDPOwnerTypeSymbol = cliAttr.NamedArguments.FirstOrDefault(kvp => kvp.Key == "DPOwnerType").Value.Value as ITypeSymbol;
            var ignorePropsConstant = cliAttr.ConstructorArguments.FirstOrDefault();
            ignoreProps = [.. ignorePropsConstant.Kind == TypedConstantKind.Array ? ignorePropsConstant.Values.Where(v => v.Value is string).Select(v => (string)v.Value!) : []];
        }
        ignoreProps.Add("HandleThemePropertyChanged");

        builder.AppendLine("// <auto-generated/>");
        builder.AppendLine();
        builder.AppendLine($"#nullable enable");
        builder.AppendLine();
        builder.AppendLine("using System.Windows;");
        builder.AppendLine();

        if (!string.IsNullOrEmpty(namespaceName)) builder.AppendLine($"namespace {namespaceName};");

        builder.AppendLine($"{accessibility} partial class {className}");
        builder.AppendLine("{");
        var existingPropertyNames = new HashSet<string>();
        var currentType = (ITypeSymbol)classSymbol;
        while (currentType != null)
        {
            foreach (var property in currentType.GetMembers().OfType<IPropertySymbol>())
            {
                existingPropertyNames.Add(property.Name);
            }
            currentType = currentType.BaseType;
        }

        foreach (var interfaceSymbol in interfaces)
        {
            var interfaceAttr = interfaceSymbol.GetAttributes().FirstOrDefault(IsDeclaresAttr);
            if (interfaceAttr is null) continue;
            // 从 GenerateDPCliDeclaresAttribute 获取 DefaultDPOwnerType 和 AutoGenerate
            var defaultDPOwnerTypeSymbol = interfaceAttr.NamedArguments.FirstOrDefault(kvp => kvp.Key == "DefaultDPOwnerType").Value.Value as ITypeSymbol;
            var autoGenerateConstant = interfaceAttr.NamedArguments.FirstOrDefault(kvp => kvp.Key == "AutoGenerate").Value;
            var autoGenerate = autoGenerateConstant.Kind != TypedConstantKind.Primitive || (bool)(autoGenerateConstant.Value ?? true);

            var dpOwnerTypeSymbol = classDPOwnerTypeSymbol ?? defaultDPOwnerTypeSymbol ?? classSymbol;
            if (!autoGenerate && classDPOwnerTypeSymbol is null)
            {
                builder.AppendLine($"\t// INFO: Auto-generation is disabled by GenerateDPCliDeclaresAttribute on interface {interfaceSymbol.Name} and the class {classSymbol.Name} has no GenerateDPCliAttribute. Skipping...");
                continue;
            }

            var dpOwnerTypeName = dpOwnerTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            builder.AppendLine($"\t// Generated properties for interface {interfaceSymbol.Name} (DPOwnerType: {dpOwnerTypeName})");

            var properties = interfaceSymbol.GetMembers().OfType<IPropertySymbol>()
                .Where(p => p.DeclaredAccessibility == Accessibility.Public)
                .ToImmutableArray();
            foreach (var property in properties)
            {
                var propertyType = property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                var propertyName = property.Name;
                if (ignoreProps.Contains(propertyName))
                {
                    builder.AppendLine($"\t// INFO: Skipping property '{propertyName}' as it is in the IgnoreProps list on class '{className}'.");
                    continue;
                }
                if (existingPropertyNames.Contains(propertyName))
                {
                    builder.AppendLine($"\t// WARNING: Skipping property '{propertyName}' as it already exists on '{className}' or its base classes.");
                    continue;
                }
                builder.AppendLine($"\tpublic {propertyType} {propertyName}");
                builder.AppendLine("\t{");
                builder.AppendLine($"\t\tget => ({propertyType})GetValue({propertyName}Property);");
                builder.AppendLine($"\t\tset => SetValue({propertyName}Property, value);");
                builder.AppendLine("\t}").AppendLine();
                builder.AppendLine($"\tpublic static readonly DependencyProperty {propertyName}Property = {dpOwnerTypeName}.{propertyName}Property;");
                builder.AppendLine();
            }
        }

        builder.AppendLine("}");
        return builder.ToString();
    }
}