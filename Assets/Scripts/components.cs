using UnityEngine;

namespace Wiggy
{
  // Tag Components

  public struct PlayerComponent
  {
  };

  public struct CursorComponent
  {
  };

  public enum Team
  {
    PLAYER,
    ENEMY,
    NEUTRAL,
  };
  public struct TeamComponent
  {
    public Team team;
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