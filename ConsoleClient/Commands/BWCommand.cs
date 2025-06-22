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
    protected async Task HandleCommandWrapper(Delegate func, params object?[]? args)
    {

        Console.ForegroundColor = _impStuff.Working.Foreground ?? Console.ForegroundColor;
        Console.BackgroundColor = _impStuff.Working.Background ?? Console.BackgroundColor;
        Console.WriteLine("Working on it...");
        Console.ResetColor();

        try
        {
            var result = func.DynamicInvoke(args);

            if (result is Task task)
            {
                await task.ConfigureAwait(false);
            }

            // Handle if the delegate returns another function
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

            Console.ForegroundColor = _impStuff.Success.Foreground ?? Console.ForegroundColor;
            Console.BackgroundColor = _impStuff.Success.Background ?? Console.BackgroundColor;
            Console.WriteLine("Action successful!");
            Console.ResetColor();

            // Optionally run the returned function
            if (returned is Delegate returnedFunc)
            {
                var invokeResult = returnedFunc.DynamicInvoke();

                if (invokeResult is Task returnedTask)
                {
                    await returnedTask;
                }
            }
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
