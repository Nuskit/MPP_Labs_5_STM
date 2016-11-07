using System;

namespace Labs_5_STM
{
  public class StmRefSavedState
  {
    public IStmRef SaveStmRef { get; private set; }
    public IStmRef NextStmRef { get; private set; }
    
    public StmRefSavedState(IStmRef saveStmRef, IStmRef nextStmRef)
    {
      this.NextStmRef = nextStmRef ?? saveStmRef;
      this.SaveStmRef = saveStmRef;
    }

    public override string ToString()
    {
      return String.Format("Save {0}, New {1}", SaveStmRef.ToString(), NextStmRef.ToString());
    }
  }

  public class StmUnmanagedRef<T>: IStmRef
  {
    protected T value;

    protected StmUnmanagedRef(T value)
    {
      this.value = value;
    }

    public T Get()
    {
      var oldValue = this.StmUnmanagedClone() as StmUnmanagedRef<T>;
      Stm.RegisterStmOperation(this, oldValue);
      return oldValue.value;
    }

    public void Set(T value)
    {
      var oldValue = this.StmUnmanagedClone() as StmUnmanagedRef<T>;
      Stm.RegisterStmOperation(this, oldValue, new StmUnmanagedRef<T>(value));
      this.value = value;
    }

    public void SetAsObject(object value)
    {
      if (value is T)
        this.value = (T)value;
      else
        throw new InvalidCastException("Error value type in SetAsObject");
    }

    public object GetAsObject()
    {
      return value;
    }

    public override bool Equals(object value)
    {
      if (ReferenceEquals(this, value))
        return true;
      if (!(value is StmUnmanagedRef<T>))
        return false;

      StmUnmanagedRef<T> stmRef = value as StmUnmanagedRef<T>;
      if (!this.value.Equals(stmRef.value))
        return false;
      return true;
    }

    public override string ToString()
    {
      return String.Format("value {0}", value.ToString());
    }

    public object StmUnmanagedClone()
    {
      return new StmUnmanagedRef<T>(value);
    }
  }

  public class StmRef<T> : StmUnmanagedRef<T>, IDisposable
  //where T : struct
  {
    public StmRef(T value = default(T)) : base(value)
    {
      Stm.RegisterRef(this);
    }

    private bool disposedValue = false;

    protected virtual void Dispose(bool disposing)
    {
      if (!disposing)
      {
        Stm.UnregisterRef(this);
        disposedValue = true;
      }
    }

    ~StmRef()
    {
      Dispose(false);
    }

    public void Dispose()
    {
      Dispose(disposedValue);
      GC.SuppressFinalize(this);
    }
  }
}
