using System.Collections.Generic;
using NHSE.Core;

namespace CrossBot.Core
{
    /// <summary>
    /// Contains details about an item request.
    /// </summary>
    public sealed class ItemRequest
    {
        /// <summary> User who requested the item. </summary>
        public readonly string User;

        /// <summary> User ID for the <see cref="User"/>. </summary>
        public readonly ulong UserID;

        /// <summary> List of items that the user requested. </summary>
        public readonly IReadOnlyCollection<Item> Items;

        public ItemRequest(string user, ulong userID, IReadOnlyCollection<Item> items)
        {
            User = user;
            UserID = userID;
            Items = items;
        }
    }
}
