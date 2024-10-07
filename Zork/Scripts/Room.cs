using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Zork;

namespace Zork
{
    public class Room : IEquatable<Room>
    {
        [JsonProperty(Order = 1)]
        public string Name { get; private set; }

        [JsonProperty(Order = 2)]
        public string Description { get; private set; }

        [JsonProperty(PropertyName = "Neighbors", Order = 3)]
        private Dictionary<Directions, string> NeighborNames { get; set; }

        [JsonIgnore]
        public IReadOnlyDictionary<Directions, Room> Neighbors { get; private set; }
        public static bool operator ==(Room lhs, Room rhs)
        {
            if (ReferenceEquals(lhs, rhs))
                return true;
            if (lhs is null || rhs is null)
                return false;
            return lhs.Name == rhs.Name;
        }
        public static bool operator !=(Room lhs, Room rhs) => !(lhs == rhs);
        public override bool Equals(object obj) => obj is Room room ? this == room : false;
        public bool Equals(Room other) => this == other;
        public override string ToString() => Name;
        public override int GetHashCode() => Name.GetHashCode();
        public void UpdateNeighbors(World world) => Neighbors = (
            from entry in NeighborNames
            let room = world.RoomsByName.TryGetValue(entry.Value, out Room room) ? room : null
            where room != null
            select (Direction: entry.Key, Room: room)).ToDictionary(pair => pair.Direction, pair => pair.Room);
    }
}
