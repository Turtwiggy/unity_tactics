using UnityEngine;

namespace Wiggy
{
  public class unit_select : MonoBehaviour
  {
    private map_manager map;
    public GameObject cursor_selected;
    public int from_index { get; private set; }

    public void DoStart()
    {
      map = FindObjectOfType<map_manager>();
      ClearSelection();
    }

    public void UpdateSelectedCursorUI(int size)
    {
      // if something is selected, show the cursor
      cursor_selected.SetActive(from_index != -1);

      // if something is selected, move cursor position
      if (from_index != -1)
      {
        var pos = Grid.IndexToPos(from_index, map.width, map.height);
        var world_space = Grid.GridSpaceToWorldSpace(pos, size);
        cursor_selected.transform.position = world_space;
      }
    }

    public void Select(int index)
    {
      if (from_index != -1)
        return;
      from_index = index;
    }

    public void ClearSelection()
    {
      from_index = -1;
    }
  }
} // namespace Wiggy