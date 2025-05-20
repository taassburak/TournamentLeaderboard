using System;
using System.Collections.Generic;

namespace Data
{
    [Serializable]
    public class PlayerData
    {
        public int Id;
        public string NickName;
        public int Score;

        public PlayerData(int id, string nickName, int score)
        {
            Id = id;
            NickName = nickName;
            Score = score;
        }
    }

    [Serializable]
    public class PlayersData
    {
        public List<PlayerData> PlayersDataList;

        public PlayersData()
        {
            PlayersDataList = new List<PlayerData>();
        }
    }
}