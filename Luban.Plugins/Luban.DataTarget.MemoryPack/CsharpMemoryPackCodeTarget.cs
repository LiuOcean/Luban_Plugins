using Luban.CodeTarget;
using Luban.CSharp.CodeTarget;
using Luban.CSharp.TemplateExtensions;
using Scriban;

namespace Luban.DataTarget.MemoryPack;


[CodeTarget("cs-memorypack")]
public class CsharpMemoryPackCodeTarget : CsharpCodeTargetBase
{
    protected override void OnCreateTemplateContext(TemplateContext ctx)
    {
        base.OnCreateTemplateContext(ctx);
        ctx.PushGlobal(new CsharpMemoryPackTemplateExtension());
    }
}