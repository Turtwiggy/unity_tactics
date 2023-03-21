// using UnityEngine;

// namespace Wiggy
// {
//   public class unit_select : MonoBehaviour
//   {
//     private map_manager map;

//     public GameObject cursor_selected;
//     public int from_index { get; private set; }

//     public void DoStart()
//     {
//       map = FindObjectOfType<map_manager>();
//       ClearSelection();
//     }

//     public void UpdateUI(int size)
//     {
//       // if something is selected, show the cursor
//       cursor_selected.SetActive(from_index != -1);

//       // if something is selected, move cursor position
//       if (from_index != -1)
//       {
//         var cell = map.cells[from_index];
//         var world_space = Grid.GridSpaceToWorldSpace(cell.pos, size);
//         cursor_selected.transform.position = world_space;
//       }
//     }

//     public void Select(Vector2Int xy, int width)
//     {
//       if (from_index != -1)
//         return;

//       var index = Grid.GetIndex(xy, width);
//       var unit = map.gos[index];
//       if (unit && unit.GetComponent<character_stats>())
//         from_index = index;
//     }

//     public void ClearSelection()
//     {
//       from_index = -1;
//     }

//   }
// } // namespace Wiggy