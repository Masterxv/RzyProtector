using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Confuser.Renamer
{
    public static class Class1
    {
        public static T RandomElement<T>(this IEnumerable<T> coll)
        {
            var rnd = new Random();
            return coll.ElementAt(rnd.Next(coll.Count()));
        }
    }
}
