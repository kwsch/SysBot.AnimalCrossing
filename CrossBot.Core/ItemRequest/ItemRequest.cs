using System;
using System.Collections.Generic;
using NHSE.Core;
using NHSE.Villagers;

namespace CrossBot.Core
{
    /// <summary>
    /// Contains details about an item request.
    /// </summary>
    public abstract class ItemRequest<T>
    {
        /// <summary> User who requested the item. </summary>
        public readonly string User;

        /// <summary> User ID for the <see cref="User"/>. </summary>
        public readonly ulong UserID;

        /// <summary> List of payloads that need to be injected. </summary>
        public readonly IReadOnlyCollection<T> Items;

        /// <summary>
        /// Indicates if all of the <see cref="Items"/> have been injected.
        /// </summary>
        public bool Injected { get; set; }

        /// <summary>
        /// Method to execute when things have been finished.
        /// </summary>
        public Action<bool>? OnFinish { private get; set; }

        protected ItemRequest(string user, ulong userID, IReadOnlyCollection<T> items)
        {
            User = user;
            UserID = userID;
            Items = items;
        }

        public void NotifyFinished() => OnFinish?.Invoke(Injected);
    }

    /// <summary>
    /// Contains details about a drop request.
    /// </summary>
    public sealed class DropRequest : ItemRequest<Item>
    {
        public DropRequest(string user, ulong userID, IReadOnlyCollection<Item> items) : base(user, userID, items)
        {
        }
    }

    /// <summary>
    /// Contains details about a field item spawn request.
    /// </summary>
    public sealed class SpawnRequest : ItemRequest<FieldItemColumn>
    {
        public SpawnRequest(string user, ulong userID, IReadOnlyCollection<FieldItemColumn> items) : base(user, userID, items)
        {
        }
    }

    /// <summary>
    /// Contains details about a villager request.
    /// </summary>
    public class VillagerRequest : ItemRequest<VillagerData>
    {
        public VillagerRequest(string user, ulong userID, IReadOnlyCollection<VillagerData> items) : base(user, userID, items)
        {
            if (items.Count > 10)
                throw new Exception("Too many villagers requested.");
        }

        public int Index { get; set; }
    }
}
