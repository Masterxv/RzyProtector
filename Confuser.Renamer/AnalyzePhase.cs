using System;
using System.Collections.Generic;
using Confuser.Core;
using Confuser.Renamer.Analyzers;
using dnlib.DotNet;

namespace Confuser.Renamer
{
    // Token: 0x02000002 RID: 2
    internal class AnalyzePhase : ProtectionPhase
    {
        // Token: 0x06000001 RID: 1 RVA: 0x000020D8 File Offset: 0x000002D8
        public AnalyzePhase(NameProtection parent) : base(parent)
        {
        }

        // Token: 0x17000001 RID: 1
        // (get) Token: 0x06000002 RID: 2 RVA: 0x000020E1 File Offset: 0x000002E1
        public override bool ProcessAll
        {
            get
            {
                return true;
            }
        }

        // Token: 0x17000002 RID: 2
        // (get) Token: 0x06000003 RID: 3 RVA: 0x000020E4 File Offset: 0x000002E4
        public override ProtectionTargets Targets
        {
            get
            {
                return ProtectionTargets.AllDefinitions;
            }
        }

        // Token: 0x17000003 RID: 3
        // (get) Token: 0x06000004 RID: 4 RVA: 0x000020E8 File Offset: 0x000002E8
        public override string Name
        {
            get
            {
                return "Name analysis";
            }
        }

        // Token: 0x06000005 RID: 5 RVA: 0x00003B48 File Offset: 0x00001D48
        private void ParseParameters(IDnlibDef def, ConfuserContext context, NameService service, ProtectionParameters parameters)
        {
            RenameMode? mode = parameters.GetParameter<RenameMode?>(context, def, "mode", null);
            if (mode.HasValue)
            {
                service.SetRenameMode(def, mode.Value);
            }
        }

        // Token: 0x06000006 RID: 6 RVA: 0x00003B84 File Offset: 0x00001D84
        protected override void Execute(ConfuserContext context, ProtectionParameters parameters)
        {
            NameService service = (NameService)context.Registry.GetService<INameService>();
            context.Logger.Debug("Building VTables & identifier list...");
            foreach (IDnlibDef def in parameters.Targets.WithProgress(context.Logger))
            {
                this.ParseParameters(def, context, service, parameters);
                if (def is ModuleDef)
                {
                    ModuleDef module = (ModuleDef)def;
                    using (IEnumerator<Resource> enumerator2 = module.Resources.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            Resource res = enumerator2.Current;
                            service.SetOriginalName(res, res.Name);
                        }
                        goto IL_B1;
                    }
                    goto IL_9F;
                }
                goto IL_9F;
                IL_B1:
                if (def is TypeDef)
                {
                    service.GetVTables().GetVTable((TypeDef)def);
                    service.SetOriginalNamespace(def, ((TypeDef)def).Namespace);
                }
                context.CheckCancellation();
                continue;
                IL_9F:
                service.SetOriginalName(def, def.Name);
                goto IL_B1;
            }
            context.Logger.Debug("Analyzing...");
            this.RegisterRenamers(context, service);
            IList<IRenamer> arg_120_0 = service.Renamers;
            foreach (IDnlibDef def2 in parameters.Targets.WithProgress(context.Logger))
            {
                this.Analyze(service, context, parameters, def2, true);
                context.CheckCancellation();
            }
        }

        // Token: 0x06000007 RID: 7 RVA: 0x00003D28 File Offset: 0x00001F28
        private void RegisterRenamers(ConfuserContext context, NameService service)
        {
            bool wpf = false;
            bool caliburn = false;
            bool winforms = false;
            foreach (ModuleDefMD module in context.Modules)
            {
                foreach (AssemblyRef asmRef in module.GetAssemblyRefs())
                {
                    if (asmRef.Name == "WindowsBase" || asmRef.Name == "PresentationCore" || asmRef.Name == "PresentationFramework" || asmRef.Name == "System.Xaml")
                    {
                        wpf = true;
                    }
                    else if (asmRef.Name == "Caliburn.Micro")
                    {
                        caliburn = true;
                    }
                    else if (asmRef.Name == "System.Windows.Forms")
                    {
                        winforms = true;
                    }
                }
            }
            if (wpf)
            {
                WPFAnalyzer wpfAnalyzer = new WPFAnalyzer();
                context.Logger.Debug("WPF found, enabling compatibility.");
                service.Renamers.Add(wpfAnalyzer);
                if (caliburn)
                {
                    context.Logger.Debug("Caliburn.Micro found, enabling compatibility.");
                    service.Renamers.Add(new CaliburnAnalyzer(wpfAnalyzer));
                }
            }
            if (winforms)
            {
                WinFormsAnalyzer winformsAnalyzer = new WinFormsAnalyzer();
                context.Logger.Debug("WinForms found, enabling compatibility.");
                service.Renamers.Add(winformsAnalyzer);
            }
        }

        // Token: 0x06000008 RID: 8 RVA: 0x00003EB0 File Offset: 0x000020B0
        internal void Analyze(NameService service, ConfuserContext context, ProtectionParameters parameters, IDnlibDef def, bool runAnalyzer)
        {
            if (def is TypeDef)
            {
                this.Analyze(service, context, parameters, (TypeDef)def);
            }
            else if (def is MethodDef)
            {
                this.Analyze(service, context, parameters, (MethodDef)def);
            }
            else if (def is FieldDef)
            {
                this.Analyze(service, context, parameters, (FieldDef)def);
            }
            else if (def is PropertyDef)
            {
                this.Analyze(service, context, parameters, (PropertyDef)def);
            }
            else if (def is EventDef)
            {
                this.Analyze(service, context, parameters, (EventDef)def);
            }
            else if (def is ModuleDef)
            {
                string pass = parameters.GetParameter<string>(context, def, "password", null);
                if (pass != null)
                {
                    service.reversibleRenamer = new ReversibleRenamer(pass);
                }
                service.SetCanRename(def, false);
            }
            if (!runAnalyzer || parameters.GetParameter<bool>(context, def, "forceRen", false))
            {
                return;
            }
            foreach (IRenamer renamer in service.Renamers)
            {
                renamer.Analyze(context, service, parameters, def);
            }
        }

