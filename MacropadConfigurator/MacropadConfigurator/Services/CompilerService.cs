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

    public Task<(bool, string, Script<object>?)> CompileScriptAsync(string script)
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

        return Task.Run(() =>
        {
            // Return type is declared explicitly to avoid confusing the compiler about the return type here
            (bool success, string message, Script<object>? script) result;

            try
            {
                // The globalsType parameter is crucial. It tells the compiler what to expect when the script is eventually run.
                var compiledScript = CSharpScript.Create(script, options, globalsType: typeof(ScriptHost));

                // Check for compilation errors before proceeding
                var diagnostics = compiledScript.GetCompilation().GetDiagnostics();
                if (diagnostics.Any(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error))
                {
                    var errors = string.Join("\n", diagnostics.Select(d => d.GetMessage()));
                    result = (false, errors, null);
                }
                else
                {
                    // If compilation is successful, store the compiled script
                    result = (true, string.Empty, compiledScript);
                }
            }
            catch (Exception ex)
            {
                // This will catch syntax errors in the script.
                result = (false, ex.Message, null);
            }

            return result;
        });
    }
}
