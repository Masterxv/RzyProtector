using System;
using System.Collections.Generic;
using dnlib.DotNet;

namespace Confuser.Renamer
{
    // Token: 0x0200005D RID: 93
    public struct GenericArgumentResolver
    {
        // Token: 0x0600022C RID: 556 RVA: 0x0001F2CC File Offset: 0x0001D4CC
        public static TypeSig Resolve(TypeSig typeSig, IList<TypeSig> typeGenArgs)
        {
            if (typeGenArgs == null)
            {
                throw new ArgumentException("No generic arguments to resolve.");
            }
            GenericArgumentResolver resolver = default(GenericArgumentResolver);
            resolver.genericArguments = new GenericArguments();
            resolver.recursionCounter = default(RecursionCounter);
            if (typeGenArgs != null)
            {
                resolver.genericArguments.PushTypeArgs(typeGenArgs);
            }
            return resolver.ResolveGenericArgs(typeSig);
        }

        // Token: 0x0600022D RID: 557 RVA: 0x0001F320 File Offset: 0x0001D520
        public static MethodSig Resolve(MethodSig methodSig, IList<TypeSig> typeGenArgs)
        {
            if (typeGenArgs == null)
            {
                throw new ArgumentException("No generic arguments to resolve.");
            }
            GenericArgumentResolver resolver = default(GenericArgumentResolver);
            resolver.genericArguments = new GenericArguments();
            resolver.recursionCounter = default(RecursionCounter);
            if (typeGenArgs != null)
            {
                resolver.genericArguments.PushTypeArgs(typeGenArgs);
            }
            return resolver.ResolveGenericArgs(methodSig);
        }

        // Token: 0x0600022E RID: 558 RVA: 0x0001F374 File Offset: 0x0001D574
        private bool ReplaceGenericArg(ref TypeSig typeSig)
        {
            if (this.genericArguments == null)
            {
                return false;
            }
            TypeSig newTypeSig = this.genericArguments.Resolve(typeSig);
            if (newTypeSig != typeSig)
            {
                typeSig = newTypeSig;
                return true;
            }
            return false;
        }

        // Token: 0x0600022F RID: 559 RVA: 0x0001F3A4 File Offset: 0x0001D5A4
        private MethodSig ResolveGenericArgs(MethodSig sig)
        {
            if (sig == null)
            {
                return null;
            }
            if (!this.recursionCounter.Increment())
            {
                return null;
            }
            MethodSig result = this.ResolveGenericArgs(new MethodSig(sig.GetCallingConvention()), sig);
            this.recursionCounter.Decrement();
            return result;
        }

        // Token: 0x06000230 RID: 560 RVA: 0x0001F3E4 File Offset: 0x0001D5E4
        private MethodSig ResolveGenericArgs(MethodSig sig, MethodSig old)
        {
            sig.RetType = this.ResolveGenericArgs(old.RetType);
            foreach (TypeSig p in old.Params)
            {
                sig.Params.Add(this.ResolveGenericArgs(p));
            }
            sig.GenParamCount = old.GenParamCount;
            if (sig.ParamsAfterSentinel != null)
            {
                foreach (TypeSig p2 in old.ParamsAfterSentinel)
                {
                    sig.ParamsAfterSentinel.Add(this.ResolveGenericArgs(p2));
                }
            }
            return sig;
        }

        // Token: 0x06000231 RID: 561 RVA: 0x0001F4AC File Offset: 0x0001D6AC
        private TypeSig ResolveGenericArgs(TypeSig typeSig)
        {
            if (!this.recursionCounter.Increment())
            {
                return null;
            }
            if (this.ReplaceGenericArg(ref typeSig))
            {
                this.recursionCounter.Decrement();
                return typeSig;
            }
            ElementType elementType = typeSig.ElementType;
            TypeSig result;
            switch (elementType)
            {
                case ElementType.Ptr:
                    result = new PtrSig(this.ResolveGenericArgs(typeSig.Next));
                    goto IL_265;
                case ElementType.ByRef:
                    result = new ByRefSig(this.ResolveGenericArgs(typeSig.Next));
                    goto IL_265;
                case ElementType.ValueType:
                case ElementType.Class:
                case ElementType.TypedByRef:
                case ElementType.I:
                case ElementType.U:
                case ElementType.R:
                case ElementType.Object:
                    break;
                case ElementType.Var:
                    result = new GenericVar((typeSig as GenericVar).Number);
                    goto IL_265;
                case ElementType.Array:
                    {
                        ArraySig arraySig = (ArraySig)typeSig;
                        List<uint> sizes = new List<uint>(arraySig.Sizes);
                        List<int> lbounds = new List<int>(arraySig.LowerBounds);
                        result = new ArraySig(this.ResolveGenericArgs(typeSig.Next), arraySig.Rank, sizes, lbounds);
                        goto IL_265;
                    }
                case ElementType.GenericInst:
                    {
                        GenericInstSig gis = (GenericInstSig)typeSig;
                        List<TypeSig> genArgs = new List<TypeSig>(gis.GenericArguments.Count);
                        foreach (TypeSig ga in gis.GenericArguments)
                        {
                            genArgs.Add(this.ResolveGenericArgs(ga));
                        }
                        result = new GenericInstSig(this.ResolveGenericArgs(gis.GenericType) as ClassOrValueTypeSig, genArgs);
                        goto IL_265;
                    }
                case ElementType.ValueArray:
                    result = new ValueArraySig(this.ResolveGenericArgs(typeSig.Next), (typeSig as ValueArraySig).Size);
                    goto IL_265;
                case ElementType.FnPtr:
                    throw new NotSupportedException("FnPtr is not supported.");
                case ElementType.SZArray:
                    result = new SZArraySig(this.ResolveGenericArgs(typeSig.Next));
                    goto IL_265;
                case ElementType.MVar:
                    result = new GenericMVar((typeSig as GenericMVar).Number);
                    goto IL_265;
                case ElementType.CModReqd:
                    result = new CModReqdSig((typeSig as ModifierSig).Modifier, this.ResolveGenericArgs(typeSig.Next));
                    goto IL_265;
                case ElementType.CModOpt:
                    result = new CModOptSig((typeSig as ModifierSig).Modifier, this.ResolveGenericArgs(typeSig.Next));
                    goto IL_265;
                default:
                    if (elementType == ElementType.Module)
                    {
                        result = new ModuleSig((typeSig as ModuleSig).Index, this.ResolveGenericArgs(typeSig.Next));
                        goto IL_265;
                    }
                    if (elementType == ElementType.Pinned)
                    {
                        result = new PinnedSig(this.ResolveGenericArgs(typeSig.Next));
                        goto IL_265;
                    }
                    break;
            }
            result = typeSig;
            IL_265:
            this.recursionCounter.Decrement();
            return result;
        }

        // Token: 0x040004D3 RID: 1235
        private GenericArguments genericArguments;

        // Token: 0x040004D4 RID: 1236
        private RecursionCounter recursionCounter;
    }
}
