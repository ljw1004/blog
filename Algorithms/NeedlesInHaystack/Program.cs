using System;
using System.Collections.Generic;
using System.Linq;

static class Program
{
    static void Main()
    {
        // Example of how to use it:
        var needles = new[] { "aystb", "sta" };
        var haystack = "haystack";
        foreach (var result in Needles.Find(needles,haystack))
        {
            if (result.Needle >= 0) Console.WriteLine("match: " + needles[result.Needle]);
            else Console.WriteLine("skip:  " + haystack.Substring(result.Start, -result.Needle));
        }

        // Regression tests:
        Test(new[] { "aystb", "sta" }, "haystack", "hay,sta,ck");
        Test(new[] { "aac" }, "123aaac456", "123a,aac,456");
        Test(new[] { "abc", "c" }, "1abc2", "1,abc,2");
        Test(new[] { "abc", "a" }, "1abc2", "1,a,bc2");
    }

    static void Test(IList<string> needles, string haystack, string expected)
    {
        var results = Needles.Find(needles, haystack);
        var spans = results.Select(r => r.Needle >= 0 ? needles[r.Needle] : haystack.Substring(r.Start, -r.Needle));
        var actual = string.Join(",", spans);
        if (actual != expected) Console.WriteLine($"FAILED - actual={actual}; expected={expected}");
    }

}