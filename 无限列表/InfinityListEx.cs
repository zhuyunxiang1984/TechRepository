using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InfinityListEx : MonoBehaviour
{
    public GameObject ItemPrefab;
    public int ItemRefHeight = 20;
    
    protected ScrollRectEx m_ScrollRect;
    protected RectTransform m_ScrollRectTrans;
    protected RectTransform m_ContainerRectTrans;

    protected bool m_HasInited;

    protected List<GameObject> m_items = new List<GameObject>();
    protected Stack<GameObject> m_caches = new Stack<GameObject>();

    protected int m_TotalCount;
    protected struct ItemData
    {
        public Vector2 poss;
        public Vector2 size;
        public Vector2 halfSize;
        public GameObject view;
    }
    protected ItemData[] m_arrItemData;

    protected bool m_reverseExpand;

    void Awake()
    {
        m_ContainerRectTrans = transform as RectTransform;
        m_ScrollRect = transform.parent.GetComponent<ScrollRectEx>();
        m_ScrollRectTrans = m_ScrollRect.transform as RectTransform;
        m_HasInited = false;
    }
    void Start()
    {
        InitList(200);
        m_reverseExpand = true;
        m_ScrollRect.normalizedPosition = new Vector2(0f, 0f);
    }
    void OnEnable()
    {
        m_ScrollRect.onValueChanged.AddListener(_OnValueChanged);
    }
    void OnDisable()
    {
        m_ScrollRect.onValueChanged.RemoveListener(_OnValueChanged);
    }
    void _OnValueChanged(Vector2 value)
    {
        //_UpdateMovement();
        //_UpdateList();
        m_reverseExpand = value.y >= 0f;
    }
    void Update()
    {
        if (m_HasInited)
        {
            _UpdateList();
        }
    }
    public void InitList(int totalCount)
    {
        if (m_HasInited) return;

        m_TotalCount = totalCount;
        m_arrItemData = new ItemData[totalCount];
        for (int i = 0; i < totalCount; ++i)
        {
            m_arrItemData[i].size = new Vector2(0, ItemRefHeight);
            m_arrItemData[i].halfSize = m_arrItemData[i].size * 0.5f;
        }
        _UpdateContainer();
        m_HasInited = true;
    }

    //获取一个item实例
    protected GameObject _CreateItem()
    {
        if (m_caches.Count > 0)
        {
            var item = m_caches.Pop();
            item.SetActive(true);
            return item;
        }
        return Instantiate(ItemPrefab, m_ContainerRectTrans);
    }
    protected void _DestroyItem(GameObject item)
    {
        item.SetActive(false);
        m_caches.Push(item);
    }
    protected void _UpdateItemData(GameObject item, int index)
    {
        var str = $"item:{index}";
        if (index % 2 == 0)
        {
            str += "\n#";
        }
        if (index % 3 == 0)
        {
            str += "\n#";
        }
        item.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = str;
    }

    protected List<int> m_list1 = new List<int>();
    protected List<int> m_list2 = new List<int>();
    protected List<int> m_list3 = new List<int>();
    protected void _UpdateList()
    {
        float mask1 = m_ContainerRectTrans.anchoredPosition.y - ItemRefHeight;
        float mask2 = m_ContainerRectTrans.anchoredPosition.y + m_ScrollRectTrans.sizeDelta.y + ItemRefHeight;

        m_list1.Clear();
        m_list2.Clear();
        m_list3.Clear();

        for (int i = 0; i < m_TotalCount; ++i)
        {
            var itemData = m_arrItemData[i];
            var top = itemData.poss.y - itemData.halfSize.y;
            var btm = itemData.poss.y + itemData.halfSize.y;
            if (btm >= mask1 && top <= mask2)
            {
                //内部
                m_list1.Add(i);
            }
            else
            {
                //外部
                m_list2.Add(i);
            }
        }
        //先移除不用的
        foreach (int index in m_list2)
        {
            var itemData = m_arrItemData[index];
            if (itemData.view == null)
                continue;
            _DestroyItem(itemData.view);
            itemData.view = null;
            m_arrItemData[index] = itemData;
        }
        bool refresh = false;
        //再新增要用的
        foreach (int index in m_list1)
        {
            var itemData = m_arrItemData[index];
            if (itemData.view != null)
                continue;
            var item  = _CreateItem();
            _UpdateItemData(item, index);
            var itemTrans = item.transform as RectTransform;
            LayoutRebuilder.ForceRebuildLayoutImmediate(itemTrans);
            var itemH = LayoutUtility.GetPreferredHeight(itemTrans);
            if (itemH != itemData.size.y)
            {
                itemData.size.y = itemH;
                itemData.halfSize.y = itemH * 0.5f;
                refresh = true;
            }
            itemData.view = item;
            m_arrItemData[index] = itemData;
            m_list3.Add(index);
        }
        var updatelist = m_list3;
        if (refresh)
        {
            _UpdateContainer();
            updatelist = m_list1;
        }
        foreach (int index in updatelist)
        {
            var itemData = m_arrItemData[index];
            if (itemData.view == null)
                continue;
            //Debug.Log($"pos:{itemData.poss} size:{itemData.size}");
            var itemTrans = itemData.view.transform as RectTransform;
            itemTrans.anchoredPosition = new Vector2(0f, m_ContainerRectTrans.sizeDelta.y * 0.5f - itemData.poss.y);
        }
    }

    //更新格子位置数据
    protected void _UpdateContainer()
    {
        var size = m_ContainerRectTrans.sizeDelta;
        var oldSize = size;
        size.y = 0f;
        for (int i = 0; i < m_TotalCount; ++i)
        {
            var itemData = m_arrItemData[i];
            if (i == 0)
            {
                itemData.poss.y = itemData.halfSize.y;
            }
            else
            {
                var tempData = m_arrItemData[i - 1];
                itemData.poss.y = tempData.poss.y + tempData.halfSize.y + itemData.halfSize.y;
            }
            m_arrItemData[i] = itemData;
            size.y += itemData.size.y;
        }
        m_ContainerRectTrans.sizeDelta = size;

        if (m_reverseExpand)
        {
            var pos = m_ContainerRectTrans.anchoredPosition;
            pos.y += size.y - oldSize.y;
            m_ContainerRectTrans.anchoredPosition = pos;
            m_ScrollRect.ChangeStartPosition(new Vector2(0, size.y - oldSize.y));
        }
    }
}
