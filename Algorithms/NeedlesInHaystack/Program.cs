using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

static class Program
{
    static void Main()
    {
        var needles = new[] { "aystb", "sta" };
        var haystack = "haystack";
        foreach (var result in Needles.Find(needles,haystack))
        {
            if (result.Needle >= 0) Console.WriteLine("match: " + needles[result.Needle]);
            else Console.WriteLine("skip:  " + haystack.Substring(result.Start, -result.Needle));
        }
    }

}