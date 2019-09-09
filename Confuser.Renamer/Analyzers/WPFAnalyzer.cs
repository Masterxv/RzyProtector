using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text.RegularExpressions;
using Confuser.Core;
using Confuser.Core.Services;
using Confuser.Renamer.BAML;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.IO;

namespace Confuser.Renamer.Analyzers
{
    // Token: 0x0200000C RID: 12
    internal class WPFAnalyzer : IRenamer
    {
        // Token: 0x06000041 RID: 65 RVA: 0x00004A90 File Offset: 0x00002C90
        public void Analyze(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def)
        {
            MethodDef method = def as MethodDef;
            if (method != null)
            {
                if (!method.HasBody)
                {
                    return;
                }
                this.AnalyzeMethod(context, service, method);
            }
            ModuleDefMD module = def as ModuleDefMD;
            if (module != null)
            {
                this.AnalyzeResources(context, service, module);
            }
        }

        // Token: 0x06000044 RID: 68 RVA: 0x00004ED4 File Offset: 0x000030D4
        private void AnalyzeMethod(ConfuserContext context, INameService service, MethodDef method)
        {
            List<Tuple<bool, Instruction>> dpRegInstrs = new List<Tuple<bool, Instruction>>();
            List<Instruction> routedEvtRegInstrs = new List<Instruction>();
            foreach (Instruction instr in method.Body.Instructions)
            {
                if (instr.OpCode.Code == Code.Call || instr.OpCode.Code == Code.Callvirt)
                {
                    IMethod regMethod = (IMethod)instr.Operand;
                    if (regMethod.DeclaringType.FullName == "System.Windows.DependencyProperty" && regMethod.Name.String.StartsWith("Register"))
                    {
                        dpRegInstrs.Add(Tuple.Create<bool, Instruction>(regMethod.Name.String.StartsWith("RegisterAttached"), instr));
                    }
                    else if (regMethod.DeclaringType.FullName == "System.Windows.EventManager" && regMethod.Name.String == "RegisterRoutedEvent")
                    {
                        routedEvtRegInstrs.Add(instr);
                    }
                }
                else if (instr.OpCode == OpCodes.Ldstr)
                {
                    string operand = ((string)instr.Operand).ToUpperInvariant();
                    if (operand.EndsWith(".BAML") || operand.EndsWith(".XAML"))
                    {
                        Match match = WPFAnalyzer.UriPattern.Match(operand);
                        if (match.Success)
                        {
                            operand = match.Groups[1].Value;
                        }
                        BAMLStringReference reference = new BAMLStringReference(instr);
                        operand = operand.TrimStart(new char[]
                        {
                            '/'
                        });
                        string baml = operand.Substring(0, operand.Length - 5) + ".BAML";
                        string xaml = operand.Substring(0, operand.Length - 5) + ".XAML";
                        this.bamlRefs.AddListEntry(baml, reference);
                        this.bamlRefs.AddListEntry(xaml, reference);
                    }
                }
            }
            if (dpRegInstrs.Count == 0)
            {
                return;
            }
            ITraceService traceSrv = context.Registry.GetService<ITraceService>();
            MethodTrace trace = traceSrv.Trace(method);
            bool erred = false;
            foreach (Tuple<bool, Instruction> instrInfo in dpRegInstrs)
            {
                int[] args = trace.TraceArguments(instrInfo.Item2);
                if (args == null)
                {
                    if (!erred)
                    {
                        context.Logger.WarnFormat("Failed to extract dependency property name in '{0}'.", new object[]
                        {
                            method.FullName
                        });
                    }
                    erred = true;
                }
                else
                {
                    Instruction ldstr = method.Body.Instructions[args[0]];
                    if (ldstr.OpCode.Code != Code.Ldstr)
                    {
                        if (!erred)
                        {
                            context.Logger.WarnFormat("Failed to extract dependency property name in '{0}'.", new object[]
                            {
                                method.FullName
                            });
                        }
                        erred = true;
                    }
                    else
                    {
                        string name = (string)ldstr.Operand;
                        TypeDef declType = method.DeclaringType;
                        bool found = false;
                        if (instrInfo.Item1)
                        {
                            MethodDef accessor;
                            if ((accessor = declType.FindMethod("Get" + name)) != null && accessor.IsStatic)
                            {
                                service.SetCanRename(accessor, false);
                                found = true;
                            }
                            if ((accessor = declType.FindMethod("Set" + name)) != null && accessor.IsStatic)
                            {
                                service.SetCanRename(accessor, false);
                                found = true;
                            }
                        }
                        PropertyDef property;
                        if ((property = declType.FindProperty(name)) != null)
                        {
                            service.SetCanRename(property, false);
                            found = true;
                            if (property.GetMethod != null)
                            {
                                service.SetCanRename(property.GetMethod, false);
                            }
                            if (property.SetMethod != null)
                            {
                                service.SetCanRename(property.SetMethod, false);
                            }
                            if (property.HasOtherMethods)
                            {
                                foreach (MethodDef accessor2 in property.OtherMethods)
                                {
                                    service.SetCanRename(accessor2, false);
                                }
                            }
                        }
                        if (!found)
                        {
                            if (instrInfo.Item1)
                            {
                                context.Logger.WarnFormat("Failed to find the accessors of attached dependency property '{0}' in type '{1}'.", new object[]
                                {
                                    name,
                                    declType.FullName
                                });
                            }
                            else
                            {
                                context.Logger.WarnFormat("Failed to find the CLR property of normal dependency property '{0}' in type '{1}'.", new object[]
                                {
                                    name,
                                    declType.FullName
                                });
                            }
                        }
                    }
                }
            }
            erred = false;
            foreach (Instruction instr2 in routedEvtRegInstrs)
            {
                int[] args2 = trace.TraceArguments(instr2);
                if (args2 == null)
                {
                    if (!erred)
                    {
                        context.Logger.WarnFormat("Failed to extract routed event name in '{0}'.", new object[]
                        {
                            method.FullName
                        });
                    }
                    erred = true;
                }
                else
                {
                    Instruction ldstr2 = method.Body.Instructions[args2[0]];
                    if (ldstr2.OpCode.Code != Code.Ldstr)
                    {
                        if (!erred)
                        {
                            context.Logger.WarnFormat("Failed to extract routed event name in '{0}'.", new object[]
                            {
                                method.FullName
                            });
                        }
                        erred = true;
                    }
                    else
                    {
                        string name2 = (string)ldstr2.Operand;
                        TypeDef declType2 = method.DeclaringType;
                        EventDef eventDef;
                        if ((eventDef = declType2.FindEvent(name2)) == null)
                        {
                            context.Logger.WarnFormat("Failed to find the CLR event of routed event '{0}' in type '{1}'.", new object[]
                            {
                                name2,
                                declType2.FullName
                            });
                        }
                        else
                        {
                            service.SetCanRename(eventDef, false);
                            if (eventDef.AddMethod != null)
                            {
                                service.SetCanRename(eventDef.AddMethod, false);
                            }
                            if (eventDef.RemoveMethod != null)
                            {
                                service.SetCanRename(eventDef.RemoveMethod, false);
                            }
                            if (eventDef.InvokeMethod != null)
                            {
                                service.SetCanRename(eventDef.InvokeMethod, false);
                            }
                            if (eventDef.HasOtherMethods)
                            {
                                foreach (MethodDef accessor3 in eventDef.OtherMethods)
                                {
                                    service.SetCanRename(accessor3, false);
                                }
                            }
                        }
                    }
                }
            }
        }

