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
        Task<Task> tt = Task.Factory.StartNew(async () =>
        {
            await Task.Delay(10);
            Console.WriteLine("A");
        });

        await tt; // we want a warning here
        // When we encounter an ExpressionStatement
        // where the expression is an AwaitExpression
        // and where the expression's type is Task or Task<T> (or any awaitable)
        // then give a warning

        Console.WriteLine("B");
    }
}