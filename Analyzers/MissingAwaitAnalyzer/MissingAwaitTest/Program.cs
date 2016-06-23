using System;
using System.Threading.Tasks;

class Program
{
    static void Main()
    {
        MainAsync().GetAwaiter().GetResult();
    }

    static async Task MainAsync()
    {
        Task t = await Task.Run(async () => { Console.WriteLine("A"); return Task.Delay(1000); });
        await t;
        Console.WriteLine("B");
    }
}