using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollRectEx : ScrollRect
{
    public void ChangeStartPosition(Vector2 offset)
    {
        m_ContentStartPosition += offset;
    }
}
