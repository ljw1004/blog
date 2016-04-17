using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

public static class Checkpoint
{
    public enum Disposition
    {
        Completed,
        Deferred
    }

    public static RunWithCheckpointingAwaitable RunWithCheckpointing(this Task task, string fn)
    {
        return new RunWithCheckpointingAwaitable { task = task, fn = fn };
        // TODO: make sure that we're already awaiting the RunWithCheckpointingAwaiter before
        // Checkpoint.Save() does its walk up the async callstack
    }

    public static Task ResumeFrom(string fn)
    {
        var sm = ReadStateMachine(JObject.Parse(File.ReadAllText(fn)));

        // TODO: something's going wrong with the restoration.
        // In this case it restores the leaf statemachine with TaskId=1 and its called with TaskId=2,
        // but when I try to walk the AsyncMethods, I'm getting TaskId=1 / TaskId=3.
        // So for some reason (presumably boxing of builder structs) the tasks I'm wiring up
        // when I restore the async callstack are not the right ones.

        var am = new AsyncMethod(sm.LeafStateMachine, sm.LeafBuilder);
        var am2 = am.GetAsyncMethodThatsAwaitingThisOne();

        sm.LeafActionToStartWork.Invoke();
        return sm.Task;
    }

    struct ReadStateMachineResult
    {
        public IAsyncStateMachine StateMachine;
        public Task Task;
        public object AwaiterForAwaitingThisStateMachine;
        //
        public Action LeafActionToStartWork;
        public IAsyncStateMachine LeafStateMachine;
        public object LeafBuilder;
    }

