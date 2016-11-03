using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labs_5_STM
{
  public enum StmOperationType
  {
    GET, SET
  };

  class StmRefSavedState
  {
    public StmOperationType OperationType { get; set; }
    public IStmRef SaveStmRef { get; set; }
    public IStmTransaction ParentTransaction { get; set; }

    public StmRefSavedState(IStmRef saveStmRef,IStmTransaction parentTransaction, StmOperationType operationType)
    {
      this.OperationType = operationType;
      this.ParentTransaction = parentTransaction;
      this.SaveStmRef = saveStmRef.Clone() as IStmRef;
    }
  }

  public class StmRef<T>: IStmCompositeRef
    where T:struct
  {
    private T value;
    public T Value
    {
      get
      {
        Stm.RegisterStmOperation(this, StmOperationType.GET);
        return value;
      }
      set
      {
        //don't change object
        if (!this.value.Equals(value))
          Stm.RegisterStmOperation(this, StmOperationType.SET);

        this.value = value;
      }
    }

    public StmRef(T value)
    {
      this.Value = value;
    }

    public StmRef() { }

    public object Clone()
    {
      return new StmRef<T>(Value);
    }

    public bool IsCorrectnessTransaction()
    {
      return Stm.IsCorrectRefState(this);
    }
  }
}
