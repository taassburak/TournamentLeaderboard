using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Ui;
using UnityEngine.Serialization;


[ExecuteInEditMode]
public class CustomSlider : MonoBehaviour
{
    public List<PlayerInfoElement> ContentItems = new List<PlayerInfoElement>();
    public AnimationCurve positionChangeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [OnValueChanged("ArrangeItems")] public float spacing = 1.0f;
    public int VisibleItemCount = 10;
    public Transform ViewportCenter;



    [Header("Scroll Settings")] public float ScrollDuration = 0.5f;
    public AnimationCurve ScrollCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public int CurrentCenterIndex => _currentCenterIndex;
    [Header("Debug")] [SerializeField] private int _currentCenterIndex = 0;
    private bool _isScrolling = false;
    
    private float _itemHeight = 0;
    private Vector3 _viewportCenterPos;

    [Header("Smooth Movement Settings")]
    public float PositionChangeDuration = 0.3f;

    [SerializeField] private bool _isEditMode;

    private Dictionary<Transform, Coroutine>
        activeMovements = new Dictionary<Transform, Coroutine>();
    
    void Awake()
    {
        if (ViewportCenter != null)
        {
            _viewportCenterPos = ViewportCenter.position;
        }
        else
        {
            _viewportCenterPos = Camera.main.transform.position +
                                Camera.main.transform.forward * Camera.main.nearClipPlane * 1.5f;
        }
    }

    void Start()
    {
        CalculateItemHeight();
        RefreshContentItemsList();
        ArrangeItems();
    }


    private void Update()
    {
        // if (_isEditMode)
        // {
        //     CalculateItemHeight();
        //     RefreshContentItemsList(); // Liste yerine transform child'ları kullan
        //     ArrangeItems();
        // }
    }
    
    [Button]
    public void RefreshContentItemsList()
    {
        ContentItems.Clear();
        List<PlayerInfoElement> childElements = new List<PlayerInfoElement>();

        for (int i = 0; i < transform.childCount; i++)
        {
            PlayerInfoElement element = transform.GetChild(i).GetComponent<PlayerInfoElement>();
            if (element != null)
            {
                childElements.Add(element);
            }
        }
        for (int i = 0; i < transform.childCount; i++)
        {
            for (int j = 0; j < childElements.Count; j++)
            {
                if (childElements[j].transform.GetSiblingIndex() == i)
                {
                    ContentItems.Add(childElements[j]);
                    break;
                }
            }
        }
    }

    
    [Button]
    public void CalculateItemHeight()
    {
        RefreshContentItemsList();

        if (ContentItems.Count > 0 && ContentItems[0] != null)
        {
            Renderer renderer = ContentItems[0].GetComponent<Renderer>();
            if (renderer != null)
            {
                _itemHeight = renderer.bounds.size.y + spacing;
            }
            else
            {
                Collider collider = ContentItems[0].GetComponent<Collider>();
                if (collider != null)
                {
                    _itemHeight = collider.bounds.size.y + spacing;
                }
                else
                {
                    _itemHeight = 1f + spacing;
                }
            }
        }
    }
    
    public void SortContentItemsByRank(bool ascending = true)
    {
        RefreshContentItemsList();
    
        if (ContentItems.Count <= 1) return;
    
        List<PlayerInfoElement> sortedList;
    
        if (ascending)
        {
            sortedList = ContentItems.OrderBy(element => element.Rank).ToList();
        }
        else
        {
            sortedList = ContentItems.OrderByDescending(element => element.Rank).ToList();
        }
    
        for (int i = 0; i < sortedList.Count; i++)
        {
            sortedList[i].transform.SetSiblingIndex(i);
        }
    
        RefreshContentItemsList();
    }
    
    
    [Button]
    public void ArrangeItems()
    {
        RefreshContentItemsList();

        if (ContentItems.Count == 0)
            return;

        Vector3 currentPos = transform.position;
        
        for (int i = 0; i < ContentItems.Count; i++)
        {
            if (ContentItems[i] != null)
            {
                ContentItems[i].transform.position = currentPos;
                currentPos += Vector3.down * _itemHeight;
            }
        }
    }
    
    public void ScrollToItem(int itemIndex, float tempDuration = -1)
    {
        if (tempDuration == -1)
        {
            tempDuration = ScrollDuration;
        }
        
        RefreshContentItemsList();

        
        itemIndex = Mathf.Clamp(itemIndex, 0, ContentItems.Count - 1);

        
        if (itemIndex == _currentCenterIndex && !_isScrolling)
        {
            return;
        }

        if (_isScrolling)
        {
            StopAllCoroutines(); 
        }

        StartCoroutine(ScrollToIndexCoroutine(itemIndex, tempDuration));
    }
    
    private IEnumerator ScrollToIndexCoroutine(int targetIndex, float duration)
    {
        _isScrolling = true;
        
        Vector3 targetItemCurrentPos = ContentItems[targetIndex].transform.position;

        
        float offsetY = targetItemCurrentPos.y - _viewportCenterPos.y;
        
        Vector3 startPos = transform.position;
        Vector3 targetPos = new Vector3(
            transform.position.x,
            transform.position.y - offsetY,
            transform.position.z
        );
        
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / ScrollDuration);
            float curveValue = ScrollCurve.Evaluate(normalizedTime);
            
            transform.position = Vector3.Lerp(startPos, targetPos, curveValue);

