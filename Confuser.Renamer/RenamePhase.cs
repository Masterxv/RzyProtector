using System;
using System.Collections.Generic;
using System.Linq;
using Confuser.Core;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Confuser.Renamer
{
    // Token: 0x02000073 RID: 115
    internal class RenamePhase : ProtectionPhase
    {
        // Token: 0x060002A4 RID: 676 RVA: 0x000020D8 File Offset: 0x000002D8
        public RenamePhase(NameProtection parent) : base(parent)
        {
        }

        // Token: 0x170000A8 RID: 168
        // (get) Token: 0x060002A5 RID: 677 RVA: 0x000020E4 File Offset: 0x000002E4
        public override ProtectionTargets Targets
        {
            get
            {
                return ProtectionTargets.AllDefinitions;
            }
        }

        // Token: 0x170000A9 RID: 169
        // (get) Token: 0x060002A6 RID: 678 RVA: 0x0000384F File Offset: 0x00001A4F
        public override string Name
        {
            get
            {
                return "Renaming";
            }
        }

        // Token: 0x060002A7 RID: 679 RVA: 0x00020498 File Offset: 0x0001E698
        protected override void Execute(ConfuserContext context, ProtectionParameters parameters)
        {
            NameService service = (NameService)context.Registry.GetService<INameService>();
            context.Logger.Debug("Renaming...");
            foreach (IRenamer renamer in service.Renamers)
            {
                foreach (IDnlibDef def in parameters.Targets)
                {
                    renamer.PreRename(context, service, parameters, def);
                }
                context.CheckCancellation();
            }
            HashSet<string> pdbDocs = new HashSet<string>();
            foreach (IDnlibDef def2 in parameters.Targets.WithProgress(context.Logger))
            {
                if (def2 is ModuleDef && parameters.GetParameter<bool>(context, def2, "rickroll", true))
                {
                    RickRoller.CommenceRickroll(context, (ModuleDef)def2);
                }
                bool canRename = service.CanRename(def2);
                RenameMode mode = service.GetRenameMode(def2);
                if (def2 is MethodDef)
                {
                    MethodDef method = (MethodDef)def2;
                    if (canRename && parameters.GetParameter<bool>(context, def2, "renameArgs", true))
                    {
                        foreach (ParamDef param in ((MethodDef)def2).ParamDefs)
                        {
                            param.Name = null;
                        }
                    }
                    if (parameters.GetParameter<bool>(context, def2, "renPdb", true) && method.HasBody)
                    {
                        foreach (Instruction instr in method.Body.Instructions)
                        {
                            if (instr.SequencePoint != null && !pdbDocs.Contains(instr.SequencePoint.Document.Url))
                            {
                                instr.SequencePoint.Document.Url = service.ObfuscateName(instr.SequencePoint.Document.Url, mode);
                                pdbDocs.Add(instr.SequencePoint.Document.Url);
                            }
                        }
                        foreach (Local local in method.Body.Variables)
                        {
                            if (!string.IsNullOrEmpty(local.Name))
                            {
                                local.Name = service.ObfuscateName(local.Name, mode);
                            }
                        }
                        method.Body.Scope = null;
                    }
                }
                if (canRename)
                {
                    IList<INameReference> references = service.GetReferences(def2);
                    bool cancel = false;
                    foreach (INameReference refer in references)
                    {
                        cancel |= refer.ShouldCancelRename();
                        if (cancel)
                        {
                            break;
                        }
                    }
                    if (!cancel)
                    {
                        if (def2 is TypeDef)
                        {
                            TypeDef typeDef = (TypeDef)def2;
                            if (parameters.GetParameter<bool>(context, def2, "flatten", true))
                            {
                                typeDef.Name = service.ObfuscateName(typeDef.FullName, mode);
                                typeDef.Namespace = "";
                            }
                            else
                            {
                                typeDef.Namespace = service.ObfuscateName(typeDef.Namespace, mode);
                                typeDef.Name = service.ObfuscateName(typeDef.Name, mode);
                            }
                            using (IEnumerator<GenericParam> enumerator8 = typeDef.GenericParameters.GetEnumerator())
                            {
                                while (enumerator8.MoveNext())
                                {
                                    GenericParam param2 = enumerator8.Current;
                                    param2.Name = ((char)(param2.Number + 1)).ToString();
                                }
                                goto IL_455;
                            }
                            goto IL_3B6;
                        }
                        goto IL_3B6;
                        IL_455:
                        foreach (INameReference refer2 in references.ToList<INameReference>())
                        {
                            if (!refer2.UpdateNameReference(context, service))
                            {
                                context.Logger.ErrorFormat("Failed to update name reference on '{0}'.", new object[]
                                {
                                    def2
                                });
                                throw new ConfuserException(null);
                            }
                        }
                        context.CheckCancellation();
                        continue;
                        IL_3B6:
                        if (def2 is MethodDef)
                        {
                            foreach (GenericParam param3 in ((MethodDef)def2).GenericParameters)
                            {
                                param3.Name = ((char)(param3.Number + 1)).ToString();
                            }
                            def2.Name = service.ObfuscateName(def2.Name, mode);
                            goto IL_455;
                        }
                        def2.Name = service.ObfuscateName(def2.Name, mode);
                        goto IL_455;
                    }
                }
            }
        }
    }
}
