using System;
using Scripts.Data;
using Scripts.Pooling;
using Ui;
using UnityEngine;

namespace Core
{
    public class GameManager : MonoBehaviour
    {
        #region Getters

        public LeaderBoardManager LeaderBoardManager => _leaderBoardManager;
        public EventManager EventManager => _eventManager;
        public UiManager UiManager => _uiManager;
        public PoolingController PoolingController => _poolingController;

        #endregion
        
        [Header("Managers")] 
        [SerializeField] private LeaderBoardManager _leaderBoardManager;
        [SerializeField] private EventManager _eventManager;
        [SerializeField] private UiManager _uiManager;
        [SerializeField] private PoolingController _poolingController;

        private void Awake()
        {
            _poolingController.Initialize(this);
            _leaderBoardManager.Initialize(this);
            _eventManager.Initialize(this);
            _uiManager.Initialize(this);
        }
    }
}