            yield return null;
        }
        
        transform.position = targetPos;
        _currentCenterIndex = targetIndex;
        _isScrolling = false;

        
    }

    [Button]
    public void ScrollCustom(int index)
    {
        int zeroBasedIndex = index - 1;
        
        RefreshContentItemsList();

        zeroBasedIndex = Mathf.Clamp(zeroBasedIndex, 5, ContentItems.Count - 6);

        ScrollToItem(zeroBasedIndex);
    }

    [Button]
    public void ScrollToNext()
    {
        ScrollToItem(_currentCenterIndex + 1);
    }

    [Button]
    public void ScrollToPrevious()
    {
        ScrollToItem(_currentCenterIndex - 1);
    }
    
    public void ScrollToNextPage()
    {
        ScrollToItem(_currentCenterIndex + VisibleItemCount);
    }

    public void ScrollToPreviousPage()
    {
        ScrollToItem(_currentCenterIndex - VisibleItemCount);
    }
    
    public void ScrollToStart()
    {
        ScrollToItem(0);
    }

    public void ScrollToEnd()
    {
        RefreshContentItemsList();

        ScrollToItem(ContentItems.Count - 1);
    }

    public void AddItemToList(PlayerInfoElement element, bool toEnd = true)
    {
        if (element == null) return;
        
        element.transform.SetParent(transform);

        if (toEnd)
        {
            element.transform.SetSiblingIndex(transform.childCount - 1);
        }
        else
        {
            element.transform.SetSiblingIndex(0);
        }
        
        RefreshContentItemsList();
        
        ArrangeItems();
    }


    public void RemoveItemFromList(PlayerInfoElement element)
    {
        if (element == null) return;
        
        element.transform.SetParent(null);
        RefreshContentItemsList();
        ArrangeItems();
    }

    public void ClearList()
    {
        while (transform.childCount > 0)
        {
            Transform child = transform.GetChild(0);
            child.SetParent(null);
        }
        ContentItems.Clear();
    }
    
    public void SetElementSiblingIndex(PlayerInfoElement element, int newIndex)
    {
        if (element == null) return;
        
        element.transform.SetSiblingIndex(newIndex);
        RefreshContentItemsList();
        ArrangeItems();
    }
    
    private Dictionary<Transform, Vector3> CalculateTargetPositions()
    {
        Dictionary<Transform, Vector3> positions = new Dictionary<Transform, Vector3>();

        Vector3 currentPos = transform.position;

        for (int i = 0; i < ContentItems.Count; i++)
        {
            if (ContentItems[i] != null)
            {
                positions[ContentItems[i].transform] = currentPos;
                currentPos += Vector3.down * _itemHeight;
            }
        }

        return positions;
    }
    
    private void SmoothMoveElement(Transform element, Vector3 targetPosition)
    {
        if (activeMovements.ContainsKey(element) && activeMovements[element] != null)
        {
            StopCoroutine(activeMovements[element]);
        }
        
        activeMovements[element] = StartCoroutine(SmoothMoveCoroutine(element, targetPosition));
    }
    
    private IEnumerator SmoothMoveCoroutine(Transform element, Vector3 targetPosition)
    {
        Vector3 startPosition = element.position;
        float elapsedTime = 0;

        while (elapsedTime < PositionChangeDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / PositionChangeDuration);
            float curveValue = positionChangeCurve.Evaluate(normalizedTime);
            
            element.position = Vector3.Lerp(startPosition, targetPosition, curveValue);

            yield return null;
        }
        
        element.position = targetPosition;
        
        activeMovements.Remove(element);
    }
    
    [Button]
    public void ArrangeItemsSmooth()
    {
        RefreshContentItemsList();

        if (ContentItems.Count == 0)
            return;
        
        Dictionary<Transform, Vector3> targetPositions = CalculateTargetPositions();
        
        foreach (var item in ContentItems)
        {
            if (item != null)
            {
                SmoothMoveElement(item.transform, targetPositions[item.transform]);
            }
        }
    }
    
    [Button]
    public void ScrollCustomWithExclusion(int index, int excludedElementId = -1)
    {
        int zeroBasedIndex = index - 1;
        
        RefreshContentItemsList();

        zeroBasedIndex = Mathf.Clamp(zeroBasedIndex, 5, ContentItems.Count - 6);
        
        PlayerInfoElement excludedElement = null;
        if (excludedElementId >= 0)
        {
            excludedElement = ContentItems.Find(item => item.AssignedData.Id == excludedElementId);
        }

        StartCoroutine(ScrollToIndexWithExclusionCoroutine(zeroBasedIndex, excludedElement));
    }
    
    private IEnumerator ScrollToIndexWithExclusionCoroutine(int targetIndex, PlayerInfoElement excludedElement)
    {
        _isScrolling = true;
        
        Vector3 targetItemCurrentPos = ContentItems[targetIndex].transform.position;
        
        float offsetY = targetItemCurrentPos.y - _viewportCenterPos.y;
        
        Vector3 startPos = transform.position;
        Vector3 targetPos = new Vector3(
            transform.position.x,
            transform.position.y - offsetY,
            transform.position.z
        );
        
        Vector3 excludedElementStartPos = Vector3.zero;
        if (excludedElement != null)
        {
            excludedElementStartPos = excludedElement.transform.position;
        }

        float elapsedTime = 0;

        while (elapsedTime < ScrollDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / ScrollDuration);
            float curveValue = ScrollCurve.Evaluate(normalizedTime);

            transform.position = Vector3.Lerp(startPos, targetPos, curveValue);

            if (excludedElement != null)
            {
                float scrollOffset = transform.position.y - startPos.y;

                excludedElement.transform.position = excludedElementStartPos + new Vector3(0, scrollOffset, 0);
            }

            yield return null;
        }

        transform.position = targetPos;
        _currentCenterIndex = targetIndex;

        if (excludedElement != null)
        {
            float finalScrollOffset = targetPos.y - startPos.y;
            excludedElement.transform.position = excludedElementStartPos + new Vector3(0, finalScrollOffset, 0);
        }

        _isScrolling = false;

    }
}