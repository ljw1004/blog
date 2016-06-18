using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

partial class PerfTests
{
    static void Test()
    {
        Test2(nameof(ConsiderThreePlusSimultaneously2), ConsiderThreePlusSimultaneously2, true);
        Test3(nameof(ConsiderThreePlusSimultaneously3), ConsiderThreePlusSimultaneously3, true);
        Test3(nameof(MaximumTheoreticalPerf), MaximumTheoreticalPerf,false);
        Test3(nameof(ConsiderFourPlusSimultaneously), ConsiderFourPlusSimultaneously,false);
        Test3(nameof(ConsiderThreePlusCleaned), ConsiderThreePlusCleaned,false);
        Test3(nameof(ConsiderSomeSimultaneously), ConsiderSomeSimultaneously,true);
        Test3(nameof(ConsiderAllSimultaneously), ConsiderAllSimultaneously,false);
        Test3(nameof(RegexAlternation), RegexAlternation,true);
        Test3(nameof(RepeatedIndexOf), RepeatedIndexOf,true);
    }

    static void Test3(string algorithmName, Func<IEnumerable<string>, string, CancellationToken, IEnumerable<NeedleResult3>> algorithm, bool expectedCorrect)
    {
        GC.Collect();
        Test3(algorithmName, algorithm, "simple case", new[] { "aystb", "sta" }, "haystack_____", "hay,sta,ck_____",expectedCorrect);
        Test3(algorithmName, algorithm, "don't rule self out", new[] { "aac" }, "123aaac456", "123a,aac,456", expectedCorrect);
        Test3(algorithmName, algorithm, "pick earliest match 1", new[] { "abc", "c" }, "1abc2", "1,abc,2", expectedCorrect);
        Test3(algorithmName, algorithm, "pick earliest match 2", new[] { "abc", "a" }, "1abc2", "1,a,bc2|1,abc,2", expectedCorrect);
        Test3(algorithmName, algorithm, "fail on empty input", new[] { "abc", "" }, "1abc2", "FAIL", expectedCorrect);
        using (var cts = new CancellationTokenSource(20000))
        {
            var sw = Stopwatch.StartNew();
            var count = algorithm(TestBigNeedles, string.Concat(Enumerable.Repeat(TestBigHaystack,20)), cts.Token).Count();
            sw.Stop();
            var timeout = cts.Token.IsCancellationRequested ? " [timeout]" : "";
            Console.WriteLine($"{algorithmName} perf - {count} matches in {sw.Elapsed}{timeout}");
        }
    }

    static void Test2(string algorithmName, Func<IEnumerable<string>, string, CancellationToken, IEnumerable<NeedleResult2>> algorithm, bool expectedCorrect)
    {
        GC.Collect();
        Test2(algorithmName, algorithm, "simple case", new[] { "aystb", "sta" }, "haystack_____", "hay,sta,ck_____", expectedCorrect);
        Test2(algorithmName, algorithm, "don't rule self out", new[] { "aac" }, "123aaac456", "123a,aac,456", expectedCorrect);
        Test2(algorithmName, algorithm, "pick earliest match 1", new[] { "abc", "c" }, "1abc2", "1,abc,2", expectedCorrect);
        Test2(algorithmName, algorithm, "pick earliest match 2", new[] { "abc", "a" }, "1abc2", "1,a,bc2|1,abc,2", expectedCorrect);
        Test2(algorithmName, algorithm, "fail on empty input", new[] { "abc", "" }, "1abc2", "FAIL", expectedCorrect);
        using (var cts = new CancellationTokenSource(20000))
        {
            var sw = Stopwatch.StartNew();
            var count = algorithm(TestBigNeedles, string.Concat(Enumerable.Repeat(TestBigHaystack, 20)), cts.Token).Count();
            sw.Stop();
            var timeout = cts.Token.IsCancellationRequested ? " [timeout]" : "";
            Console.WriteLine($"{algorithmName} perf - {count} matches in {sw.Elapsed}{timeout}");
        }
    }

