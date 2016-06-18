using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

partial class Program
{
    static void Main()
    {
        Test("ConsiderSomeSimultaneously", ConsiderSomeSimultaneously);
        Test("RepeatedIndexOf", RepeatedIndexOf);
        Test("ConsiderAllSimultaneously", ConsiderAllSimultaneously);
    }

    static void Test(string algorithmName, Func<IEnumerable<string>, string, CancellationToken, IEnumerable<NeedleResult>> algorithm)
    {
        Test(algorithmName, algorithm, "simple case", new[] { "aystb", "sta" }, "haystack", "hay,sta,ck");
        Test(algorithmName, algorithm, "don't rule self out", new[] { "aac" }, "123aaac456", "123a,aac,456");
        Test(algorithmName, algorithm, "pick earliest match 1", new[] { "abc", "c" }, "1abc2", "1,abc,2");
        Test(algorithmName, algorithm, "pick earliest match 2", new[] { "abc", "a" }, "1abc2", "1,a,bc2|1,abc,2");
        Test(algorithmName, algorithm, "fail on empty input", new[] { "abc", "" }, "1abc2", "FAIL");
        using (var cts = new CancellationTokenSource(20000))
        {
            var sw = Stopwatch.StartNew();
            var count = algorithm(TestBigNeedles, TestBigHaystack, cts.Token).Count();
            sw.Stop();
            Console.WriteLine($"{algorithmName} perf - {count} matches in {sw.Elapsed}");
        }
    }

    static void Test(string algorithmName, Func<IEnumerable<string>, string, CancellationToken, IEnumerable<NeedleResult>> algorithm, string testName, IEnumerable<string> needles, string haystack, string expected)
    {
        string actual;
        try
        {
            var results = algorithm(needles, haystack, CancellationToken.None).ToList();
            actual = string.Join(",", results.Select(r => haystack.Substring(r.Start, r.Length)));
        }
        catch (ArgumentNullException)
        {
            actual = "FAIL";
        }
        var expecteds = expected.Split('|');
        if (expecteds.Contains(actual)) return;
        Console.WriteLine($"{algorithmName} - failed {testName} - actual={actual}; expected={expected}");
    }

    public struct NeedleResult
    {
        public int Needle;
        public int Start;
        public int Length;
    }

    private class NeedleResultClass
    {
        public int Needle;
        public int Start;
        public int Length;
    }

    public static IEnumerable<NeedleResult> RepeatedIndexOf(IEnumerable<string> needles, string haystack, CancellationToken cancel)
    {
        if (needles == null) throw new ArgumentNullException(nameof(needles));
        if (haystack == null) throw new ArgumentNullException(nameof(haystack));
        if (needles.Any(string.IsNullOrEmpty)) throw new ArgumentNullException(nameof(needles));

        for (int hi=0; hi<haystack.Length; )
        {
            int lowestIndexOf = int.MaxValue;
            int lowestNeedle = -1;
            int lowestNeedleLength = -1;
            int ni = 0;
            foreach (var needle in needles)
            {
                if (ni%10 == 0 && cancel.IsCancellationRequested) yield break;
                int i = haystack.IndexOf(needle, hi);
                if (i != -1 && i<lowestIndexOf) {lowestIndexOf = i; lowestNeedle = ni; lowestNeedleLength = needle.Length; }
                ni++;
            }
            if (lowestNeedle == -1)
            {
                yield return new NeedleResult { Needle = -1, Start = hi, Length = haystack.Length - hi };
                hi = haystack.Length;
            }
            else
            { 
                if (lowestIndexOf > 0) yield return new NeedleResult { Needle = -1, Start = hi, Length = lowestIndexOf - hi };
                yield return new NeedleResult { Needle = lowestNeedle, Start = lowestIndexOf, Length = lowestNeedleLength };
                hi = lowestIndexOf + lowestNeedleLength;
            }
        }
    }


