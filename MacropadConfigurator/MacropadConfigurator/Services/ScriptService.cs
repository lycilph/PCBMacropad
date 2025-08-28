using System.Diagnostics;
using System.Windows;
using MacropadConfigurator.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using NLog;

namespace MacropadConfigurator.Services;

public class ScriptService
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private readonly ScriptHost host = new(App.Current);
    private readonly ScriptOptions options;
    private ScriptState<object> state = null!;

    public ScriptService()
    {
        options = ScriptOptions.Default
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
    }
    
    public async Task RunAsync(string script)
    {
        try
        {
            state = await state.ContinueWithAsync(script);
        }
        catch (Exception e)
        {
            MessageBox.Show($"Error while executing script {script}: {e.Message}");
            throw;
        }
    }

    public Task RunOnUIThreadAsync(string script)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher == null || dispatcher.CheckAccess())
        {
            // Already on UI thread
            return RunAsync(script);
        }
        else
        {
            var tcs = new TaskCompletionSource<object?>();
            dispatcher.BeginInvoke(new Action(async () =>
            {
                try
                {
                    await RunAsync(script);
                    tcs.SetResult(null);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }));
            return tcs.Task;
        }
    }

    public async Task UpdateStateAsync(string script)
    {
        state = await CSharpScript.RunAsync(script, options, host, globalsType: typeof(ScriptHost));
    }

    public Task<CompilationResult> CompileAsync(string script)
    {
        return Task.Run(() =>
        {
            CompilationResult result = new();

            if (string.IsNullOrWhiteSpace(script))
            {
                result.Error("Empty script");
                return result;
            }

            try
            {
                // The globalsType parameter is crucial. It tells the compiler what to expect when the script is eventually run.
                result.Script = CSharpScript.Create(script, options, globalsType: typeof(ScriptHost));

                // Check for compilation errors before proceeding
                var diagnostics = result.Script.GetCompilation().GetDiagnostics();
                if (diagnostics.Any(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error))
                {
                    var errors = string.Join("\n", diagnostics.Select(d => d.GetMessage()));
                    result.Error(errors);
                }
            }
            catch (Exception ex)
            {
                // This will catch syntax errors in the script.
                result.Error(ex.Message);
            }

            return result;
        });
    }

    // This compiles a script within the context of a master script
    public async Task<CompilationResult> CompileAsync(string master, string script)
    {
        CompilationResult result = new();

        if (string.IsNullOrWhiteSpace(master) || string.IsNullOrWhiteSpace(script))
        {
            result.Error("Empty script(s)");
            return result;
        }

        // First compile master script
        var master_result = await CompileAsync(master);
        if (!master_result.Success || master_result.Script == null)
        {
            return master_result;
        }

        // Then compile actual script
        try
        {
            var compiled_script = master_result.Script.ContinueWith(script);

            // Check for compilation errors before proceeding
            var diagnostics = compiled_script.GetCompilation().GetDiagnostics();
            if (diagnostics.Any(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error))
            {
                var errors = string.Join("\n", diagnostics.Select(d => d.GetMessage()));
                result.Error(errors);
            }
        }
        catch (Exception ex)
        {
            // This will catch syntax errors in the script.
            result.Error(ex.Message);
        }

        return result;
    }
}
