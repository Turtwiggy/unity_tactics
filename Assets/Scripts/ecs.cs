using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

// This is heavily based off of:
// https://austinmorlan.com/posts/entity_component_system/
// Note: should probably replace dictionaries

namespace Wiggy
{
  using ComponentType = System.Int32;

  public static class ECS_CONFIG
  {
    public static int max_entities = 4096;
    public static int max_registered_components = 64;
  };

  public struct Entity
  {
    public int id;

    public Entity(int id)
    {
      this.id = id;
    }
  };

  public class Signature
  {
    public bool[] data;
    private static bool[] comparer;

    public Signature()
    {
      data = new bool[ECS_CONFIG.max_registered_components];
      comparer = new bool[ECS_CONFIG.max_registered_components];
    }

    public void Set(int bit_position, bool bit = true)
    {
      data[bit_position] = bit;
    }

    public void Reset()
    {
      Array.Fill(data, false);
    }

    public bool[] And(bool[] other)
    {
      Array.Fill(comparer, false);
      for (int i = 0; i < data.Length; i++)
        comparer[i] = other[i] & data[i];
      return comparer;
    }
  }


  interface IComponentArray
  {
    public abstract void EntityDestroyed(Entity e);
  }

  public class ComponentArray<T> : IComponentArray
  {
    // Packed array of components
    public T[] component_array { get; private set; }

    // Size of valid entries in array
    public int size { get; private set; }

    public Dictionary<Entity, int> entity_to_index_map { get; private set; }
    public Dictionary<int, Entity> index_to_entity_map { get; private set; }

    public ComponentArray(int max_entities)
    {
      component_array = new T[max_entities];
      entity_to_index_map = new();
      index_to_entity_map = new();
    }

    public void Insert(Entity e, T component)
    {
      int new_index = size;

      if (new_index >= component_array.Length)
        UnityEngine.Debug.LogError("Component array out of space!");

      entity_to_index_map[e] = new_index;
      index_to_entity_map[new_index] = e;
      component_array[new_index] = component;

      ++size;
    }

    public void Remove(Entity e)
    {
      var exists = entity_to_index_map.TryGetValue(e, out _);
      if (!exists)
      {
        // Debug.Log($"{temp.GetType()} does not exist for destroyed entity");
        return;
      }

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
      // var exists = entity_to_index_map.TryGetValue(e, out _);
      // if (!exists)
      //   UnityEngine.Debug.LogError($"{typeof(T)} does not exist");
      return ref component_array[entity_to_index_map[e]];
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

      available_entities = new Queue<Entity>();
      for (int i = 0; i < max_entities; i++)
        available_entities.Enqueue(new Entity(i));
    }

    public Entity Create()
    {
      var entity = available_entities.Dequeue();
      alive += 1;
      return entity;
    }

    public void Destroy(Entity e)
    {
      signatures[e.id].Reset();
      available_entities.Enqueue(e);
      alive--;
    }

    public void SetSignature(Entity e, Signature s)
    {
      // Convert signature to readable bits
      // System.Collections.BitArray b = new(new int[] { s.data });
      // byte[] bits = new byte[b.Count];
      // b.CopyTo(bits, 0);
      // string str = string.Join("", bits);
      // Debug.Log("entity " + e + " signature set to: " + str);

      signatures[e.id] = s;
    }

    public ref Signature GetSignature(Entity e)
    {
      return ref signatures[e.id];
    }
  }

  public class ComponentManager
  {
    private Dictionary<string, ComponentType> component_types = new();
    private Dictionary<string, IComponentArray> component_arrays = new();
    private ComponentType next_component_type_index;
    private readonly int max_entities;

    public ComponentManager(int max_entities)
    {
      this.max_entities = max_entities;
    }

    public void RegisterComponent<T>()
    {
      var name = typeof(T).ToString();
      component_types[name] = next_component_type_index;
      component_arrays[name] = new ComponentArray<T>(max_entities);
      ++next_component_type_index;
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
      var name = typeof(T).ToString();
      return ref (component_arrays[name] as ComponentArray<T>).Get(e);
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

    public ComponentArray<T> GetComponentArray<T>()
    {
      var name = typeof(T).ToString();
      return component_arrays[name] as ComponentArray<T>;
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
        if (Enumerable.SequenceEqual(entity_sig.And(sys_sig.data), sys_sig.data))
          sys.entities.Add(e);
        else
          sys.entities.Remove(e);
      }
    }
  }

  // All systems to inherit from this
  public abstract class ECSSystem
  {
    public HashSet<Entity> entities = new();

    public abstract void SetSignature(Wiggy.registry ecs);
  }

  public class registry
  {
    public EntityManager entity_manager { get; private set; }
    public ComponentManager component_manager { get; private set; }
    public SystemManager system_manager { get; private set; }

    public registry()
    {
      entity_manager = new(ECS_CONFIG.max_entities);
      component_manager = new(ECS_CONFIG.max_entities);
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

    public ref T TryGetComponent<T>(Entity e, ref T def, out bool success)
    {
      try
      {
        success = true;
        return ref component_manager.GetComponent<T>(e);
      }
      catch (System.Exception)
      {
        // Debug.Log($"entity did not have component of type: {def.GetType()}");
        // component did not exist for that entity
      }
      success = false;
      return ref def;
    }


    public Entity[] View<T>() where T : struct
    {
      ComponentArray<T> comp_array = component_manager.GetComponentArray<T>();

      Entity[] entities = new Entity[comp_array.size];
      for (int i = 0; i < comp_array.size; i++)
        entities[i] = comp_array.index_to_entity_map[i];

      return entities;
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