using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.IO;

namespace Wiggy
{
  public class persistent_unit_info : MonoBehaviour
  {
    private static persistent_unit_info singleton;

    private unit_manager unit_manager = new();
    private string to;

    // should probably be called by scene's Main()
    void Start()
    {
      if (singleton == null)
      {
        singleton = this;
        DontDestroyOnLoad(this);
      }
      else
        Destroy(this.gameObject);
      Debug.Log("persistent_unit_info start() called");
      to = Path.Combine(Application.persistentDataPath, "units.txt");
      Debug.Log($"save path: {to}");

      // Deleting save data!!
      if (File.Exists(to))
        File.Delete(to);
    }

    void Update()
    {
      if (Keyboard.current.oKey.wasPressedThisFrame)
      {
        Debug.Log("saving...");
        unit_manager.Save(to);
        Debug.Log($"units: {unit_manager.units.Count}");
      }

      if (Keyboard.current.pKey.wasPressedThisFrame)
      {
        Debug.Log("loading...");
        unit_manager.Load(to);
        Debug.Log($"units: {unit_manager.units.Count}");
      }

    }
  }

  public class unit_manager
  {
    public List<Entity> units = new();

    public void Save(string path)
    {
      Debug.Log("save...");
      io.save_to_disk<Entity>(path, new Entity(-1));
    }

    public void Load(string path)
    {
      // load from api?
      // load from disk? (easy to cheat game, which is fine)
      var from_data = io.load_from_disk<Entity>(path);
      if (from_data.IsSet)
      {
        var data = from_data.Data;
        Debug.Log($"loaded id: {data.id}");
      }
    }

    public void Delete()
    {

    }


    public void Recruit(Wiggy.registry ecs)
    {
      // Entities.create_player(
      //   ecs,
      //   Vector2Int.zero,
      //   "name",
      //   new Optional<GameObject>(),
      //   new Optional<GameObject>()
      // );
    }


  }

}
