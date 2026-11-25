using Godot.Collections;

namespace Soteo.Main;

public sealed partial class CrashOnErrorLogger(SceneTree sceneTree) : Logger
{
    public override void _LogError
    (
        string function,
        string file,
        int line,
        string code,
        string rationale,
        bool editorNotify,
        int errorType,
        Array<ScriptBacktrace> scriptBacktraces
    )
    {
        sceneTree.Quit(1);
    }
}
