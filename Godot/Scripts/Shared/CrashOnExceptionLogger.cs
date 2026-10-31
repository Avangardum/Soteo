using Godot.Collections;

namespace Soteo.Main.Shared;

public sealed partial class CrashOnExceptionLogger(SceneTree sceneTree) : Logger
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
        if (function == "void Godot.NativeInterop.ExceptionUtils.LogException(System.Exception)")
            sceneTree.Quit(1);
    }
}
