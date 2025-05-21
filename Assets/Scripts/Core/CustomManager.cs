using UnityEngine;

namespace Core
{
    public class CustomManager : MonoBehaviour
    {
        public GameManager GameManager { get; set; }

        public virtual void Initialize(GameManager gameManager)
        {
            GameManager = gameManager;
        }
    }
}