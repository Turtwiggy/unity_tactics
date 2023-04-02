using System.Collections.Generic;

namespace Wiggy
{
  public class Indexable<T>
  {
    public string display_name;
    public List<T> list;
    public int index;
    public bool enabled = true;

    public Indexable(string name, List<T> list, int index)
    {
      this.display_name = name;
      this.list = list;
      this.index = index;
    }
  }
}