        // Token: 0x06000009 RID: 9 RVA: 0x00003FD8 File Offset: 0x000021D8
        private static bool IsVisibleOutside(ConfuserContext context, ProtectionParameters parameters, IMemberDef def)
        {
            TypeDef type = def as TypeDef;
            if (type == null)
            {
                type = def.DeclaringType;
            }
            bool? renPublic = parameters.GetParameter<bool?>(context, type, "renPublic", null);
            if (!renPublic.HasValue)
            {
                return type.IsVisibleOutside(true);
            }
            return type.IsVisibleOutside(false) && renPublic.Value;
        }

        // Token: 0x0600000A RID: 10 RVA: 0x00004030 File Offset: 0x00002230
        private void Analyze(NameService service, ConfuserContext context, ProtectionParameters parameters, TypeDef type)
        {
            if (AnalyzePhase.IsVisibleOutside(context, parameters, type))
            {
                service.SetCanRename(type, false);
            }
            else if (type.IsRuntimeSpecialName || type.IsGlobalModuleType)
            {
                service.SetCanRename(type, false);
            }
            else if (type.FullName == "ConfusedBy")
            {
                service.SetCanRename(type, false);
            }
            if (parameters.GetParameter<bool>(context, type, "forceRen", false))
            {
                return;
            }
            if (type.InheritsFromCorlib("System.Attribute"))
            {
                service.ReduceRenameMode(type, RenameMode.ASCII);
            }
            if (type.InheritsFrom("System.Configuration.SettingsBase"))
            {
                service.SetCanRename(type, false);
            }
        }

        // Token: 0x0600000B RID: 11 RVA: 0x000040CC File Offset: 0x000022CC
        private void Analyze(NameService service, ConfuserContext context, ProtectionParameters parameters, MethodDef method)
        {
            if (method.DeclaringType.IsVisibleOutside(true) && (method.IsFamily || method.IsFamilyOrAssembly || method.IsPublic) && AnalyzePhase.IsVisibleOutside(context, parameters, method))
            {
                service.SetCanRename(method, false);
                return;
            }
            if (method.IsRuntimeSpecialName)
            {
                service.SetCanRename(method, false);
                return;
            }
            if (parameters.GetParameter<bool>(context, method, "forceRen", false))
            {
                return;
            }
            if (method.DeclaringType.IsComImport() && !method.HasAttribute("System.Runtime.InteropServices.DispIdAttribute"))
            {
                service.SetCanRename(method, false);
                return;
            }
            if (method.DeclaringType.IsDelegate())
            {
                service.SetCanRename(method, false);
            }
        }

        // Token: 0x0600000C RID: 12 RVA: 0x0000417C File Offset: 0x0000237C
        private void Analyze(NameService service, ConfuserContext context, ProtectionParameters parameters, FieldDef field)
        {
            if (field.DeclaringType.IsVisibleOutside(true) && (field.IsFamily || field.IsFamilyOrAssembly || field.IsPublic) && AnalyzePhase.IsVisibleOutside(context, parameters, field))
            {
                service.SetCanRename(field, false);
                return;
            }
            if (field.IsRuntimeSpecialName)
            {
                service.SetCanRename(field, false);
                return;
            }
            if (parameters.GetParameter<bool>(context, field, "forceRen", false))
            {
                return;
            }
            if (field.DeclaringType.IsSerializable && !field.IsNotSerialized)
            {
                service.SetCanRename(field, false);
                return;
            }
            if (field.IsLiteral && field.DeclaringType.IsEnum)
            {
                service.SetCanRename(field, false);
            }
        }

        // Token: 0x0600000D RID: 13 RVA: 0x00004230 File Offset: 0x00002430
        private void Analyze(NameService service, ConfuserContext context, ProtectionParameters parameters, PropertyDef property)
        {
            if (property.DeclaringType.IsVisibleOutside(true) && AnalyzePhase.IsVisibleOutside(context, parameters, property))
            {
                service.SetCanRename(property, false);
                return;
            }
            if (property.IsRuntimeSpecialName)
            {
                service.SetCanRename(property, false);
                return;
            }
            if (parameters.GetParameter<bool>(context, property, "forceRen", false))
            {
                return;
            }
            if (property.DeclaringType.Implements("System.ComponentModel.INotifyPropertyChanged"))
            {
                service.SetCanRename(property, false);
                return;
            }
            if (property.DeclaringType.Name.String.Contains("AnonymousType"))
            {
                service.SetCanRename(property, false);
            }
        }

        // Token: 0x0600000E RID: 14 RVA: 0x000020EF File Offset: 0x000002EF
        private void Analyze(NameService service, ConfuserContext context, ProtectionParameters parameters, EventDef evt)
        {
            if (evt.DeclaringType.IsVisibleOutside(true) && AnalyzePhase.IsVisibleOutside(context, parameters, evt))
            {
                service.SetCanRename(evt, false);
                return;
            }
            if (evt.IsRuntimeSpecialName)
            {
                service.SetCanRename(evt, false);
            }
        }
    }
}
