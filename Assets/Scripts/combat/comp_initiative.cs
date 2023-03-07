using UnityEngine;

[System.Serializable]
public class comp_initiative : MonoBehaviour
{
  public int initiative;

  public void Generate()
  {
    initiative = Random.Range(0, 20);
  }

}