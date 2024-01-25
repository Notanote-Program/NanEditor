using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIdrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private Vector3 offset;
    public void OnBeginDrag(PointerEventData eventData)
    {
        this.GetComponent<RectTransform>().SetAsLastSibling();// render on top
        Vector3 pos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(this.GetComponent<RectTransform>(), eventData.position, eventData.enterEventCamera, out pos);
        offset = this.GetComponent<RectTransform>().position - pos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 pos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(this.GetComponent<RectTransform>(), eventData.position, eventData.enterEventCamera, out pos);
        Vector3 screenpos = Camera.main.WorldToScreenPoint(pos);
        if(screenpos.x > 0 && screenpos.y > 0 && screenpos.x < Screen.width && screenpos.y < Screen.height)
            this.GetComponent<RectTransform>().position = pos + offset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        this.GetComponent<RectTransform>().SetAsLastSibling();// render on top
    }
}
