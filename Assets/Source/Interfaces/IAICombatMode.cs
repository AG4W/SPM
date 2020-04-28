public interface IAICombatMode
{
    AICombatMode Mode { get; }
}
public enum AICombatMode
{
    Aggressive = 1,
    Cautious,
    Defensive
}
