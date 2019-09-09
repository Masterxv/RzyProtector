using System;
using Confuser.Core;
using dnlib.DotNet;

namespace Confuser.Renamer
{
    // Token: 0x02000059 RID: 89
    internal class PostRenamePhase : ProtectionPhase
    {
        // Token: 0x06000227 RID: 551 RVA: 0x0001F180 File Offset: 0x0001D380
        public PostRenamePhase(NameProtection parent) : base(parent)
        {
        }

        // Token: 0x0600022B RID: 555 RVA: 0x0001F198 File Offset: 0x0001D398
        protected override void Execute(ConfuserContext context, ProtectionParameters parameters)
        {
            NameService service = (NameService)context.Registry.GetService<INameService>();
            foreach (IRenamer renamer in service.Renamers)
            {
                foreach (IDnlibDef def in parameters.Targets)
                {
                    renamer.PostRename(context, service, parameters, def);
                }
                context.CheckCancellation();
            }
        }

        // Token: 0x1700009E RID: 158
        public override string Name
        {
            // Token: 0x0600022A RID: 554 RVA: 0x0001F190 File Offset: 0x0001D390
            get
            {
                return "Post-renaming";
            }
        }

        // Token: 0x1700009C RID: 156
        public override bool ProcessAll
        {
            // Token: 0x06000228 RID: 552 RVA: 0x0001F189 File Offset: 0x0001D389
            get
            {
                return true;
            }
        }

        // Token: 0x1700009D RID: 157
        public override ProtectionTargets Targets
        {
            // Token: 0x06000229 RID: 553 RVA: 0x0001F18C File Offset: 0x0001D38C
            get
            {
                return ProtectionTargets.AllDefinitions;
            }
        }
    }
}
