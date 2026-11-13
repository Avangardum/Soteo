namespace Soteo.Core.Abilities;

public enum AbilityValidationMode
{
    /// <summary>
    /// Check whether an ability usage can be initiated
    /// </summary>
    Initial,

    /// <summary>
    /// Check whether an ongoing ability usage can be continued
    /// </summary>
    Continuous
}
