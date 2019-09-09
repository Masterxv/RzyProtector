using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Confuser.Core;
using Confuser.Core.Services;
using dnlib.DotNet;
using dnlib.DotNet.MD;
using dnlib.DotNet.Writer;
using dnlib.IO;

namespace Confuser.Protections
{
    [AfterProtection(new string[]
    {
        "Ki.InvalidMD",
        "Ki.AntiTamper"
    })]
    internal class FakeNative : Protection
    {
        public override string Name
        {
            get
            {
                return "Fake Native Protection";
            }
        }

        public override string Description
        {
            get
            {
                return "This protection fuck the metadata and show fake native assembly.";
            }
        }

        public override string Id
        {
            get
            {
                return "Fake Native";
            }
        }

        public override string FullId
        {
            get
            {
                return "Ki.FakeNative";
            }
        }

        public override ProtectionPreset Preset
        {
            get
            {
                return ProtectionPreset.Maximum;
            }
        }

        protected override void Initialize(ConfuserContext context)
        {
        }
        protected override void PopulatePipeline(ProtectionPipeline pipeline)
        {
            pipeline.InsertPostStage(PipelineStage.BeginModule, new FakeNative.FakeNativePhase(this));
        }

        public const string _Id = "Fake Native";
        public const string _FullId = "Ki.FakeNative";
        private static Random R = new Random();
        public static string SectionName;

		private class FakeNativePhase : ProtectionPhase
        {
            public override ProtectionTargets Targets
            {
                get
                {
                    return ProtectionTargets.Modules;
                }
            }

            public override string Name
            {
                get
                {
                    return "Fake Native MD addition";
                }
            }

            public FakeNativePhase(FakeNative parent) : base(parent)
            {
            }

            protected override void Execute(ConfuserContext context, ProtectionParameters parameters)
            {
                if (parameters.Targets.Contains(context.CurrentModule))
                {
                    this.random = context.Registry.GetService<IRandomService>().GetRandomGenerator("Ki.FakeNative");
                    context.CurrentModuleWriterListener.OnWriterEvent += new EventHandler<ModuleWriterListenerEventArgs>(this.OnWriterEvent);
                }
            }

            private void Randomize<T>(MDTable<T> table) where T : IRawRow
            {
                List<T> list = table.ToList<T>();
                this.random.Shuffle<T>(list);
                table.Reset();
                foreach (T current in list)
                {
                    table.Add(current);
                }
            }

            public static string GetRandomString()
            {
                string randomFileName = Path.GetRandomFileName();
                return randomFileName.Replace(".", "");
            }