        // Token: 0x06000045 RID: 69 RVA: 0x0000556C File Offset: 0x0000376C
        private void AnalyzeResources(ConfuserContext context, INameService service, ModuleDefMD module)
        {
            if (this.analyzer == null)
            {
                this.analyzer = new BAMLAnalyzer(context, service);
                this.analyzer.AnalyzeElement += this.AnalyzeBAMLElement;
            }
            Dictionary<string, Dictionary<string, BamlDocument>> wpfResInfo = new Dictionary<string, Dictionary<string, BamlDocument>>();
            foreach (EmbeddedResource res in module.Resources.OfType<EmbeddedResource>())
            {
                Match match = WPFAnalyzer.ResourceNamePattern.Match(res.Name);
                if (match.Success)
                {
                    Dictionary<string, BamlDocument> resInfo = new Dictionary<string, BamlDocument>();
                    res.Data.Position = 0L;
                    ResourceReader reader = new ResourceReader(new ImageStream(res.Data));
                    IDictionaryEnumerator enumerator = reader.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        string name = (string)enumerator.Key;
                        if (name.EndsWith(".baml"))
                        {
                            string typeName;
                            byte[] data;
                            reader.GetResourceData(name, out typeName, out data);
                            BamlDocument document = this.analyzer.Analyze(module, name, data);
                            document.DocumentName = name;
                            resInfo.Add(name, document);
                        }
                    }
                    if (resInfo.Count > 0)
                    {
                        wpfResInfo.Add(res.Name, resInfo);
                    }
                }
            }
            if (wpfResInfo.Count > 0)
            {
                context.Annotations.Set<Dictionary<string, Dictionary<string, BamlDocument>>>(module, WPFAnalyzer.BAMLKey, wpfResInfo);
            }
        }

