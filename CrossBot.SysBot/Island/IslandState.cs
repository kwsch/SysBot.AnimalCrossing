using System.Collections.Generic;
using System.Threading;

namespace CrossBot.SysBot;

public sealed class IslandState
{
    private readonly List<IslandVisitor> List = [];
    private readonly Lock _sync = new();

    public string DodoCode { get; set; } = "No code set yet.";

    public int Count { get { lock (_sync) return List.Count; } }
    public IslandVisitor[] GetCurrentVisitors() { { lock (_sync) return List.ToArray(); } }

    public IslandVisitor? GetVisitor(ulong userID) { lock (_sync) return List.Find(z => z.UserID == userID); }
    public IslandVisitor? GetVisitor(string name) { lock (_sync) return List.Find(z => z.Name == name); }

    public bool Arrive(string name)
    {
        lock (_sync)
        {
            var list = List;
            var match = list.Find(z => z.Name == name);
            if (match != null)
                return false;

            var visitor = new IslandVisitor(name, 0);
            list.Add(visitor);
        }

        return true;
    }

    public bool Arrive(string name, ulong userID)
    {
        lock (_sync)
        {
            var list = List;
            var match = list.Find(z => z.UserID == userID);
            if (match != null)
                return false;

            var visitor = new IslandVisitor(name, userID);
            list.Add(visitor);
        }

        return true;
    }

    public bool Depart(string name)
    {
        lock (_sync)
        {
            var list = List;
            var match = list.Find(z => z.Name == name);
            if (match == null)
                return false;

            list.Remove(match);
        }
        return true;
    }

    public IslandVisitor? Depart(ulong userID)
    {
        lock (_sync)
        {
            var list = List;
            var match = list.Find(z => z.UserID == userID);
            if (match == null)
                return null;

            list.Remove(match);
            return match;
        }
    }
}