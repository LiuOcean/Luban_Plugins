using System.Text.Json;
using Luban.DataExporter.Builtin.Json;
using Luban.DataTarget;
using Luban.Defs;
using Luban.Utils;

namespace Luban.Plugins.Json;

[DataTarget("newtonjson")]
public class NewtonJsonDataTarget : JsonDataTarget
{
    private void WriteAsObject(DefTable table, List<Record> datas, Utf8JsonWriter x)
    {
        switch (table.Mode)
        {
            case TableMode.ONE:
            {
                datas[0].Data.Apply(NewtonJsonDataVisitor.Ins, x);
                break;
            }
            case TableMode.MAP:
            {

                x.WriteStartObject();
                string indexName = table.IndexField.Name;
                foreach (var rec in datas)
                {
                    var indexFieldData = rec.Data.GetField(indexName);
                    x.WritePropertyName(indexFieldData.Apply(ToJsonPropertyNameVisitor.Ins));
                    rec.Data.Apply(NewtonJsonDataVisitor.Ins, x);
                }

                x.WriteEndObject();
                break;
            }
            case TableMode.LIST:
            {
                WriteAsArray(datas, x, NewtonJsonDataVisitor.Ins);
                break;
            }
            default:
            {
                throw new NotSupportedException($"not support table mode:{table.Mode}");
            }
        }
    }

    public override OutputFile ExportTable(DefTable table, List<Record> records)
    {                  
        var ss = new MemoryStream();
        var jsonWriter = new Utf8JsonWriter(ss, new JsonWriterOptions()
        {
            Indented       = ! UseCompactJson,
            SkipValidation = false,
            Encoder        = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        });
        WriteAsObject(table, records, jsonWriter);
        jsonWriter.Flush();
        return new OutputFile()
        {
            File    = $"{table.OutputDataFile}.{OutputFileExt}",
            Content = DataUtil.StreamToBytes(ss),
        };
    }
}