using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wiggy
{
  public class IsDeadSystem : ECSSystem
  {
    private map_manager map;
    private GameObject vfx_death;

    public override void SetSignature(Wiggy.registry ecs)
    {
      Signature s = new();
      s.Set(ecs.GetComponentType<IsDeadComponent>());
      ecs.SetSystemSignature<IsDeadSystem>(s);
    }

    public void Start(Wiggy.registry ecs, GameObject vfx_death)
    {
      map = GameObject.FindObjectOfType<map_manager>();
      this.vfx_death = vfx_death;
    }

    public void Update(Wiggy.registry ecs)
    {
      foreach (var e in entities.ToArray())
      {
        Debug.Log("something died!");

        var dead = ecs.GetComponent<IsDeadComponent>(e);
        var pos = ecs.GetComponent<GridPositionComponent>(e);

        // Does this unit explode?
        ExplodesOnDeath explode_default = default;
        ref var explodes = ref ecs.TryGetComponent(e, ref explode_default, out var unit_explodes);
        if (unit_explodes)
        {
          // Create an attack event around this unit
          var neighbours = a_star.square_neighbour_indicies_with_diagonals(pos.position.x, pos.position.y, map.width, map.height);
          for (int i = 0; i < neighbours.Length; i++)
          {
            var damage_idx = neighbours[i].Item2;

            var ents = map.entity_map[damage_idx].entities;
            foreach (var defender_entity in ents)
            {
              AttackEvent evt = new()
              {
                // HMM - this entity is now dead
                amount = new Optional<int>(10), // shuld not be hard coded
                from = new Optional<Entity>(),
                to = defender_entity
              };
              var ent = ecs.Create();
              ecs.AddComponent(ent, evt);
            }
          }
        }

        // TODO: Does this unit drop loot? 

        InstantiatedComponent instance_default = default;
        ref var instance = ref ecs.TryGetComponent(e, ref instance_default, out var has_instance);
        if (has_instance)
          Object.Destroy(instance.instance);

        // Remove units record
        var idx = Grid.GetIndex(pos.position.x, pos.position.y, map.width);
        map.entity_map[idx].entities.Remove(e);

        // Death Effects
        Entities.create_effect(ecs, pos.position, vfx_death, "Death Effect");

        // Remove ecs record
        ecs.Destroy(e);
      }
    }
  }
}