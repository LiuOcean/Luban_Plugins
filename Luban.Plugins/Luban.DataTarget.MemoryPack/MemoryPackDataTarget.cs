using System.Buffers;
using Luban.Defs;
using Luban.Plugins.Json;
using MemoryPack;

namespace Luban.DataTarget.MemoryPack;

[DataTarget("memorypack")]
public class MemoryPackDataTarget : DataTargetBase
{
    public static readonly string I18N_DIR = EnvManager.Current.GetOption("i18n", "dir", true);

    protected override string OutputFileExt => "bytes";

    private readonly NewtonJsonDataTarget _json_data = new();

    public void WriteList(DefTable table, List<Record> records, ref MemoryPackWriter<ArrayBufferWriter<byte>> writer)
    {
        writer.WriteObjectHeader(1);

        switch(table.Mode)
        {
            case TableMode.ONE:

                if(records.Count <= 0)
                {
                    return;
                }

                MemoryPackDataVisitor.Ins.Accept(records[0].Data, ref writer);
                break;
            case TableMode.MAP:

                if(records.Count <= 0)
                {
                    writer.WriteNullCollectionHeader();
                    return;
                }

                writer.WriteCollectionHeader(records.Count);

                foreach(var record in records)
                {
                    MemoryPackDataVisitor.Ins.WriteTableKey(record.Data, ref writer);
                    MemoryPackDataVisitor.Ins.Accept(record.Data, ref writer);
                }

                break;
            case TableMode.LIST:
                if(records.Count <= 0)
                {
                    writer.WriteNullCollectionHeader();
                    return;
                }

                writer.WriteCollectionHeader(records.Count);

                foreach(var record in records)
                {
                    MemoryPackDataVisitor.Ins.Accept(record.Data, ref writer);
                }

                break;
            default: throw new ArgumentOutOfRangeException();
        }
    }

    public override OutputFile ExportTable(DefTable table, List<Record> records)
    {
        // 本地化相关内容 仍然使用 json
        if(_IsI18N(table))
        {
            return _json_data.ExportTable(table, records);
        }

        using var state = MemoryPackWriterOptionalStatePool.Rent(MemoryPackSerializerOptions.Default);

        var ms = new ArrayBufferWriter<byte>();

        var writer = new MemoryPackWriter<ArrayBufferWriter<byte>>(ref ms, state);

        WriteList(table, records, ref writer);
        writer.Flush();
        return new OutputFile {File = $"{table.OutputDataFile}.{OutputFileExt}", Content = ms.WrittenSpan.ToArray(),};
    }

    private bool _IsI18N(DefTable table)
    {
        foreach(var path in table.InputFiles)
        {
            if(path.Contains(I18N_DIR))
            {
                return true;
            }
        }

        return false;
    }
}