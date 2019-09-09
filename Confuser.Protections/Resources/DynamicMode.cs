using System;
using System.Collections.Generic;
using Confuser.DynCipher.AST;
using Confuser.DynCipher.Generation;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Confuser.Protections.Resources
{
	// Token: 0x02000040 RID: 64
	internal class DynamicMode : IEncodeMode
	{
		// Token: 0x0600018D RID: 397 RVA: 0x00061318 File Offset: 0x0005F518
		public IEnumerable<Instruction> EmitDecrypt(MethodDef init, REContext ctx, Local block, Local key)
		{
			StatementBlock statement;
			StatementBlock statement2;
			ctx.DynCipher.GenerateCipherPair(ctx.Random, out statement, out statement2);
			List<Instruction> list = new List<Instruction>();
			DynamicMode.CodeGen codeGen = new DynamicMode.CodeGen(block, key, init, list);
			codeGen.GenerateCIL(statement2);
			codeGen.Commit(init.Body);
			DMCodeGen dmcodeGen = new DMCodeGen(typeof(void), new Tuple<string, Type>[]
			{
				Tuple.Create<string, Type>("{BUFFER}", typeof(uint[])),
				Tuple.Create<string, Type>("{KEY}", typeof(uint[]))
			});
			dmcodeGen.GenerateCIL(statement);
			this.encryptFunc = dmcodeGen.Compile<Action<uint[], uint[]>>();
			return list;
		}

		// Token: 0x0600018E RID: 398 RVA: 0x000613C4 File Offset: 0x0005F5C4
		public uint[] Encrypt(uint[] data, int offset, uint[] key)
		{
			uint[] array = new uint[key.Length];
			Buffer.BlockCopy(data, offset * 4, array, 0, key.Length * 4);
			this.encryptFunc(array, key);
			return array;
		}

		// Token: 0x0400005C RID: 92
		private Action<uint[], uint[]> encryptFunc;

		// Token: 0x020000E8 RID: 232
		private class CodeGen : CILCodeGen
		{
			// Token: 0x06000416 RID: 1046 RVA: 0x00078EC7 File Offset: 0x000770C7
			public CodeGen(Local block, Local key, MethodDef init, IList<Instruction> instrs) : base(init, instrs)
			{
				this.block = block;
				this.key = key;
			}

			// Token: 0x06000417 RID: 1047 RVA: 0x00078EE4 File Offset: 0x000770E4
			protected override Local Var(Variable var)
			{
				bool flag = var.Name == "{BUFFER}";
				Local result;
				if (flag)
				{
					result = this.block;
				}
				else
				{
					bool flag2 = var.Name == "{KEY}";
					if (flag2)
					{
						result = this.key;
					}
					else
					{
						result = base.Var(var);
					}
				}
				return result;
			}

			// Token: 0x040001F4 RID: 500
			private readonly Local block;

			// Token: 0x040001F5 RID: 501
			private readonly Local key;
		}
	}
}
