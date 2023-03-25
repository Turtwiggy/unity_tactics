using System.Threading.Tasks;
using UnityEngine;

namespace Wiggy
{
  public class unit_manager
  {
    public static GameObject create_unit(GameObject[] units, GameObject prefab, Vector2Int gpos, map_manager map, string name)
    {
      var index = Grid.GetIndex(gpos, map.width);

      if (units[index] != null)
      {
        Debug.LogError("Unit exists in the array");
        return null;
      }

      var wpos = Grid.GridSpaceToWorldSpace(gpos, map.size);
      var obj = Object.Instantiate(prefab, wpos, Quaternion.identity);
      units[index] = obj;
      obj.name = name;
      return obj;
    }
  }
}