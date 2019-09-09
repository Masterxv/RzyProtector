using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Arithmetic_Obfuscation.Arithmetic;
using Confuser.Core;
using Confuser.Core.Helpers;
using Confuser.Core.Services;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Confuser.Protections.Constants {
	internal class MutationPhase : ProtectionPhase {
		public MutationPhase(MutationProtection parent)
			: base(parent) { }

		public override ProtectionTargets Targets {
			get { return ProtectionTargets.Modules; }
		}

		public override string Name {
			get { return "Constants Mutation"; }
		}

		protected override void Execute(ConfuserContext context, ProtectionParameters parameters) {
            
           
            foreach (ModuleDef module in parameters.Targets.OfType<ModuleDef>())
            {
                new Arithmetic(module);
            }

        }

        

 


    }
}