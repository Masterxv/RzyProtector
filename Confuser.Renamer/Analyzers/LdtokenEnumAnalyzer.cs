using System;
using Confuser.Core;
using Confuser.Renamer.References;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Confuser.Renamer.Analyzers
{
    // Token: 0x02000006 RID: 6
    internal class LdtokenEnumAnalyzer : IRenamer
    {
        // Token: 0x0600001F RID: 31 RVA: 0x00002FC4 File Offset: 0x000011C4
        public void Analyze(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def)
        {
            MethodDef method = def as MethodDef;
            if (method == null || !method.HasBody)
            {
                return;
            }
            for (int i = 0; i < method.Body.Instructions.Count; i++)
            {
                Instruction instr = method.Body.Instructions[i];
                if (instr.OpCode.Code == Code.Ldtoken)
                {
                    if (instr.Operand is MemberRef)
                    {
                        IMemberForwarded member = ((MemberRef)instr.Operand).ResolveThrow();
                        if (context.Modules.Contains((ModuleDefMD)member.Module))
                        {
                            service.SetCanRename(member, false);
                        }
                    }
                    else if (instr.Operand is IField)
                    {
                        FieldDef field = ((IField)instr.Operand).ResolveThrow();
                        if (context.Modules.Contains((ModuleDefMD)field.Module))
                        {
                            service.SetCanRename(field, false);
                        }
                    }
                    else if (instr.Operand is IMethod)
                    {
                        IMethod im = (IMethod)instr.Operand;
                        if (!im.IsArrayAccessors())
                        {
                            MethodDef j = im.ResolveThrow();
                            if (context.Modules.Contains((ModuleDefMD)j.Module))
                            {
                                service.SetCanRename(method, false);
                            }
                        }
                    }
                    else
                    {
                        if (!(instr.Operand is ITypeDefOrRef))
                        {
                            throw new UnreachableException();
                        }
                        if (!(instr.Operand is TypeSpec))
                        {
                            TypeDef type = ((ITypeDefOrRef)instr.Operand).ResolveTypeDefThrow();
                            if (context.Modules.Contains((ModuleDefMD)type.Module) && this.HandleTypeOf(context, service, method, i))
                            {
                                TypeDef t = type;
                                do
                                {
                                    this.DisableRename(service, t, false);
                                    t = t.DeclaringType;
                                }
                                while (t != null);
                            }
                        }
                    }
                }
                else if ((instr.OpCode.Code == Code.Call || instr.OpCode.Code == Code.Callvirt) && ((IMethod)instr.Operand).Name == "ToString")
                {
                    this.HandleEnum(context, service, method, i);
                }
                else if (instr.OpCode.Code == Code.Ldstr)
                {
                    TypeDef typeDef = method.Module.FindReflection((string)instr.Operand);
                    if (typeDef != null)
                    {
                        service.AddReference<TypeDef>(typeDef, new StringTypeReference(instr, typeDef));
                    }
                }
            }
        }

        // Token: 0x06000024 RID: 36 RVA: 0x00003614 File Offset: 0x00001814
        private void DisableRename(INameService service, TypeDef typeDef, bool memberOnly = true)
        {
            service.SetCanRename(typeDef, false);
            foreach (MethodDef i in typeDef.Methods)
            {
                service.SetCanRename(i, false);
            }
            foreach (FieldDef field in typeDef.Fields)
            {
                service.SetCanRename(field, false);
            }
            foreach (PropertyDef prop in typeDef.Properties)
            {
                service.SetCanRename(prop, false);
            }
            foreach (EventDef evt in typeDef.Events)
            {
                service.SetCanRename(evt, false);
            }
            foreach (TypeDef nested in typeDef.NestedTypes)
            {
                this.DisableRename(service, nested, false);
            }
        }

        // Token: 0x06000022 RID: 34 RVA: 0x0000321C File Offset: 0x0000141C
        private void HandleEnum(ConfuserContext context, INameService service, MethodDef method, int index)
        {
            IMethod target = (IMethod)method.Body.Instructions[index].Operand;
            if (target.FullName == "System.String System.Object::ToString()" || target.FullName == "System.String System.Enum::ToString(System.String)")
            {
                int prevIndex = index - 1;
                while (prevIndex >= 0 && method.Body.Instructions[prevIndex].OpCode.Code == Code.Nop)
                {
                    prevIndex--;
                }
                if (prevIndex < 0)
                {
                    return;
                }
                Instruction prevInstr = method.Body.Instructions[prevIndex];
                TypeSig targetType;
                if (prevInstr.Operand is MemberRef)
                {
                    MemberRef memberRef = (MemberRef)prevInstr.Operand;
                    targetType = (memberRef.IsFieldRef ? memberRef.FieldSig.Type : memberRef.MethodSig.RetType);
                }
                else if (prevInstr.Operand is IField)
                {
                    targetType = ((IField)prevInstr.Operand).FieldSig.Type;
                }
                else if (prevInstr.Operand is IMethod)
                {
                    targetType = ((IMethod)prevInstr.Operand).MethodSig.RetType;
                }
                else if (prevInstr.Operand is ITypeDefOrRef)
                {
                    targetType = ((ITypeDefOrRef)prevInstr.Operand).ToTypeSig();
                }
                else if (prevInstr.GetParameter(method.Parameters) != null)
                {
                    targetType = prevInstr.GetParameter(method.Parameters).Type;
                }
                else
                {
                    if (prevInstr.GetLocal(method.Body.Variables) == null)
                    {
                        return;
                    }
                    targetType = prevInstr.GetLocal(method.Body.Variables).Type;
                }
                ITypeDefOrRef targetTypeRef = targetType.ToBasicTypeDefOrRef();
                if (targetTypeRef == null)
                {
                    return;
                }
                TypeDef targetTypeDef = targetTypeRef.ResolveTypeDefThrow();
                if (targetTypeDef != null && targetTypeDef.IsEnum && context.Modules.Contains((ModuleDefMD)targetTypeDef.Module))
                {
                    this.DisableRename(service, targetTypeDef, true);
                }
            }
        }

        // Token: 0x06000023 RID: 35 RVA: 0x000033F8 File Offset: 0x000015F8
        private bool HandleTypeOf(ConfuserContext context, INameService service, MethodDef method, int index)
        {
            if (index + 1 >= method.Body.Instructions.Count)
            {
                return true;
            }
            IMethod gtfh = method.Body.Instructions[index + 1].Operand as IMethod;
            if (gtfh == null || gtfh.FullName != "System.Type System.Type::GetTypeFromHandle(System.RuntimeTypeHandle)")
            {
                return true;
            }
            if (index + 2 < method.Body.Instructions.Count)
            {
                Instruction instr = method.Body.Instructions[index + 2];
                IMethod operand = instr.Operand as IMethod;
                if (instr.OpCode == OpCodes.Newobj && operand.FullName == "System.Void System.ComponentModel.ComponentResourceManager::.ctor(System.Type)")
                {
                    return false;
                }
                string fullName;
                if ((instr.OpCode == OpCodes.Call || instr.OpCode == OpCodes.Callvirt) && (fullName = operand.DeclaringType.FullName) != null)
                {
                    if (fullName == "System.Runtime.InteropServices.Marshal")
                    {
                        return false;
                    }
                    if (fullName == "System.Type")
                    {
                        return operand.Name.StartsWith("Get") || operand.Name == "InvokeMember" || (operand.Name == "get_AssemblyQualifiedName" || operand.Name == "get_FullName" || operand.Name == "get_Namespace");
                    }
                    if (fullName == "System.Reflection.MemberInfo")
                    {
                        return operand.Name == "get_Name";
                    }
                    if (fullName == "System.Object")
                    {
                        return operand.Name == "ToString";
                    }
                }
            }
            if (index + 3 < method.Body.Instructions.Count)
            {
                Instruction instr2 = method.Body.Instructions[index + 3];
                IMethod operand2 = instr2.Operand as IMethod;
                string fullName2;
                if ((instr2.OpCode == OpCodes.Call || instr2.OpCode == OpCodes.Callvirt) && (fullName2 = operand2.DeclaringType.FullName) != null && fullName2 == "System.Runtime.InteropServices.Marshal")
                {
                    return false;
                }
            }
            return false;
        }

        // Token: 0x06000021 RID: 33 RVA: 0x00003217 File Offset: 0x00001417
        public void PostRename(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def)
        {
        }

        // Token: 0x06000020 RID: 32 RVA: 0x00003215 File Offset: 0x00001415
        public void PreRename(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def)
        {
        }
    }
}
