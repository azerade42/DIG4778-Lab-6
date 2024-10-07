using System.Collections.Generic;
using Newtonsoft.Json;

namespace Zork
{
    public class Player
    {
        public World World { get; }

        [JsonIgnore]
        public Room Location { get; private set; }

        [JsonIgnore]
        public string LocationName
        {
            get
            {
                return Location?.Name;
            }
            set
            {
                Location = World.RoomsByName.TryGetValue(value, out Room newRoom) ? newRoom : null;
            }
        }

        public int Moves { get; set; }

        public Player(World world, string startingLocation)
        {
            this.World = world;
            this.LocationName = startingLocation;
        }

        public bool Move(Directions direction)
        {
            bool isValidMove = Location.Neighbors.TryGetValue(direction, out Room destination);
            if (isValidMove)
                Location = destination;
            return isValidMove;
        }

    }
}

