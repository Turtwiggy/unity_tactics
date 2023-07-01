using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Wiggy
{
  public static class io
  {
    public static void save_to_disk<T>(string path, T data)
    {
      string json = JsonUtility.ToJson(data);
      using var writer = new StreamWriter(path);
      writer.WriteLine(json);
    }

    public static Optional<T> load_from_disk<T>(string path)
    {
      try
      {
        var json = File.ReadAllText(path);
        T obj = JsonUtility.FromJson<T>(json);
        return new Optional<T>(obj);
      }
      catch
      {
        Debug.Log("(load_from_disk) no data at path!");
        return new Optional<T>();
      }
    }
  }
}