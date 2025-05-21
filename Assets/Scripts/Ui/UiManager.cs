using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using Scripts.Data;
using Scripts.Pooling;
using Sirenix.OdinInspector;
using UnityEngine;


namespace Ui
{
    public class UiManager : CustomManager
    {
        [SerializeField] private LeaderBoardManager _leaderBoardManager;
        [SerializeField] private PoolingController _poolingController;
        [SerializeField] private Transform _playerInfoElementParent;
        [SerializeField] private CustomSlider _scroll;
        [SerializeField] private int _size;
        [SerializeField] private CustomButton _updateButton;

        private int _currentExtraSize = 0;

        public override void Initialize(GameManager gameManager)
        {
            base.Initialize(gameManager);
            _updateButton.Initialize(this);
            CreateLeaderBoardForTesting();
            GameManager.EventManager.OnUpdateButtonClicked += Test;
            GameManager.EventManager.OnScroll += SetUpdateButtonClickable;
        }

        private void OnDestroy()
        {
            GameManager.EventManager.OnUpdateButtonClicked -= Test;
            GameManager.EventManager.OnScroll -= SetUpdateButtonClickable;

        }


        [Button]
        private void CreateLeaderBoardForTesting()
        {
            var players = _leaderBoardManager.GetPlayersAroundMe();
            for (int i = 0; i < _size; i++)
            {
                var element = _poolingController.GetPoolItem();
                element.transform.position = new Vector3(0, i * 1.25f, 0);
                element.transform.SetParent(_playerInfoElementParent);
                _scroll.AddItemToList(element);
                var rank = _leaderBoardManager.GetPlayerRank(players[i].Id);
                element.PopulateView(players[i], rank);
            }

            _scroll.CalculateItemHeight();
            _scroll.RefreshContentItemsList();
            _scroll.ArrangeItems();
        }

        [Button]
        private void AddExtraElement(int extraSize, bool toEnd)
        {
            for (int i = 0; i < extraSize; i++)
            {
                var element = _poolingController.GetPoolItem();
                element.transform.position = new Vector3(0, i * 1.25f, 0);
                element.transform.SetParent(_playerInfoElementParent);
                _scroll.AddItemToList(element, toEnd);
            }

            _currentExtraSize = extraSize;
            if (!toEnd)
            {
                _scroll.ScrollToItem(extraSize + 5, 0);
            }
        }

        [Button]
        private void ResetListWithOnlyVisibles()
        {
            _scroll.ClearList();
            var players = _leaderBoardManager.GetPlayersAroundMe();
            for (int i = 0; i < players.Count; i++)
            {
                var element = _poolingController.GetPoolItem();
                element.transform.position = new Vector3(0, i * 1.25f, 0);
                element.transform.SetParent(_playerInfoElementParent);
                element.PopulateView(players[i], 0);
                var rank = _leaderBoardManager.GetPlayerRank(players[i].Id);
                element.PopulateView(players[i], rank);
            }

            _poolingController.DeactiveUnusedPoolItems(_scroll.ContentItems);
        }

        [Button]
        private void Test()
        {
            StartCoroutine(UpdateVisualAfterScoresUpdate());
        }

        private IEnumerator UpdateVisualAfterScoresUpdate()
        {
            GameManager.EventManager.ScrollStarted();
            int meOldRank = _leaderBoardManager.GetPlayerRank(0);
            _leaderBoardManager.UpdateScoresRandomly();

            int meNewRank = _leaderBoardManager.GetPlayerRank(0);

            int rankDiff = meNewRank - meOldRank;

            var me = _scroll.ContentItems.Find(x => x.AssignedData.Id == 0);
            me.transform.SetParent(null);
            var meData = _leaderBoardManager.GetPlayerData(me.AssignedData.Id);
            me.PopulateView(meData, meNewRank);

            if (rankDiff > 0)
            {
                AddExtraElement(Mathf.Abs(rankDiff), false);

                var players = _leaderBoardManager.GetPlayersAroundMe(Mathf.Abs(rankDiff) + 5, 5);

                for (int i = 0; i < _scroll.ContentItems.Count; i++)
                {
                    if (players[i].Id != 0)
                    {
                        _scroll.ContentItems[i]
                            .PopulateView(players[i], _leaderBoardManager.GetPlayerRank(players[i].Id));
                    }
                }
            }
            else
            {
                AddExtraElement(Mathf.Abs(rankDiff), true);

                var players = _leaderBoardManager.GetPlayersAroundMe(5, Mathf.Abs(rankDiff) + 5);

                for (int i = 0; i < _scroll.ContentItems.Count; i++)
                {
                    if (players[i].Id != 0)
                    {
                        _scroll.ContentItems[i]
                            .PopulateView(players[i], _leaderBoardManager.GetPlayerRank(players[i].Id));
                    }
                }
            }

            var unusedItems = FindElementsWithSameRank();
            foreach (var unusedItem in unusedItems)
            {
                unusedItem.transform.SetParent(null);
                unusedItem.transform.gameObject.SetActive(false);
            }
            _scroll.SortContentItemsByRank();
            _scroll.ArrangeItems();
            yield return null;
            _scroll.ScrollToItem(_currentExtraSize + 5 - unusedItems.Count, 0);
            me.transform.position = new Vector3(-0.25f, -1.5f);
            yield return new WaitForSeconds(1f);

            

            var nextItem = _scroll.ContentItems.Find(x => x.Rank + 1 == meNewRank);
            var desiredScrollIndex = nextItem.transform.GetSiblingIndex();


            _scroll.ScrollCustom(desiredScrollIndex);
            yield return new WaitForSeconds(_scroll.scrollDuration);
            me.transform.SetParent(_scroll.transform);
            me.transform.SetSiblingIndex(desiredScrollIndex + 1);
            _scroll.RefreshContentItemsList();
            _scroll.CalculateItemHeight();
            _scroll.ArrangeItemsSmooth();
            yield return new WaitForSeconds(1f);

            
            var elementsToReturnPool = _scroll.ContentItems.FindAll(x => x.Rank < meNewRank - 5 || x.Rank > meNewRank + 5);
            _poolingController.DeactiveUnusedPoolItems(elementsToReturnPool);
            foreach (var element in elementsToReturnPool)
            {
                _scroll.ContentItems.Remove(element);
            }
            
            _currentExtraSize = 0;
            yield return null;
            GameManager.EventManager.ScrollEnded();
        }

        private void SetUpdateButtonClickable(bool isUnclickable)
        {
            _updateButton.SetClickable(!isUnclickable);
        }
        
        private List<PlayerInfoElement> FindElementsWithSameRank()
        {
            _scroll.RefreshContentItemsList();
    
            Dictionary<int, List<PlayerInfoElement>> rankGroups = new Dictionary<int, List<PlayerInfoElement>>();
    
            foreach (var element in _scroll.ContentItems)
            {
                if (element == null) continue;
        
                int rank = element.Rank;
        
                if (!rankGroups.ContainsKey(rank))
                {
                    rankGroups[rank] = new List<PlayerInfoElement>();
                }
        
                rankGroups[rank].Add(element);
            }
    
            List<PlayerInfoElement> sameRankGroups = new List<PlayerInfoElement>();
    
            foreach (var group in rankGroups)
            {
                if (group.Value.Count > 1)
                {
                    for (int i = 1; i < group.Value.Count; i++)
                    {
                        sameRankGroups.Add(group.Value[i]);
                    }
                }
            }
            return sameRankGroups;
        }
    }
}