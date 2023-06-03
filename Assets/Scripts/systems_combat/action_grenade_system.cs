using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wiggy
{
  public class GrenadeSystem : ECSSystem
  {
    private map_manager map;
    private GameObject vfx_grenade;

    public override void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<WantsToGrenade>());
      ecs.SetSystemSignature<GrenadeSystem>(s);
    }

    public void Start(Wiggy.registry ecs, GameObject vfx_grenade)
    {
      map = Object.FindObjectOfType<map_manager>();
      this.vfx_grenade = vfx_grenade;
    }

    public void Update(Wiggy.registry ecs)
    {
      foreach (var e in entities.ToArray())
      {
        var request = ecs.GetComponent<WantsToGrenade>(e);
        var action = new Grenade();
        if (!ActionHelpers.Valid<WantsToGrenade>(ecs, e, action))
        {
          Debug.Log("WantsToGrenade invalid action");
          ecs.RemoveComponent<WantsToGrenade>(e);
          continue;
        }

        Debug.Log("grenade!");
        var grenade_idx = request.index;
        var grenade_pos = Grid.IndexToPos(grenade_idx, map.width, map.height);
        var grenade = Entities.create_grenade(ecs, grenade_pos, "Grenade", new Optional<GameObject>(), new Optional<GameObject>());

        // Grenade Effects
        Entities.create_effect(ecs, grenade_pos, vfx_grenade, "Grenade Effect");

        // AOE damage
        List<Vector2Int> positions_to_take_damage = new();
        {
          // grenade spot takes damage
          positions_to_take_damage.Add(grenade_pos);

          // neighbours take damage, regardless of team
          var neighbours = a_star.square_neighbour_indicies_with_diagonals(grenade_pos.x, grenade_pos.y, map.width, map.height);
          for (int i = 0; i < neighbours.Length; i++)
            positions_to_take_damage.Add(Grid.IndexToPos(neighbours[i].Item2, map.width, map.height));
        }

        // deal damage
        for (int i = 0; i < positions_to_take_damage.Count; i++)
        {
          var damage_pos = positions_to_take_damage[i];
          var damage_idx = Grid.GetIndex(damage_pos, map.width);

          Debug.Log("spot taking damage");
          AttackEvent evt = new()
          {
            amount = new Optional<int>(),
            from = new Optional<Entity>(grenade),
            to = map.entity_map[damage_idx].entities
          };
          var ent = ecs.Create();
          ecs.AddComponent(ent, evt);
        }

        // Request is processed
        ActionHelpers.Complete<WantsToGrenade>(ecs, e, action);
      }
    }
  }

}