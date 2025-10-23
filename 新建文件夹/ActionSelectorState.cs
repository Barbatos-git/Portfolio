public enum SelectorMode
{
    None,
    Move,
    Attack
}

public static class ActionSelectorState
{
    public static SelectorMode CurrentMode = SelectorMode.None;

    public static bool IsMoveMode => CurrentMode == SelectorMode.Move;
    public static bool IsAttackMode => CurrentMode == SelectorMode.Attack;
    public static void SetNone() => CurrentMode = SelectorMode.None;
    public static void SetMove() => CurrentMode = SelectorMode.Move;
    public static void SetAttack() => CurrentMode = SelectorMode.Attack;
}
