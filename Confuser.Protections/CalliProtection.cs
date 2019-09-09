using System;
using System.Linq;
using Confuser.Core;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Confuser.Protections
{
	internal class CalliProtection : Protection
	{
		public const string _Id = "calli protection";
		public const string _FullId = "Ki.Calli";

		public override string Name
		{
			get { return "Calli Protection"; }
		}

		public override string Description
		{
			get { return "This protection will convert calls to calli"; }
		}

		public override string Id
		{
			get { return _Id; }
		}

		public override string FullId
		{
			get { return _FullId; }
		}

		public override ProtectionPreset Preset
		{
			get { return ProtectionPreset.Minimum; }
		}

		protected override void Initialize(ConfuserContext context)
		{
			//
		}

		protected override void PopulatePipeline(ProtectionPipeline pipeline)
		{
			pipeline.InsertPreStage(PipelineStage.ProcessModule, new CalliPhase(this));
		}

		class CalliPhase : ProtectionPhase
		{
			public CalliPhase(CalliProtection parent)
				: base(parent) { }

			public override ProtectionTargets Targets
			{
				get { return ProtectionTargets.Modules; }
			}

			public override string Name
			{
				get { return "Call to Calli conversion"; }
			}

			protected override void Execute(ConfuserContext context, ProtectionParameters parameters)
			{

				foreach (ModuleDef module in parameters.Targets.OfType<ModuleDef>())
				{

					foreach (TypeDef type in module.Types.ToArray())
					{

						foreach (MethodDef method in type.Methods.ToArray())
						{



							if (method.HasBody)
							{
								if (method.Body.HasInstructions)
								{
									if (method.FullName.Contains("My.")) continue;
									if (method.FullName.Contains(".My")) continue;
									if (method.FullName.Contains("Costura")) continue;
									if (method.IsConstructor) continue;
									if (method.DeclaringType.IsGlobalModuleType) continue;
									for (int i = 0; i < method.Body.Instructions.Count - 1; i++)
									{
										try
										{

											if (method.Body.Instructions[i].ToString().Contains("ISupportInitialize")) continue;
											if (method.Body.Instructions[i].OpCode == OpCodes.Call || method.Body.Instructions[i].OpCode == OpCodes.Callvirt/* || method.Body.Instructions[i].OpCode == OpCodes.Ldloc_S*/)
											{



												try
												{

													MemberRef membertocalli = (MemberRef)method.Body.Instructions[i].Operand;


													method.Body.Instructions[i].OpCode = OpCodes.Calli;
													method.Body.Instructions[i].Operand = membertocalli.MethodSig;
													method.Body.Instructions.Insert(i, Instruction.Create(OpCodes.Ldftn, membertocalli));

												}
												catch (Exception ex)
												{
													string str = ex.Message;
												}

											}
										}
										catch
										{

										}

									}
								}
								else
								{

								}
							}


						}
						foreach (MethodDef md in module.GlobalType.Methods)
						{
							if (md.Name == ".ctor")
							{
								module.GlobalType.Remove(md);
								break;
							}

						}
					}
				}
			}
		}
	}
}