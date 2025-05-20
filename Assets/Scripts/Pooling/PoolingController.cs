using UnityEngine;
using System.Collections.Generic;
using Ui;

namespace Scripts.Pooling
{
    /// <summary>
    /// Leaderboard için object pooling yönetimi
    /// </summary>
    public class PoolingController : MonoBehaviour
    {
        [Header("Pooling Settings")]
        [SerializeField] private PlayerInfoElement _playerInfoElementPrefab;
        [SerializeField] private Transform _parent;
        [SerializeField] private int initialPoolSize = 25;
        [SerializeField] private bool expandPoolIfNeeded = true;
        
        private List<PlayerInfoElement> _entryPool = new List<PlayerInfoElement>();
        
        private void Awake()
        {
            CreatePool(initialPoolSize);
        }
        
        
        public void CreatePool(int size)
        {
            
            ClearPool();
            
            
            for (int i = 0; i < size; i++)
            {
                CreatePoolItem();
            }
            
            Debug.Log($"Object pool created with {size} items");
        }
        
        
        private PlayerInfoElement CreatePoolItem()
        {
            PlayerInfoElement entry = Instantiate(_playerInfoElementPrefab, _parent);
            entry.gameObject.SetActive(false);
            _entryPool.Add(entry);
            return entry;
        }
        
        
        public PlayerInfoElement GetPoolItem()
        {
            
            foreach (var entry in _entryPool)
            {
                if (!entry.gameObject.activeInHierarchy)
                {
                    entry.transform.gameObject.SetActive(true);
                    return entry;
                }
            }
            
            if (expandPoolIfNeeded)
            {
                Debug.Log("Pool expanded with a new item");
                var newEntry = CreatePoolItem();
                newEntry.gameObject.SetActive(true);
                return newEntry;
            }
            
            
            Debug.LogWarning("No available items in pool and expansion is disabled");
            return null;
        }

        public void DeactivateAllPoolItems()
        {
            foreach (var entry in _entryPool)
            {
                entry.gameObject.SetActive(false);
                entry.transform.SetParent(this.transform);
            }
        }

        public void DeactiveUnusedPoolItems(List<PlayerInfoElement> activeElements)
        {
            foreach (var pooledItem in activeElements)
            {
                
                if (pooledItem.gameObject.activeInHierarchy)
                {
                    pooledItem.gameObject.SetActive(false);
                    pooledItem.transform.SetParent(this.transform);
                }
            }
        }

        public void ClearPool()
        {
            foreach (var entry in _entryPool)
            {
                if (entry != null)
                {
                    Destroy(entry.gameObject);
                }
            }
            _entryPool.Clear();
        }
        
        private void OnDestroy()
        {
            ClearPool();
        }
        
        public void ReturnPoolItem(PlayerInfoElement element)
        {
            if (element == null) return;
            element.gameObject.SetActive(false);
            element.transform.SetParent(_parent);
            element.transform.localPosition = Vector3.zero;
            element.transform.localRotation = Quaternion.identity;
            element.transform.localScale = Vector3.one;

        }
    }
}