using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace ScriptingTest;

internal class Program
{
    static void Main(string[] args)
    {
        var master_script = 
            @"int script_version = 1;
              public bool IsWorkLocation() => true;";
        var shortcut_script1 =
            @"if (IsWorkLocation()) 
                ShowMessage(""Info"", ""At work"");
              else
                ShowMessage(""Info"", ""At home"");
              script_version++;";
        var shortcut_script2 =
            @"ShowMessage($""Version {script_version}"")";

        var options = ScriptOptions.Default
                .AddReferences(
                    typeof(Process).Assembly,
                    typeof(ScriptHost).Assembly
                )
                .AddImports(
                    "System",
                    "System.Diagnostics",
                    "System.Windows"
                 );

        var host = new ScriptHost();
        var master_script_compiled = CSharpScript.Create(master_script, options, globalsType: typeof(ScriptHost));

        // Check for compilation errors before proceeding
        var diagnostics = master_script_compiled.GetCompilation().GetDiagnostics();
        if (diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
        {
            var errors = string.Join("\n", diagnostics.Select(d => d.GetMessage()));
            Console.WriteLine(errors);
        }

        var script_state = master_script_compiled.RunAsync(host).Result;

        if (CanCompile(master_script_compiled, shortcut_script1) && CanCompile(master_script_compiled, shortcut_script2))
        {
            script_state = script_state.ContinueWithAsync(shortcut_script2).Result;
            script_state = script_state.ContinueWithAsync(shortcut_script1).Result;
            script_state = script_state.ContinueWithAsync(shortcut_script2).Result;
        }

        Console.Write("Press any key to continue...");
        Console.ReadKey();
    }

    private static bool CanCompile(Script<object> master, string code)
    {
        var compiled = master.ContinueWith(code);
        var diagnostics = compiled.GetCompilation().GetDiagnostics();
        if (diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
        {
            var errors = string.Join("\n", diagnostics.Select(d => d.ToString()));
            Console.WriteLine(errors);
        }

        return !diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
    }
}
