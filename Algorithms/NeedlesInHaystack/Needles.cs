using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/// <summary>
/// String search utility, to find all the "needles" in a "haystack".
/// An instance of this class embodies a compiled set of needles ready for searching further haystacks.
/// </summary>
public class Needles
{
    /// <summary>
    /// A span of the haystack that either matched a needle, or didn't match any
    /// </summary>
    public struct Result
    {
        /// <summary>
        /// Start index in the haystack of this span
        /// </summary>
        public int Start;

        /// <summary>
        /// If positive, it means that needle number 'Needle' was matched.
        /// If negative, it means a span of length '-Needle' in the haystack didn't match any needle.
        /// </summary>
        public int Needle;

        public Result(int start, int needle) { Start = start;  Needle = needle; }
    }

    /// <summary>
    /// Finds all the "needles" in a "haystack"
    /// </summary>
    /// <param name="needles">The needles to search for</param>
    /// <param name="haystack">The haystack to search inside</param>
    /// <returns>A sequence that combines both matched and unmatched spans of the haystack, in order of appearance.</returns>
    public static IEnumerable<Result> Find(IList<string> needles, string haystack)
    {
        return Compile(needles).Find(haystack);
    }


    /// <summary>
    /// If you need to find the same set of needles in many haystacks, it's more efficient
    /// to compile the needles first using this method.
    /// </summary>
    /// <param name="needles">The needles to search for</param>
    /// <returns>A compiled needle-searcher, able to search future haystacks</returns>
    public static Needles Compile(IList<string> needles)
    {
        if (needles == null) throw new ArgumentNullException(nameof(needles));
        var n = new Needles();
        n.needles = needles;

        for (int ni = 0; ni < needles.Count; ni++)
        {
            var needle = needles[ni];
            if (string.IsNullOrEmpty(needle)) throw new ArgumentNullException(nameof(needles));
            if (needle.Length == 1) n.match1 = Tuple.Create(ni, needle[0]);
            else if (needle.Length == 2) n.match2 = Tuple.Create(ni, needle[0], needle[1]);
            else if (needle.Length == 3) n.match3 = Tuple.Create(ni, needle[0], needle[1], needle[2]);
            else
            {
                long three = needle[0] | ((long)needle[1] << 16) | ((long)needle[2] << 32);
                if (!n.matchN.ContainsKey(three)) n.matchN[three] = new List<int>();
                n.matchN[three].Add(ni);
            }
        }
        return n;
    }


    /// <summary>
    /// Finds all the needles in a "haystack"
    /// </summary>
    /// <param name="haystack">The haystack to search inside</param>
    /// <returns>A sequence that combines both matched and unmatched spans of the haystack, in order of appearance.</returns>
    public IEnumerable<Result> Find(string haystack)
    {
        if (haystack == null) throw new ArgumentNullException(nameof(haystack));

        var underConsiderations = new LinkedList<Result>();
        int count=0, reportNeedle = -1, reportLength = -1;

        for (int i = 0; i < haystack.Length; i++, count++)
        {
            var c = haystack[i];
            for (LinkedListNode<Result> uc = underConsiderations.First; uc != null;)
            {
                var needle = needles[uc.Value.Needle];
                if (needle[uc.Value.Start] == c)
                {
                    uc.Value = new Result(uc.Value.Start + 1, uc.Value.Needle);
                    if (uc.Value.Start == needle.Length)
                    {
                        if (count > needle.Length) yield return new Result(i - count, needle.Length - count - 1);
                        yield return new Result(i + 1 - needle.Length, uc.Value.Needle);
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

            // PERF: I tried having the main "for i" loop only go up to haystack.Length-3 to avoid testing
            // length each iteration. That gave about 10% perf improvement but made for way uglier code.
            // PERF: We could also keep a rolling "key", which gets shifted left 16 bits each time.
            // PERF: I tried having "match4" as well but that didn't help.
            // PERF: I tried having each match be a "long", but that didn't help.
            if (match1.Item2 == c)
            {
                reportNeedle = match1.Item1; reportLength = 1;
            }
            else if (i + 1 < haystack.Length && match2.Item2 == c && match2.Item3 == haystack[i + 1])
            {
                reportNeedle = match2.Item1; reportLength = 2;
            }
            else if (i + 2 < haystack.Length)
            {
                if (match3.Item2 == c && match3.Item3 == haystack[i + 1] && match3.Item4 == haystack[i + 2])
                {
                    reportNeedle = match3.Item1; reportLength = 3;
                }
                else
                {
                    long key = c | ((long)haystack[i + 1] << 16) | ((long)haystack[i + 2] << 32);
                    // PERF: 25% of this algorithm's time is in TryGetValue
                    List<int> l; if (matchN.TryGetValue(key, out l))
                    {
                        foreach (var ni in l) underConsiderations.AddLast(new Result(1, ni));
                    }
                    continue;
                }
            }
            else
            {
                continue;
            }
            if (count > 0) yield return new Result(i - count, -count);
            yield return new Result(i, reportNeedle);
            i += reportLength - 1;
            nextchar:
            count = -1;
            underConsiderations.Clear();
        }

        if (count > 0) yield return new Result(haystack.Length - count, -count);
    }

    private IList<string> needles;
    private Tuple<int, char> match1 = Tuple.Create(-1, '\0');
    private Tuple<int, char, char> match2 = Tuple.Create(-1, '\0', '\0');
    private Tuple<int, char, char, char> match3 = Tuple.Create(-1, '\0', '\0', '\0');
    private Dictionary<long, List<int>> matchN = new Dictionary<long, List<int>>();

}

