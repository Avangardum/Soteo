namespace Soteo.Core.Statuses;

public abstract class AbilityPassiveStatus : Status
{
    public sealed override DuplicateStatusResolution DuplicateResolution => DuplicateStatusResolution.Throw;
    public sealed override bool HudVisible => false;
}
