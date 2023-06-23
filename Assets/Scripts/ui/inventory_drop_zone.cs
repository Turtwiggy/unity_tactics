using UnityEngine;

[System.Serializable]
public enum UIInventorySlot
{
  WEAPON,
  ARMOUR,
  INVENTORY,
}

namespace Wiggy
{
  public class inventory_drop_zone : MonoBehaviour
  {
    public UIInventorySlot type;
  }
}