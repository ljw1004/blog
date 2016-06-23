Missing Await Analyzer
========================

> * Download NuGet package [MissingAwaitAnalyzer](https://www.nuget.org/packages/MissingAwaitAnalyzer)

```cs
await Task.Factory.StartNew(async () =>
{
    await Task.Delay(10);
    Console.WriteLine("A");
});
Console.WriteLine("B");
```

We awaited the task, with the intent that this code print `A` first and then `B`. But it doesn't. It actually prints `B` first.

*Can you see why?*

If you can, congratulations, you're an async expert. If you can't then you should download this analyzer!


## What it does

This analyzer detects any statements of the form `await expr;` where the returned
value is itself a `Task`. In those cases it suggests that maybe you want to do `await await expr`.


Note: in this particular case, a better suggestion would have been to use `Task.Run` instead of `Task.New`.