            private void OnWriterEvent(object sender, ModuleWriterListenerEventArgs e)
            {
                ModuleWriterBase moduleWriterBase = (ModuleWriterBase)sender;
                if (e.WriterEvent == ModuleWriterEvent.MDEndCreateTables)
                {
                    PESection pESection = new PESection("Rzy", 1073741888u);
                    moduleWriterBase.Sections.Add(pESection);
                    pESection.Add(new ByteArrayChunk(new byte[123]), 4u);
                    pESection.Add(new ByteArrayChunk(new byte[10]), 4u);
                    string text = ".Rzy";
                    string s = null;
                    for (int i = 0; i < 80; i++)
                    {
                        text += FakeNative.FakeNativePhase.GetRandomString();
                    }
                    for (int j = 0; j < 80; j++)
                    {
                        byte[] bytes = Encoding.ASCII.GetBytes(text);
                        s = Utils.EncodeString(bytes, FakeNative.FakeNativePhase.asciiCharset);
                    }
                    byte[] bytes2 = Encoding.ASCII.GetBytes(s);
                    moduleWriterBase.TheOptions.MetaDataOptions.OtherHeapsEnd.Add(new FakeNative.RawHeap("#Rzy-Private-Protector", bytes2));
                    pESection.Add(new ByteArrayChunk(bytes2), 4u);

                    var writer = (ModuleWriterBase)sender;

                    uint signature = (uint)(moduleWriterBase.MetaData.TablesHeap.TypeSpecTable.Rows + 1);
                    List<uint> list = (from row in moduleWriterBase.MetaData.TablesHeap.TypeDefTable
                                       select row.Namespace).Distinct<uint>().ToList<uint>();
                    List<uint> list2 = (from row in moduleWriterBase.MetaData.TablesHeap.MethodTable
                                        select row.Name).Distinct<uint>().ToList<uint>();
                    uint num2 = Convert.ToUInt32(FakeNative.R.Next(15, 3546));
                    using (List<uint>.Enumerator enumerator = list.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            uint current = enumerator.Current;
                            if (current != 0u)
                            {
                                foreach (uint current2 in list2)
                                {
                                    if (current2 != 0u)
                                    {
                                        moduleWriterBase.MetaData.TablesHeap.TypeSpecTable.Add(new RawTypeSpecRow(signature));
                                        moduleWriterBase.MetaData.TablesHeap.ModuleTable.Add(new RawModuleRow(65535, 0u, 4294967295u, 4294967295u, 4294967295u));
                                        moduleWriterBase.MetaData.TablesHeap.ParamTable.Add(new RawParamRow(254, 254, moduleWriterBase.MetaData.TablesHeap.ENCMapTable.Add(new RawENCMapRow(this.random.NextUInt32()))));
                                        moduleWriterBase.MetaData.TablesHeap.FieldTable.Add(new RawFieldRow((ushort)(num2 * 4u + 77u), 31u + num2 / 2u * 3u, this.random.NextUInt32()));
                                        moduleWriterBase.MetaData.TablesHeap.MemberRefTable.Add(new RawMemberRefRow(num2 + 18u, num2 * 4u + 77u, 31u + num2 / 2u * 3u));
                                        moduleWriterBase.MetaData.TablesHeap.TypeSpecTable.Add(new RawTypeSpecRow(3391u + num2 / 2u * 3u));
                                        moduleWriterBase.MetaData.TablesHeap.PropertyTable.Add(new RawPropertyRow((ushort)(num2 + 44u - 1332u), num2 / 2u + 2u, this.random.NextUInt32()));
                                        moduleWriterBase.MetaData.TablesHeap.TypeSpecTable.Add(new RawTypeSpecRow(3391u + num2 / 2u * 3u));
                                        moduleWriterBase.MetaData.TablesHeap.PropertyPtrTable.Add(new RawPropertyPtrRow(this.random.NextUInt32()));
                                        moduleWriterBase.MetaData.TablesHeap.AssemblyRefTable.Add(new RawAssemblyRefRow(55, 44, 66, 500, this.random.NextUInt32(), this.random.NextUInt32(), moduleWriterBase.MetaData.TablesHeap.ENCMapTable.Add(new RawENCMapRow(this.random.NextUInt32())), this.random.NextUInt32(), this.random.NextUInt32()));
                                        moduleWriterBase.MetaData.TablesHeap.ENCLogTable.Add(new RawENCLogRow(this.random.NextUInt32(), moduleWriterBase.MetaData.TablesHeap.ENCMapTable.Add(new RawENCMapRow(this.random.NextUInt32()))));
                                        moduleWriterBase.MetaData.TablesHeap.ENCLogTable.Add(new RawENCLogRow(this.random.NextUInt32(), (uint)(moduleWriterBase.MetaData.TablesHeap.ENCMapTable.Rows - 1)));
                                        moduleWriterBase.MetaData.TablesHeap.ImplMapTable.Add(new RawImplMapRow(18, num2 * 4u + 77u, 31u + num2 / 2u * 3u, num2 * 4u + 77u));
                                    }
                                }
                            }
                        }
                    }
                }
                if (e.WriterEvent == ModuleWriterEvent.MDOnAllTablesSorted)
                {
                    moduleWriterBase.MetaData.TablesHeap.DeclSecurityTable.Add(new RawDeclSecurityRow(32767, 4294934527u, 4294934527u));
                }



            }

            // Token: 0x0400018C RID: 396
            private RandomGenerator random;

            // Token: 0x0400018D RID: 397
            private static readonly char[] asciiCharset = (from ord in Enumerable.Range(32, 95)
                                                           select (char)ord).Except(new char[]
            {
                '.'
            }).ToArray<char>();
        }

        // Token: 0x02000081 RID: 129
        private class RawHeap : HeapBase
        {
            // Token: 0x1700007A RID: 122
            // (get) Token: 0x0600023A RID: 570 RVA: 0x00017178 File Offset: 0x00015378
            public override string Name
            {
                get
                {
                    return this.name;
                }
            }

            // Token: 0x0600023B RID: 571 RVA: 0x00017180 File Offset: 0x00015380
            public RawHeap(string name, byte[] content)
            {
                this.name = name;
                this.content = content;
            }

            // Token: 0x0600023C RID: 572 RVA: 0x00017196 File Offset: 0x00015396
            public override uint GetRawLength()
            {
                return (uint)this.content.Length;
            }

            // Token: 0x0600023D RID: 573 RVA: 0x000171A0 File Offset: 0x000153A0
            protected override void WriteToImpl(BinaryWriter writer)
            {
                writer.Write(this.content);
            }

            // Token: 0x04000191 RID: 401
            private readonly byte[] content;

            // Token: 0x04000192 RID: 402
            private readonly string name;
        }
    }
}