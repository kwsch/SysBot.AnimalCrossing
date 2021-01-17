using System;

namespace CrossBot.SysBot
{
    public sealed record IslandVisitor
    {
        public readonly string Name;
        public readonly ulong UserID;
        public readonly DateTime JoinTime;

        public IslandVisitor(string name, ulong userID) : this(name, userID, DateTime.Now) { }

        public IslandVisitor(string name, ulong userID, DateTime joinTime)
        {
            Name = name;
            UserID = userID;
            JoinTime = joinTime;
        }

        public TimeSpan Duration => DateTime.Now - JoinTime;
    }
}
