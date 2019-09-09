using System;
using Confuser.Core;
using dnlib.DotNet;

namespace Confuser.Renamer
{
    // Token: 0x02000003 RID: 3
    public interface IRenamer
    {
        // Token: 0x0600000F RID: 15
        void Analyze(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def);

        // Token: 0x06000011 RID: 17
        void PostRename(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def);

        // Token: 0x06000010 RID: 16
        void PreRename(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def);
    }
}
