using System;
using Scripts.Data;
using Ui;
using UnityEngine;

namespace Core
{
    public class GameManager : MonoBehaviour
    {
        public LeaderBoardManager LeaderBoardManager => _leaderBoardManager;
        public EventManager EventManager => _eventManager;
        
        [Header("Managers")] 
        [SerializeField] private LeaderBoardManager _leaderBoardManager;
        [SerializeField] private EventManager _eventManager;
        [SerializeField] private UiManager _uiManager;


        private void Awake()
        {
            _leaderBoardManager.Initialize(this);
            _eventManager.Initialize(this);
            _uiManager.Initialize(this);
        }
    }
}