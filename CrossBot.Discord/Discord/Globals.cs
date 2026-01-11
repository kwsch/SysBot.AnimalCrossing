using CrossBot.SysBot;

namespace CrossBot.Discord
{
    /// <summary>
    /// Silly global variables so that the command handlers can fetch the <see cref="Bot"/> and Discord <see cref="Self"/> instance object.
    /// </summary>
    public static class Globals
    {
        /// <summary> Discord Bot </summary>
        public static SysCord Self { get; set; } = null!;

        /// <summary> Hardware bot that executes the in-game commands. </summary>
        public static Bot Bot { get; set; } = null!;
    }
}
