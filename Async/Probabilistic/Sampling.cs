using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously


class Sampling
{
    static Random RND = new Random();

    public static async Task MainAsync()
    {
        // A probabilistic function returns a pull-based stream of values, like IEnumerable:
        ISampleable<int> p = Foo();
        foreach (var sample in p.Take(3)) Console.WriteLine($"sample: {sample}");

        // If the probabilistic function awaits, then it returns a pull-based asynchronous stream of values, like IAsyncEnumerable.
        IAsyncSampleable<int> q = Bar();
        await q.Take(3).Do(sample => Console.WriteLine(sample)).ToArray();
    }

    static async ISampleable<int> Foo()
    {
        Console.WriteLine("Foo");
        return RND.Next();
    }

    static async IAsyncSampleable<int> Bar()
    {
        Console.WriteLine("Bar");
        await Task.Delay(500);
        return RND.Next();
    }

}

[AsyncMethodBuilder(typeof(SampleableBuilder<>))]
public interface ISampleable<T> : IEnumerable<T> { }

[AsyncMethodBuilder(typeof(AsyncSampleableBuilder<>))]
public interface IAsyncSampleable<T> : IAsyncEnumerable<T> { }


public class SampleableBuilder<T>
{
    // used by the factory
    Sampleable _task;
    public static SampleableBuilder<T> Create() => new SampleableBuilder<T>();
    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine => _task = new Sampleable(stateMachine);
    public ISampleable<T> Task => _task;

    // used per instance:
    private Sampler _Sampler = null;
    public void SetStateMachine(IAsyncStateMachine stateMachine) => throw new InvalidOperationException("SampleableBuilder<T>.SetStateMachine");
    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine => throw new InvalidOperationException("SampleableBuilder<T>.AwaitOnCompleted");
    [SecurityCritical] public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine => throw new InvalidOperationException("SampleableBuilder<T>.AwaitUnsafeOnCompleted)");
    public void SetException(Exception exception) => _Sampler._currentException = ExceptionDispatchInfo.Capture(exception);
    public void SetResult(T result) => _Sampler._currentResult = result;

    class Sampleable : ISampleable<T>
    {
        private Func<Sampler, IAsyncStateMachine> clone;
        public IEnumerator<T> GetEnumerator() => new Sampler { clone = clone };
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public Sampleable(IAsyncStateMachine sm)
        {
            clone = (Sampler) =>
            {
                var memberwiseCloneMethod = sm.GetType().GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
                var builderField = sm.GetType().GetField("<>t__builder", BindingFlags.Instance | BindingFlags.Public);
                var clone = (IAsyncStateMachine)memberwiseCloneMethod.Invoke(sm, null);
                builderField.SetValue(clone, new SampleableBuilder<T> { _Sampler = Sampler });
                return clone;
            };
        }
    }

    class Sampler : IEnumerator<T>
    {
        public Func<Sampler, IAsyncStateMachine> clone;
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

public class AsyncSampleableBuilder<T>
{
    // used by the factory
    Sampleable _task;
    public static AsyncSampleableBuilder<T> Create() => new AsyncSampleableBuilder<T>();
    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine => _task = new Sampleable(stateMachine);
    public IAsyncSampleable<T> Task => _task;

    // used per instance:
    private Sampler _Sampler = null;
    public void SetStateMachine(IAsyncStateMachine stateMachine) => throw new InvalidOperationException("SampleableBuilder<T>.SetStateMachine");
    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine => awaiter.OnCompleted(_Sampler._currentSm.MoveNext);
    [SecurityCritical] public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine => awaiter.UnsafeOnCompleted(_Sampler._currentSm.MoveNext);
    public void SetException(Exception exception) => _Sampler._current.SetException(exception);
    public void SetResult(T result) => _Sampler._current.SetResult(result);

    class Sampleable : IAsyncSampleable<T>
    {
        private Func<Sampler, IAsyncStateMachine> clone;
        public IAsyncEnumerator<T> GetEnumerator() => new Sampler { clone = clone };

        public Sampleable(IAsyncStateMachine sm)
        {
            clone = (Sampler) =>
            {
                var memberwiseCloneMethod = sm.GetType().GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
                var builderField = sm.GetType().GetField("<>t__builder", BindingFlags.Instance | BindingFlags.Public);
                var clone = (IAsyncStateMachine)memberwiseCloneMethod.Invoke(sm, null);
                builderField.SetValue(clone, new AsyncSampleableBuilder<T> { _Sampler = Sampler });
                return clone;
            };
        }

    }

    class Sampler : IAsyncEnumerator<T>
    {
        public Func<Sampler, IAsyncStateMachine> clone;
        public IAsyncStateMachine _currentSm;
        public TaskCompletionSource<T> _current;

        public T Current
        {
            get
            {
                var awaiter = _current.Task.GetAwaiter();
                if (awaiter.IsCompleted) return awaiter.GetResult();
                throw new InvalidOperationException("IAsyncSampleable<T> - must await MoveNext() before obtaining Current");
            }
        }

        public Task<bool> MoveNext(CancellationToken cancel)
        {
            _current = new TaskCompletionSource<T>();
            _currentSm = clone(this);
            _currentSm.MoveNext();
            var tcs = new TaskCompletionSource<bool>();
            _current.Task.ContinueWith(_ =>
            {
                tcs.SetResult(true);
            });
            return tcs.Task;
        }

        public void Dispose() { }
    }
}

