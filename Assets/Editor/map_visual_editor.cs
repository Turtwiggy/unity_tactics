#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace Wiggy
{
  [CustomEditor(typeof(map_visual_manager))]
  public class map_visual_editor : Editor
  {
    public override void OnInspectorGUI()
    {
      map_visual_manager mvm = target as map_visual_manager;

      if (DrawDefaultInspector())
      {
        // map.GenerateMap();
      }

      if (GUILayout.Button("Refresh Visuals"))
        mvm.RefreshVisuals();
    }
  }
}

#endif