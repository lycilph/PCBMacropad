namespace ScriptingTest;

public class ScriptHost
{
    public void ShowMessage(string title, string message)
    {
        Console.WriteLine($"[{title}] {message}");
    }

    public void ShowMessage(string message)
    {
        Console.WriteLine(message);
    }
}
