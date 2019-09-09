using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Confuser.Core;
using Confuser.Core.Helpers;
using Confuser.Core.Services;
using Confuser.Renamer;
using dnlib.DotNet;
using dnlib.DotNet.MD;
using dnlib.DotNet.Writer;

namespace Confuser.Protections.Resources
{
	// Token: 0x02000043 RID: 67
	internal class MDPhase
	{
		// Token: 0x06000198 RID: 408 RVA: 0x00061B3C File Offset: 0x0005FD3C
		public MDPhase(REContext ctx)
		{
			this.ctx = ctx;
		}

		// Token: 0x06000199 RID: 409 RVA: 0x00061B4D File Offset: 0x0005FD4D
		public void Hook()
		{
			this.ctx.Context.CurrentModuleWriterListener.OnWriterEvent += this.OnWriterEvent;
		}

		// Token: 0x0600019A RID: 410 RVA: 0x00061B74 File Offset: 0x0005FD74
		private void OnWriterEvent(object sender, ModuleWriterListenerEventArgs e)
		{
			ModuleWriterBase moduleWriterBase = (ModuleWriterBase)sender;
			bool flag = e.WriterEvent == ModuleWriterEvent.MDBeginAddResources;
			if (flag)
			{
				this.ctx.Context.CheckCancellation();
				this.ctx.Context.Logger.Debug("Encrypting resources...");
				bool flag2 = this.ctx.Context.Packer != null;
				List<EmbeddedResource> list = this.ctx.Module.Resources.OfType<EmbeddedResource>().ToList<EmbeddedResource>();
				bool flag3 = !flag2;
				if (flag3)
				{
					this.ctx.Module.Resources.RemoveWhere((Resource res) => res is EmbeddedResource);
				}
				string text = this.ctx.Name.RandomName(RenameMode.Letters);
				PublicKey publicKey = null;
				bool flag4 = moduleWriterBase.TheOptions.StrongNameKey != null;
				if (flag4)
				{
					publicKey = PublicKeyBase.CreatePublicKey(moduleWriterBase.TheOptions.StrongNameKey.PublicKey);
				}
				AssemblyDefUser assemblyDefUser = new AssemblyDefUser(text, new Version(0, 0), publicKey);
				assemblyDefUser.Modules.Add(new ModuleDefUser(text + ".dll"));
				ModuleDef manifestModule = assemblyDefUser.ManifestModule;
				assemblyDefUser.ManifestModule.Kind = ModuleKind.Dll;
				AssemblyRefUser asmRef = new AssemblyRefUser(manifestModule.Assembly);
				bool flag5 = !flag2;
				if (flag5)
				{
					foreach (EmbeddedResource embeddedResource in list)
					{
						embeddedResource.Attributes = ManifestResourceAttributes.Public;
						manifestModule.Resources.Add(embeddedResource);
						this.ctx.Module.Resources.Add(new AssemblyLinkedResource(embeddedResource.Name, asmRef, embeddedResource.Attributes));
					}
				}
				byte[] array;
				using (MemoryStream memoryStream = new MemoryStream())
				{
					manifestModule.Write(memoryStream, new ModuleWriterOptions
					{
						StrongNameKey = moduleWriterBase.TheOptions.StrongNameKey
					});
					array = memoryStream.ToArray();
				}
				array = this.ctx.Context.Registry.GetService<ICompressionService>().Compress(array, delegate(double progress)
				{
					this.ctx.Context.Logger.Progress((int)(progress * 10000.0), 10000);
				});
				this.ctx.Context.Logger.EndProgress();
				this.ctx.Context.CheckCancellation();
				uint num = (uint)((array.Length + 3) / 4);
				num = (num + 15u & 4294967280u);
				uint[] array2 = new uint[num];
				Buffer.BlockCopy(array, 0, array2, 0, array.Length);
				Debug.Assert(num % 16u == 0u);
				uint num2 = this.ctx.Random.NextUInt32() | 16u;
				uint[] array3 = new uint[16];
				uint num3 = num2;
				for (int i = 0; i < 16; i++)
				{
					num3 ^= num3 >> 13;
					num3 ^= num3 << 25;
					num3 ^= num3 >> 27;
					array3[i] = num3;
				}
				byte[] array4 = new byte[array2.Length * 4];
				int j;
				for (j = 0; j < array2.Length; j += 16)
				{
					uint[] src = this.ctx.ModeHandler.Encrypt(array2, j, array3);
					for (int k = 0; k < 16; k++)
					{
						array3[k] ^= array2[j + k];
					}
					Buffer.BlockCopy(src, 0, array4, j * 4, 64);
				}
				Debug.Assert(j == array2.Length);
				uint num4 = (uint)array4.Length;
				TablesHeap tablesHeap = moduleWriterBase.MetaData.TablesHeap;
				tablesHeap.ClassLayoutTable[moduleWriterBase.MetaData.GetClassLayoutRid(this.ctx.DataType)].ClassSize = num4;
				RawFieldRow rawFieldRow = tablesHeap.FieldTable[moduleWriterBase.MetaData.GetRid(this.ctx.DataField)];
				rawFieldRow.Flags |= 256;
				this.encryptedResource = moduleWriterBase.Constants.Add(new ByteArrayChunk(array4), 8u);
				MutationHelper.InjectKeys(this.ctx.InitMethod, new int[]
				{
					0,
					1
				}, new int[]
				{
					(int)(num4 / 4u),
					(int)num2
				});
			}
			else
			{
				bool flag6 = e.WriterEvent == ModuleWriterEvent.EndCalculateRvasAndFileOffsets;
				if (flag6)
				{
					TablesHeap tablesHeap2 = moduleWriterBase.MetaData.TablesHeap;
					tablesHeap2.FieldRVATable[moduleWriterBase.MetaData.GetFieldRVARid(this.ctx.DataField)].RVA = (uint)this.encryptedResource.RVA;
				}
			}
		}

		// Token: 0x0400005D RID: 93
		private readonly REContext ctx;

		// Token: 0x0400005E RID: 94
		private ByteArrayChunk encryptedResource;
	}
}
