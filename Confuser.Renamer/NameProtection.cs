using System;
using System.Collections.Generic;
using System.IO;
using Confuser.Core;

namespace Confuser.Renamer
{
    // Token: 0x0200005A RID: 90
    internal class NameProtection : Protection
    {
        // Token: 0x06000231 RID: 561 RVA: 0x0001F25B File Offset: 0x0001D45B
        protected override void Initialize(ConfuserContext context)
        {
            context.Registry.RegisterService("Ki.Rename", typeof(INameService), new NameService(context));
        }

        // Token: 0x06000232 RID: 562 RVA: 0x0001F27D File Offset: 0x0001D47D
        protected override void PopulatePipeline(ProtectionPipeline pipeline)
        {
            pipeline.InsertPostStage(PipelineStage.Inspection, new AnalyzePhase(this));
            pipeline.InsertPostStage(PipelineStage.BeginModule, new RenamePhase(this));
            pipeline.InsertPreStage(PipelineStage.EndModule, new PostRenamePhase(this));
            pipeline.InsertPostStage(PipelineStage.SaveModules, new NameProtection.ExportMapPhase(this));
        }

        // Token: 0x170000A0 RID: 160
        public override string Description
        {
            // Token: 0x0600022D RID: 557 RVA: 0x0001F243 File Offset: 0x0001D443
            get
            {
                return "This protection obfuscate the symbols' name so the decompiled source code can neither be compiled nor read.";
            }
        }

        // Token: 0x170000A2 RID: 162
        public override string FullId
        {
            // Token: 0x0600022F RID: 559 RVA: 0x0001F251 File Offset: 0x0001D451
            get
            {
                return "Ki.Rename";
            }
        }

        // Token: 0x170000A1 RID: 161
        public override string Id
        {
            // Token: 0x0600022E RID: 558 RVA: 0x0001F24A File Offset: 0x0001D44A
            get
            {
                return "rename";
            }
        }

        // Token: 0x1700009F RID: 159
        public override string Name
        {
            // Token: 0x0600022C RID: 556 RVA: 0x0001F23C File Offset: 0x0001D43C
            get
            {
                return "Name Protection";
            }
        }

        // Token: 0x170000A3 RID: 163
        public override ProtectionPreset Preset
        {
            // Token: 0x06000230 RID: 560 RVA: 0x0001F258 File Offset: 0x0001D458
            get
            {
                return ProtectionPreset.Maximum;
            }
        }

        // Token: 0x040004D1 RID: 1233
        public const string _FullId = "Ki.Rename";

        // Token: 0x040004D0 RID: 1232
        public const string _Id = "rename";

        // Token: 0x040004D2 RID: 1234
        public const string _ServiceId = "Ki.Rename";

        // Token: 0x0200005B RID: 91
        private class ExportMapPhase : ProtectionPhase
        {
            // Token: 0x06000234 RID: 564 RVA: 0x0001F2BB File Offset: 0x0001D4BB
            public ExportMapPhase(NameProtection parent) : base(parent)
            {
            }

            // Token: 0x06000238 RID: 568 RVA: 0x0001F2D4 File Offset: 0x0001D4D4
            protected override void Execute(ConfuserContext context, ProtectionParameters parameters)
            {
                NameService srv = (NameService)context.Registry.GetService<INameService>();
                ICollection<KeyValuePair<string, string>> map = srv.GetNameMap();
                if (map.Count == 0)
                {
                    return;
                }
                string path = Directory.GetCurrentDirectory() + "\\Configs\\symbols.map";
                string dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                using (StreamWriter writer = new StreamWriter(File.OpenWrite(path)))
                {
                    foreach (KeyValuePair<string, string> entry in map)
                    {
                        writer.WriteLine("{0}\t{1}", entry.Key, entry.Value);
                    }
                }
            }

            // Token: 0x170000A5 RID: 165
            public override string Name
            {
                // Token: 0x06000236 RID: 566 RVA: 0x0001F2C8 File Offset: 0x0001D4C8
                get
                {
                    return "Export symbol map";
                }
            }

            // Token: 0x170000A6 RID: 166
            public override bool ProcessAll
            {
                // Token: 0x06000237 RID: 567 RVA: 0x0001F2CF File Offset: 0x0001D4CF
                get
                {
                    return true;
                }
            }

            // Token: 0x170000A4 RID: 164
            public override ProtectionTargets Targets
            {
                // Token: 0x06000235 RID: 565 RVA: 0x0001F2C4 File Offset: 0x0001D4C4
                get
                {
                    return ProtectionTargets.Modules;
                }
            }
        }
    }
}
