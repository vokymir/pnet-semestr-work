using System.CommandLine;

namespace BirdWatching.ConsoleClient;

public class BWCommand : Command
{
    protected ImportantStuff _impStuff;

    public BWCommand(string name, string description, ImportantStuff impStuff) : base(name, description)
    {
        _impStuff = impStuff;
    }

    /// <summary>
    /// Call your HandleCommand in this wrapper to get nice Console output.
    /// </summary>
    /// <param name="func">The function you actually want to run.</param>
    /// <param name="args">Anything you function needs as parameters, passed in the right order.</param>
    protected async Task HandleCommandWrapper(Delegate func, params object?[]? args)
    {
        Console.ForegroundColor = _impStuff.Working.Foreground ?? Console.ForegroundColor;
        Console.BackgroundColor = _impStuff.Working.Background ?? Console.BackgroundColor;
        Console.WriteLine("Working on it...");
        Console.ResetColor();

        try
        {
            // ##### START: Call the function, if is async, await it.
            var result = func.DynamicInvoke(args);

            if (result is Task task)
            {
                await task.ConfigureAwait(false);
            }
            // ##### END

            // ##### START: If the function 'func' returned some other function, save it.
            object? returned = null;

            if (result is Task tWithResult)
            {
                var resultProp = tWithResult.GetType().GetProperty("Result");
                returned = resultProp?.GetValue(tWithResult);
            }
            else
            {
                returned = result;
            }
            // ##### END

            Console.ForegroundColor = _impStuff.Success.Foreground ?? Console.ForegroundColor;
            Console.BackgroundColor = _impStuff.Success.Background ?? Console.BackgroundColor;
            Console.WriteLine("Action successful!");
            Console.ResetColor();

            // ##### START: If we have a function to run, run it (async-friendly).
            if (returned is Delegate returnedFunc)
            {
                var invokeResult = returnedFunc.DynamicInvoke();

                if (invokeResult is Task returnedTask)
                {
                    await returnedTask;
                }
            }
            // ##### END
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = _impStuff.Failure.Foreground ?? Console.ForegroundColor;
            Console.BackgroundColor = _impStuff.Failure.Background ?? Console.BackgroundColor;
            Console.Error.WriteLine($"Action failed: {ex.Message}");
            Console.ResetColor();
            Environment.Exit(1);
        }
    }
}
