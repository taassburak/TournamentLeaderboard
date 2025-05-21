using System;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Core;
using Scripts.Pooling;
using Ui;
namespace Scripts.Data
{
    
    public class LeaderBoardManager : CustomManager
    {
        [Header("Settings")]
        [SerializeField] private int visibleEntries = 20;
        [Tooltip("Oyundan çıkış dışında manuel kaydetme işlemi yapılmasını istiyorsanız işaretleyin")]
        [SerializeField] private bool allowManualSave = false;
        
        [Header("Debug")]
        [SerializeField, ReadOnly] private int totalPlayerCount;
        [SerializeField, ReadOnly] private int myRank;
        [SerializeField, ReadOnly] private bool isDirty = false;
        
        [SerializeField] private PlayerInfoElement _playerInfoElementPrefab;
        [SerializeField] private Transform _parent;
        private LeaderBoardDataHelper _leaderBoardDataHelper;
        private bool _isInitialized;

        [SerializeField] private PoolingController _poolingController;

        public event Action OnUpdateButtonClicked; 

        
        
        public System.Action OnLeaderboardChanged;

        public override void Initialize(GameManager gameManager)
        {
            base.Initialize(gameManager);
            SelfInit();
        }
        
        private void OnEnable()
        {
            if (!_isInitialized)
            {
                SelfInit();
            }
        }
        
        private void OnDestroy()
        {
            if (_isInitialized)
            {
                
                _leaderBoardDataHelper.Cleanup();
            }
        }

        
        private void SelfInit()
        {
            if (_isInitialized) return;
            
            _leaderBoardDataHelper = new LeaderBoardDataHelper();
            _leaderBoardDataHelper.InitSaveData();
            Application.focusChanged += OnApplicationFocusChanged;
            UpdateDebugInfo();
            _isInitialized = true;
        }

        private void OnApplicationFocusChanged(bool hasFocus)
        {
            if (!hasFocus && _isInitialized)
            {
                
                Debug.Log("Application paused, saving data if needed...");
                _leaderBoardDataHelper.SaveManually();
            }
        }
        
        
        [Button]
        public void UpdateScoresRandomly()
        {
            if (!_isInitialized) SelfInit();
            
            _leaderBoardDataHelper.UpdateScoresRandomly();
            UpdateDebugInfo();
            isDirty = true;
            
            
            OnLeaderboardChanged?.Invoke();
            
            if (allowManualSave)
            {
                SaveData();
            }
            
            
        }
        
        
        public List<PlayerData> GetTopPlayers(int count = 10)
        {
            if (!_isInitialized) SelfInit();
            return _leaderBoardDataHelper.GetTopPlayers(count);
        }
        
        
        [Button]
        public List<PlayerData> GetPlayersAroundMe(int above = 5, int below = 5)
        {
            if (!_isInitialized) SelfInit();
            return _leaderBoardDataHelper.GetPlayersAroundMe(above, below);
        }

        public int GetPlayerRank(int id)
        {
            return _leaderBoardDataHelper.GetPlayerRank(id);
        }
        
        
        public List<PlayerData> GetLeaderboardEntries()
        {
            if (!_isInitialized) SelfInit();
            
         
            int halfVisible = visibleEntries / 2;
            

            List<PlayerData> topPlayers = _leaderBoardDataHelper.GetTopPlayers(halfVisible);
            

            List<PlayerData> aroundMe = _leaderBoardDataHelper.GetPlayersAroundMe(halfVisible, halfVisible);
            
  
            HashSet<int> addedIds = new HashSet<int>();
            List<PlayerData> result = new List<PlayerData>();
            

            foreach (var player in topPlayers)
            {
                if (!addedIds.Contains(player.Id))
                {
                    result.Add(player);
                    addedIds.Add(player.Id);
                }
            }

            foreach (var player in aroundMe)
            {
                if (!addedIds.Contains(player.Id))
                {
                    result.Add(player);
                    addedIds.Add(player.Id);
                }
            }

            result.Sort((a, b) => b.Score.CompareTo(a.Score));
            
            UpdateDebugInfo();
            return result;
        }

        public void UpdatePlayersScore(int playerId, int newScore)
        {
            if (!_isInitialized) SelfInit();
            
            _leaderBoardDataHelper.UpdatePlayersScore(playerId, newScore);

            OnLeaderboardChanged?.Invoke();
            
            UpdateDebugInfo();
            isDirty = true;
            
            if (allowManualSave)
            {
                SaveData();
            }
        }
        
        [Button("Save Data Manually")]
        public void SaveData()
        {
            if (!_isInitialized) SelfInit();
            
            if (allowManualSave)
            {
                _leaderBoardDataHelper.SaveManually();
                isDirty = false;
                Debug.Log("Leaderboard data saved manually.");
            }
            else
            {
                Debug.Log("Manual save is disabled. Data will be saved on application quit.");
            }
        }
        
        
        private void UpdateDebugInfo()
        {
            totalPlayerCount = _leaderBoardDataHelper.RuntimePlayerData.PlayersDataList.Count;

            List<PlayerData> allSorted = _leaderBoardDataHelper.GetTopPlayers(totalPlayerCount);
            myRank = allSorted.FindIndex(p => p.Id == 0) + 1; // ID 0 = "Me"
        }

        public PlayerData GetPlayerData(int assignedDataId)
        {
            return _leaderBoardDataHelper.GetPlayerData(assignedDataId);
        }
    }
}