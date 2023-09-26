using System.Text.Json;
using Luban.DataExporter.Builtin.Json;
using Luban.DataLoader;
using Luban.Datas;
using Luban.Defs;
using Luban.Utils;

namespace Luban.Plugins.Json;

public class NewtonJsonDataVisitor : JsonDataVisitor
{
    public static NewtonJsonDataVisitor Ins { get; } = new();

    public const string DllOption = "newtonsoft";

    private static readonly string s_dllName = EnvManager.Current.GetOption(DllOption, "dll", true);

    public override void Accept(DMap type, Utf8JsonWriter x)
    {
        x.WriteStartObject();
        foreach (var d in type.Datas)
        {
            x.WritePropertyName(d.Key.Apply(ToJsonPropertyNameVisitor.Ins));
            d.Value.Apply(this, x);
        }
        x.WriteEndObject();
    }
    
    public override void Accept(DBean type, Utf8JsonWriter x)
    {
        x.WriteStartObject();

        if(type.Type.IsAbstractType)
        {
            x.WritePropertyName(FieldNames.JsonTypeNameKey);

            x.WriteStringValue($"{DataUtil.GetImplTypeName(type)}, {s_dllName}");
        }

        var defFields = type.ImplType.HierarchyFields;
        int index     = 0;
        foreach(var d in type.Fields)
        {
            var defField = (DefField) defFields[index++];

            // 特殊处理 bean 多态类型
            // 另外，不生成  xxx:null 这样
            if(d == null || !defField.NeedExport())
            {
                //x.WriteNullValue();
            }
            else
            {
                x.WritePropertyName(defField.Name);
                d.Apply(this, x);
            }
        }

        x.WriteEndObject();
    }
}