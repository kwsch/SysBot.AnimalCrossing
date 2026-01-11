using System;

namespace CrossBot.SysBot
{
    public sealed record IslandVisitor(string Name, ulong UserID, DateTime JoinTime)
    {
        public IslandVisitor(string Name, ulong UserID) : this(Name, UserID, DateTime.Now) { }

        public TimeSpan Duration => DateTime.Now - JoinTime;
    }
}
