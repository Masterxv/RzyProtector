using System;
using System.Collections.Generic;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Confuser.Protections.Resources
{
	// Token: 0x02000041 RID: 65
	internal interface IEncodeMode
	{
		// Token: 0x06000190 RID: 400
		IEnumerable<Instruction> EmitDecrypt(MethodDef init, REContext ctx, Local block, Local key);

		// Token: 0x06000191 RID: 401
		uint[] Encrypt(uint[] data, int offset, uint[] key);
	}
}
