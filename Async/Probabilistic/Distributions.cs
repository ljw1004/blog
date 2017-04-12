using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

static class Distributions {

  static int count = 0;

  public static void Test() {
    Stopwatch s;
    var observed = new[] { 50, 50, 49, 48, 47, 48, 46 };

    for (int repeat = 0; repeat < 3; repeat++) {
      Console.WriteLine("*** EAGER PRUNE");
      s = Stopwatch.StartNew();
      count = 0;
      foreach (var (p, state) in Normalize(FinalStateOfReadingsThatMatchFast(observed))) {
        Console.WriteLine($"p({state}) = {p:.000}");
      }
      Console.WriteLine($"{count} traces in {s.Elapsed.TotalSeconds:.00} seconds");

      Console.WriteLine("*** NAIVE FULL-STATE EXPLORATION");
      s = Stopwatch.StartNew();
      count = 0;
      foreach (var (p, state) in Normalize(FinalStateOfReadingsThatMatch(observed))) {
        Console.WriteLine($"p({state}) = {p:.000}");
      }
      Console.WriteLine($"{count} traces in {s.Elapsed.TotalSeconds:.0} seconds");
    }
  }

  struct State {
    public readonly int Location;
    public readonly int Target;
    
    public bool IsAtPlace => Target == -1;
    public bool IsInTransit => Target != -1;
    public static State AtPlace(int loc) { return new State(loc, -1); }
    public static State InTransit(int loc, int target) { return new State(loc, target); }
    private State(int loc, int target) { Location = loc; Target = target; }
    public override string ToString() => IsAtPlace ? $"At {Location}" : $"InTransit {Location}->{Target}";
  }

  static IEnumerable<(double,State)> NextState(State s) {
    if (s.IsAtPlace) {
      foreach (var (p, shouldStay) in Flip(0.8)) {
        if (shouldStay) {
          yield return (p, s);
        } else {
          var targets = (s.Location == 0) ? new[] { 20, 50 } : (s.Location == 20) ? new[] { 0, 50 } : new[] { 0, 20 };
          foreach (var (q, newDest) in Uniform(targets)) {
            yield return (p * q, State.InTransit(s.Location, newDest));
          }
        }
      }
    } else {
      if (s.Location == s.Target) {
        yield return (1.0, State.AtPlace(s.Location));
      } else if (s.Location < s.Target) {
        yield return (1.0, State.InTransit(s.Location + 1, s.Target));
      } else {
        yield return (1.0, State.InTransit(s.Location - 1, s.Target));
      }
    }
  }

  static IEnumerable<(double, State)> GenerateInitialStates() {
    foreach (var (p, loc) in Uniform(new[] { 0, 20, 50 })) {
      foreach (var (q, flip) in Flip(0.5)) {
        if (flip) {
          yield return (p * q, State.AtPlace(loc));
        } else {
          foreach (var (r, start) in UniformRange(0, 60)) {
            yield return (p * (1-q) * r, State.InTransit(start, loc));
          }
        }
      }
    }
  }

  static IEnumerable<(double, (IEnumerable<int>, State))> GenerateReadingsAndSubsequentState(int num, State initialState) {
    if (num == 0) {
      yield return (1, (new int[] {}, initialState));
    } else { 
      foreach (var (p, nextState) in NextState(initialState)) {
        foreach (var (q, sensor) in SensorReading(nextState)) {
          foreach (var (r, (readings,subsequentState)) in GenerateReadingsAndSubsequentState(num - 1, nextState)) {
            yield return (p * q * r, (new[] { sensor }.Concat(readings), subsequentState));
          }
        }
      }
    }
  }

  static IEnumerable<(double, (IEnumerable<int>, State))> GenerateReadingsThatMatchAndSubsequentState(int i, IList<int> observed, State initialState) {
    if (i == observed.Count) {
      yield return (1, (new int[] { }, initialState));
    } else {
      foreach (var (p, nextState) in NextState(initialState)) {
        foreach (var (q, sensor) in SensorReading(nextState)) {
          if (sensor == observed[i]) {
            foreach (var (r, (readings, subsequentState)) in GenerateReadingsThatMatchAndSubsequentState(i+1, observed, nextState)) {
              yield return (p * q * r, (new[] { sensor }.Concat(readings), subsequentState));
            }
          }
        }
      }
    }
  }


  static IEnumerable<(double, State)> FinalStateOfReadingsThatMatch(IList<int> observed) {
    foreach (var (p, initialState) in GenerateInitialStates()) {
      foreach (var (q, (readings, subsequentState)) in GenerateReadingsAndSubsequentState(observed.Count, initialState)) {
        count++;
        if (readings.SequenceEqual(observed)) yield return (p * q, subsequentState);
      }
    }
  }

  static IEnumerable<(double, State)> FinalStateOfReadingsThatMatchFast(IList<int> observed) {
    foreach (var (p, initialState) in GenerateInitialStates()) {
      foreach (var (q, (readings, subsequentState)) in GenerateReadingsThatMatchAndSubsequentState(0, observed, initialState)) {
        count++;
        yield return (p * q, subsequentState);
      }
    }
  }

  static IEnumerable<(double, int)> SensorReading(State s) {
    return new[] { (0.1, s.Location - 2), (0.2, s.Location - 1), (0.4, s.Location), (0.2, s.Location + 1), (0.1, s.Location) };
  }

  static IEnumerable<(double,T)> Uniform<T>(IList<T> values) {
    return values.Select(v => (1.0/values.Count, v));
  }

  static IEnumerable<(double, int)> UniformRange(int min, int count) {
    for (int i=min; i<min+count; i++) {
      yield return (1.0 / count, i);
    }
  }

  static IEnumerable<(double, bool)> Flip(double p) {
    return new[] { (p, true), (1.0 - p, false) };
  }

  static IEnumerable<(double, T)> Normalize<T>(IEnumerable<(double prob, T val)> src) {
    var grouped = src.GroupBy(pv => pv.val).Select(group => (prob: group.Select(pv => pv.prob).Sum(), val: group.Key)).ToList();
    var sum = grouped.Select(pv => pv.prob).Sum();
    return grouped.Select(pv => (pv.prob / sum, pv.val));
  }

}
