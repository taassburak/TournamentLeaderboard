using System;
using Data;
using UnityEngine;

namespace Core
{
    public class EventManager : CustomManager
    {
        public event Action OnUpdateButtonClicked;
        public event Action<bool> OnScroll;
        
        
        public override void Initialize(GameManager gameManager)
        {
            base.Initialize(gameManager);
        }

        public void UpdateButtonClicked()
        {
            OnUpdateButtonClicked?.Invoke();
        }

        public void ScrollStarted()
        {
            OnScroll?.Invoke(true);
        }

        public void ScrollEnded()
        {
            OnScroll?.Invoke(false);
        }
    }
}