using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Security;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously


class Program
{
    static Random RND = new Random();

    static void Main()
    {
        var p = Foo();
        foreach (var sample in p.Take(3)) Console.WriteLine($"sample: {sample}");
    }

    static async IProbable<int> Foo()
    {
        Console.WriteLine("Foo");
        return RND.Next();
    }

}

[AsyncMethodBuilder(typeof(ProbableBuilder<>))]
public interface IProbable<T> : IEnumerable<T> { }

public class ProbableBuilder<T>
{
    // used by the factory
    Probable _task;
    public static ProbableBuilder<T> Create() => new ProbableBuilder<T>();
    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine => _task = new Probable(stateMachine);
    public IProbable<T> Task => _task;

    // used per instance:
    private Probator _probator = null;
    public void SetStateMachine(IAsyncStateMachine stateMachine) => throw new InvalidOperationException("ProbableBuilder<T>.SetStateMachine");
    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine => throw new InvalidOperationException("ProbableBuilder<T>.AwaitOnCompleted");
    [SecurityCritical] public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine => throw new InvalidOperationException("ProbableBuilder<T>.AwaitUnsafeOnCompleted)");
    public void SetException(Exception exception) => _probator._currentException = ExceptionDispatchInfo.Capture(exception);
    public void SetResult(T result) => _probator._currentResult = result;

    class Probable : IProbable<T>
    {
        private Func<Probator, IAsyncStateMachine> clone;
        public IEnumerator<T> GetEnumerator() => new Probator { clone = clone };
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public Probable(IAsyncStateMachine sm)
        {
            clone = (probator) =>
            {
                var memberwiseCloneMethod = sm.GetType().GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
                var builderField = sm.GetType().GetField("<>t__builder", BindingFlags.Instance | BindingFlags.Public);
                var clone = (IAsyncStateMachine)memberwiseCloneMethod.Invoke(sm, null);
                builderField.SetValue(clone, new ProbableBuilder<T> { _probator = probator });
                return clone;
            };
        }
    }

    class Probator : IEnumerator<T>
    {
        public Func<Probator, IAsyncStateMachine> clone;
        public T _currentResult;
        public ExceptionDispatchInfo _currentException;

        public T Current
        {
            get
            {
                if (_currentException != null) _currentException.Throw();
                return _currentResult;
            }
        }

        public bool MoveNext()
        {
            _currentException = null;
            clone(this).MoveNext();
            return true;
        }

        object IEnumerator.Current => Current;
        public void Dispose() { }
        public void Reset() { }
    }
}
