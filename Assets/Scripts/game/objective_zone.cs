// using System.Collections.Generic;
// using UnityEngine;

// namespace Wiggy
// {
//   public class objective_zone : MonoBehaviour
//   {
//     map_manager map;

//     public List<int> indicies { get; private set; }

//     SpriteRenderer sprite;

//     void Start()
//     {
//       map = FindObjectOfType<map_manager>();
//       sprite = GetComponentInChildren<SpriteRenderer>();
//       indicies = new();
//     }

//     public void SetColour(Color c)
//     {
//       sprite.color = c;
//     }

//     public void AddIndex(int index)
//     {
//       indicies.Add(index);
//     }

//     public Team WhichTeamControlsThisPoint()
//     {
//       int players = 0;
//       int enemies = 0;

//       foreach (int idx in indicies)
//       {
//         var ent = map.gos[idx];
//         if (ent != null)
//         {
//           var team = ent.GetComponent<comp_team>().team;
//           if (team == Team.ENEMY)
//             enemies++;
//           if (team == Team.PLAYER)
//             players++;
//         }
//       }

//       if (players > enemies)
//         return Team.PLAYER;
//       if (enemies > players)
//         return Team.ENEMY;
//       return Team.NEUTRAL;

//     }

//   }
// }
