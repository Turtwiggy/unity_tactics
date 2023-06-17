#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace Wiggy
{
  [CustomEditor(typeof(main))]
  public class map_editor : Editor
  {
    public override void OnInspectorGUI()
    {
      main m = target as main;

      if (DrawDefaultInspector())
      {
        // map.GenerateMap();
      }

      if (GUILayout.Button("Generate Map"))
      {
        m.StartEditor();
      }
    }
  }
}

#endif