namespace Deckbuilder.Debugging
{
    public enum DebugClickMode
    {
        Move,
        PlayCard
    }

    public static class DebugClickRouter
    {
        public static DebugClickMode Mode { get; set; } = DebugClickMode.Move;
    }
}
