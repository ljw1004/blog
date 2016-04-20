using System;
using System.IO;
using System.Threading.Tasks;


class Program
{
    static Random RND = new Random();

    static void Main()
    {
        if (File.Exists("a.json")) WorkerAsync().GetAwaiter().GetResult();
        else MainAsync().GetAwaiter().GetResult();
    }


    static async Task MainAsync()
    {
        Console.WriteLine("INITIATING...");
        var disposition = await AlphaAsync().RunWithCheckpointing("a.json");
        // This runs the async method, and instucts where any checkpoints along the way should be saved to.
        // (I'm using a filename here, but I think it should support Azure blobs or arbitrary things.

        if (disposition == Checkpoint.Disposition.Completed) Console.WriteLine("COMPLETED.");
        else if (disposition == Checkpoint.Disposition.Deferred) Console.WriteLine("DEFERRED for a worker to finish");
    }


    static async Task AlphaAsync()
    {
        await Task.Delay(100);
        Console.WriteLine("PHASE 1");
        await TestAsync("phase1", 1, 5);
        Console.WriteLine("PHASE 2");
        await TestAsync("phase2", 1, 10);
    }


    static async Task<string> TestAsync(string desc, int min, int max)
    {
        for (int i = min; i < max; i++)
        {
            Console.WriteLine($"{desc} - {i}");
            await Task.Delay(100);

            int resumeCount = await Checkpoint.Save();
            // This saves the method's state, and also the state all the way up the async callstack

            if (resumeCount > 0) Console.WriteLine("   ... resuming from a saved checkpoint");
            // The "resumeCount" says how many times we've been asked to resume from that particular
            // checkpoint. If the computer crashed abruptly now, we'd trust a worker to eventually
            // resume from the checkpoint, in which case resumeCount would be >0.

            Console.WriteLine(i + "+");
            if (resumeCount == 0 && RND.NextDouble() > 0.2) throw new Checkpoint.DeferRemainderException(TimeSpan.FromHours(1));
            // If I want to deliberately halt my current work, and have the checkpoint resumed by the
            // worker either at some specified time in the future or in response to some specified
            // message or HTTP request in the future, I can do so by throwing this exception.

            Console.WriteLine(i + "++");
        }
        return "";
    }


    static async Task WorkerAsync()
    {
        Console.WriteLine("WORKER");
        // There obviously has to be some long-running service which is capable of picking up
        // checkpoints that need to be resumed. Each checkpoint will be accompanied by a condition
        // which says "please resume me after such and such an event" (e.g. a particular HTTP request),
        // or "please resume me after such and such a time" (e.g. because the async method wants to
        // be resumed in an hour, or because it crashed and hasn't touched its checkpoint for over
        // ten minutes). I've just written a simplistic service to demonstrate how to resume.

        Task t = Checkpoint.ResumeFrom("a.json");
        // Here the task "t" resumes the work that the original "RunWithCheckpointing()" had started,
        // right in mid-flight from the most recent Checkpoint.Save().

        var disposition = await t.RunWithCheckpointing("a.json");
        if (disposition == Checkpoint.Disposition.Completed) Console.WriteLine("WORKER COMPLETED.");
        else if (disposition == Checkpoint.Disposition.Deferred) Console.WriteLine("WORKER DEFERRED for a further worker to finish");
    }


}

