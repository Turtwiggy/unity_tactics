using System.Collections;
using System.Collections.Specialized;
using UnityEngine;

namespace Wiggy
{
  // Tag Components

  public struct PlayerComponent
  {
  };

  // Other

  public struct GridPositionComponent
  {
    public Vector2Int position;
  };

  // Helpers

  public struct ToBeInstantiatedComponent
  {
    public GameObject prefab;
    public string name;
  };

  public struct InstantiatedComponent
  {
    public GameObject instance;
  };

};