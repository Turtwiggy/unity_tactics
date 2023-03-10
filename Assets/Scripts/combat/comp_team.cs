using UnityEngine;

public enum Team
{
  PLAYER,
  ENEMY,
  NEUTRAL,
}

[System.Serializable]
public class comp_team : MonoBehaviour
{
  public Team team;
}