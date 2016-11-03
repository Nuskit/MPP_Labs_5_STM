using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labs_5_STM
{

  public class StmTransaction : IStmCompositeTransaction
  { 
    private List<IStmComposite> components = new List<IStmComposite>();

    public void TryAddComponent(IStmComposite sourse)
    {
      if (!components.Contains(sourse))
        components.Add(sourse);
    }

    public void Begin()
    {
      //may be this create new Pull
    }

    public void Commit()
    {
      foreach (var component in components)
        if (component is IStmRef)
          Stm.RemoveSavedState(this, component as IStmRef);
        else if (component is IStmTransaction)
          (component as IStmTransaction).Commit();
    }

    public void Rollback()
    {
      Stm.RollBackTransaction(this);
    }

    public bool IsCorrectnessTransaction()
    {
      foreach (var component in components)
        if (!component.IsCorrectnessTransaction())
          return false;
      return true;
    }
  }
}
