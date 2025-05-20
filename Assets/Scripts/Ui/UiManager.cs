using System;
using System.Collections;
using System.Collections.Generic;
using MangoramaStudio.Scripts.Data;
using Scripts.Pooling;
using Sirenix.OdinInspector;
using UnityEngine;


namespace Ui
{
    public class UiManager : MonoBehaviour
    {
        [SerializeField] private LeaderBoardManager _leaderBoardManager;
        [SerializeField] private PoolingController _poolingController;
        [SerializeField] private Transform _playerInfoElementParent;
        [SerializeField] private CustomSlider _scroll;
        [SerializeField] private int _size;

        private int _currentExtraSize = 0;

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

            _scroll.ScrollToItem(_currentExtraSize + 5, 0);
            me.transform.position = new Vector3(-0.25f, -1.5f);
            yield return new WaitForSeconds(1f);

            var unusedItems = _scroll.ContentItems.FindAll(x => x.Rank == meOldRank + 1);
            foreach (var unusedItem in unusedItems)
            {
                unusedItem.transform.SetParent(null);
                unusedItem.transform.gameObject.SetActive(false);
            }

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

            
            if (rankDiff > 0)
            {
                var elementsToReturnPool = _scroll.ContentItems.FindAll(x => x.Rank < meNewRank - 6 || x.Rank > meNewRank + 4);
                _poolingController.DeactiveUnusedPoolItems(elementsToReturnPool);
                foreach (var element in elementsToReturnPool)
                {
                    _scroll.ContentItems.Remove(element);
                }
            }
            else
            {
                var elementsToReturnPool = _scroll.ContentItems.FindAll(x => x.Rank < meNewRank - 4 || x.Rank > meNewRank + 6);
                _poolingController.DeactiveUnusedPoolItems(elementsToReturnPool);
                foreach (var element in elementsToReturnPool)
                {
                    _scroll.ContentItems.Remove(element);
                }
            
            }

            _currentExtraSize = 0;
            _scroll.transform.position = new Vector3(0, 1.2f, 0);
            yield return null;
            _scroll.ArrangeItemsSmooth();
        }
    }
}