using System.Linq;
using System.Collections.Generic;

// This is heavily based off of:
// https://austinmorlan.com/posts/entity_component_system/
// Note: should probably replace dictionaries

namespace Wiggy
{
  using Entity = System.Int32;
  using ComponentType = System.Int32;

  public class Signature
  {
    public int data { get; private set; }

    public void Set(int bit_position, bool bit = true)
    {
      if (bit)
        data |= 1 << bit_position;
      else
        data &= ~(1 << bit_position);
    }

    public void Reset()
    {
      data = 0;
    }
  }

  interface IComponentArray
  {
    public abstract void EntityDestroyed(Entity e);
  }

  public class ComponentArray<T> : IComponentArray
  {
    // Packed array of components
    private T[] component_array;

    // Size of valid entries in array
    public int size { get; private set; }

    private Dictionary<Entity, int> entity_to_index_map = new();
    private Dictionary<Entity, int> index_to_entity_map = new();

    public ComponentArray(int max_entities)
    {
      component_array = new T[max_entities];
    }

    public void Insert(Entity e, T component)
    {
      var new_index = size;

      entity_to_index_map[e] = new_index;
      index_to_entity_map[new_index] = e;
      component_array[new_index] = component;

      ++size;
    }

    public void Remove(Entity e)
    {
      var index_of_removed_entity = entity_to_index_map[e];
      var index_of_last_element = size - 1;

      // keep it packed
      component_array[index_of_removed_entity] = component_array[index_of_last_element];

      // update the map to point to moved spot
      var entity_of_last_element = index_to_entity_map[index_of_last_element];
      entity_to_index_map[entity_of_last_element] = index_of_removed_entity;
      index_to_entity_map[index_of_removed_entity] = entity_of_last_element;

      entity_to_index_map.Remove(e);
      index_to_entity_map.Remove(index_of_last_element);

      --size;
    }

    public ref T Get(Entity e)
    {
      return ref component_array[e];
    }

    public void EntityDestroyed(Entity e)
    {
      Remove(e);
    }
  }

  public class EntityManager
  {
    // config 
    public int alive { get; private set; }

    // storage
    private readonly Queue<Entity> available_entities;
    private readonly Signature[] signatures;

    public EntityManager(int max_entities)
    {
      signatures = new Signature[max_entities];
      for (int i = 0; i < signatures.Length; i++)
        signatures[i] = new();
      available_entities = new(Enumerable.Range(0, max_entities));
    }

    public Entity Create()
    {
      var entity = available_entities.Dequeue();
      alive += 1;
      return entity;
    }

    public void Destroy(Entity e)
    {
      signatures[e].Reset();
      available_entities.Enqueue(e);
      alive--;
    }

    public void SetSignature(Entity e, Signature s)
    {
      signatures[e] = s;
    }

    public ref Signature GetSignature(Entity e)
    {
      return ref signatures[e];
    }
  }

  public class ComponentManager
  {
    Dictionary<string, ComponentType> component_types = new();
    Dictionary<string, IComponentArray> component_arrays = new();
    ComponentType next_component_type;
    private readonly int max_entities;

    public ComponentManager(int max_entities)
    {
      this.max_entities = max_entities;
    }

    public void RegisterComponent<T>()
    {
      var name = typeof(T).ToString();
      component_types[name] = next_component_type;
      component_arrays[name] = new ComponentArray<T>(max_entities);
      ++next_component_type;
    }

    public ComponentType GetComponentType<T>()
    {
      var name = typeof(T).ToString();
      return component_types[name];
    }

    public void AddComponent<T>(Entity e, T component)
    {
      GetComponentArray<T>().Insert(e, component);
    }

    public void RemoveComponent<T>(Entity e)
    {
      GetComponentArray<T>().Remove(e);
    }

    public ref T GetComponent<T>(Entity e)
    {
      return ref GetComponentArray<T>().Get(e);
    }

    public void EntityDestroyed(Entity e)
    {
      // Notify each component array that an entity has been destroyed
      for (int i = 0; i < component_arrays.Count; i++)
      {
        var (_, array) = component_arrays.ElementAt(i);
        array.EntityDestroyed(e);
      }
    }

    private ref ComponentArray<T> GetComponentArray<T>()
    {
      var name = typeof(T).ToString();
      ref var array = component_arrays[name];
      ref var tarray = array as ComponentArray<T>;
      return ref tarray;
    }
  }

  public class SystemManager
  {
    // Map from system type to a signature
    private Dictionary<string, Signature> signatures = new();

    // Map from system type to string pointer
    private Dictionary<string, ECSSystem> systems = new();

    public T RegisterSystem<T>() where T : ECSSystem, new()
    {
      var name = typeof(T).ToString();
      systems[name] = new T();
      return (T)systems[name];
    }

    public void SetSignature<T>(Signature s)
    {
      var name = typeof(T).ToString();
      signatures[name] = s;
    }

    public void EntityDestroyed(Entity e)
    {
      foreach (var (_, sys) in systems)
        sys.entities.Remove(e);
    }

    public void EntitySignatureChanged(Entity e, Signature entity_sig)
    {
      foreach (var (name, sys) in systems)
      {
        var sys_sig = signatures[name];

        // Entity signature matches system signature - insert into set
        if ((entity_sig.data & sys_sig.data) == sys_sig.data)
          sys.entities.Add(e);
        else
          sys.entities.Remove(e);
      }
    }
  }

  // All systems to inherit from this
  [System.Serializable]
  public abstract class ECSSystem
  {
    public HashSet<Entity> entities = new();
  }

  public class registry
  {
    private const int max_entities = 5000;
    private const int max_components = 32;
    public EntityManager entity_manager { get; private set; }
    public ComponentManager component_manager { get; private set; }
    public SystemManager system_manager { get; private set; }

    public registry()
    {
      entity_manager = new(max_entities);
      component_manager = new(max_components);
      system_manager = new();
    }

    //
    // Entity Methods
    //

    public Entity Create()
    {
      return entity_manager.Create();
    }

    public void Destroy(Entity e)
    {
      entity_manager.Destroy(e);
      component_manager.EntityDestroyed(e);
      system_manager.EntityDestroyed(e);
    }

    //
    // Component Methods
    //

    public void RegisterComponent<T>()
    {
      component_manager.RegisterComponent<T>();
    }

    public void AddComponent<T>(Entity e, T component)
    {
      component_manager.AddComponent<T>(e, component);

      Signature signature = entity_manager.GetSignature(e);
      var bit_position = component_manager.GetComponentType<T>();
      signature.Set(bit_position, true);
      entity_manager.SetSignature(e, signature);

      system_manager.EntitySignatureChanged(e, signature);
    }

    public void RemoveComponent<T>(Entity e)
    {
      component_manager.RemoveComponent<T>(e);

      Signature signature = entity_manager.GetSignature(e);
      var bit_position = component_manager.GetComponentType<T>();
      signature.Set(bit_position, false);
      entity_manager.SetSignature(e, signature);

      system_manager.EntitySignatureChanged(e, signature);
    }

    public ref T GetComponent<T>(Entity e)
    {
      return ref component_manager.GetComponent<T>(e);
    }

    public ComponentType GetComponentType<T>()
    {
      return component_manager.GetComponentType<T>();
    }


    //
    // System Methods
    //
    public T RegisterSystem<T>() where T : ECSSystem, new()
    {
      return system_manager.RegisterSystem<T>();
    }
    public void SetSystemSignature<T>(Signature s) where T : ECSSystem
    {
      system_manager.SetSignature<T>(s);
    }
  }
}