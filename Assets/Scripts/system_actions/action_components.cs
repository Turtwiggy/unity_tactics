using UnityEngine;
using UnityEngine.Events;

namespace Wiggy
{
  using Entity = System.Int32;

  [System.Serializable]
  public struct AttackEvent
  {
    [SerializeField]
    public Entity from;
    [SerializeField]
    public Entity target;
  };

  [System.Serializable]
  public struct MoveEvent
  {
    [SerializeField]
    public int from_index;
    [SerializeField]
    public int to_index;
  };

}