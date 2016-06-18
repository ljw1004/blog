using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/// <summary>
/// String search utility, to find all the "needles" in a "haystack"
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


    private IList<string> needles;
    private Tuple<int, char> oneMatch = Tuple.Create(-1, '\0');
    private Tuple<int, char, char> twoMatch = Tuple.Create(-1, '\0', '\0');
    private Tuple<int, char, char, char> threeMatch = Tuple.Create(-1, '\0', '\0', '\0');
    private Dictionary<long,List<int>> threePlusMatches = new Dictionary<long, List<int>>();


    public static Needles Compile(IList<string> needles)
    {
        if (needles == null) throw new ArgumentNullException(nameof(needles));
        var n = new Needles();
        n.needles = needles;

        for (int ni = 0; ni < needles.Count; ni++)
        {
            var needle = needles[ni];
            if (string.IsNullOrEmpty(needle)) throw new ArgumentNullException(nameof(needles));
            if (needle.Length == 1) n.oneMatch = Tuple.Create(ni, needle[0]);
            else if (needle.Length == 2) n.twoMatch = Tuple.Create(ni, needle[0], needle[1]);
            else if (needle.Length == 3) n.threeMatch = Tuple.Create(ni, needle[0], needle[1], needle[2]);
            else
            {
                long three = needle[0] | ((long)needle[1] << 16) | ((long)needle[2] << 32);
                if (!n.threePlusMatches.ContainsKey(three)) n.threePlusMatches[three] = new List<int>();
                n.threePlusMatches[three].Add(ni);
            }
        }
        return n;
    }


    public IEnumerable<Result> Find(string haystack)
    {
        if (haystack == null) throw new ArgumentNullException(nameof(haystack));

        var underConsiderations = new LinkedList<Result>();
        int reportNeedle = -1, reportLength = -1;
        var xcount = 0;
        for (int ic = 0; ic < haystack.Length; ic++)
        {

            var c = haystack[ic];
            xcount++;

            for (LinkedListNode<Result> uc = underConsiderations.First; uc != null;)
            {
                var needle = needles[uc.Value.Needle];
                if (needle[uc.Value.Start] == c)
                {
                    uc.Value = new Result { Needle = uc.Value.Needle, Start = uc.Value.Start + 1 };
                    if (uc.Value.Start == needle.Length)
                    {
                        if (xcount > needle.Length) yield return new Result { Needle = needle.Length - xcount, Start = ic + 1 - xcount };
                        yield return new Result { Needle = uc.Value.Needle, Start = ic + 1 - needle.Length };
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
                            var uc = new Result { Needle = i, Start = 1 };
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
            if (xcount > 1) yield return new Result { Needle = 1 - xcount, Start = ic + 1 - xcount };
            yield return new Result { Needle = reportNeedle, Start = ic };
            ic += reportLength - 1;
            nextchar:
            xcount = 0;
            underConsiderations.Clear();
        }

        if (xcount > 0) yield return new Result { Needle = -xcount, Start = haystack.Length - xcount };
    }

}

