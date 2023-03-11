#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace Wiggy
{
  [CustomEditor(typeof(map_manager))]
  public class map_editor : Editor
  {

    public override void OnInspectorGUI()
    {
      map_manager map = target as map_manager;

      if (DrawDefaultInspector())
      {
        // map.GenerateMap();
      }

      if (GUILayout.Button("Generate Map"))
        map.GenerateMap();
    }
  }
}

#endif