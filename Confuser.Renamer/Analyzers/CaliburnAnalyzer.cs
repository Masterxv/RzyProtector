using System;
using Confuser.Core;
using Confuser.Renamer.BAML;
using dnlib.DotNet;

namespace Confuser.Renamer.Analyzers
{
    // Token: 0x02000004 RID: 4
    internal class CaliburnAnalyzer : IRenamer
    {
        // Token: 0x06000012 RID: 18 RVA: 0x000028A1 File Offset: 0x00000AA1
        public CaliburnAnalyzer(WPFAnalyzer wpfAnalyzer)
        {
            wpfAnalyzer.AnalyzeBAMLElement += new Action<BAMLAnalyzer, BamlElement>(this.AnalyzeBAMLElement);
        }

        // Token: 0x06000013 RID: 19 RVA: 0x000028BC File Offset: 0x00000ABC
        public void Analyze(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def)
        {
            TypeDef type = def as TypeDef;
            if (type == null || type.DeclaringType != null)
            {
                return;
            }
            if (type.Name.Contains("ViewModel"))
            {
                string viewNs = type.Namespace.Replace("ViewModels", "Views");
                string viewName = type.Name.Replace("PageViewModel", "Page").Replace("ViewModel", "View");
                TypeDef view = type.Module.Find(viewNs + "." + viewName, true);
                if (view != null)
                {
                    service.SetCanRename(type, false);
                    service.SetCanRename(view, false);
                }
                string multiViewNs = type.Namespace + "." + type.Name.Replace("ViewModel", "");
                foreach (TypeDef t in type.Module.Types)
                {
                    if (t.Namespace == multiViewNs)
                    {
                        service.SetCanRename(type, false);
                        service.SetCanRename(t, false);
                    }
                }
            }
        }

        // Token: 0x06000017 RID: 23 RVA: 0x00002D14 File Offset: 0x00000F14
        private void AnalyzeActionMessage(BAMLAnalyzer analyzer, Tuple<IDnlibDef, AttributeInfoRecord, TypeDef> attr, string value)
        {
            if (attr.Item2 == null)
            {
                return;
            }
            TypeDef attrDeclType = analyzer.ResolveType(attr.Item2.OwnerTypeId);
            if (attrDeclType.FullName != "Caliburn.Micro.ActionMessage")
            {
                return;
            }
            foreach (MethodDef method in analyzer.LookupMethod(value))
            {
                analyzer.NameService.SetCanRename(method, false);
            }
        }

        // Token: 0x06000016 RID: 22 RVA: 0x00002C4C File Offset: 0x00000E4C
        private void AnalyzeAutoBind(BAMLAnalyzer analyzer, Tuple<IDnlibDef, AttributeInfoRecord, TypeDef> attr, string value)
        {
            if (!(attr.Item1 is PropertyDef) || ((PropertyDef)attr.Item1).DeclaringType.FullName != "System.Windows.FrameworkElement")
            {
                return;
            }
            foreach (MethodDef method in analyzer.LookupMethod(value))
            {
                analyzer.NameService.SetCanRename(method, false);
            }
            foreach (PropertyDef method2 in analyzer.LookupProperty(value))
            {
                analyzer.NameService.SetCanRename(method2, false);
            }
        }

        // Token: 0x06000014 RID: 20 RVA: 0x000029FC File Offset: 0x00000BFC
        private void AnalyzeBAMLElement(BAMLAnalyzer analyzer, BamlElement elem)
        {
            foreach (BamlRecord rec in elem.Body)
            {
                PropertyWithConverterRecord prop = rec as PropertyWithConverterRecord;
                if (prop != null)
                {
                    Tuple<IDnlibDef, AttributeInfoRecord, TypeDef> attr = analyzer.ResolveAttribute(prop.AttributeId);
                    string attrName = null;
                    if (attr.Item2 != null)
                    {
                        attrName = attr.Item2.Name;
                    }
                    else if (attr.Item1 != null)
                    {
                        attrName = attr.Item1.Name;
                    }
                    if (attrName == "Attach")
                    {
                        this.AnalyzeMessageAttach(analyzer, attr, prop.Value);
                    }
                    if (attrName == "Name")
                    {
                        this.AnalyzeAutoBind(analyzer, attr, prop.Value);
                    }
                    if (attrName == "MethodName")
                    {
                        this.AnalyzeActionMessage(analyzer, attr, prop.Value);
                    }
                }
            }
        }

        // Token: 0x06000015 RID: 21 RVA: 0x00002AEC File Offset: 0x00000CEC
        private void AnalyzeMessageAttach(BAMLAnalyzer analyzer, Tuple<IDnlibDef, AttributeInfoRecord, TypeDef> attr, string value)
        {
            if (attr.Item2 == null)
            {
                return;
            }
            TypeDef attrDeclType = analyzer.ResolveType(attr.Item2.OwnerTypeId);
            if (attrDeclType.FullName != "Caliburn.Micro.Message")
            {
                return;
            }
            string[] array = value.Split(new char[]
            {
                ';'
            });
            for (int i = 0; i < array.Length; i++)
            {
                string msg = array[i];
                string msgStr;
                if (msg.Contains("="))
                {
                    msgStr = msg.Split(new char[]
                    {
                        '='
                    })[1].Trim(new char[]
                    {
                        '[',
                        ']',
                        ' '
                    });
                }
                else
                {
                    msgStr = msg.Trim(new char[]
                    {
                        '[',
                        ']',
                        ' '
                    });
                }
                if (msgStr.StartsWith("Action"))
                {
                    msgStr = msgStr.Substring(6);
                }
                int parenIndex = msgStr.IndexOf('(');
                if (parenIndex != -1)
                {
                    msgStr = msgStr.Substring(0, parenIndex);
                }
                string actName = msgStr.Trim();
                foreach (MethodDef method in analyzer.LookupMethod(actName))
                {
                    analyzer.NameService.SetCanRename(method, false);
                }
            }
        }

        // Token: 0x06000019 RID: 25 RVA: 0x00002D9A File Offset: 0x00000F9A
        public void PostRename(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def)
        {
        }

        // Token: 0x06000018 RID: 24 RVA: 0x00002D98 File Offset: 0x00000F98
        public void PreRename(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def)
        {
        }
    }
}