    static void Test3(string algorithmName, Func<IEnumerable<string>, string, CancellationToken, IEnumerable<NeedleResult3>> algorithm, string testName, IEnumerable<string> needles, string haystack, string expected, bool expectedCorrect)
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
        if (!expectedCorrect || expecteds.Contains(actual)) return;
        Console.WriteLine($"{algorithmName} - failed {testName} - actual={actual}; expected={expected}");
    }

    static void Test2(string algorithmName, Func<IEnumerable<string>, string, CancellationToken, IEnumerable<NeedleResult2>> algorithm, string testName, IList<string> needles, string haystack, string expected, bool expectedCorrect)
    {
        string actual;
        try
        {
            var results = algorithm(needles, haystack, CancellationToken.None).ToList();
            actual = string.Join(",", results.Select(r => r.Needle >= 0 ? needles[r.Needle] : haystack.Substring(r.Start, -r.Needle)));
        }
        catch (ArgumentNullException)
        {
            actual = "FAIL";
        }
        var expecteds = expected.Split('|');
        if (!expectedCorrect || expecteds.Contains(actual)) return;
        Console.WriteLine($"{algorithmName} - failed {testName} - actual={actual}; expected={expected}");
    }

    public struct NeedleResult3
    {
        public int Needle;
        public int Start;
        public int Length;
    }

    public struct NeedleResult2
    {
        public int Needle;
        public int Start;
    }

    public static IEnumerable<NeedleResult3> RepeatedIndexOf(IEnumerable<string> needles, string haystack, CancellationToken cancel)
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
                yield return new NeedleResult3 { Needle = -1, Start = hi, Length = haystack.Length - hi };
                hi = haystack.Length;
            }
            else
            { 
                if (lowestIndexOf > 0) yield return new NeedleResult3 { Needle = -1, Start = hi, Length = lowestIndexOf - hi };
                yield return new NeedleResult3 { Needle = lowestNeedle, Start = lowestIndexOf, Length = lowestNeedleLength };
                hi = lowestIndexOf + lowestNeedleLength;
            }
        }
    }


    public static IEnumerable<NeedleResult3> RegexAlternation(IEnumerable<string> needles, string haystack, CancellationToken cancel)
    {
        if (needles == null) throw new ArgumentNullException(nameof(needles));
        if (haystack == null) throw new ArgumentNullException(nameof(haystack));
        if (needles.Any(string.IsNullOrEmpty)) throw new ArgumentNullException(nameof(needles));

        var sb = new StringBuilder();
        var dn = new Dictionary<string, int>();
        int ni = 0;
        foreach (var needle in needles)
        {
            dn[needle] = ni;
            ni++;
            if (ni > 1) sb.Append("|");
            sb.Append(needle.Replace("\\", "\\\\").Replace("|", "\\|"));
        }
        var re = new Regex(sb.ToString(), RegexOptions.Compiled);
        var matches = re.Matches(haystack);
        int xcount = 0;
        foreach (Match match in matches)
        {
            if (cancel.IsCancellationRequested) yield break;
            if (match.Index > xcount) yield return new NeedleResult3 { Needle = -1, Start = xcount, Length = match.Index - xcount };
            ni = dn[match.Value];
            yield return new NeedleResult3 { Needle = ni, Start = match.Index, Length = match.Length };
            xcount = match.Index + match.Length;
        }
        if (haystack.Length > xcount) yield return new NeedleResult3 { Needle = -1, Start = xcount, Length = haystack.Length - xcount };
    }


    public static IEnumerable<NeedleResult3> ConsiderAllSimultaneously(IEnumerable<string> needles0, string haystack, CancellationToken cancel)
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
                        if (xcount > needleCounts[i]) yield return new NeedleResult3 { Needle = -1, Start = ic + 1 - xcount, Length = xcount - needleCounts[i] };
                        yield return new NeedleResult3 { Needle = i, Start = ic + 1 - needleCounts[i], Length = needleCounts[i] };
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
        if (xcount > 0) yield return new NeedleResult3 { Needle = -1, Start = haystack.Length - xcount, Length = xcount };
    }


    public static IEnumerable<NeedleResult3> ConsiderSomeSimultaneously(IEnumerable<string> needles0, string haystack, CancellationToken cancel)
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

        var underConsiderations = new LinkedList<NeedleResult3>();

        var xcount = 0;
        for (int ic = 0; ic < haystack.Length; ic++)
        {
            if (ic % 100 == 0 && cancel.IsCancellationRequested) yield break;

            var c = haystack[ic];
            xcount++;

            // Consider all the ones we've had so far
            for (LinkedListNode<NeedleResult3> uc = underConsiderations.First; uc!=null;)
            {
                var needle = needles[uc.Value.Needle];
                if (needle[uc.Value.Length] == c)
                {
                    uc.Value = new NeedleResult3 { Needle = uc.Value.Needle, Length = uc.Value.Length + 1 };
                    if (uc.Value.Length == needle.Length)
                    {
                        if (xcount > needle.Length) yield return new NeedleResult3 { Needle = -1, Start = ic + 1 - xcount, Length = xcount - needle.Length };
                        yield return new NeedleResult3 { Needle = uc.Value.Needle, Start = ic + 1 - needle.Length, Length = needle.Length };
                        xcount = 0;
                        underConsiderations.Clear();
                        goto nextchar;
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
                if (xcount > 1) yield return new NeedleResult3 { Needle = -1, Start = ic + 1 - xcount, Length = xcount - 1 };
                yield return new NeedleResult3 { Needle = oneMatch.Item2, Start = ic, Length = 1 };
                xcount = 0;
                underConsiderations.Clear();
                goto nextchar;
            }

            // Consider the one+ matches
            List<int> l; if (onePlusMatches.TryGetValue(c, out l))
            {
                foreach (var i in l)
                {
                    var uc = new NeedleResult3 { Needle = i, Length = 1 };
                    underConsiderations.AddLast(uc);
                }
            }
            nextchar:;
        }

        if (xcount > 0) yield return new NeedleResult3 { Needle = -1, Start = haystack.Length - xcount, Length = xcount };

    }




    public static IEnumerable<NeedleResult3> ConsiderThreePlusSimultaneously3(IEnumerable<string> needles0, string haystack, CancellationToken cancel)
    {
        IList<string> needles = (needles0 as IList<string>) ?? new List<string>(needles0);
        if (needles == null) throw new ArgumentNullException(nameof(needles));
        if (haystack == null) throw new ArgumentNullException(nameof(haystack));
        if (needles.Any(string.IsNullOrEmpty)) throw new ArgumentNullException(nameof(needles));

        Tuple<int, char> oneMatch = Tuple.Create(-1, '\0');
        Tuple<int, char, char> twoMatch = Tuple.Create(-1, '\0', '\0');
        Tuple<int, char, char, char> threeMatch = Tuple.Create(-1, '\0', '\0', '\0');
        var threePlusMatches = new Dictionary<long, List<int>>();
        for (int ni = 0; ni < needles.Count; ni++)
        {
            var needle = needles[ni];
            if (needle.Length == 1) oneMatch = Tuple.Create(ni, needle[0]);
            else if (needle.Length == 2) twoMatch = Tuple.Create(ni, needle[0], needle[1]);
            else if (needle.Length == 3) threeMatch = Tuple.Create(ni, needle[0], needle[1], needle[2]);
            else
            {
                long three = needle[0] | ((long)needle[1] << 16) | ((long)needle[2] << 32);
                if (!threePlusMatches.ContainsKey(three)) threePlusMatches[three] = new List<int>();
                threePlusMatches[three].Add(ni);
            }
        }

        var underConsiderations = new LinkedList<NeedleResult3>();
        int reportNeedle = -1, reportLength = -1;
        var xcount = 0;
        for (int ic = 0; ic < haystack.Length; ic++)
        {
            if (ic % 100 == 0 && cancel.IsCancellationRequested) yield break;

            var c = haystack[ic];
            xcount++;

            for (LinkedListNode<NeedleResult3> uc = underConsiderations.First; uc != null;)
            {
                var needle = needles[uc.Value.Needle];
                if (needle[uc.Value.Length] == c)
                {
                    uc.Value = new NeedleResult3 { Needle = uc.Value.Needle, Length = uc.Value.Length + 1 };
                    if (uc.Value.Length == needle.Length)
                    {
                        if (xcount > needle.Length) yield return new NeedleResult3 { Needle = -1, Start = ic + 1 - xcount, Length = xcount - needle.Length };
                        yield return new NeedleResult3 { Needle = uc.Value.Needle, Start = ic + 1 - needle.Length, Length = needle.Length };
                        goto nextchar;
                    }
                    uc = uc.Next;
                }
                else
                {
                    var temp = uc.Next;
                    underConsiderations.Remove(uc);
                    uc = temp;
                }
            }

            if (oneMatch.Item2 == c)
            {
                reportNeedle = oneMatch.Item1; reportLength = 1;
            }
            else if (ic + 1 < haystack.Length && twoMatch.Item2 == c && twoMatch.Item3 == haystack[ic + 1])
            {
                reportNeedle = twoMatch.Item1; reportLength = 2;
            }
            else if (ic + 2 < haystack.Length)
            {
                if (threeMatch.Item2 == c && threeMatch.Item3 == haystack[ic + 1] && threeMatch.Item4 == haystack[ic + 2])
                {
                    reportNeedle = threeMatch.Item1; reportLength = 3;
                }
                else
                {
                    long three = c | ((long)haystack[ic + 1] << 16) | ((long)haystack[ic + 2] << 32);
                    List<int> l; if (threePlusMatches.TryGetValue(three, out l))
                    {
                        foreach (var i in l)
                        {
                            var uc = new NeedleResult3 { Needle = i, Length = 1 };
                            underConsiderations.AddLast(uc);
                        }
                    }
                    continue;
                }
            }
            else
            {
                continue;
            }
            if (xcount > 1) yield return new NeedleResult3 { Needle = -1, Start = ic + 1 - xcount, Length = xcount - 1 };
            yield return new NeedleResult3 { Needle = reportNeedle, Start = ic, Length = reportLength };
            ic += reportLength - 1;
            nextchar:
            xcount = 0;
            underConsiderations.Clear();
        }

        if (xcount > 0) yield return new NeedleResult3 { Needle = -1, Start = haystack.Length - xcount, Length = xcount };

    }


    public static IEnumerable<NeedleResult2> ConsiderThreePlusSimultaneously2(IEnumerable<string> needles0, string haystack, CancellationToken cancel)
    {
        IList<string> needles = (needles0 as IList<string>) ?? new List<string>(needles0);
        if (needles == null) throw new ArgumentNullException(nameof(needles));
        if (haystack == null) throw new ArgumentNullException(nameof(haystack));
        if (needles.Any(string.IsNullOrEmpty)) throw new ArgumentNullException(nameof(needles));

        Tuple<int, char> oneMatch = Tuple.Create(-1, '\0');
        Tuple<int, char, char> twoMatch = Tuple.Create(-1, '\0', '\0');
        Tuple<int, char, char, char> threeMatch = Tuple.Create(-1, '\0', '\0', '\0');
        var threePlusMatches = new Dictionary<long, List<int>>();
        for (int ni = 0; ni < needles.Count; ni++)
        {
            var needle = needles[ni];
            if (needle.Length == 1) oneMatch = Tuple.Create(ni, needle[0]);
            else if (needle.Length == 2) twoMatch = Tuple.Create(ni, needle[0], needle[1]);
            else if (needle.Length == 3) threeMatch = Tuple.Create(ni, needle[0], needle[1], needle[2]);
            else
            {
                long three = needle[0] | ((long)needle[1] << 16) | ((long)needle[2] << 32);
                if (!threePlusMatches.ContainsKey(three)) threePlusMatches[three] = new List<int>();
                threePlusMatches[three].Add(ni);
            }
        }

        var underConsiderations = new LinkedList<NeedleResult2>();
        int reportNeedle = -1, reportLength = -1;
        var xcount = 0;
        for (int ic = 0; ic < haystack.Length; ic++)
        {
            if (ic % 100 == 0 && cancel.IsCancellationRequested) yield break;

            var c = haystack[ic];
            xcount++;

            for (LinkedListNode<NeedleResult2> uc = underConsiderations.First; uc != null;)
            {
                var needle = needles[uc.Value.Needle];
                if (needle[uc.Value.Start] == c)
                {
                    uc.Value = new NeedleResult2 { Needle = uc.Value.Needle, Start = uc.Value.Start + 1 };
                    if (uc.Value.Start == needle.Length)
                    {
                        if (xcount > needle.Length) yield return new NeedleResult2 { Needle = needle.Length-xcount, Start = ic + 1 - xcount };
                        yield return new NeedleResult2 { Needle = uc.Value.Needle, Start = ic + 1 - needle.Length };
                        goto nextchar;
                    }
                    uc = uc.Next;
                }
                else
                {
                    var temp = uc.Next;
                    underConsiderations.Remove(uc);
                    uc = temp;
                }
            }

            if (oneMatch.Item2 == c)
            {
                reportNeedle = oneMatch.Item1; reportLength = 1;
            }
            else if (ic + 1 < haystack.Length && twoMatch.Item2 == c && twoMatch.Item3 == haystack[ic + 1])
            {
                reportNeedle = twoMatch.Item1; reportLength = 2;
            }
            else if (ic + 2 < haystack.Length)
            {
                if (threeMatch.Item2 == c && threeMatch.Item3 == haystack[ic + 1] && threeMatch.Item4 == haystack[ic + 2])
                {
                    reportNeedle = threeMatch.Item1; reportLength = 3;
                }
                else
                {
                    long three = c | ((long)haystack[ic + 1] << 16) | ((long)haystack[ic + 2] << 32);
                    List<int> l; if (threePlusMatches.TryGetValue(three, out l))
                    {
                        foreach (var i in l)
                        {
                            var uc = new NeedleResult2 { Needle = i, Start = 1 };
                            underConsiderations.AddLast(uc);
                        }
                    }
                    continue;
                }
            }
            else
            {
                continue;
            }
            if (xcount > 1) yield return new NeedleResult2 { Needle = 1-xcount, Start = ic + 1 - xcount };
            yield return new NeedleResult2 { Needle = reportNeedle, Start = ic };
            ic += reportLength - 1;
            nextchar:
            xcount = 0;
            underConsiderations.Clear();
        }

        if (xcount > 0) yield return new NeedleResult2 { Needle = -xcount, Start = haystack.Length - xcount };
    }



    public static IEnumerable<NeedleResult3> ConsiderThreePlusCleaned(IEnumerable<string> needles0, string haystack, CancellationToken cancel)
    {
        IList<string> needles = (needles0 as IList<string>) ?? new List<string>(needles0);
        if (needles == null) throw new ArgumentNullException(nameof(needles));
        if (haystack == null) throw new ArgumentNullException(nameof(haystack));
        if (needles.Any(string.IsNullOrEmpty)) throw new ArgumentNullException(nameof(needles));

        Tuple<int, char> oneMatch = Tuple.Create(-1, '\0');
        Tuple<int, char, char> twoMatch = Tuple.Create(-1, '\0', '\0');
        Tuple<int, char, char, char> threeMatch = Tuple.Create(-1, '\0', '\0', '\0');
        var threePlusMatches = new Dictionary<long, List<int>>();
        for (int ni = 0; ni < needles.Count; ni++)
        {
            var needle = needles[ni];
            if (needle.Length == 1) oneMatch = Tuple.Create(ni, needle[0]);
            else if (needle.Length == 2) twoMatch = Tuple.Create(ni, needle[0], needle[1]);
            else if (needle.Length == 3) threeMatch = Tuple.Create(ni, needle[0], needle[1], needle[2]);
            else
            {
                long three = needle[0] | (needle[1] << 16) | (needle[2] << 24);
                if (!threePlusMatches.ContainsKey(three)) threePlusMatches[three] = new List<int>();
                threePlusMatches[three].Add(ni);
            }
        }

        var underConsiderations = new LinkedList<NeedleResult3>();
        var xcount = 0;
        int reportNeedle = -1, reportLength = -1;

        for (int ic = 0; ic < haystack.Length-2; ic++)
        {
            if (ic % 100 == 0 && cancel.IsCancellationRequested) yield break;

            var c = haystack[ic];
            xcount++;

            // Consider all the ones we've had so far
            for (LinkedListNode<NeedleResult3> uc = underConsiderations.First; uc != null;)
            {
                var needle = needles[uc.Value.Needle];
                if (needle[uc.Value.Length] == c)
                {
                    uc.Value = new NeedleResult3 { Needle = uc.Value.Needle, Length = uc.Value.Length + 1 };
                    if (uc.Value.Length == needle.Length)
                    {
                        if (xcount > needle.Length) yield return new NeedleResult3 { Needle = -1, Start = ic + 1 - xcount, Length = xcount - needle.Length };
                        yield return new NeedleResult3 { Needle = uc.Value.Needle, Start = ic + 1 - needle.Length, Length = needle.Length };
                        xcount = 0;
                        underConsiderations.Clear();
                        goto nextchar;
                    }
                    uc = uc.Next;
                }
                else
                {
                    var temp = uc.Next;
                    underConsiderations.Remove(uc);
                    uc = temp;
                }
            }

            if (oneMatch.Item2 == c)
            {
                reportNeedle = oneMatch.Item1; reportLength = 1;
            }
            else if (twoMatch.Item2 == c && twoMatch.Item3 == haystack[ic + 1])
            {
                reportNeedle = twoMatch.Item1; reportLength = 2;
            }
            else if (threeMatch.Item2 == c && threeMatch.Item3 == haystack[ic + 1] && threeMatch.Item4 == haystack[ic + 2])
            {
                reportNeedle = threeMatch.Item1; reportLength = 3;
            }
            else 
            {
                long three = c | (haystack[ic + 1] << 16) | (haystack[ic + 2] << 24);
                List<int> l; if (threePlusMatches.TryGetValue(three, out l))
                {
                    foreach (var i in l)
                    {
                        var uc = new NeedleResult3 { Needle = i, Length = 1 };
                        underConsiderations.AddLast(uc);
                    }
                }
                continue;
            }
            if (xcount > 1) yield return new NeedleResult3 { Needle = -1, Start = ic + 1 - xcount, Length = xcount - 1 };
            yield return new NeedleResult3 { Needle = reportNeedle, Start = ic, Length = reportLength };
            ic += reportLength-1;
            xcount = 0;
            underConsiderations.Clear();

            nextchar:;
        }

        if (xcount > 0) yield return new NeedleResult3 { Needle = -1, Start = haystack.Length - xcount, Length = xcount };

    }


    public static IEnumerable<NeedleResult3> ConsiderFourPlusSimultaneously(IEnumerable<string> needles0, string haystack, CancellationToken cancel)
    {
        IList<string> needles = (needles0 as IList<string>) ?? new List<string>(needles0);
        if (needles == null) throw new ArgumentNullException(nameof(needles));
        if (haystack == null) throw new ArgumentNullException(nameof(haystack));
        if (needles.Any(string.IsNullOrEmpty)) throw new ArgumentNullException(nameof(needles));

        long oneMatch = 0, twoMatch = 0, threeMatch = 0, fourMatch = 0;
        int oneNeedle = -1, twoNeedle = -1, threeNeedle = -1, fourNeedle = -1;
        var fourPlusMatches = new Dictionary<long, List<int>>();
        for (int ni = 0; ni < needles.Count; ni++)
        {
            var needle = needles[ni];
            if (needle.Length == 1)
            {
                oneMatch = needle[0]; oneNeedle = ni;
            }
            else if (needle.Length == 2)
            {
                twoMatch = needle[0] | ((long)needle[1] << 16); twoNeedle = ni;
            }
            else if (needle.Length == 3)
            {
                threeMatch = needle[0] | ((long)needle[1] << 16) | ((long)needle[2] << 32); threeNeedle = ni;
            }
            else if (needle.Length == 4)
            {
                fourMatch = needle[0] | ((long)needle[1] << 16) | ((long)needle[2] << 32) | ((long)needle[3] << 48); fourNeedle = ni;
            }
            else
            {
                long four = needle[0] | ((long)needle[1] << 16) | ((long)needle[2] << 32) | ((long)needle[3] << 48);
                List<int> l; if (!fourPlusMatches.TryGetValue(four, out l)) { l = new List<int>(); fourPlusMatches.Add(four, l); }
                l.Add(ni);
            }
        }

        var underConsiderations = new LinkedList<NeedleResult3>();

        var xcount = 0;
        int reportNeedle = -1, reportLength = -1;
        for (int ic = 0; ic < haystack.Length-3; ic++)
        {
            if (ic % 100 == 0 && cancel.IsCancellationRequested) yield break;

            var c = haystack[ic];
            xcount++;

            // Consider all the ones we've had so far
            for (LinkedListNode<NeedleResult3> uc = underConsiderations.First; uc != null;)
            {
                var needle = needles[uc.Value.Needle];
                if (needle[uc.Value.Length] == c)
                {
                    uc.Value = new NeedleResult3 { Needle = uc.Value.Needle, Length = uc.Value.Length + 1 };
                    if (uc.Value.Length == needle.Length)
                    {
                        if (xcount > needle.Length) yield return new NeedleResult3 { Needle = -1, Start = ic+1-needle.Length - xcount + reportLength, Length = xcount - needle.Length };
                        yield return new NeedleResult3 { Needle = uc.Value.Needle, Start = ic+1-needle.Length, Length = needle.Length };
                        xcount = 0;
                        underConsiderations.Clear();
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

            long fourcc = (long)c | ((long)haystack[ic + 1] << 16) | ((long)haystack[ic + 2] << 32) | ((long)haystack[ic + 3] << 48);
            List<int> l;

            if (oneMatch == (fourcc & 0xffff))
            {
                reportNeedle = oneNeedle; reportLength = 1;
            }
            else if (twoMatch == (fourcc & 0xffffffff))
            {
                reportNeedle = twoNeedle; reportLength = 2; ic += 1;
            }
            else if (threeMatch == (fourcc & 0xffffffffffff))
            {
                reportNeedle = threeNeedle; reportLength = 3; ic += 2;
            }
            else if (fourMatch == fourcc)
            {
                reportNeedle = fourNeedle; reportLength = 4; ic += 3;
            }
            else
            {
                if (!fourPlusMatches.TryGetValue(fourcc, out l)) continue;
                foreach (var i in l) underConsiderations.AddLast(new NeedleResult3 { Needle = i, Length = 1 });
                continue;
            }
            if (xcount > 1) yield return new NeedleResult3 { Needle = -1, Start = ic +1 - xcount, Length = xcount - 1 };
            yield return new NeedleResult3 { Needle = reportNeedle, Start = ic+1-reportLength, Length = reportLength };
            xcount = 0;
            underConsiderations.Clear();
        }

        if (xcount > 0) yield return new NeedleResult3 { Needle = -1, Start = haystack.Length - xcount, Length = xcount };
    }


    public static IEnumerable<NeedleResult3> MaximumTheoreticalPerf(IEnumerable<string> needles, string haystack, CancellationToken cancel)
    {
        for (int ic=0; ic<haystack.Length; ic++)
        {
            if (ic % 100 == 0 && cancel.IsCancellationRequested) yield break;
            var c = haystack[ic];
            if (ic < 3000000) yield return new NeedleResult3 { Needle = c, Start = ic, Length = 1 };
        }
    }

}
