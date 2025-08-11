using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis.Scripting;

namespace MacropadConfigurator.Models;

public class MasterScript
{
    public string Script { get; set; } = string.Empty;

    [JsonIgnore]
    public Script<object>? CompiledScript { get; set; } = null;

    [JsonIgnore]
    public ScriptState<object>? MasterScriptState { get; set; } = null;
}