        // Token: 0x06000043 RID: 67 RVA: 0x00004D54 File Offset: 0x00002F54
        public void PostRename(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def)
        {
            ModuleDefMD module = def as ModuleDefMD;
            if (module == null)
            {
                return;
            }
            Dictionary<string, Dictionary<string, BamlDocument>> wpfResInfo = context.Annotations.Get<Dictionary<string, Dictionary<string, BamlDocument>>>(module, WPFAnalyzer.BAMLKey, null);
            if (wpfResInfo == null)
            {
                return;
            }
            foreach (EmbeddedResource res in module.Resources.OfType<EmbeddedResource>())
            {
                Dictionary<string, BamlDocument> resInfo;
                if (wpfResInfo.TryGetValue(res.Name, out resInfo))
                {
                    MemoryStream stream = new MemoryStream();
                    ResourceWriter writer = new ResourceWriter(stream);
                    res.Data.Position = 0L;
                    ResourceReader reader = new ResourceReader(new ImageStream(res.Data));
                    IDictionaryEnumerator enumerator = reader.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        string name = (string)enumerator.Key;
                        string typeName;
                        byte[] data;
                        reader.GetResourceData(name, out typeName, out data);
                        BamlDocument document;
                        if (resInfo.TryGetValue(name, out document))
                        {
                            MemoryStream docStream = new MemoryStream();
                            docStream.Position = 4L;
                            BamlWriter.WriteDocument(document, docStream);
                            docStream.Position = 0L;
                            docStream.Write(BitConverter.GetBytes((int)docStream.Length - 4), 0, 4);
                            data = docStream.ToArray();
                            name = document.DocumentName;
                        }
                        writer.AddResourceData(name, typeName, data);
                    }
                    writer.Generate();
                    res.Data = MemoryImageStream.Create(stream.ToArray());
                }
            }
        }

        // Token: 0x06000042 RID: 66 RVA: 0x00004AD0 File Offset: 0x00002CD0
        public void PreRename(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def)
        {
            ModuleDefMD module = def as ModuleDefMD;
            if (module == null || !parameters.GetParameter<bool>(context, def, "renXaml", true))
            {
                return;
            }
            Dictionary<string, Dictionary<string, BamlDocument>> wpfResInfo = context.Annotations.Get<Dictionary<string, Dictionary<string, BamlDocument>>>(module, WPFAnalyzer.BAMLKey, null);
            if (wpfResInfo == null)
            {
                return;
            }
            foreach (Dictionary<string, BamlDocument> res in wpfResInfo.Values)
            {
                foreach (BamlDocument doc in res.Values)
                {
                    List<IBAMLReference> references;
                    if (this.bamlRefs.TryGetValue(doc.DocumentName, out references))
                    {
                        string newName = doc.DocumentName.ToUpperInvariant();
                        string[] completePath = newName.Split(new string[]
                        {
                            "/"
                        }, StringSplitOptions.RemoveEmptyEntries);
                        string newShinyName = string.Empty;
                        for (int i = 0; i <= completePath.Length - 2; i++)
                        {
                            newShinyName = newShinyName + completePath[i].ToLowerInvariant() + "/";
                        }
                        if (newName.EndsWith(".BAML"))
                        {
                            newName = newShinyName + service.RandomName(RenameMode.Letters).ToLowerInvariant() + ".baml";
                        }
                        else if (newName.EndsWith(".XAML"))
                        {
                            newName = newShinyName + service.RandomName(RenameMode.Letters).ToLowerInvariant() + ".xaml";
                        }
                        context.Logger.Debug(string.Format("Preserving virtual paths. Replaced {0} with {1}", doc.DocumentName, newName));
                        bool renameOk = true;
                        foreach (IBAMLReference bamlRef in references)
                        {
                            if (!bamlRef.CanRename(doc.DocumentName, newName))
                            {
                                renameOk = false;
                                break;
                            }
                        }
                        if (renameOk)
                        {
                            foreach (IBAMLReference bamlRef2 in references)
                            {
                                bamlRef2.Rename(doc.DocumentName, newName);
                            }
                            doc.DocumentName = newName;
                        }
                    }
                }
            }
        }

        // Token: 0x14000001 RID: 1
        // Token: 0x0600003F RID: 63 RVA: 0x00004A20 File Offset: 0x00002C20
        // Token: 0x06000040 RID: 64 RVA: 0x00004A58 File Offset: 0x00002C58
        public event Action<BAMLAnalyzer, BamlElement> AnalyzeBAMLElement;

        // Token: 0x04000008 RID: 8
        private BAMLAnalyzer analyzer;

        // Token: 0x04000005 RID: 5
        private static readonly object BAMLKey = new object();

        // Token: 0x04000009 RID: 9
        internal Dictionary<string, List<IBAMLReference>> bamlRefs = new Dictionary<string, List<IBAMLReference>>(StringComparer.OrdinalIgnoreCase);

        // Token: 0x04000006 RID: 6
        private static readonly Regex ResourceNamePattern = new Regex("^.*\\.g\\.resources$");

        // Token: 0x04000007 RID: 7
        internal static readonly Regex UriPattern = new Regex(";COMPONENT/(.+\\.[BX]AML)$");
    }
}
