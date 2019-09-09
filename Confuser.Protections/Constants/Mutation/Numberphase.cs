using System;
using System.Linq;
using Confuser.Core;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Confuser.Protections.Constants.Mutation
{
	// Token: 0x020000B2 RID: 178
	internal class Numberphase : ProtectionPhase
	{
		// Token: 0x0600033A RID: 826 RVA: 0x00003324 File Offset: 0x00001524
		public Numberphase(NumberMutation parent) : base(parent)
		{
		}

		// Token: 0x170000E9 RID: 233
		// (get) Token: 0x0600033B RID: 827 RVA: 0x00071990 File Offset: 0x0006FB90
		public override ProtectionTargets Targets
		{
			get
			{
				return ProtectionTargets.Modules;
			}
		}

		// Token: 0x170000EA RID: 234
		// (get) Token: 0x0600033C RID: 828 RVA: 0x000719A4 File Offset: 0x0006FBA4
		public override string Name
		{
			get
			{
				return "Number mutate";
			}
		}

		// Token: 0x0600033D RID: 829 RVA: 0x000719BC File Offset: 0x0006FBBC
		protected override void Execute(ConfuserContext context, ProtectionParameters parameters)
		{
			foreach (ModuleDef moduleDef in parameters.Targets.OfType<ModuleDef>())
			{
				foreach (TypeDef typeDef in moduleDef.Types)
				{
					foreach (MethodDef methodDef in typeDef.Methods)
					{
						bool flag = methodDef.HasBody && methodDef.Body.HasInstructions;
						if (flag)
						{
							for (int i = 0; i < methodDef.Body.Instructions.Count; i++)
							{
								bool flag2 = methodDef.Body.Instructions[i].OpCode == OpCodes.Ldc_I4;
								if (flag2)
								{
									this.body = methodDef.Body;
									int ldcI4Value = this.body.Instructions[i].GetLdcI4Value();
									int num = Numberphase.rnd.Next(1, 3);
									int num2 = ldcI4Value - num;
									this.body.Instructions[i].Operand = num2;
									this.Start(i, num, num2, moduleDef, methodDef);
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x0600033E RID: 830 RVA: 0x00071B9C File Offset: 0x0006FD9C
		private void Start(int i, int sub, int calculado, ModuleDef module, MethodDef method)
		{
			switch (sub)
			{
			case 1:
			{
				Local local = new Local(module.CorLibTypes.Object);
				Local local2 = new Local(module.CorLibTypes.Object);
				Local local3 = new Local(module.CorLibTypes.Object);
				Local local4 = new Local(module.CorLibTypes.Object);
				method.Body.Variables.Add(local);
				method.Body.Variables.Add(local2);
				method.Body.Variables.Add(local3);
				method.Body.Variables.Add(local4);
				this.body.Instructions.Insert(i + 1, new Instruction(OpCodes.Sizeof, module.Import(typeof(GCNotificationStatus))));
				this.body.Instructions.Insert(i + 2, new Instruction(OpCodes.Stloc_S, local));
				this.body.Instructions.Insert(i + 3, new Instruction(OpCodes.Ldloc_S, local));
				this.body.Instructions.Insert(i + 4, OpCodes.Add.ToInstruction());
				this.body.Instructions.Insert(i + 5, new Instruction(OpCodes.Sizeof, module.Import(typeof(sbyte))));
				this.body.Instructions.Insert(i + 6, new Instruction(OpCodes.Stloc_S, local2));
				this.body.Instructions.Insert(i + 7, new Instruction(OpCodes.Ldloc_S, local2));
				this.body.Instructions.Insert(i + 8, OpCodes.Sub.ToInstruction());
				this.body.Instructions.Insert(i + 9, new Instruction(OpCodes.Sizeof, module.Import(typeof(sbyte))));
				this.body.Instructions.Insert(i + 10, new Instruction(OpCodes.Stloc_S, local3));
				this.body.Instructions.Insert(i + 11, new Instruction(OpCodes.Ldloc_S, local3));
				this.body.Instructions.Insert(i + 12, OpCodes.Sub.ToInstruction());
				this.body.Instructions.Insert(i + 13, new Instruction(OpCodes.Sizeof, module.Import(typeof(sbyte))));
				this.body.Instructions.Insert(i + 14, new Instruction(OpCodes.Stloc_S, local4));
				this.body.Instructions.Insert(i + 15, new Instruction(OpCodes.Ldloc_S, local4));
				this.body.Instructions.Insert(i + 16, OpCodes.Sub.ToInstruction());
				break;
			}
			case 2:
			{
				Local local = new Local(module.CorLibTypes.Object);
				method.Body.Variables.Add(local);
				this.body.Instructions.Insert(i + 1, new Instruction(OpCodes.Sizeof, module.Import(typeof(char))));
				this.body.Instructions.Insert(i + 2, new Instruction(OpCodes.Stloc_S, local));
				this.body.Instructions.Insert(i + 3, new Instruction(OpCodes.Ldloc_S, local));
				this.body.Instructions.Insert(i + 4, OpCodes.Add.ToInstruction());
				break;
			}
			case 3:
			{
				Local local = new Local(module.CorLibTypes.Object);
				Local local2 = new Local(module.CorLibTypes.Object);
				method.Body.Variables.Add(local);
				method.Body.Variables.Add(local2);
				this.body.Instructions.Insert(i + 1, Instruction.Create(OpCodes.Sizeof, module.Import(typeof(int))));
				this.body.Instructions.Insert(i + 2, new Instruction(OpCodes.Stloc_S, local));
				this.body.Instructions.Insert(i + 3, new Instruction(OpCodes.Ldloc_S, local));
				this.body.Instructions.Insert(i + 4, Instruction.Create(OpCodes.Sizeof, module.Import(typeof(byte))));
				this.body.Instructions.Insert(i + 5, new Instruction(OpCodes.Stloc_S, local2));
				this.body.Instructions.Insert(i + 6, new Instruction(OpCodes.Ldloc_S, local2));
				this.body.Instructions.Insert(i + 7, Instruction.Create(OpCodes.Sub));
				this.body.Instructions.Insert(i + 8, Instruction.Create(OpCodes.Add));
				break;
			}
			case 4:
			{
				Local local = new Local(module.CorLibTypes.Object);
				Local local2 = new Local(module.CorLibTypes.Object);
				Local local3 = new Local(module.CorLibTypes.Object);
				Local local4 = new Local(module.CorLibTypes.Object);
				method.Body.Variables.Add(local);
				method.Body.Variables.Add(local2);
				method.Body.Variables.Add(local3);
				method.Body.Variables.Add(local4);
				this.body.Instructions.Insert(i + 1, Instruction.Create(OpCodes.Add));
				this.body.Instructions.Insert(i + 2, Instruction.Create(OpCodes.Sizeof, module.Import(typeof(decimal))));
				this.body.Instructions.Insert(i + 3, new Instruction(OpCodes.Stloc_S, local));
				this.body.Instructions.Insert(i + 4, new Instruction(OpCodes.Ldloc_S, local));
				this.body.Instructions.Insert(i + 5, Instruction.Create(OpCodes.Sizeof, module.Import(typeof(GCCollectionMode))));
				this.body.Instructions.Insert(i + 6, new Instruction(OpCodes.Stloc_S, local2));
				this.body.Instructions.Insert(i + 7, new Instruction(OpCodes.Ldloc_S, local2));
				this.body.Instructions.Insert(i + 8, Instruction.Create(OpCodes.Sub));
				this.body.Instructions.Insert(i + 9, Instruction.Create(OpCodes.Sizeof, module.Import(typeof(int))));
				this.body.Instructions.Insert(i + 10, new Instruction(OpCodes.Stloc_S, local3));
				this.body.Instructions.Insert(i + 11, new Instruction(OpCodes.Ldloc_S, local3));
				this.body.Instructions.Insert(i + 12, Instruction.Create(OpCodes.Sizeof, module.Import(typeof(byte))));
				this.body.Instructions.Insert(i + 13, new Instruction(OpCodes.Stloc_S, local4));
				this.body.Instructions.Insert(i + 14, new Instruction(OpCodes.Ldloc_S, local4));
				this.body.Instructions.Insert(i + 15, Instruction.Create(OpCodes.Sizeof, module.Import(typeof(byte))));
				this.body.Instructions.Insert(i + 16, new Instruction(OpCodes.Stloc_S, local));
				this.body.Instructions.Insert(i + 17, new Instruction(OpCodes.Ldloc_S, local));
				this.body.Instructions.Insert(i + 18, Instruction.Create(OpCodes.Sub));
				this.body.Instructions.Insert(i + 19, Instruction.Create(OpCodes.Sizeof, module.Import(typeof(byte))));
				this.body.Instructions.Insert(i + 20, new Instruction(OpCodes.Stloc_S, local2));
				this.body.Instructions.Insert(i + 21, new Instruction(OpCodes.Ldloc_S, local2));
				this.body.Instructions.Insert(i + 22, Instruction.Create(OpCodes.Sizeof, module.Import(typeof(byte))));
				this.body.Instructions.Insert(i + 23, new Instruction(OpCodes.Stloc_S, local2));
				this.body.Instructions.Insert(i + 24, new Instruction(OpCodes.Ldloc_S, local2));
				this.body.Instructions.Insert(i + 25, Instruction.Create(OpCodes.Add));
				break;
			}
			case 5:
			{
				Local local = new Local(module.CorLibTypes.Object);
				Local local2 = new Local(module.CorLibTypes.Object);
				method.Body.Variables.Add(local);
				method.Body.Variables.Add(local2);
				this.body.Instructions.Insert(i + 1, new Instruction(OpCodes.Sizeof, module.Import(typeof(EnvironmentVariableTarget))));
				this.body.Instructions.Insert(i + 2, Instruction.Create(OpCodes.Stloc_S, local));
				this.body.Instructions.Insert(i + 3, Instruction.Create(OpCodes.Ldloc_S, local));
				this.body.Instructions.Insert(i + 4, OpCodes.Add.ToInstruction());
				this.body.Instructions.Insert(i + 5, new Instruction(OpCodes.Sizeof, module.Import(typeof(sbyte))));
				this.body.Instructions.Insert(i + 6, Instruction.Create(OpCodes.Stloc_S, local2));
				this.body.Instructions.Insert(i + 7, Instruction.Create(OpCodes.Ldloc_S, local2));
				this.body.Instructions.Insert(i + 9, OpCodes.Add.ToInstruction());
				break;
			}
			}
		}

		// Token: 0x04000179 RID: 377
		private CilBody body;

		// Token: 0x0400017A RID: 378
		private static Random rnd = new Random();
	}
}