    public static IEnumerable<NeedleResult> ConsiderAllSimultaneously(IEnumerable<string> needles0, string haystack, CancellationToken cancel)
    {
        IList<string> needles = (needles0 as IList<string>) ?? new List<string>(needles0);
        if (needles == null) throw new ArgumentNullException(nameof(needles));
        if (haystack == null) throw new ArgumentNullException(nameof(haystack));
        if (needles.Any(string.IsNullOrEmpty)) throw new ArgumentNullException(nameof(needles));

        var needleCounts = new List<int>(); foreach (var needle in needles) needleCounts.Add(0);
        //
        var xcount = 0;
        for (int ic = 0; ic < haystack.Length; ic++)
        {
            if (ic % 100 == 0 && cancel.IsCancellationRequested) yield break;

            var c = haystack[ic];
            xcount++;
            for (int i = 0; i < needles.Count; i++)
            {
                if (needles[i][needleCounts[i]] == c)
                {
                    needleCounts[i]++;
                    if (needleCounts[i] == needles[i].Length)
                    {
                        if (xcount > needleCounts[i]) yield return new NeedleResult { Needle = -1, Start = ic + 1 - xcount, Length = xcount - needleCounts[i] };
                        yield return new NeedleResult { Needle = i, Start = ic + 1 - needleCounts[i], Length = needleCounts[i] };
                        xcount = 0;
                        for (int j = 0; j < needles.Count; j++) needleCounts[j] = 0;
                        break;
                    }
                }
                else
                {
                    needleCounts[i] = 0;
                }
            }
        }
        if (xcount > 0) yield return new NeedleResult { Needle = -1, Start = haystack.Length - xcount, Length = xcount };
    }


    public static IEnumerable<NeedleResult> ConsiderSomeSimultaneously(IEnumerable<string> needles0, string haystack, CancellationToken cancel)
    {
        IList<string> needles = (needles0 as IList<string>) ?? new List<string>(needles0);
        if (needles == null) throw new ArgumentNullException(nameof(needles));
        if (haystack == null) throw new ArgumentNullException(nameof(haystack));
        if (needles.Any(string.IsNullOrEmpty)) throw new ArgumentNullException(nameof(needles));

        Tuple<char, int> oneMatch = null;
        var onePlusMatches = new Dictionary<char, List<int>>();
        for (int ni=0; ni<needles.Count; ni++)
        {
            var needle = needles[ni];
            var c = needle[0];
            if (needle.Length == 1)
            {
                oneMatch = Tuple.Create(c, ni);
            }
            else
            {
                List<int> l; if (!onePlusMatches.TryGetValue(c, out l)) { l = new List<int>(); onePlusMatches.Add(c, l); }
                l.Add(ni);
            }
        }

        var underConsiderations = new LinkedList<NeedleResult>();

        var xcount = 0;
        for (int ic = 0; ic < haystack.Length; ic++)
        {
            if (ic % 100 == 0 && cancel.IsCancellationRequested) yield break;

            var c = haystack[ic];
            xcount++;

            // Consider all the ones we've had so far
            for (LinkedListNode<NeedleResult> uc = underConsiderations.First; uc!=null;)
            {
                var needle = needles[uc.Value.Needle];
                if (needle[uc.Value.Length] == c)
                {
                    uc.Value = new NeedleResult { Needle = uc.Value.Needle, Length = uc.Value.Length + 1 };
                    if (uc.Value.Length == needle.Length)
                    {
                        if (xcount > needle.Length) yield return new NeedleResult { Needle = -1, Start = ic + 1 - xcount, Length = xcount - needle.Length };
                        yield return new NeedleResult { Needle = uc.Value.Needle, Start = ic + 1 - needle.Length, Length = needle.Length };
                        xcount = 0;
                        underConsiderations.Clear();
                        break;
                    }
                    uc = uc.Next;
                }
                else
                {
                    var tuc = uc.Next;
                    underConsiderations.Remove(uc);
                    uc = tuc;
                }
            }

            // Consider the one-match
            if (oneMatch != null && oneMatch.Item1 == c)
            {
                if (xcount > 1) yield return new NeedleResult { Needle = -1, Start = ic + 1 - xcount, Length = xcount - 1 };
                yield return new NeedleResult { Needle = oneMatch.Item2, Start = ic, Length = 1 };
                xcount = 0;
                underConsiderations.Clear();
                break;
            }

            // Consider the one+ matches
            List<int> l; if (onePlusMatches.TryGetValue(c, out l))
            {
                foreach (var i in l)
                {
                    var uc = new NeedleResult { Needle = i, Length = 1 };
                    underConsiderations.AddLast(uc);
                }
            }
        }

        if (xcount > 0) yield return new NeedleResult { Needle = -1, Start = haystack.Length - xcount, Length = xcount };

    }


}
