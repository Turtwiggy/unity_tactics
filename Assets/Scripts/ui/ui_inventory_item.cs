using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace Wiggy
{
  public class ui_inventory_item : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
  {
    public Transform parent_slot;
    public Vector2 offset;
    private ui_inventory inventory;

    void Start()
    {
      inventory = FindObjectOfType<ui_inventory>();
    }

    public void SetSlotToReturnToOnFinishDrag(Transform t)
    {
      parent_slot = t;
    }

    public void OnBeginDrag(PointerEventData evt)
    {
      var xy = new Vector2(transform.position.x, transform.position.y);
      offset = xy - evt.pressPosition;
    }

    public void OnDrag(PointerEventData evt)
    {
      transform.position = evt.position + offset;
    }

    public void OnEndDrag(PointerEventData evt)
    {
      List<RaycastResult> results = new();
      EventSystem.current.RaycastAll(evt, results);
      foreach (var r in results)
      {
        var has_zone = r.gameObject.TryGetComponent<inventory_drop_zone>(out var drop_zone);
        if (has_zone)
        {
          Debug.Log($"Dropped on: {drop_zone.type}");
          inventory.MoveItem(this, drop_zone);
          break;
        }
      }

      transform.position = parent_slot.transform.position;
      offset = Vector2.zero;
    }
  }
}