using System.Diagnostics;
using System.Windows;
using MacropadConfigurator.Models;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using NLog;

namespace MacropadConfigurator.Services;

public class CompilerService
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private readonly ScriptHost host = new(App.Current);

    public async void ExecuteScript(Script<object>? script)
    {
        // Ensure we have a compiled script to run
        if (script == null)
        {
            logger.Warn("Couldn't execute null script");
            return; 
        }

        try
        {
            await script.RunAsync(globals: host);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred while running the script [{ex.Message}]",
                            "Script Runtime Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
        }

    }

    public (bool, string, Script<object>?) CompileScript(string script)
    {
        var options = ScriptOptions.Default
                .AddReferences(
                    typeof(Process).Assembly,
                    typeof(MessageBox).Assembly,
                    typeof(ScriptHost).Assembly
                )
                .AddImports(
                    "System",
                    "System.Diagnostics",
                    "System.Windows"
                 );
        try
        {
            // The globalsType parameter is crucial. It tells the compiler what to expect when the script is eventually run.
            var compiledScript = CSharpScript.Create(script, options, globalsType: typeof(ScriptHost));

            // Check for compilation errors before proceeding
            var diagnostics = compiledScript.GetCompilation().GetDiagnostics();
            if (diagnostics.Any(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error))
            {
                var errors = string.Join("\n", diagnostics.Select(d => d.GetMessage()));
                return (false, errors, null);
            }

            // If compilation is successful, store the compiled script
            return (true, string.Empty, compiledScript);
        }
        catch (Exception ex)
        {
            // This will catch syntax errors in the script.
            return (false, ex.Message, null);
        }
    }
}
