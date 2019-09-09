using System;
using Confuser.Core;
using Confuser.Protections.Resources;

namespace Confuser.Protections
{
	// Token: 0x02000024 RID: 36
	[BeforeProtection(new string[]
	{
		"Ki.ControlFlow"
	})]
	[AfterProtection(new string[]
	{
		"Ki.Constants"
	})]
	internal class ResourceProtection : Protection
	{
		// Token: 0x17000086 RID: 134
		// (get) Token: 0x060000F6 RID: 246 RVA: 0x00003E54 File Offset: 0x00002054
		public override string Name
		{
			get
			{
				return "Resources Protection";
			}
		}

		// Token: 0x17000087 RID: 135
		// (get) Token: 0x060000F7 RID: 247 RVA: 0x00003E6C File Offset: 0x0000206C
		public override string Description
		{
			get
			{
				return "This protection encodes and compresses the embedded resources.";
			}
		}

		// Token: 0x17000088 RID: 136
		// (get) Token: 0x060000F8 RID: 248 RVA: 0x00003E84 File Offset: 0x00002084
		public override string Id
		{
			get
			{
				return "resources";
			}
		}

		// Token: 0x17000089 RID: 137
		// (get) Token: 0x060000F9 RID: 249 RVA: 0x00003E9C File Offset: 0x0000209C
		public override string FullId
		{
			get
			{
				return "Ki.Resources";
			}
		}

		// Token: 0x1700008A RID: 138
		// (get) Token: 0x060000FA RID: 250 RVA: 0x00003EB4 File Offset: 0x000020B4
		public override ProtectionPreset Preset
		{
			get
			{
				return ProtectionPreset.Normal;
			}
		}

		// Token: 0x060000FB RID: 251 RVA: 0x000020C3 File Offset: 0x000002C3
		protected override void Initialize(ConfuserContext context)
		{
		}

		// Token: 0x060000FC RID: 252 RVA: 0x00003EC7 File Offset: 0x000020C7
		protected override void PopulatePipeline(ProtectionPipeline pipeline)
		{
			pipeline.InsertPreStage(PipelineStage.ProcessModule, new InjectPhase(this));
		}

		// Token: 0x04000042 RID: 66
		public const string _Id = "resources";

		// Token: 0x04000043 RID: 67
		public const string _FullId = "Ki.Resources";

		// Token: 0x04000044 RID: 68
		public const string _ServiceId = "Ki.Resources";
	}
}
