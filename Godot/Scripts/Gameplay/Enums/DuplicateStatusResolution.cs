namespace Soteo.Gameplay.Enums;

/// <summary>
/// What happens when a status is added to a unit that already has a status of this type
/// </summary>
public enum DuplicateStatusResolution
{
    /// <summary>
    /// New duplicate is added independently of existing duplicates
    /// </summary>
    Stack,
    /// <summary>
    /// New duplicate is discarded, existing duplicates' remaining time is floored to the new duplicate's remaining
    /// time, existing duplicates' source and ability context are set to the new duplicate's values
    /// </summary>
    Refresh,
    /// <summary>
    /// New duplicate is added, existing duplicates' time is floored to the new duplicate's remaining time,
    /// existing duplicates' source and ability context are set to the new duplicate's values
    /// </summary>
    StackAndRefresh,
    /// <summary>
    /// Existing duplicate is removed, new duplicate is added
    /// </summary>
    Replace,
    /// <summary>
    /// New duplicate is discarded
    /// </summary>
    Discard,
    /// <summary>
    /// An exception is thrown, used when uniqueness is enforced manually
    /// </summary>
    Throw
}