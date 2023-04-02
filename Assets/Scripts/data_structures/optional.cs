namespace Wiggy
{
  public class Optional<T>
  {
    public bool IsSet { get; private set; }
    public T Data { get; private set; }

    public Optional()
    {
      IsSet = false;
    }

    public Optional(T val)
    {
      Data = val;
      IsSet = true;
    }

    public void Set(T t)
    {
      Data = t;
      IsSet = true;
    }

    public void Reset()
    {
      Data = default;
      IsSet = false;
    }
  }
}