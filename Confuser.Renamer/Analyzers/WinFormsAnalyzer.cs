using System;
using System.Collections.Generic;
using Confuser.Core;
using Confuser.Core.Services;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Confuser.Renamer.Analyzers
{
    // Token: 0x0200000B RID: 11
    public class WinFormsAnalyzer : IRenamer
    {
        // Token: 0x0600003A RID: 58 RVA: 0x00004574 File Offset: 0x00002774
        public void Analyze(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def)
        {
            if (def is ModuleDef)
            {
                foreach (TypeDef type in ((ModuleDef)def).GetTypes())
                {
                    foreach (PropertyDef prop in type.Properties)
                    {
                        this.properties.AddListEntry(prop.Name, prop);
                    }
                }
                return;
            }
            MethodDef method = def as MethodDef;
            if (method == null || !method.HasBody)
            {
                return;
            }
            this.AnalyzeMethod(context, service, method);
        }

        // Token: 0x0600003B RID: 59 RVA: 0x00004638 File Offset: 0x00002838
        private void AnalyzeMethod(ConfuserContext context, INameService service, MethodDef method)
        {
            List<Tuple<bool, Instruction>> binding = new List<Tuple<bool, Instruction>>();
            foreach (Instruction instr in method.Body.Instructions)
            {
                if (instr.OpCode.Code == Code.Call || instr.OpCode.Code == Code.Callvirt)
                {
                    IMethod target = (IMethod)instr.Operand;
                    if ((target.DeclaringType.FullName == "System.Windows.Forms.ControlBindingsCollection" || target.DeclaringType.FullName == "System.Windows.Forms.BindingsCollection") && target.Name == "Add" && target.MethodSig.Params.Count != 1)
                    {
                        binding.Add(Tuple.Create<bool, Instruction>(true, instr));
                    }
                    else if (target.DeclaringType.FullName == "System.Windows.Forms.Binding" && target.Name.String == ".ctor")
                    {
                        binding.Add(Tuple.Create<bool, Instruction>(false, instr));
                    }
                }
            }
            if (binding.Count == 0)
            {
                return;
            }
            ITraceService traceSrv = context.Registry.GetService<ITraceService>();
            MethodTrace trace = traceSrv.Trace(method);
            bool erred = false;
            foreach (Tuple<bool, Instruction> instrInfo in binding)
            {
                int[] args = trace.TraceArguments(instrInfo.Item2);
                if (args == null)
                {
                    if (!erred)
                    {
                        context.Logger.WarnFormat("Failed to extract binding property name in '{0}'.", new object[]
                        {
                            method.FullName
                        });
                    }
                    erred = true;
                }
                else
                {
                    Instruction propertyName = method.Body.Instructions[args[instrInfo.Item1 ? 1 : 0]];
                    List<PropertyDef> props;
                    if (propertyName.OpCode.Code != Code.Ldstr)
                    {
                        if (!erred)
                        {
                            context.Logger.WarnFormat("Failed to extract binding property name in '{0}'.", new object[]
                            {
                                method.FullName
                            });
                        }
                        erred = true;
                    }
                    else if (!this.properties.TryGetValue((string)propertyName.Operand, out props))
                    {
                        if (!erred)
                        {
                            context.Logger.WarnFormat("Failed to extract target property in '{0}'.", new object[]
                            {
                                method.FullName
                            });
                        }
                        erred = true;
                    }
                    else
                    {
                        foreach (PropertyDef property in props)
                        {
                            service.SetCanRename(property, false);
                        }
                    }
                    Instruction dataMember = method.Body.Instructions[args[2 + (instrInfo.Item1 ? 1 : 0)]];
                    List<PropertyDef> props2;
                    if (dataMember.OpCode.Code != Code.Ldstr)
                    {
                        if (!erred)
                        {
                            context.Logger.WarnFormat("Failed to extract binding property name in '{0}'.", new object[]
                            {
                                method.FullName
                            });
                        }
                        erred = true;
                    }
                    else if (!this.properties.TryGetValue((string)dataMember.Operand, out props2))
                    {
                        if (!erred)
                        {
                            context.Logger.WarnFormat("Failed to extract target property in '{0}'.", new object[]
                            {
                                method.FullName
                            });
                        }
                        erred = true;
                    }
                    else
                    {
                        foreach (PropertyDef property2 in props2)
                        {
                            service.SetCanRename(property2, false);
                        }
                    }
                }
            }
        }

        // Token: 0x0600003D RID: 61 RVA: 0x00004A0A File Offset: 0x00002C0A
        public void PostRename(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def)
        {
        }

        // Token: 0x0600003C RID: 60 RVA: 0x00004A08 File Offset: 0x00002C08
        public void PreRename(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def)
        {
        }

        // Token: 0x04000004 RID: 4
        private Dictionary<string, List<PropertyDef>> properties = new Dictionary<string, List<PropertyDef>>();
    }
}
