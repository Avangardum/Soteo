namespace Soteo.Util;

/// <summary>
/// A type that never occurs.
/// Examples:<br/>
/// Task&lt;Never&gt; - never completes successfully
/// Task&lt;Never?&gt; - only completes successfully with null result
/// </summary>
public sealed class Never
{
    private Never() { }
}
