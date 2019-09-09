using Core.Helper;
using Core.Helper.Generator.Context;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Confuser.Protections
{
    public class Blockss
    {
        public List<Block> blocks = new List<Block>();
        private Generator generator = new Generator();
        public Block getBlock(int id)
        {
            return blocks.Single(block => block.ID == id);
        }

        public void Scramble(out Blockss incGroups)
        {
            Blockss groups = new Blockss();
            foreach (var group in blocks)
                groups.blocks.Insert(generator.Generate<int>(GeneratorType.Integer, groups.blocks.Count), group);
            incGroups = groups;
        }

    }
}
