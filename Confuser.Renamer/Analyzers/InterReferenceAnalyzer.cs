using System;
using Confuser.Core;
using Confuser.Renamer.References;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.MD;

namespace Confuser.Renamer.Analyzers
{
    // Token: 0x02000005 RID: 5
    internal class InterReferenceAnalyzer : IRenamer
    {
        // Token: 0x0600001A RID: 26 RVA: 0x00002D9C File Offset: 0x00000F9C
        public void Analyze(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def)
        {
            ModuleDefMD module = def as ModuleDefMD;
            if (module == null)
            {
                return;
            }
            MDTable table = module.TablesStream.Get(Table.Method);
            uint len = table.Rows;
            for (uint i = 1u; i <= len; i += 1u)
            {
                MethodDef methodDef = module.ResolveMethod(i);
                foreach (MethodOverride ov in methodDef.Overrides)
                {
                    this.ProcessMemberRef(context, service, module, ov.MethodBody);
                    this.ProcessMemberRef(context, service, module, ov.MethodDeclaration);
                }
                if (methodDef.HasBody)
                {
                    foreach (Instruction instr in methodDef.Body.Instructions)
                    {
                        if (instr.Operand is MemberRef || instr.Operand is MethodSpec)
                        {
                            this.ProcessMemberRef(context, service, module, (IMemberRef)instr.Operand);
                        }
                    }
                }
            }
            table = module.TablesStream.Get(Table.TypeRef);
            len = table.Rows;
            for (uint j = 1u; j <= len; j += 1u)
            {
                TypeRef typeRef = module.ResolveTypeRef(j);
                TypeDef typeDef = typeRef.ResolveTypeDefThrow();
                if (typeDef.Module != module && context.Modules.Contains((ModuleDefMD)typeDef.Module))
                {
                    service.AddReference<TypeDef>(typeDef, new TypeRefReference(typeRef, typeDef));
                }
            }
        }

        // Token: 0x0600001D RID: 29 RVA: 0x00002FB7 File Offset: 0x000011B7
        public void PostRename(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def)
        {
        }

        // Token: 0x0600001C RID: 28 RVA: 0x00002FB5 File Offset: 0x000011B5
        public void PreRename(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def)
        {
        }

        // Token: 0x0600001B RID: 27 RVA: 0x00002F2C File Offset: 0x0000112C
        private void ProcessMemberRef(ConfuserContext context, INameService service, ModuleDefMD module, IMemberRef r)
        {
            MemberRef memberRef = r as MemberRef;
            if (r is MethodSpec)
            {
                memberRef = (((MethodSpec)r).Method as MemberRef);
            }
            if (memberRef != null)
            {
                if (memberRef.DeclaringType.TryGetArraySig() != null)
                {
                    return;
                }
                TypeDef declType = memberRef.DeclaringType.ResolveTypeDefThrow();
                if (declType.Module != module && context.Modules.Contains((ModuleDefMD)declType.Module))
                {
                    IDnlibDef memberDef = (IDnlibDef)declType.ResolveThrow(memberRef);
                    service.AddReference<IDnlibDef>(memberDef, new MemberRefReference(memberRef, memberDef));
                }
            }
        }
    }
}
