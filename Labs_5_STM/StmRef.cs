using System;

namespace Labs_5_STM
{
  public class StmRefSavedState
  {
    public IStmRef SaveStmRef { get; set; }
    public IStmRef NextStmRef { get; set; }

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

  public class StmRef<T> : IStmRef
  //where T : struct
  {
    private T value;

    public StmRef(T value = default(T))
    {
      this.value = value;
    }

    public object Clone()
    {
      return new StmRef<T>(value);
    }

    public T Get()
    {
      var oldValue = this.Clone() as StmRef<T>;
      Stm.RegisterStmOperation(this, oldValue);
      return oldValue.value;
    }

    public void Set(T value)
    {
      var oldValue = this.Clone() as StmRef<T>;
      Stm.RegisterStmOperation(this, oldValue, new StmRef<T>(value));
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
      if (!(value is StmRef<T>))
        return false;

      StmRef<T> stmRef = value as StmRef<T>;
      if (!this.value.Equals(stmRef.value))
        return false;
      return true;
    }

    public override string ToString()
    {
      return String.Format("value {0}", value.ToString());
    }
  }
}
