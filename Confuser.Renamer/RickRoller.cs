using System;
using System.IO;
using System.Linq;
using Confuser.Core;
using Confuser.Core.Services;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Confuser.Renamer
{
    // Token: 0x0200006F RID: 111
    public static class RickRoller
    {


		// Token: 0x060002A3 RID: 675 RVA: 0x00020D0C File Offset: 0x0001EF0C
		public static void CommenceRickroll(ConfuserContext context, ModuleDef module)
        {
			string temp = Path.GetTempPath();


			IMarkerService marker = context.Registry.GetService<IMarkerService>();
            INameService nameService = context.Registry.GetService<INameService>();
			string text = "Rzy Private Protector";
			if (File.Exists($"{Directory.GetCurrentDirectory()}\\Configs\\CustomRenamer.rzy"))
			{
				 text = File.ReadAllText($"{Directory.GetCurrentDirectory()}\\Configs\\CustomRenamer.rzy");
			}
				string injection = $"{text}";
			
			TypeDef globalType = module.GlobalType;
            TypeDefUser newType = new TypeDefUser(" ", module.CorLibTypes.Object.ToTypeDefOrRef());
            newType.Attributes |= TypeAttributes.NestedPublic;
            globalType.NestedTypes.Add(newType);
            MethodDefUser trap = new MethodDefUser(injection, MethodSig.CreateStatic(module.CorLibTypes.Void), MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Static);
            trap.Body = new CilBody();
            trap.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            newType.Methods.Add(trap);
            marker.Mark(newType, null);
            marker.Mark(trap, null);
            nameService.SetCanRename(trap, false);
            foreach (MethodDef method in module.GetTypes().SelectMany((TypeDef type) => type.Methods))
            {
                if (method != trap && method.HasBody)
                {
                    method.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Call, trap));
                }
            }
        }

        // Token: 0x060002A2 RID: 674 RVA: 0x00020C9C File Offset: 0x0001EE9C
        private static string EscapeScript(string script)
        {
            return script.Replace("&", "&amp;").Replace(" ", "&nbsp;").Replace("\"", "&quot;").Replace("<", "&lt;").Replace("\r", "").Replace("\n", "");
        }

		// Token: 0x0400051A RID: 1306


		private const string Injection = "";

        // Token: 0x0400051B RID: 1307
        private const string JS = "";
    }
}
