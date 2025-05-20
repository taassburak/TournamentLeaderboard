using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Ui;


[ExecuteInEditMode]
public class CustomSlider : MonoBehaviour
{
    public List<PlayerInfoElement> ContentItems = new List<PlayerInfoElement>();

    [OnValueChanged("ArrangeItems")] public float spacing = 1.0f;

    public int visibleItemCount = 10;
    public Transform viewportCenter;

    [Header("Scroll Settings")] public float scrollDuration = 0.5f;
    public AnimationCurve scrollCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public int CurrentCenterIndex => currentCenterIndex;
    [Header("Debug")] [SerializeField] private int currentCenterIndex = 0;
    private bool isScrolling = false;
    
    private float itemHeight = 0;
    private Vector3 viewportCenterPos;

    [Header("Smooth Movement Settings")]
    public float positionChangeDuration = 0.3f;

    public AnimationCurve positionChangeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Dictionary<Transform, Coroutine>
        activeMovements = new Dictionary<Transform, Coroutine>();
    
    void Awake()
    {
        if (viewportCenter != null)
        {
            viewportCenterPos = viewportCenter.position;
        }
        else
        {
            viewportCenterPos = Camera.main.transform.position +
                                Camera.main.transform.forward * Camera.main.nearClipPlane * 1.5f;
        }
    }

    void Start()
    {
        
        CalculateItemHeight();
        RefreshContentItemsList();
        ArrangeItems();
    }

    [SerializeField] private bool _isEditMode;

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
                itemHeight = renderer.bounds.size.y + spacing;
            }
            else
            {
                Collider collider = ContentItems[0].GetComponent<Collider>();
                if (collider != null)
                {
                    itemHeight = collider.bounds.size.y + spacing;
                }
                else
                {
                    itemHeight = 1f + spacing;
                }
            }
        }
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
                currentPos += Vector3.down * itemHeight;
            }
        }
    }
    
    public void ScrollToItem(int itemIndex, float tempDuration = -1)
    {
        if (tempDuration == -1)
        {
            tempDuration = scrollDuration;
        }
        
        RefreshContentItemsList();

        
        itemIndex = Mathf.Clamp(itemIndex, 0, ContentItems.Count - 1);

        
        if (itemIndex == currentCenterIndex && !isScrolling)
        {
            return;
        }

        if (isScrolling)
        {
            StopAllCoroutines(); 
        }

        StartCoroutine(ScrollToIndexCoroutine(itemIndex, tempDuration));
    }
    
    private IEnumerator ScrollToIndexCoroutine(int targetIndex, float duration)
    {
        isScrolling = true;
        
        Vector3 targetItemCurrentPos = ContentItems[targetIndex].transform.position;

        
        float offsetY = targetItemCurrentPos.y - viewportCenterPos.y;
        
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
            float normalizedTime = Mathf.Clamp01(elapsedTime / scrollDuration);
            float curveValue = scrollCurve.Evaluate(normalizedTime);
            
            transform.position = Vector3.Lerp(startPos, targetPos, curveValue);

            yield return null;
        }
        
        transform.position = targetPos;
        currentCenterIndex = targetIndex;
        isScrolling = false;

        
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
        ScrollToItem(currentCenterIndex + 1);
    }

    [Button]
    public void ScrollToPrevious()
    {
        ScrollToItem(currentCenterIndex - 1);
    }
    
    public void ScrollToNextPage()
    {
        ScrollToItem(currentCenterIndex + visibleItemCount);
    }

    public void ScrollToPreviousPage()
    {
        ScrollToItem(currentCenterIndex - visibleItemCount);
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
                currentPos += Vector3.down * itemHeight;
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

        while (elapsedTime < positionChangeDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / positionChangeDuration);
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
        isScrolling = true;
        
        Vector3 targetItemCurrentPos = ContentItems[targetIndex].transform.position;
        
        float offsetY = targetItemCurrentPos.y - viewportCenterPos.y;
        
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

        while (elapsedTime < scrollDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / scrollDuration);
            float curveValue = scrollCurve.Evaluate(normalizedTime);

            transform.position = Vector3.Lerp(startPos, targetPos, curveValue);

            if (excludedElement != null)
            {
                float scrollOffset = transform.position.y - startPos.y;

                excludedElement.transform.position = excludedElementStartPos + new Vector3(0, scrollOffset, 0);
            }

            yield return null;
        }

        transform.position = targetPos;
        currentCenterIndex = targetIndex;

        if (excludedElement != null)
        {
            float finalScrollOffset = targetPos.y - startPos.y;
            excludedElement.transform.position = excludedElementStartPos + new Vector3(0, finalScrollOffset, 0);
        }

        isScrolling = false;

    }
}