    private static ReadStateMachineResult ReadStateMachine(JObject json, string indent = "")
    {
        var smAssembly = json["asyncMethodAssembly"].Value<string>();
        var smType = json["asyncMethodStateMachineType"].Value<string>();
        var sm = Activator.CreateInstance(smAssembly, smType).Unwrap() as IAsyncStateMachine;

        var state = json["state"];
        var stateField = state["fieldName"].Value<string>();
        var stateValue = state["value"].Value<int>();
        sm.GetType().GetField(stateField).SetValue(sm, stateValue);

        var locals = json["locals"] as JArray;
        foreach (JObject local in locals)
        {
            var localField = local["fieldName"].Value<string>();
            var localAssembly = local["assembly"].Value<string>();
            var localType = local["type"].Value<string>();
            if (localAssembly == null) continue;
            var t = Assembly.Load(localAssembly).GetType(localType);
            var localValue = local["value"].ToObject(t);
            sm.GetType().GetField(localField, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(sm, localValue);
        }

        var awaiter = json["awaiter"];
        var awaiterField = awaiter["fieldName"].Value<string>();
        var awaitedValue = awaiter["value"] as JObject;
        ReadStateMachineResult awaited;
        if (awaitedValue == null)
        {
            awaited = new ReadStateMachineResult();
            awaited.AwaiterForAwaitingThisStateMachine = new CheckpointSaveAwaiter() { _result = 1 };
        }
        else
        {
            awaited = ReadStateMachine(awaitedValue, indent + "   ");
        }
        sm.GetType().GetField(awaiterField, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(sm, awaited.AwaiterForAwaitingThisStateMachine);

        var builderType = (sm.GetType().GetField("<>t__builder") ?? sm.GetType().GetField("$Builder")).FieldType;
        var builderCreate = builderType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
        var builderSetStateMachine = builderType.GetMethod("SetStateMachine", BindingFlags.Public | BindingFlags.Instance);
        var builderTask = builderType.GetProperty("Task", BindingFlags.Public | BindingFlags.Instance);
        var builderAwaitOnCompleted = builderType.GetMethod("AwaitOnCompleted", BindingFlags.Public | BindingFlags.Instance);
        var builder = builderCreate.Invoke(null, new object[] { });
        builderSetStateMachine.Invoke(builder, new object[] { sm });
        Action lambda = () => builderAwaitOnCompleted.MakeGenericMethod(awaited.AwaiterForAwaitingThisStateMachine.GetType(), sm.GetType()).Invoke(builder, new object[] { awaited.AwaiterForAwaitingThisStateMachine, sm });
        if (awaitedValue == null) awaited.LeafActionToStartWork = lambda;
        else lambda();


        var task = builderTask.GetValue(builder) as Task;
        var taskType = task.GetType();
        var taskGetAwaiter = taskType.GetMethod("GetAwaiter", BindingFlags.Public | BindingFlags.Instance);
        var taskAwaiter = taskGetAwaiter.Invoke(task, new object[] { });

        Console.WriteLine($"{indent}RESUMED {json["asyncMethodName"]} with taskID = {(task as Task).Id}");


        return new ReadStateMachineResult
        {
            StateMachine = sm,
            Task = task,
            AwaiterForAwaitingThisStateMachine = taskAwaiter,
            LeafActionToStartWork = awaited.LeafActionToStartWork,
            LeafStateMachine = awaited.LeafStateMachine ?? sm,
            LeafBuilder = awaited.LeafBuilder ?? builder
        };

    }



    public class DeferRemainderException : System.Exception
    {
        public DeferRemainderException(TimeSpan t) { }
        public DeferRemainderException() { }
    }

    public static CheckpointSaveAwaitable Save() => new CheckpointSaveAwaitable();

    public class RunWithCheckpointingAwaitable
    {
        public Task task;
        public string fn;
        public RunWithCheckpointingAwaiter GetAwaiter() => new RunWithCheckpointingAwaiter { awaiter = task.GetAwaiter(), fn = fn };
    }

    public class RunWithCheckpointingAwaiter : INotifyCompletion
    {
        public TaskAwaiter awaiter;
        public string fn;
        public Action continuation;
        public bool IsCompleted => awaiter.IsCompleted;
        public Disposition GetResult()
        {
            try
            {
                awaiter.GetResult();
                return Disposition.Completed;
            }
            catch (DeferRemainderException)
            {
                return Disposition.Deferred;
            }
        }
        public void OnCompleted(Action continuation)
        {
            this.continuation = continuation;
            awaiter.OnCompleted(OnCompletedRunner);
        }
        public void OnCompletedRunner()
        {
            var deleteFile = true;
            try
            {
                awaiter.GetResult();
            }
            catch (DeferRemainderException)
            {
                deleteFile = false;
            }
            catch (Exception)
            {
            }
            if (deleteFile && File.Exists(fn)) File.Delete(fn);
            continuation();
        }
    }

    public class CheckpointSaveAwaitable
    {
        public CheckpointSaveAwaiter GetAwaiter() => new CheckpointSaveAwaiter() { _result = 0 };
    }

    public class CheckpointSaveAwaiter : INotifyCompletion
    {
        public int _result;
        public bool IsCompleted => false;
        public int GetResult() => _result;
        public void OnCompleted(Action continuation)
        {
            if (_result > 0)
            {
                continuation();
                return;
            }

            var asyncMethod = new AsyncMethod(continuation);
            JObject json = null;
            while (true)
            {
                var state = asyncMethod.GetCurrentState();
                var locals = asyncMethod.GetLocalsAndParameters();
                var awaiter = asyncMethod.GetCurrentAwaiter();

                var jsonState = new JObject();
                jsonState.Add("fieldName", state.Item1.Name);
                jsonState.Add("value", JToken.FromObject(state.Item2));

                var jsonAwaiter = new JObject();
                jsonAwaiter.Add("fieldName", awaiter.Item1.Name);
                jsonAwaiter.Add("value", json);

                var jsonLocals = new JArray();
                foreach (var local in locals)
                {
                    var jsonLocal = new JObject();
                    jsonLocal.Add("fieldName", local.Item1.Name);
                    if (local.Item2 == null) { jsonLocals.Add(jsonLocal); continue; }
                    jsonLocal.Add("assembly", local.Item2.GetType().Assembly.FullName);
                    jsonLocal.Add("type", local.Item2.GetType().FullName);
                    jsonLocal.Add("value", JToken.FromObject(local.Item2));
                    jsonLocals.Add(jsonLocal);
                }

                var jsonMethod = new JObject();
                jsonMethod.Add("asyncMethodName", asyncMethod.Name);
                jsonMethod.Add("asyncMethodAssembly", asyncMethod.StateMachineType.Assembly.FullName);
                jsonMethod.Add("asyncMethodStateMachineType", asyncMethod.StateMachineType.FullName);
                jsonMethod.Add("state", jsonState);
                jsonMethod.Add("awaiter", jsonAwaiter);
                jsonMethod.Add("locals", jsonLocals);
                json = jsonMethod;

                var parent = asyncMethod.GetAsyncMethodThatsAwaitingThisOne();
                if (parent == null) throw new NotSupportedException($"Can't figure out which async method is awaiting {asyncMethod.Name}");
                var parentAwaiter = parent.GetCurrentAwaiter();
                if (parentAwaiter == null) throw new NotSupportedException($"Async method {parent.Name} has awaiter types that we don't know how to checkpoint");
                var name = parentAwaiter.Item2.GetType().ToString();
                if (parentAwaiter.Item2 is RunWithCheckpointingAwaiter)
                {
                    var saver = parentAwaiter.Item2 as RunWithCheckpointingAwaiter;
                    File.WriteAllText(saver.fn, json.ToString());
                    break;
                }
                else if (parentAwaiter.Item2.GetType().ToString().StartsWith("System.Runtime.CompilerServices.TaskAwaiter"))
                {
                    asyncMethod = parent;
                }
                else
                {
                    throw new NotSupportedException($"Async method {parent.Name} is awaiting a {parentAwaiter.Item2.GetType().ToString()}, but we only know how to checkpoint awaits on normal Tasks");
                }
            }

            continuation();

        }

    }

    class AsyncMethod
    {
        private IAsyncStateMachine _stateMachine;
        private object _builder;

        public AsyncMethod(IAsyncStateMachine stateMachine, object builder)
        {
            _stateMachine = stateMachine;
            _builder = builder;
        }

        public AsyncMethod(Action awaiterOnCompletedContinuationAction)
        {
            _stateMachine = TryGetStateMachineForDebugger(awaiterOnCompletedContinuationAction).Target as IAsyncStateMachine;
            _builder = _stateMachine == null ? null : GetBuilder(_stateMachine);
            if (_builder == null) throw new ArgumentException("Not the continuation action of an awaiter's OnCompleted method", nameof(awaiterOnCompletedContinuationAction));
        }

        public AsyncMethod GetAsyncMethodThatsAwaitingThisOne()
        {
            var task = _builder.GetType().GetProperty("Task").GetValue(_builder) as Task;
            if (task == null) return null;

            var continuationDelegates = GetDelegatesFromContinuationObject(task);
            var continuationDelegate = (continuationDelegates?.Length == 1 ? continuationDelegates[0] as Action : null);
            if (continuationDelegate == null) return null;

            var stateMachine = TryGetStateMachineForDebugger(continuationDelegate).Target as IAsyncStateMachine;
            if (stateMachine is RunWithCheckpointingAwaiter)
            {
                continuationDelegate = (stateMachine as RunWithCheckpointingAwaiter).continuation;
                stateMachine = TryGetStateMachineForDebugger(continuationDelegate).Target as IAsyncStateMachine;
            }

            var builder = GetBuilder(stateMachine);
            if (builder == null) return null;
            return new AsyncMethod(stateMachine, builder);
        }

        public override string ToString() => Name;

        public Type StateMachineType => _stateMachine.GetType();

        public string Name
        {
            get
            {
                var task = _builder.GetType().GetProperty("Task").GetValue(_builder) as Task;

                var t = _stateMachine.GetType();
                var s = t.Name;
                var i = s.LastIndexOf(">");
                s = s.Substring(1, i - 1);
                while (true)
                {
                    t = t.DeclaringType;
                    if (t == null) return s + $" [task id {task?.Id}]";
                    s = t.Name + "." + s;
                }
            }
        }

        public IEnumerable<Tuple<FieldInfo, object>> GetLocalsAndParameters()
        {
            var fieldInfos = _stateMachine.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var fieldInfo in fieldInfos)
            {
                if (fieldInfo.Name == "<>t__builder") continue; // builder
                if (fieldInfo.Name.StartsWith("<>u__")) continue; // awaiters
                if (fieldInfo.Name == "<>1__state") continue; // state
                // all other fields are locals and parameters
                var value = fieldInfo.GetValue(_stateMachine);
                yield return Tuple.Create(fieldInfo, value);
            }
        }

        public Tuple<FieldInfo, object> GetCurrentState()
        {
            var fieldInfo = _stateMachine.GetType().GetField("<>1__state");
            var value = fieldInfo.GetValue(_stateMachine);
            return Tuple.Create(fieldInfo, value);
        }

        public Tuple<FieldInfo, object> GetCurrentAwaiter()
        {
            var candidateAwaiters = new List<Tuple<FieldInfo, object>>();

            var fieldInfos = _stateMachine.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var fieldInfo in fieldInfos)
            {
                if (!fieldInfo.Name.StartsWith("<>u__")) continue;
                var awaiter = fieldInfo.GetValue(_stateMachine); if (awaiter == null) continue;

                // shortcut to avoid throwing an exception in the common case: typical awaiters have a field named "m_task" of type Task
                var taskFieldInfo = awaiter.GetType().GetField("m_task", BindingFlags.NonPublic | BindingFlags.Instance);
                if (taskFieldInfo != null && typeof(Task).IsAssignableFrom(taskFieldInfo.FieldType))
                {
                    var m_task = taskFieldInfo.GetValue(awaiter);
                    if (m_task == null) continue;
                }

                var propInfo = awaiter.GetType().GetProperty("IsCompleted");
                try
                {
                    bool isCompleted = Convert.ToBoolean(propInfo.GetValue(awaiter));
                    if (isCompleted) continue;
                    candidateAwaiters.Add(Tuple.Create(fieldInfo, awaiter));
                }
                catch (Exception)
                {
                    continue;
                }
            }

            if (candidateAwaiters.Count != 1)
            {
                throw new NotSupportedException($"Async method {Name} has awaiters that are too complicated to process");
            }
            return candidateAwaiters[0];
        }

        // Here I'm exposing some internal methods from mscorlib that I need to walk the async callstack
        private static Type _type_AsyncMethodBuilderCore = typeof(AsyncTaskMethodBuilder).Assembly.GetType("System.Runtime.CompilerServices.AsyncMethodBuilderCore");
        private static Type _type_Task = typeof(Task);
        private static MethodInfo _mi_TryGetStateMachineForDebugger = _type_AsyncMethodBuilderCore.GetMethod("TryGetStateMachineForDebugger", BindingFlags.NonPublic | BindingFlags.Static);
        private static MethodInfo _mi_TryGetContinuationAction = _type_AsyncMethodBuilderCore.GetMethod("TryGetContinuationTask", BindingFlags.NonPublic | BindingFlags.Static);
        private static MethodInfo _mi_GetDelegatesFromContinuationObject = _type_Task.GetMethod("GetDelegatesFromContinuationObject", BindingFlags.NonPublic | BindingFlags.Static);

        /// <summary>
        /// Retrieves the async state machine's MoveNext method
        /// </summary>
        private static Action TryGetStateMachineForDebugger(Action action) => _mi_TryGetStateMachineForDebugger.Invoke(null, new object[] { action }) as Action;

        /// <summary>
        /// Given an action (e.g. one of the delegates that will be executed upon task completion),
        /// see if it is a contiunation wrapper and has a Task associated with it.  If so return it; null otherwise.
        /// </summary>
        private static Task TryGetContinuationTask(Action action) => _mi_TryGetContinuationAction.Invoke(null, new object[] { action }) as Task;

        /// <summary>
        /// Given a task, finds all the delegates that will be executed when the task is complete
        /// </summary>
        private static Delegate[] GetDelegatesFromContinuationObject(object continuationObject) => _mi_GetDelegatesFromContinuationObject.Invoke(null, new object[] { continuationObject }) as Delegate[];


        /// <summary>
        /// Given a state machine object, returns its builder
        /// </summary>
        private static object GetBuilder(object stateMachine)
        {
            var fieldInfo = stateMachine.GetType().GetField("<>t__builder") ?? stateMachine.GetType().GetField("$Builder");
            return fieldInfo?.GetValue(stateMachine);
        }

    }


}
