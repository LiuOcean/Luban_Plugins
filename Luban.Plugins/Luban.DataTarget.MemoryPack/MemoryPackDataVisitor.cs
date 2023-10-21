using System.Buffers;
using System.Runtime.CompilerServices;
using Luban.Datas;
using Luban.Defs;
using Luban.Types;
using Luban.Utils;
using MemoryPack;

namespace Luban.DataTarget.MemoryPack;

public class MemoryPackDataVisitor
{
    public static MemoryPackDataVisitor Ins { get; } = new();

    public void Apply(DType type, ref MemoryPackWriter<ArrayBufferWriter<byte>> writer)
    {
        // @formatter:off
        switch(type)
        {
            case DInt x:      Accept(x, ref writer); break;
            case DString x:   Accept(x, ref writer); break;
            case DFloat x:    Accept(x, ref writer); break;
            case DBean x:     Accept(x, ref writer); break;
            case DBool x:     Accept(x, ref writer); break;
            case DEnum x:     Accept(x, ref writer); break;
            case DList x:     Accept(x, ref writer); break;
            case DArray x:    Accept(x, ref writer); break;
            case DLong x:     Accept(x, ref writer); break;
            case DDateTime x: Accept(x, ref writer); break;
            case DMap x:      Accept(x, ref writer); break;
            case DByte x:     Accept(x, ref writer); break;
            case DDouble x:   Accept(x, ref writer); break;
            case DSet x:      Accept(x, ref writer); break;
            case DShort x:    Accept(x, ref writer); break;
            case null: break;
            default:          throw new NotSupportedException($"DType:{type.GetType().FullName} not support");
        }
        // @formatter:on
    }


    public void Accept(DBool type, ref MemoryPackWriter<ArrayBufferWriter<byte>> writer)
    {
        writer.WriteUnmanaged(type.Value);
    }

    public void Accept(DByte type, ref MemoryPackWriter<ArrayBufferWriter<byte>> writer)
    {
        writer.WriteUnmanaged(type.Value);
    }

    public void Accept(DShort type, ref MemoryPackWriter<ArrayBufferWriter<byte>> writer)
    {
        writer.WriteUnmanaged(type.Value);
    }

    public void Accept(DInt type, ref MemoryPackWriter<ArrayBufferWriter<byte>> writer)
    {
        writer.WriteUnmanaged(type.Value);
    }

    public void Accept(DLong type, ref MemoryPackWriter<ArrayBufferWriter<byte>> writer)
    {
        writer.WriteUnmanaged(type.Value);
    }

    public void Accept(DFloat type, ref MemoryPackWriter<ArrayBufferWriter<byte>> writer)
    {
        writer.WriteUnmanaged(type.Value);
    }

    public void Accept(DDouble type, ref MemoryPackWriter<ArrayBufferWriter<byte>> writer)
    {
        writer.WriteUnmanaged(type.Value);
    }

    public void Accept(DEnum type, ref MemoryPackWriter<ArrayBufferWriter<byte>> writer)
    {
        writer.WriteUnmanaged(type.Value);
    }

    public void Accept(DString type, ref MemoryPackWriter<ArrayBufferWriter<byte>> writer)
    {
        writer.WriteString(type.Value);
    }

    public void Accept(DDateTime type, ref MemoryPackWriter<ArrayBufferWriter<byte>> writer)
    {
        writer.GetFormatter<DateTime>().Serialize(ref writer, ref Unsafe.AsRef(type.Time));
    }


    public void Accept(DArray type, ref MemoryPackWriter<ArrayBufferWriter<byte>> writer)
    {
        if(type.Datas.Count <= 0)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        writer.WriteCollectionHeader(type.Datas.Count);
        foreach(var d in type.Datas)
        {
            Apply(d, ref writer);
        }
    }

    public void Accept(DList type, ref MemoryPackWriter<ArrayBufferWriter<byte>> writer)
    {
        if(type.Datas.Count <= 0)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        writer.WriteCollectionHeader(type.Datas.Count);
        foreach(var d in type.Datas)
        {
            Apply(d, ref writer);
        }
    }

