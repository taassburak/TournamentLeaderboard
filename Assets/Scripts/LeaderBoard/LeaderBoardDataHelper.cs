using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;



    [Serializable]
    public class PlayerData
    {
        public int Id;
        public string Nickname;
        public int Score;

        public PlayerData(int id, string nickname, int score)
        {
            Id = id;
            Nickname = nickname;
            Score = score;
        }
    }

    [Serializable]
    public class PlayersData
    {
        public List<PlayerData> PlayersDataList = new List<PlayerData>();
    }

   
    public class LeaderBoardDataHelper
    {
        private const string DataFileName = "leaderboard_data.json";
        private const int DefaultPlayerCount = 1000;

        public PlayersData RuntimePlayerData { get; private set; }
        
        private string _fullPath;
        private bool _isDirty = false;
        private Dictionary<int, int> _playerIndexCache;
        private List<PlayerData> _sortedCache; 
        private bool _isSortedCacheDirty = true;

        public LeaderBoardDataHelper()
        {
            _fullPath = Path.Combine(Application.persistentDataPath, DataFileName);
            _playerIndexCache = new Dictionary<int, int>();
            
           
            Application.quitting += SaveOnQuit;
        }


        ~LeaderBoardDataHelper()
        {
            
            Application.quitting -= SaveOnQuit;
            
            #if UNITY_ANDROID || UNITY_IOS
            Application.focusChanged -= OnApplicationFocusChanged;
            #endif
        }

        
        private void SaveOnQuit()
        {
            if (_isDirty)
            {
                SavePlayersData();
            }
        }

        
        private void LoadPlayerData()
        {
            try
            {
                if (File.Exists(_fullPath))
                {
                    string json = File.ReadAllText(_fullPath);
                    RuntimePlayerData = JsonUtility.FromJson<PlayersData>(json);
                    
                    
                    RebuildPlayerIndexCache();
                    
                    _isDirty = false;
                    _isSortedCacheDirty = true;
                    
                    
                }
                else
                {
                    
                    CreateInitData();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading leaderboard data: {e.Message}");
                
               
                CreateInitData();
            }
        }

        
        private void SavePlayersData()
        {
            try
            {
                string json = JsonUtility.ToJson(RuntimePlayerData);
                File.WriteAllText(_fullPath, json);
                _isDirty = false;
                
                Debug.Log($"Saved {RuntimePlayerData.PlayersDataList.Count} players to file.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving leaderboard data: {e.Message}");
            }
        }

        
        public void InitSaveData()
        {
            RuntimePlayerData = new PlayersData();
            
            if (File.Exists(_fullPath))
            {
                LoadPlayerData();
            }
            else
            {
                CreateInitData();
            }
        }

        
        private void CreateInitData()
        {
            RuntimePlayerData.PlayersDataList.Clear();
            
            for (int i = 1; i < DefaultPlayerCount; i++)
            {
                RuntimePlayerData.PlayersDataList.Add(new PlayerData(i, $"Player_{i}", i));
            }
            
           
            RuntimePlayerData.PlayersDataList.Add(new PlayerData(0, "Me", 500));
            
            RebuildPlayerIndexCache();
            _isDirty = true;
            _isSortedCacheDirty = true;
            
            SavePlayersData();
        }
        
        
        private void RebuildPlayerIndexCache()
        {
            _playerIndexCache.Clear();
            
            for (int i = 0; i < RuntimePlayerData.PlayersDataList.Count; i++)
            {
                _playerIndexCache[RuntimePlayerData.PlayersDataList[i].Id] = i;
            }
        }
        
        
        public void UpdatePlayersScore(int playerId, int newScore)
        {
            if (_playerIndexCache.TryGetValue(playerId, out int index))
            {
                RuntimePlayerData.PlayersDataList[index].Score = newScore;
                _isDirty = true;
                _isSortedCacheDirty = true;
            }
            else
            {
                Debug.LogWarning($"Player with ID {playerId} not found.");
            }
        }
        
        public int GetPlayerRank(int id)
        {
            int rank = _sortedCache.FindIndex(x => x.Id == id);
            return rank;
        }
        
        
        public void UpdateScoresRandomly(int minIncrease = 5, int maxIncrease = 500, float updateProbability = 0.9f)
        {
            System.Random random = new System.Random();
            
            for (int i = 0; i < RuntimePlayerData.PlayersDataList.Count; i++)
            {
                
                if (random.NextDouble() < updateProbability)
                {
                    RuntimePlayerData.PlayersDataList[i].Score += random.Next(minIncrease, maxIncrease + 1);
                    _isDirty = true;
                }
            }
            
            if (_isDirty)
            {
                _isSortedCacheDirty = true;
            }
        }
        
        
        public List<PlayerData> GetTopPlayers(int count)
        {
            
            if (_isSortedCacheDirty || _sortedCache == null)
            {
                RefreshSortedCache();
            }
            
            
            count = Math.Min(count, _sortedCache.Count);
            return _sortedCache.GetRange(0, count);
        }

        public PlayerData GetPlayerData(int id)
        {
            return _sortedCache.Find(x => x.Id == id);
        }
        
        
        public List<PlayerData> GetPlayersAroundMe(int above, int below)
        {
            
            if (_isSortedCacheDirty || _sortedCache == null)
            {
                RefreshSortedCache();
            }
            
            
            int meIndex = _sortedCache.FindIndex(p => p.Id == 0); 
            
            if (meIndex < 0)
            {
                Debug.LogWarning("'Me' player not found in sorted list.");
                return new List<PlayerData>();
            }
            
            
            int startIndex = Math.Max(0, meIndex - above);
            int endIndex = Math.Min(_sortedCache.Count - 1, meIndex + below);
            int count = endIndex - startIndex + 1;
            
            return _sortedCache.GetRange(startIndex, count);
        }
        
        
        private void RefreshSortedCache()
        {
            
            _sortedCache = new List<PlayerData>(RuntimePlayerData.PlayersDataList);
            
            
            _sortedCache.Sort((a, b) => b.Score.CompareTo(a.Score));
            
            _isSortedCacheDirty = false;
        }
        
        
        public void SaveManually()
        {
            if (_isDirty)
            {
                SavePlayersData();
            }
        }
        
        
        public void Cleanup()
        {
            SaveOnQuit();
            Application.quitting -= SaveOnQuit;
            
            _playerIndexCache?.Clear();
            _sortedCache?.Clear();
            
            _playerIndexCache = null;
            _sortedCache = null;
        }
    }
