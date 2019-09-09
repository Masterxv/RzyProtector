using System;
using System.Collections.Generic;
using Confuser.Core;
using Confuser.Renamer.References;
using dnlib.DotNet;

namespace Confuser.Renamer.Analyzers
{
    // Token: 0x02000009 RID: 9
    internal class VTableAnalyzer : IRenamer
    {
        // Token: 0x06000032 RID: 50 RVA: 0x000041C0 File Offset: 0x000023C0
        public void Analyze(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def)
        {
            if (def is TypeDef)
            {
                TypeDef type = (TypeDef)def;
                if (type.IsInterface)
                {
                    return;
                }
                VTable vTbl = service.GetVTables()[type];
                using (IEnumerator<IList<VTableSlot>> enumerator = vTbl.InterfaceSlots.Values.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        IList<VTableSlot> ifaceVTbl = enumerator.Current;
                        foreach (VTableSlot slot in ifaceVTbl)
                        {
                            if (slot.Overrides != null)
                            {
                                bool baseUnderCtrl = context.Modules.Contains(slot.MethodDef.DeclaringType.Module as ModuleDefMD);
                                bool ifaceUnderCtrl = context.Modules.Contains(slot.Overrides.MethodDef.DeclaringType.Module as ModuleDefMD);
                                if ((!baseUnderCtrl && ifaceUnderCtrl) || !service.CanRename(slot.MethodDef))
                                {
                                    service.SetCanRename(slot.Overrides.MethodDef, false);
                                }
                                else if ((baseUnderCtrl && !ifaceUnderCtrl) || !service.CanRename(slot.Overrides.MethodDef))
                                {
                                    service.SetCanRename(slot.MethodDef, false);
                                }
                            }
                        }
                    }
                    return;
                }
            }
            if (def is MethodDef)
            {
                MethodDef method = (MethodDef)def;
                if (!method.IsVirtual)
                {
                    return;
                }
                VTable vTbl = service.GetVTables()[method.DeclaringType];
                VTableSignature.FromMethod(method);
                IEnumerable<VTableSlot> slots = vTbl.FindSlots(method);
                if (!method.IsAbstract)
                {
                    using (IEnumerator<VTableSlot> enumerator3 = slots.GetEnumerator())
                    {
                        while (enumerator3.MoveNext())
                        {
                            VTableSlot slot2 = enumerator3.Current;
                            if (slot2.Overrides != null)
                            {
                                service.AddReference<MethodDef>(method, new OverrideDirectiveReference(slot2, slot2.Overrides));
                                service.AddReference<MethodDef>(slot2.Overrides.MethodDef, new OverrideDirectiveReference(slot2, slot2.Overrides));
                            }
                        }
                        return;
                    }
                }
                foreach (VTableSlot slot3 in slots)
                {
                    if (slot3.Overrides != null)
                    {
                        service.SetCanRename(method, false);
                        service.SetCanRename(slot3.Overrides.MethodDef, false);
                    }
                }
            }
        }

        // Token: 0x06000034 RID: 52 RVA: 0x00004468 File Offset: 0x00002668
        public void PostRename(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def)
        {
            MethodDef method = def as MethodDef;
            if (method == null || !method.IsVirtual || method.Overrides.Count == 0)
            {
                return;
            }
            new HashSet<IMethodDefOrRef>(VTableAnalyzer.MethodDefOrRefComparer.Instance);
            method.Overrides.RemoveWhere((MethodOverride impl) => VTableAnalyzer.MethodDefOrRefComparer.Instance.Equals(impl.MethodDeclaration, method));
        }

        // Token: 0x06000033 RID: 51 RVA: 0x00004444 File Offset: 0x00002644
        public void PreRename(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def)
        {
        }

        // Token: 0x0200000A RID: 10
        private class MethodDefOrRefComparer : IEqualityComparer<IMethodDefOrRef>
        {
            // Token: 0x06000036 RID: 54 RVA: 0x000044E0 File Offset: 0x000026E0
            private MethodDefOrRefComparer()
            {
            }

            // Token: 0x06000037 RID: 55 RVA: 0x000044E8 File Offset: 0x000026E8
            public bool Equals(IMethodDefOrRef x, IMethodDefOrRef y)
            {
                return default(SigComparer).Equals(x, y) && default(SigComparer).Equals(x.DeclaringType, y.DeclaringType);
            }

            // Token: 0x06000038 RID: 56 RVA: 0x0000452C File Offset: 0x0000272C
            public int GetHashCode(IMethodDefOrRef obj)
            {
                return default(SigComparer).GetHashCode(obj) * 5 + default(SigComparer).GetHashCode(obj.DeclaringType);
            }

            // Token: 0x04000003 RID: 3
            public static readonly VTableAnalyzer.MethodDefOrRefComparer Instance = new VTableAnalyzer.MethodDefOrRefComparer();
        }
    }
}
