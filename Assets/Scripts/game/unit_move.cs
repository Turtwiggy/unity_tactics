// using System.Collections.Generic;
// using System.Threading.Tasks;
// using UnityEngine;

// namespace Wiggy
// {
//   class unit_move : MonoBehaviour
//   {
//     public GameObject cursor_available_prefab;
//     private List<GameObject> cursor_available = new();

//     public async Task Move(map_manager map, int from_index, int to_index, int width, int size)
//     {
//       var path = a_star.generate_direct(map.cells, from_index, to_index, width);
//       if (path == null)
//       {
//         Debug.Log("no path...");
//         return;
//       }
//       DisplayPathUI(path, size);

//       GameObject go = map.gos[from_index];
//       map.gos[from_index] = null;
//       map.cells[from_index].path_cost = 0;
//       map.gos[to_index] = go;
//       map.cells[to_index].path_cost = -1;

//       await Animate.AlongPath(go, path, width, size);

//       // TODO: improve this
//       foreach (GameObject ca in cursor_available)
//         Destroy(ca);
//     }

//     public void DisplayPathUI(cell[] paths, int size)
//     {
//       for (int i = 0; i < paths.Length; i++)
//       {
//         var pos = Grid.GridSpaceToWorldSpace(paths[i].pos, size);
//         var ca = Instantiate(cursor_available_prefab);
//         ca.transform.position = pos;
//         ca.SetActive(true);
//         cursor_available.Add(ca);
//       }
//     }

//     // private void HandleShowArea(int width, int size)
//     //   var avilable = a_star.generate_area(map_manager.cells, from, 2, width);

//   }
// } // namespace Wiggy