using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

static class AwaitDistributions {
  public static void Test() {
    //var state = State.AtPlace(0);
    //Console.WriteLine($"Initial {state}");
    //foreach (var (p, nextState) in NextState(state)) {
    //  Console.WriteLine($"-> {nextState}  [{p}]");
    //}
    var d = Oned().ToList();
    Console.WriteLine(String.Join(",", d));
  }

  static async IDistribution<string> Oned() {
    var b = await Distribution.Flip(0.8);
    return b ? "hello" : "world";
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

  static async IDistribution<State> NextState(State s) {
    if (s.IsAtPlace) {
      var shouldStay = await Distribution.Flip(0.8);
      if (shouldStay) return s;
      var newDest = (s.Location == 0) ? new[] { 20, 50 } : (s.Location == 20) ? new[] { 0, 50 } : new[] { 0, 20 };
      return State.InTransit(s.Location, await Distribution.Uniform(newDest));
    } else {
      if (s.Location == s.Target) return State.AtPlace(s.Location);
      else if (s.Location < s.Target) return State.InTransit(s.Location + 1, s.Target);
      else return State.InTransit(s.Location - 1, s.Target);
    }
  }

}

[AsyncMethodBuilder(typeof(Distribution.DistributionBuilder<>))]
public interface IDistribution<T> : IEnumerable<(double prob, T value)> { }

public static class Distribution {
  public static IDistribution<T> ToDistribution<T>(this IEnumerable<(double, T)> src) => new EnumerableDistribution<T>(src);

  public static IDistribution<bool> Flip(double d) => new[] { (d, true), (1.0 - d, false) }.ToDistribution();
  public static IDistribution<T> Uniform<T>(IList<T> values) => values.Select(v => (1.0 / values.Count, v)).ToDistribution();
  public static IEnumerable<(double, int)> UniformRange(int start, int count) => Enumerable.Range(start, count).Select(v => (1.0 / count, v)).ToDistribution();

  public static DistributionBuilder<T>.DistributionAwaiter GetAwaiter<T>(this IDistribution<T> src) => new DistributionBuilder<T>.DistributionAwaiter(src);

  private class EnumerableDistribution<T> : IDistribution<T> {
    private IEnumerable<(double, T)> _src;
    public EnumerableDistribution(IEnumerable<(double, T)> src) { _src = src; }
    public IEnumerator<(double prob, T value)> GetEnumerator() => _src.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
  }

  public class DistributionBuilder<T> {
    // used by the factory
    DistributionAble _task;
    public static DistributionBuilder<T> Create() => new DistributionBuilder<T>();
    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine => _task = new DistributionAble(stateMachine);
    public IDistribution<T> Task => _task;

    // used per instance:
    private DistributionAtor _DistributionAtor = null;
    public void SetStateMachine(IAsyncStateMachine stateMachine) => throw new InvalidOperationException("DistributionBuilder<T>.SetStateMachine");
    [SecurityCritical] public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine => AwaitOnCompleted(ref awaiter, ref stateMachine);

    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine {
      throw new NotImplementedException("DistributionBuilder.AwaitOnCompleted");
    }
    public void SetException(Exception exception) {
      throw new NotImplementedException("DistributionBuilder.SetException");
      // _DistributionAtor._currentException = ExceptionDispatchInfo.Capture(exception);
    } 
    public void SetResult(T result) {
      _DistributionAtor.currentResult = result;
    }

    public class DistributionAwaiter : INotifyCompletion, ICriticalNotifyCompletion {
      public IEnumerator<(double prob, T value)> enumerator;
      public DistributionAwaiter(IEnumerable<(double prob, T value)> src) {
        enumerator = src.GetEnumerator();
      }
      public bool IsCompleted => false;
      public void OnCompleted(Action continuation) { }
      public void UnsafeOnCompleted(Action continuation) { }
      public T GetResult() => default(T);
    }

    class DistributionAble : IDistribution<T> {
      private Func<DistributionAtor, IAsyncStateMachine> clone;

      public IEnumerator<(double prob, T value)> GetEnumerator() {
        var ator = new DistributionAtor();
        ator.sm = clone(ator);
        return ator;
      }
      IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

      public DistributionAble(IAsyncStateMachine sm) {
        clone = (distributionAtor) =>
        {
          var memberwiseCloneMethod = sm.GetType().GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
          var builderField = sm.GetType().GetField("<>t__builder", BindingFlags.Instance | BindingFlags.Public);
          var clone = (IAsyncStateMachine)memberwiseCloneMethod.Invoke(sm, null);
          builderField.SetValue(clone, new DistributionBuilder<T> { _DistributionAtor = distributionAtor });
          return clone;
        };
      }
    }

    class ResumePoint {
      public IEnumerator<object> Enumerator;
      public int State;
      public double Probability;
    }

    class DistributionAtor : IEnumerator<(double prob, T value)> {
      public IAsyncStateMachine sm;
      public Stack<ResumePoint> stack = new Stack<ResumePoint>(new ResumePoint[] { null });
      public T currentResult;
      public ExceptionDispatchInfo currentException;

      public (double prob, T value) Current {
        get {
          if (currentException != null) currentException.Throw();
          double prob = stack.Count == 0 ? 1.0 : stack.Peek().Probability;
          return (prob, currentResult);
        }
      }

      public bool MoveNext() {
        if (stack.Count == 1 && stack.Peek() == null) {
          // This is the sentinel. We are at the first execution of the async method. We'll necessarily
          // get either a value or an exception.
          stack.Pop();
          currentException = null;
          sm.MoveNext();
          return true;
        } else if (stack.Count == 0) {
          // We had removed the sentinel, and no additional things got pushed on the stack, so we're
          // at the end of a singletone execution
          return false;
        }
        if (stack.Count == 1) {
          // If we're called with an empty stack, then we are the first execution of this async method.
          // There will necessarily be either a value or an exception.
          currentException = null;
          sm.MoveNext();
          return true;
        } else {
          // Otherwise, the previous call to MoveNext must have pushed at least some resume points
          // on the stack, and executed through to a return statement or an exception. We will pop
          // as many resume points as needed to get back.
          while (stack.Count > 0) {
            bool hasMore = stack.Peek().Enumerator.MoveNext();
            if (!hasMore) { stack.Pop(); continue; }
            var current = stack.Peek().Enumerator.Current;
            // TODO: extract out its value (to stick into awaiter result) and probability (to multiply by parent)
            return true;
          }
          // If we've run out of stack, then we're at the end of the enumeration.
          return false;
        }
      }

      object IEnumerator.Current => Current;
      public void Dispose() {
        // TODO: this needs a way to jump into the state machine and execute finally blocks.
        // Not sure how to do it.
      }
      public void Reset() { }
    }

  }

}