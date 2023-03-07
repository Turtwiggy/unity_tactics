using UnityEngine;

public enum Team
{
  PLAYER,
  ENEMY,
}

[System.Serializable]
public class comp_team : MonoBehaviour
{
  public Team team;
}