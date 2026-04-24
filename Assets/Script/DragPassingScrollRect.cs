using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragPassingScrollRect : ScrollRect
{
    public ScrollRect parentScroll;

    public override void OnDrag(PointerEventData eventData)
    {
        parentScroll.OnDrag(eventData);
        base.OnDrag(eventData);
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        parentScroll.OnBeginDrag(eventData);
        base.OnBeginDrag(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        parentScroll.OnEndDrag(eventData);  
        base.OnEndDrag(eventData);
    }
}
