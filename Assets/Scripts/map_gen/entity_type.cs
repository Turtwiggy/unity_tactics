
namespace Wiggy
{
  [System.Serializable]
  public enum EntityType
  {
    empty,

    actor_player,
    actor_enemy,
    actor_barrel,
    // actor_bat,
    // actor_troll,
    // actor_shopkeeper,

    tile_type_wall,
    tile_type_floor,
    tile_type_exit,
    tile_type_door,
    tile_type_trap,

    // offensive equipment
    pistol,
    sniper,
    shotgun,
    rifle,
    sword,
    grenade,

    // shield,

    // misc
    keycard,

    // consumable
    // potion,
    // scroll_damage_nearest,
    // scroll_damage_selected_on_grid,
  };
}