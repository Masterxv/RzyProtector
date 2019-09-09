using System;
using Confuser.Core;
using Confuser.Core.Services;
using Confuser.DynCipher;
using Confuser.Renamer;
using dnlib.DotNet;

namespace Confuser.Protections.Resources
{
	// Token: 0x02000046 RID: 70
	internal class REContext
	{
		// Token: 0x04000062 RID: 98
		public ConfuserContext Context;

		// Token: 0x04000063 RID: 99
		public FieldDef DataField;

		// Token: 0x04000064 RID: 100
		public TypeDef DataType;

		// Token: 0x04000065 RID: 101
		public IDynCipherService DynCipher;

		// Token: 0x04000066 RID: 102
		public MethodDef InitMethod;

		// Token: 0x04000067 RID: 103
		public IMarkerService Marker;

		// Token: 0x04000068 RID: 104
		public Mode Mode;

		// Token: 0x04000069 RID: 105
		public IEncodeMode ModeHandler;

		// Token: 0x0400006A RID: 106
		public ModuleDef Module;

		// Token: 0x0400006B RID: 107
		public INameService Name;

		// Token: 0x0400006C RID: 108
		public RandomGenerator Random;
	}
}
