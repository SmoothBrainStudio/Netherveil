using Map;
using Map.Generation;
using System.Collections.Generic;
using UnityEngine;

public struct GenerationParameters
{
    public Dictionary<RoomType, int> nbRoomByType;
    public Dictionary<int, List<Door>> availableDoorsByRotation;

    public GenerationParameters(int nbNormal = 0, int nbTreasure = 0, int nbChallenge = 0, int nbMerchant = 0, int nbSecret = 0, int nbMiniBoss = 0, int nbBoss = 0)
    {
        nbRoomByType = new Dictionary<RoomType, int>
            {
                { RoomType.Lobby, 1 },
                { RoomType.Tutorial, 0 },
                { RoomType.Normal, nbNormal },
                { RoomType.Treasure, nbTreasure },
                { RoomType.Challenge, nbChallenge },
                { RoomType.Merchant, nbMerchant },
                { RoomType.Secret, nbSecret },
                { RoomType.MiniBoss, nbMiniBoss },
                { RoomType.Boss, nbBoss },
            };

        availableDoorsByRotation = new Dictionary<int, List<Door>>
            {
                { 0, new List<Door>() },
                { 90, new List<Door>() },
                { 180, new List<Door>() },
                { 270, new List<Door>() }
            };
    }

    public readonly int NbRoom
    {
        get
        {
            int totalCount = 0;
            foreach (int count in nbRoomByType.Values)
            {
                totalCount += count;
            }

            return totalCount;
        }
    }

    public int AvailableDoorsCount
    {
        get
        {
            int count = 0;

            foreach (var list in availableDoorsByRotation.Values)
            {
                count += list.Count;
            }

            return count;
        }
    }

    public readonly void AddDoors(DoorsGenerator doorsGenerator)
    {
        foreach (var door in doorsGenerator.doors)
        {
            int rotation = (int)Mathf.Round(door.Rotation);
            if (availableDoorsByRotation.ContainsKey(rotation))
            {
                availableDoorsByRotation[rotation].Add(door);
            }
            else
            {
                Debug.LogError("Attempt to insert an object with a disallowed rotation : " + door.Rotation, doorsGenerator);
            }
        }
    }

    public readonly List<Door> GetFarestDoors()
    {
        List<Door> result = new List<Door>();
        foreach (var doors in availableDoorsByRotation)
        {
            result.AddRange(doors.Value);
        }

        result.Sort((a, b) => (int)-(b.Position.magnitude - a.Position.magnitude));
        return result;
    }

    public readonly List<Door> GetFarestDoorsByRot(int rot)
    {
        List<Door> result = availableDoorsByRotation[rot];

        result.Sort((a, b) => (int)-(b.Position.magnitude - a.Position.magnitude));
        return result;
    }

    public readonly void RemoveDoor(Door door)
    {
        foreach (var doors in availableDoorsByRotation.Values)
        {
            if (doors.Remove(door))
            {
                return;
            }
        }
    }
}
