using System.Text;
using Luban.CSharp.TypeVisitors;
using Luban.Defs;
using Luban.Utils;
using Scriban.Runtime;

namespace Luban.DataTarget.MemoryPack;

public partial class CsharpMemoryPackTemplateExtension : ScriptObject
{
    public static string GenUnion(DefBean bean)
    {
        if(bean is null)
        {
            return string.Empty;
        }

        if(!string.IsNullOrEmpty(bean.Parent))
        {
            return string.Empty;
        }

        if(bean.Children is null || bean.Children.Count <= 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();

        int index = 0;

        foreach(var child in bean.Children)
        {
            sb.AppendLine($"[MemoryPackUnion({index++}, typeof({child.FullName}))]");
        }

        return sb.ToString().TrimEnd('\n');
    }

    public static string GenCategoryInit(DefTable table, string function)
    {
        if(table.Name.StartsWith("Localize"))
        {
            return string.Empty;
        }

        return$"{table.Name}.Instance.{function}";
    }

    public static string GenConfigConstructor(DefBean bean)
    {
        var constructor      = new StringBuilder();
        var body             = new StringBuilder();
        var base_constructor = new StringBuilder();

        foreach(var field in bean.Fields)
        {
            var declare_type = _GetDeclareType(field);
            if(!field.NeedExport())
            {
                continue;
            }

            if(field.CType.HasTag("text"))
            {
                constructor.Append($"{declare_type} _{field.Name}_key,");
                body.AppendLine($"\tthis._{field.Name}_key = _{field.Name}_key;");
            }
            else
            {
                constructor.Append($"{declare_type} {field.Name},");
                body.AppendLine($"\tthis.{field.Name} = {field.Name};");
            }
        }

        return$$"""
                [MemoryPackConstructor]
                public {{bean.Name}}({{constructor.ToString().TrimEnd(',')}}) {{base_constructor}}
                {
                {{body.ToString().TrimEnd('\n')}}
                }
                """;
    }

    private static string _GetDeclareType(DefField field)
    {
        switch(field.CType.TypeName)
        {
            case "list": return$"IReadOnlyList<{field.CType.ElementType.Apply(EditorDeclaringTypeNameVisitor.Ins)}>";
        }

        return field.CType.Apply(EditorDeclaringTypeNameVisitor.Ins);
    }
}