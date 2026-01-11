using System;
using System.Collections.Generic;
using NHSE.Core;
using NHSE.Villagers;

namespace CrossBot.Core;

/// <summary>
/// Contains details about an item request.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="User">User who requested the item.</param>
/// <param name="UserID">User ID for the <see cref="User"/></param>
/// <param name="Items">List of payloads that need to be injected.</param>
public abstract record ItemRequest<T>(string User, ulong UserID, IReadOnlyCollection<T> Items)
{
    /// <summary>
    /// Indicates if all of the <see cref="Items"/> have been injected.
    /// </summary>
    public bool Injected { get; set; }

    /// <summary>
    /// Method to execute when things have been finished.
    /// </summary>
    public Action<bool>? OnFinish { private get; set; }

    public void NotifyFinished() => OnFinish?.Invoke(Injected);
}

/// <summary>
/// Contains details about a drop request.
/// </summary>
public sealed record DropRequest(string User, ulong UserID, IReadOnlyCollection<Item> Items)
    : ItemRequest<Item>(User, UserID, Items);

/// <summary>
/// Contains details about a field item spawn request.
/// </summary>
public sealed record SpawnRequest(string User, ulong UserID, IReadOnlyCollection<FieldItemColumn> Columns, IReadOnlyList<Item> RawItems)
    : ItemRequest<FieldItemColumn>(User, UserID, Columns);

/// <summary>
/// Contains details about a villager request.
/// </summary>
public sealed record VillagerRequest : ItemRequest<VillagerData>
{
    public VillagerRequest(string user, ulong UserID, IReadOnlyCollection<VillagerData> items) : base(user, UserID, items)
    {
        if (items.Count > 10)
            throw new Exception("Too many villagers requested.");
    }

    public int Index { get; set; }
}