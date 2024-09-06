using Map.Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Map
{
    static public class MapUtilities
    {
        static public Action onEarlyFirstEnter;
        static public Action onFirstEnter;
        static public Action onFirstExit;
        static public Action onEarlyExitRoom;
        static public Action onExitRoom;
        static public Action onEarlyAllEnemiesDead;
        static public Action onAllEnemiesDead;
        static public Action onEarlyAllChestOpen;
        static public Action onAllChestOpen;
        static public Action onFinishStage;

        static public RoomData currentRoomData;

        static private MapGenerator mapGenerator = null;
        static public MapGenerator MapGenerator
        {
            get
            {
                if (mapGenerator == null)
                {
                    mapGenerator = GameObject.FindObjectOfType<MapGenerator>(true);
                }

                return mapGenerator;
            }
        }

        static public int Stage { get => MapGenerator.Stage; }

        static public Dictionary<RoomType, int> nbRoomByType = new Dictionary<RoomType, int>
        {
            { RoomType.Lobby, 0 },
            { RoomType.Tutorial, 0 },
            { RoomType.Normal, 0 },
            { RoomType.Treasure, 0 },
            { RoomType.Challenge, 0 },
            { RoomType.Merchant, 0 },
            { RoomType.Secret, 0 },
            { RoomType.MiniBoss, 0 },
            { RoomType.Boss, 0 },
        };

        static public int NbRoom
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

        static public Dictionary<RoomType, int> nbEnterRoomByType = new Dictionary<RoomType, int>
        {
            { RoomType.Lobby, 0 },
            { RoomType.Tutorial, 0 },
            { RoomType.Normal, 0 },
            { RoomType.Treasure, 0 },
            { RoomType.Challenge, 0 },
            { RoomType.Merchant, 0 },
            { RoomType.Secret, 0 },
            { RoomType.MiniBoss, 0 },
            { RoomType.Boss, 0 },
        };

        static public int NbEnterRoom
        {
            get
            {
                int totalCount = 0;
                foreach (int count in nbEnterRoomByType.Values)
                {
                    totalCount += count;
                }

                return totalCount;
            }
        }

        static public void SetDatas(GenerationParameters genParam)
        {
            nbRoomByType = genParam.nbRoomByType.ToDictionary(entry => entry.Key, entry => entry.Value);

            nbEnterRoomByType = new Dictionary<RoomType, int>
            {
                { RoomType.Lobby, 0 },
                { RoomType.Tutorial, 0 },
                { RoomType.Normal, 0 },
                { RoomType.Treasure, 0 },
                { RoomType.Challenge, 0 },
                { RoomType.Merchant, 0 },
                { RoomType.Secret, 0 },
                { RoomType.MiniBoss, 0 },
                { RoomType.Boss, 0 },
            };
        }

        static public void ResetActions()
        {
            onEarlyExitRoom = null;
            onEarlyFirstEnter = null;
            onFirstEnter = null;
            onFirstExit = null;
            onAllChestOpen = null;
            onEarlyAllChestOpen = null;
            onAllEnemiesDead = null;
            onEarlyAllEnemiesDead = null;
            onFinishStage = null;
        }
    }
}