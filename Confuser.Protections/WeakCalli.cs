using System;
using System.Linq;
using System.Windows.Forms;
using Confuser.Core;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Confuser.Protections
{
    [AfterProtection("Ki.Constants")]
    internal class WeakCalli : Protection
    {
        public const string _Id = "Weak Calli";
        public const string _FullId = "Ki.WeakCalli";

        public override string Name
        {
            get { return "Call to Calli"; }
        }

        public override string Description
        {
            get { return "This protection will convert calls to calli :/"; }
        }

        public override string Id
        {
            get { return _Id; }
        }

        public override string FullId
        {
            get { return _FullId; }
        }

        public override ProtectionPreset Preset
        {
            get { return ProtectionPreset.Maximum; }
        }

        protected override void Initialize(ConfuserContext context)
        {

        }

        protected override void PopulatePipeline(ProtectionPipeline pipeline)
        {
            pipeline.InsertPreStage(PipelineStage.ProcessModule, new AidsPhase(this));
        }

        class AidsPhase : ProtectionPhase
        {
            public AidsPhase(WeakCalli parent)
                : base(parent) { }

            public override ProtectionTargets Targets
            {
                get { return ProtectionTargets.Methods; }
            }

            public override string Name
            {
                get { return "Call to Calli conversion"; }
            }

            protected override void Execute(ConfuserContext context, ProtectionParameters parameters)
            {
                var cctor = context.CurrentModule.GlobalType.FindOrCreateStaticConstructor();
                foreach (MethodDef method in parameters.Targets.OfType<MethodDef>())
                {
                    if (method.IsConstructor) continue;
                    if (method.Body == null) continue;
                    if (method.HasBody && method.Body.HasInstructions && !method.FullName.Contains("My.") && !method.FullName.Contains(".My") && !method.IsConstructor && !method.DeclaringType.IsGlobalModuleType)
                    {
                        var instr = method.Body.Instructions;
                        for (int i = 0; i < instr.Count(); i++)
                        {
                            if ((instr[i].OpCode == OpCodes.Call || instr[i].OpCode == OpCodes.Callvirt) && (instr[i].Operand is MemberRef member))
                            {
                                if (!IsPublicKEK(context, member, method.Body.Instructions[i])) continue;
                                if (new System.IO.FileInfo(context.CurrentModule.Location).Length > 1000000) // 1000000 = 1mb
                                {
                                    instr[i].OpCode = OpCodes.Ldftn;
                                    instr[i].Operand = member;
                                    instr.Insert(++i, Instruction.Create(OpCodes.Calli, member.MethodSig));
                                }
                                else
                                {
                                    FieldDef field = new FieldDefUser(RandomString(6), new FieldSig(context.CurrentModule.CorLibTypes.Object), FieldAttributes.Public | FieldAttributes.Static);
                                    method.DeclaringType.Fields.Add(field);

                                    cctor.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Stsfld, field));
                                    cctor.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Ldftn, member));

                                    instr[i].OpCode = OpCodes.Ldsfld;
                                    instr[i].Operand = field;
                                    instr.Insert(++i, Instruction.Create(OpCodes.Calli, member.MethodSig));
                                }
                            } 
                        }
                    }
                }
            }

            static bool IsPublicKEK(ConfuserContext context, MemberRef member, Instruction instr)
            {
                if (member.HasThis) return false;
                if (member.ResolveMethodDef().ParamDefs.Any(x => x.IsOut)) return false;
                if (member.ResolveMethodDef().IsVirtual) return false;
                if (member.ResolveMethodDef().ReturnType.FullName.ToLower().Contains("bool")) return false;
                return true;
            }

            public static Random rnd = new Random();
            public static string RandomString(int length)
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                return new string(Enumerable.Repeat(chars, length)
                  .Select(s => s[rnd.Next(s.Length)]).ToArray());
            }
        }
    }
}