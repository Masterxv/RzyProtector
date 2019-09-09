using System;
using System.Collections.Generic;
using System.Linq;
using Confuser.Core;
using Confuser.Renamer.References;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.MD;

namespace Confuser.Renamer.Analyzers
{
    // Token: 0x02000008 RID: 8
    internal class TypeBlobAnalyzer : IRenamer
    {
        // Token: 0x0600002B RID: 43 RVA: 0x00003AAC File Offset: 0x00001CAC
        public void Analyze(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def)
        {
            ModuleDefMD module = def as ModuleDefMD;
            if (module == null)
            {
                return;
            }
            MDTable table = module.TablesStream.Get(Table.Method);
            uint len = table.Rows;
            IEnumerable<MethodDef> methods = from rid in Enumerable.Range(1, (int)len)
                                             select module.ResolveMethod((uint)rid);
            foreach (MethodDef method in methods)
            {
                foreach (MethodOverride methodImpl in method.Overrides)
                {
                    if (methodImpl.MethodBody is MemberRef)
                    {
                        this.AnalyzeMemberRef(context, service, (MemberRef)methodImpl.MethodBody);
                    }
                    if (methodImpl.MethodDeclaration is MemberRef)
                    {
                        this.AnalyzeMemberRef(context, service, (MemberRef)methodImpl.MethodDeclaration);
                    }
                }
                if (method.HasBody)
                {
                    foreach (Instruction instr in method.Body.Instructions)
                    {
                        if (instr.Operand is MemberRef)
                        {
                            this.AnalyzeMemberRef(context, service, (MemberRef)instr.Operand);
                        }
                        else if (instr.Operand is MethodSpec)
                        {
                            MethodSpec spec = (MethodSpec)instr.Operand;
                            if (spec.Method is MemberRef)
                            {
                                this.AnalyzeMemberRef(context, service, (MemberRef)spec.Method);
                            }
                        }
                    }
                }
            }
            table = module.TablesStream.Get(Table.CustomAttribute);
            len = table.Rows;
            IEnumerable<CustomAttribute> attrs = (from rid in Enumerable.Range(1, (int)len)
                                                  select module.ResolveHasCustomAttribute(module.TablesStream.ReadCustomAttributeRow((uint)rid).Parent)).Distinct<IHasCustomAttribute>().SelectMany((IHasCustomAttribute owner) => owner.CustomAttributes);
            foreach (CustomAttribute attr in attrs)
            {
                if (attr.Constructor is MemberRef)
                {
                    this.AnalyzeMemberRef(context, service, (MemberRef)attr.Constructor);
                }
                foreach (CAArgument arg in attr.ConstructorArguments)
                {
                    this.AnalyzeCAArgument(context, service, arg);
                }
                foreach (CANamedArgument arg2 in attr.Fields)
                {
                    this.AnalyzeCAArgument(context, service, arg2.Argument);
                }
                foreach (CANamedArgument arg3 in attr.Properties)
                {
                    this.AnalyzeCAArgument(context, service, arg3.Argument);
                }
                TypeDef attrType = attr.AttributeType.ResolveTypeDefThrow();
                if (context.Modules.Contains((ModuleDefMD)attrType.Module))
                {
                    foreach (CANamedArgument fieldArg in attr.Fields)
                    {
                        FieldDef field = attrType.FindField(fieldArg.Name, new FieldSig(fieldArg.Type));
                        if (field == null)
                        {
                            context.Logger.WarnFormat("Failed to resolve CA field '{0}::{1} : {2}'.", new object[]
                            {
                                attrType,
                                fieldArg.Name,
                                fieldArg.Type
                            });
                        }
                        else
                        {
                            service.AddReference<IDnlibDef>(field, new CAMemberReference(fieldArg, field));
                        }
                    }
                    foreach (CANamedArgument propertyArg in attr.Properties)
                    {
                        PropertyDef property = attrType.FindProperty(propertyArg.Name, new PropertySig(true, propertyArg.Type));
                        if (property == null)
                        {
                            context.Logger.WarnFormat("Failed to resolve CA property '{0}::{1} : {2}'.", new object[]
                            {
                                attrType,
                                propertyArg.Name,
                                propertyArg.Type
                            });
                        }
                        else
                        {
                            service.AddReference<IDnlibDef>(property, new CAMemberReference(propertyArg, property));
                        }
                    }
                }
            }
        }

        // Token: 0x0600002E RID: 46 RVA: 0x00003FF4 File Offset: 0x000021F4
        private void AnalyzeCAArgument(ConfuserContext context, INameService service, CAArgument arg)
        {
            if (arg.Type.DefinitionAssembly.IsCorLib() && arg.Type.FullName == "System.Type")
            {
                TypeSig typeSig = (TypeSig)arg.Value;
                using (IEnumerator<ITypeDefOrRef> enumerator = typeSig.FindTypeRefs().GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        ITypeDefOrRef typeRef = enumerator.Current;
                        TypeDef typeDef = typeRef.ResolveTypeDefThrow();
                        if (context.Modules.Contains((ModuleDefMD)typeDef.Module))
                        {
                            if (typeRef is TypeRef)
                            {
                                service.AddReference<TypeDef>(typeDef, new TypeRefReference((TypeRef)typeRef, typeDef));
                            }
                            service.ReduceRenameMode(typeDef, RenameMode.ASCII);
                        }
                    }
                    return;
                }
            }
            if (arg.Value is CAArgument[])
            {
                CAArgument[] array = (CAArgument[])arg.Value;
                for (int i = 0; i < array.Length; i++)
                {
                    CAArgument elem = array[i];
                    this.AnalyzeCAArgument(context, service, elem);
                }
            }
        }

        // Token: 0x0600002F RID: 47 RVA: 0x00004104 File Offset: 0x00002304
        private void AnalyzeMemberRef(ConfuserContext context, INameService service, MemberRef memberRef)
        {
            ITypeDefOrRef declType = memberRef.DeclaringType;
            TypeSpec typeSpec = declType as TypeSpec;
            if (typeSpec == null)
            {
                return;
            }
            TypeSig sig = typeSpec.TypeSig;
            while (sig.Next != null)
            {
                sig = sig.Next;
            }
            if (sig is GenericInstSig)
            {
                GenericInstSig inst = (GenericInstSig)sig;
                TypeDef openType = inst.GenericType.TypeDefOrRef.ResolveTypeDefThrow();
                if (!context.Modules.Contains((ModuleDefMD)openType.Module) || memberRef.IsArrayAccessors())
                {
                    return;
                }
                IDnlibDef member;
                if (memberRef.IsFieldRef)
                {
                    member = memberRef.ResolveFieldThrow();
                }
                else
                {
                    if (!memberRef.IsMethodRef)
                    {
                        throw new UnreachableException();
                    }
                    member = memberRef.ResolveMethodThrow();
                }
                service.AddReference<IDnlibDef>(member, new MemberRefReference(memberRef, member));
            }
        }

        // Token: 0x0600002D RID: 45 RVA: 0x00003FF2 File Offset: 0x000021F2
        public void PostRename(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def)
        {
        }

        // Token: 0x0600002C RID: 44 RVA: 0x00003FF0 File Offset: 0x000021F0
        public void PreRename(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def)
        {
        }
    }
}
