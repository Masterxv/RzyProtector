using System;
using Confuser.Core;

namespace Confuser.Protections.Constants.Mutation
{
	// Token: 0x020000B1 RID: 177
	[BeforeProtection(new string[]
	{
		"Ki.Constants"
	})]

	internal class NumberMutation : Protection
	{
		// Token: 0x170000E4 RID: 228
		// (get) Token: 0x06000332 RID: 818 RVA: 0x0007190C File Offset: 0x0006FB0C
		public override string Name
		{
			get
			{
				return "Mutate Constants";
			}
		}

		// Token: 0x170000E5 RID: 229
		// (get) Token: 0x06000333 RID: 819 RVA: 0x00071924 File Offset: 0x0006FB24
		public override string Description
		{
			get
			{
				return "Mutate Contants";
			}
		}

		// Token: 0x170000E6 RID: 230
		// (get) Token: 0x06000334 RID: 820 RVA: 0x0007193C File Offset: 0x0006FB3C
		public override string Id
		{
			get
			{
				return "Mutate Constants";
			}
		}

		// Token: 0x170000E7 RID: 231
		// (get) Token: 0x06000335 RID: 821 RVA: 0x00071954 File Offset: 0x0006FB54
		public override string FullId
		{
			get
			{
				return "Ki.mutate";
			}
		}

		// Token: 0x170000E8 RID: 232
		// (get) Token: 0x06000336 RID: 822 RVA: 0x0007196C File Offset: 0x0006FB6C
		public override ProtectionPreset Preset
		{
			get
			{
				return ProtectionPreset.Maximum;
			}
		}

		// Token: 0x06000337 RID: 823 RVA: 0x000020C3 File Offset: 0x000002C3
		protected override void Initialize(ConfuserContext context)
		{
		}

		// Token: 0x06000338 RID: 824 RVA: 0x0007197F File Offset: 0x0006FB7F
		protected override void PopulatePipeline(ProtectionPipeline pipeline)
		{
			pipeline.InsertPostStage(PipelineStage.BeginModule, new Numberphase(this));
		}

		// Token: 0x04000177 RID: 375
		public const string _Id = "Mutate Constants";

		// Token: 0x04000178 RID: 376
		public const string _FullId = "Ki.mutate";
	}
}
