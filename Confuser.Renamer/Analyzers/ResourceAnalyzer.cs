using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Confuser.Core;
using Confuser.Renamer.References;
using dnlib.DotNet;

namespace Confuser.Renamer.Analyzers
{
    // Token: 0x02000007 RID: 7
    internal class ResourceAnalyzer : IRenamer
    {
        // Token: 0x06000026 RID: 38 RVA: 0x000037A4 File Offset: 0x000019A4
        public void Analyze(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def)
        {
            ModuleDef module = def as ModuleDef;
            if (module == null)
            {
                return;
            }
            string asmName = module.Assembly.Name.String;
            if (!string.IsNullOrEmpty(module.Assembly.Culture) && asmName.EndsWith(".resources"))
            {
                Regex satellitePattern = new Regex(string.Format("^(.*)\\.{0}\\.resources$", module.Assembly.Culture));
                string nameAsmName = asmName.Substring(0, asmName.Length - ".resources".Length);
                ModuleDef mainModule = context.Modules.SingleOrDefault((ModuleDefMD mod) => mod.Assembly.Name == nameAsmName);
                if (mainModule == null)
                {
                    context.Logger.ErrorFormat("Could not find main assembly of satellite assembly '{0}'.", new object[]
                    {
                        module.Assembly.FullName
                    });
                    throw new ConfuserException(null);
                }
                string format = "{0}." + module.Assembly.Culture + ".resources";
                using (IEnumerator<Resource> enumerator = module.Resources.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Resource res = enumerator.Current;
                        Match match = satellitePattern.Match(res.Name);
                        if (match.Success)
                        {
                            string typeName = match.Groups[1].Value;
                            TypeDef type = mainModule.FindReflectionThrow(typeName);
                            if (type == null)
                            {
                                context.Logger.WarnFormat("Could not find resource type '{0}'.", new object[]
                                {
                                    typeName
                                });
                            }
                            else
                            {
                                service.ReduceRenameMode(type, RenameMode.ASCII);
                                service.AddReference<TypeDef>(type, new ResourceReference(res, type, format));
                            }
                        }
                    }
                    return;
                }
            }
            string format2 = "{0}.resources";
            foreach (Resource res2 in module.Resources)
            {
                Match match2 = ResourceAnalyzer.ResourceNamePattern.Match(res2.Name);
                if (match2.Success && res2.ResourceType == ResourceType.Embedded)
                {
                    string typeName2 = match2.Groups[1].Value;
                    if (!typeName2.EndsWith(".g"))
                    {
                        TypeDef type2 = module.FindReflection(typeName2);
                        if (type2 == null)
                        {
                            context.Logger.WarnFormat("Could not find resource type '{0}'.", new object[]
                            {
                                typeName2
                            });
                        }
                        else
                        {
                            service.ReduceRenameMode(type2, RenameMode.ASCII);
                            service.AddReference<TypeDef>(type2, new ResourceReference(res2, type2, format2));
                        }
                    }
                }
            }
        }

        // Token: 0x06000028 RID: 40 RVA: 0x00003A4E File Offset: 0x00001C4E
        public void PostRename(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def)
        {
        }

        // Token: 0x06000027 RID: 39 RVA: 0x00003A4C File Offset: 0x00001C4C
        public void PreRename(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def)
        {
        }

        // Token: 0x04000001 RID: 1
        private static readonly Regex ResourceNamePattern = new Regex("^(.*)\\.resources$");
    }
}
