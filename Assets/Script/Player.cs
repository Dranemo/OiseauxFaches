using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class Player : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private Vector2 mouseInitialPos;
    private Vector2 mouseLastPos;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Player Start");
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        mouseInitialPos = eventData.position;
        Debug.Log("Start Drag");    
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("Drag");
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        mouseLastPos = eventData.position;

        Vector2 adj = new Vector2(mouseLastPos.x, mouseInitialPos.y);

        float angle = Vector2.Angle(adj, mouseLastPos);

        Debug.Log(angle);
    }
}
