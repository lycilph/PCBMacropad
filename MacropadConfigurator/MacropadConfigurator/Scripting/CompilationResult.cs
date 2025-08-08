using Microsoft.CodeAnalysis.Scripting;

namespace MacropadConfigurator.Scripting;

public class CompilationResult
{
    public bool Success { get; set; } = true;
    public string Message { get; set; } = string.Empty;
    public Script<object>? Script { get; set; } = null;

    public void Error(string message)
    {
        Success = false;
        Message = message;
    }
}
