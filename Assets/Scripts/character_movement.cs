using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace Wiggy
{
  class character_movement : MonoBehaviour
  {
    public map_manager map_manager;

    public int from_index = -1;
    public GameObject cursor_selected;
    public GameObject cursor_available_prefab;
    private List<GameObject> instantiated_cursor_available = new();

    private bool busy = false;

    private void Start()
    {
      map_manager = FindObjectOfType<map_manager>();
    }

    private void Update()
    {
      // if something is selected, show the cursor
      cursor_selected.SetActive(from_index != -1);
    }

    public void SelectUnit(Vector2Int xy, int width, int size)
    {
      Select(xy, width, size);
    }

    public async Task MoveUnit(Vector2Int xy, int width, int size)
    {
      if (from_index == -1)
        return;

      if (busy)
        return;
      busy = true;

      await PathTo(xy, width, size);

      busy = false;
      from_index = -1;
    }


    private void Select(Vector2Int xy, int width, int size)
    {
      if (from_index != -1)
      {
        Debug.Log("You've already selected something...");
        return;
      }

      int index = Grid.GetIndex(xy, width);
      var cell = map_manager.cells[index];
      var go = map_manager.gos[index];
      var hcs = map_manager.high_cover_spots[index];

      // select only characters
      if (go != null && go.GetComponent<character_handler>())
      {
        var world_space = Grid.GridSpaceToWorldSpace(cell.pos, size);
        cursor_selected.transform.position = world_space;
        from_index = index;
      }
      else
        from_index = -1;

      Debug.Log("index is now..." + index);
    }

    private async Task PathTo(Vector2Int dest, int width, int size)
    {
      // from: the lmb clicked cell
      var from = map_manager.cells[from_index];
      // to: the currently hovered cell
      var to = map_manager.cells[Grid.GetIndex(dest, width)];
      var paths = a_star.generate_direct(map_manager.cells, from, to, width);

      if (paths == null)
      {
        Debug.Log("no path...");
        return;
      }

      for (int i = 0; i < paths.Length; i++)
      {
        var pos = Grid.GridSpaceToWorldSpace(paths[i].pos, size);
        var cursor_available = Instantiate(cursor_available_prefab);
        cursor_available.transform.position = pos;
        cursor_available.SetActive(true);
        instantiated_cursor_available.Add(cursor_available);
      }

      await AnimateUnitAlongPath(paths, width, size);

      // TODO: improve this
      foreach (GameObject asdf in instantiated_cursor_available)
        Destroy(asdf);
    }

    private async Task AnimateUnitAlongPath(cell[] path, int width, int size)
    {
      if (path.Length <= 1)
        return;
      var from = path[0];
      var to = path[^1];

      int from_index = Grid.GetIndex(from.pos, width);
      int to_index = Grid.GetIndex(to.pos, width);
      GameObject go = map_manager.gos[from_index];
      map_manager.gos[from_index] = null;
      map_manager.cells[from_index].path_cost = 0;
      map_manager.gos[to_index] = go;
      map_manager.cells[to_index].path_cost = -1;

      int nodes = path.Length;
      int prev_index = 0;
      int next_index = 0;
      float percent = 0.0f;
      float percentage_amount = 1 / ((float)nodes - 1);

      const float seconds_per_move = 0.5f;
      const float EPSILON = 0.001f;
      float move_speed_seconds = seconds_per_move * nodes;

      while (1f - percent > EPSILON)
      {
        percent += Time.deltaTime / move_speed_seconds;
        percent = Mathf.Clamp01(percent);

        // Take the percent (0-1) and convert it to an index (0 to nodes-1)
        prev_index = (int)Mathf.Lerp(0, nodes - 1, percent); // rounds down e.g. 0.9 should be 0
        prev_index = Mathf.Clamp(prev_index, 0, nodes - 1);
        next_index = Mathf.Clamp(prev_index + 1, 0, nodes - 1);

        // Convert index to percentage amount (1/nodes-1)
        // e.g. 1/3 = 0.333
        // <0.333 = 0-1 convert 0.00-0.32 to (0-1)
        // <0.666 = 1-2 convert 0.33-0.65 to (0-1)
        // <0.999 = 2-3 convert 0.66-0.99 to (0-1)
        float relative_percentage_amount = (percent / percentage_amount) - prev_index;
        float scaled_percentage_amount = Mathf.Clamp(relative_percentage_amount, 0.0f, 1.0f);

        var this_cell = path[prev_index];
        var next_cell = path[next_index];

        var a = Grid.GridSpaceToWorldSpace(this_cell.pos, size);
        var b = Grid.GridSpaceToWorldSpace(next_cell.pos, size);
        go.transform.localPosition = Vector3.Lerp(a, b, scaled_percentage_amount);

        // Rotation?
        //  var dir = (next_cell.transform.position - this_cell.transform.position).normalized;
        //  if (dir.sqrMagnitude > EPSILON)
        //    unit.transform.localRotation = Quaternion.LookRotation(dir);

        await Task.Yield();
      }

      // Done Move
    }

    private void HandleShowArea(int width, int size)
    {
      // from: the lmb clicked cell
      var from = map_manager.cells[from_index];
      var avilable = a_star.generate_area(map_manager.cells, from, 2, width);
      // TODO: improve this
      foreach (GameObject go in instantiated_cursor_available)
        Destroy(go);
      for (int i = 0; i < avilable.Length; i++)
      {
        var pos = Grid.GridSpaceToWorldSpace(avilable[i].pos, size);
        var cursor_available = Instantiate(cursor_available_prefab);
        cursor_available.transform.position = pos;
        cursor_available.SetActive(true);
        instantiated_cursor_available.Add(cursor_available);
      }
    }

  }

} // namespace Wiggy