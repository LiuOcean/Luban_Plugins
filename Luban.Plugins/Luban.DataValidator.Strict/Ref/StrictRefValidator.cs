using System.Runtime.CompilerServices;
using Luban.Datas;
using Luban.DataValidator.Builtin.Ref;
using Luban.DataVisitors;
using Luban.Defs;
using Luban.Types;
using Luban.Validator;

namespace Luban.DataValidator.Strict.Ref;

public readonly struct RefValidatorProxy(RefValidator target)
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_compiledTables")]
    static extern ref List<(DefTable Table, string Index, bool IgnoreDefault)> __compiledTables__(RefValidator target);

    public List<(DefTable Table, string Index, bool IgnoreDefault)> _compiledTables => __compiledTables__(target);
}

[Validator("ref", Priority = 1)]
public class StrictRefValidator : RefValidator
{
    private static readonly NLog.Logger s_logger = NLog.LogManager.GetCurrentClassLogger();

    private RefValidatorProxy _proxy = new(new RefValidator());

    public override void Validate(DataValidatorContext ctx, TType type, DType key)
    {
        var genCtx      = GenerationContext.Current;
        var excludeTags = genCtx.ExcludeTags;

        foreach(var tableInfo in _proxy._compiledTables)
        {
            var (defTable, field, zeroAble) = tableInfo;
            if(zeroAble && key.Apply(IsDefaultValueVisitor.Ins))
            {
                return;
            }
        
            switch(defTable.Mode)
            {
                case TableMode.ONE:
                {
                    throw new NotSupportedException($"{defTable.FullName} 是singleton表，不支持ref");
                }
                case TableMode.MAP:
                {
                    var recordMap = genCtx.GetTableDataInfo(defTable).FinalRecordMap;
                    if(recordMap.TryGetValue(key, out Record rec))
                    {
                        if(!rec.IsNotFiltered(excludeTags))
                        {
                            s_logger.Error(
                                "记录 {} = {} (来自文件:{}) 在引用表:{} 中存在，但导出时被过滤了",
                                RecordPath,
                                key,
                                Source,
                                defTable.FullName
                            );
                        }
        
                        goto MARK_ERROR;
                    }
        
                    break;
                }
                case TableMode.LIST:
                {
                    var recordMap = genCtx.GetTableDataInfo(defTable).FinalRecordMapByIndexs[field];
                    if(recordMap.TryGetValue(key, out Record rec))
                    {
                        if(!rec.IsNotFiltered(excludeTags))
                        {
                            s_logger.Error(
                                "记录 {} = {} (来自文件:{}) 在引用表:{} 中存在，但导出时被过滤了",
                                RecordPath,
                                key,
                                Source,
                                defTable.FullName
                            );
                        }
        
                        goto MARK_ERROR;
                    }
        
                    break;
                }
                default: throw new NotSupportedException();
            }
        }
        
        foreach(var table in _proxy._compiledTables)
        {
            s_logger.Error(
                "记录 {} = {} (来自文件:{}) 在引用表:{} 中不存在",
                RecordPath,
                key,
                Source,
                table.Table.FullName
            );
        }

        MARK_ERROR:
        GenerationContext.Current.LogValidatorFail(this);
    }
}