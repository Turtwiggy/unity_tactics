using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Wiggy
{
  public class DisplayInventorySystem : ECSSystem
  {
    private GameObject item_holder;
    private GameObject item_row_prefab;
    private List<Button> buttons = new();
    private SelectSystem select_system;

    public override void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<InBackpackComponent>());
      ecs.SetSystemSignature<DisplayInventorySystem>(s);
    }

    public void Start(Wiggy.registry ecs, SelectSystem select_system, GameObject item_holder, GameObject item_row_prefab)
    {
      this.item_holder = item_holder;
      this.item_row_prefab = item_row_prefab;
      this.item_row_prefab.SetActive(false);
      this.select_system = select_system;
    }

    public void Update(Wiggy.registry ecs)
    {
      // Does the intenvory have more items?
      bool need_refresh = entities.Count != item_holder.transform.childCount;
      if (!need_refresh)
        return;

      // Clear all listeners
      foreach (var btn in buttons)
        btn.onClick.RemoveAllListeners();
      buttons.Clear();

      // Destroy all rows
      foreach (Transform child in item_holder.transform)
        GameObject.Destroy(child.gameObject);

      // Instantiate all rows
      foreach (var e in entities)
      {
        // Display Item
        var obj = GameObject.Instantiate(item_row_prefab, Vector3.zero, Quaternion.identity, item_holder.transform);
        obj.SetActive(true);

        // Text
        var tmp = obj.GetComponentInChildren<table_row_label_tag>().GetComponent<TextMeshProUGUI>();
        tmp.SetText(ecs.GetComponent<TagComponent>(e).name);

        // Use Button
        var btn = obj.GetComponentInChildren<table_row_button_tag>().GetComponent<Button>();
        btn.onClick.AddListener(() =>
        {
          Debug.Log($"wants to use item: {e.id}");

          if (!select_system.HasAnySelected())
          {
            Debug.Log("Cant use item -- nothing selected!");
            return;
          }
          var selected = select_system.GetSelected();

          // Attach request directly to item
          // Warning: multiple attach = bad
          WantsToUse use = new();
          use.targets = new() { selected };
          ecs.AddComponent(e, use);
        });
        buttons.Add(btn);
      }
    }
  }
}