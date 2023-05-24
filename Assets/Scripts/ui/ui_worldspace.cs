// using UnityEngine;
// using TMPro;

// public static class util_worldspace_ui
// {
//   public static TextMeshProUGUI GenerateWorldSpaceCanvasAbove(
//       Transform parent,
//       string text = "Default Text!",
//       float heightAboveGameObject = 1f,
//       bool rotate_to_camera = true)
//   {
//     GameObject go = new GameObject();
//     go.name = "Worldspace UI";

//     Canvas c = go.AddComponent<Canvas>();
//     c.renderMode = RenderMode.WorldSpace;

//     RectTransform rt = c.GetComponent<RectTransform>();
//     rt.localScale = 0.002f * Vector3.one;

//     TextMeshProUGUI unitText = go.AddComponent<TextMeshProUGUI>();
//     unitText.SetText(text);
//     unitText.fontSize = 200f;
//     unitText.color = Color.white;
//     unitText.alignment = TextAlignmentOptions.Center;
//     unitText.alignment = TextAlignmentOptions.Midline;

//     go.transform.position = new Vector3(
//         parent.position.x,
//         parent.position.y + heightAboveGameObject,
//         parent.position.z);

//     go.transform.parent = parent;

//     float canvas_width = 800.0f;
//     float canvas_height = 800.0f;
//     rt.sizeDelta = new Vector2(canvas_width, canvas_height);

//     return unitText;
//   }
// }
