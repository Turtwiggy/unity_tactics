using UnityEngine;

namespace Wiggy
{
  class ui_inventory : MonoBehaviour
  {
    public GameObject slot_holder;
    public GameObject item_holder;

    public GameObject slot_prefab;
    public GameObject item_prefab;
    public GameObject no_item_prefab;

    private GameObject[] slots;
    private ui_inventory_item[] items;

    void Start()
    {
      var inventory_slots_count = 40;

      // Spawn slots
      slots = new GameObject[inventory_slots_count];
      for (int i = 0; i < inventory_slots_count; i++)
      {
        slots[i] = Instantiate(slot_prefab, Vector3.zero, Quaternion.identity, slot_holder.transform);
        foreach (Transform child in slots[i].transform)
          Destroy(child.gameObject);
      }

      // Spawn items
      items = new ui_inventory_item[inventory_slots_count]; // max
      for (int i = 0; i < inventory_slots_count; i++)
      {
        if (i == 0)
        {
          var go = Instantiate(item_prefab, Vector3.zero, Quaternion.identity, item_holder.transform);
          items[i] = go.GetComponent<ui_inventory_item>();
          items[i].enabled = true;
          items[i].SetSlotToReturnToOnFinishDrag(slots[i].transform);
          items[i].transform.position = slots[i].transform.position;
        }
      }

      // Remove Prefabs
      Destroy(slot_prefab);
      Destroy(item_prefab);
      Destroy(no_item_prefab);
    }

    public void MoveItem(ui_inventory_item item, inventory_drop_zone drop)
    {
      for (int i = 0; i < items.Length; i++)
      {
        if (item == items[i])
        {
          item.SetSlotToReturnToOnFinishDrag(drop.transform);
          item.transform.position = drop.transform.position;
          break;
        }
      }
    }
  }
}