    public void Accept(DSet type, ref MemoryPackWriter<ArrayBufferWriter<byte>> writer)
    {
        if(type.Datas.Count <= 0)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        writer.WriteCollectionHeader(type.Datas.Count);
        foreach(var d in type.Datas)
        {
            Apply(d, ref writer);
        }
    }

    public void Accept(DMap type, ref MemoryPackWriter<ArrayBufferWriter<byte>> writer)
    {
        if(type.Datas.Count <= 0)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        writer.WriteCollectionHeader(type.Datas.Count);
        foreach(var d in type.Datas)
        {
            Apply(d.Key,   ref writer);
            Apply(d.Value, ref writer);
        }
    }

    public void Accept(DBean type, ref MemoryPackWriter<ArrayBufferWriter<byte>> writer)
    {
        var implType        = type.ImplType;
        var hierarchyFields = implType.HierarchyFields;
        int exportCount     = 0;


        int  idx     = 0;
        bool allNull = true;
        foreach(var field in type.Fields)
        {
            var defField = hierarchyFields[idx++];

            if(field != null)
            {
                allNull = false;
            }

            if(!defField.NeedExport())
            {
                continue;
            }

            ++exportCount;
        }

        if(allNull)
        {
            if(type.Type.IsAbstractType)
            {
                writer.WriteNullUnionHeader();
            }
            else
            {
                writer.WriteNullObjectHeader();
            }

            return;
        }

        if(type.Type.IsAbstractType)
        {
            writer.WriteUnionHeader((ushort) type.Type.Children.IndexOf(type.ImplType));
        }

        writer.WriteObjectHeader((byte) exportCount);

        int index = 0;
        foreach(DType? field in type.Fields)
        {
            var defField = (DefField) hierarchyFields[index++];
            if(!defField.NeedExport())
            {
                continue;
            }

            if(field is null)
            {
                ApplyNull(defField, ref writer);
                continue;
            }

            Apply(field, ref writer);
        }
    }

    public void WriteTableKey(DBean bean, ref MemoryPackWriter<ArrayBufferWriter<byte>> writer)
    {
        Apply(bean.Fields.First(), ref writer);
    }

    public void ApplyNull(DefField field, ref MemoryPackWriter<ArrayBufferWriter<byte>> writer)
    {
        switch(field.CType)
        {
            case TSet:
            case TString:
            case TList:
            case TMap:
            case TArray:
                writer.WriteNullCollectionHeader();
                break;
            case TInt:
            case TEnum:
                if(field.IsNullable)
                {
                    writer.WriteUnmanaged(0);
                }
                writer.WriteUnmanaged(0);
                break;
            case TBean:
                writer.WriteNullObjectHeader();
                break;
            case TBool:
                if(field.IsNullable)
                {
                    writer.WriteUnmanaged(false);
                }
                writer.WriteUnmanaged(false);
                break;
            case TByte:
                if(field.IsNullable)
                {
                    writer.WriteUnmanaged((byte)0);
                }
                writer.WriteUnmanaged((byte) 0);
                break;
            case TDateTime:
                writer.WriteUnmanaged(0);
                break;
            case TDouble:
                if(field.IsNullable)
                {
                    writer.WriteUnmanaged((double) 0);
                }
                writer.WriteUnmanaged((double) 0);
                break;
            case TFloat:
                if(field.IsNullable)
                {
                    writer.WriteUnmanaged((float) 0);
                }
                writer.WriteUnmanaged((float) 0);
                break;
            case TLong:
                if(field.IsNullable)
                {
                    writer.WriteUnmanaged((long) 0);
                }
                
                writer.WriteUnmanaged((long) 0);
                break;
            case TShort:
                if(field.IsNullable)
                {
                    writer.WriteUnmanaged((short) 0);
                }
                writer.WriteUnmanaged((short) 0);
                break;
        }
    }
}