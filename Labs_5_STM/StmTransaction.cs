using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labs_5_STM
{

  public class StmTransaction : IStmTransaction
  {
    private Dictionary<IStmRef, Queue<StmRefSavedState>> components = new Dictionary<IStmRef, Queue<StmRefSavedState>>();

    public void TryAddComponent(IStmRef stmRef, StmRefSavedState stmRefSavedState)
    {
      Queue<StmRefSavedState> refSavedState;
      if (components.TryGetValue(stmRef, out refSavedState))
        refSavedState.Enqueue(stmRefSavedState);
      else
        components.Add(stmRef, new Queue<StmRefSavedState>(new[] { stmRefSavedState }));
    }

    public void Begin()
    {
      Stm.NotifyBeginTransaction(this);
    }

    public void Commit()
    {
      Stm.NotifyEndTransaction(components.Select(x => new KeyValuePair<IStmRef, StmRefSavedState>(x.Key, GetCommitValue(x.Key))));
      ClearComponents();
    }

    private void ClearComponents()
    {
      components.Clear();
    }

    private StmRefSavedState GetCommitValue(IStmRef key)
    {
      var keyValues = components.Single(x => x.Key.Equals(key)).Value;
      return new StmRefSavedState(keyValues.First().SaveStmRef, keyValues.Last().NextStmRef);
    }

    public void Rollback()
    {
      foreach (var component in components)
        component.Key.SetAsObject(component.Value);
      ClearComponents();
    }

    public bool IsCorrectnessTransaction()
    {
      foreach (var component in components.Values)
        if (!isCorrectStmRef(component))
          return false;
      return true;
    }

    private bool isCorrectStmRef(Queue<StmRefSavedState> refSavedStates)
    {
      StmRefSavedState lastSavedState = refSavedStates.First();
      foreach (var currentSavedState in refSavedStates.Skip(1))
      {
        if (currentSavedState.SaveStmRef.Equals(lastSavedState.NextStmRef))
          lastSavedState = currentSavedState;
        else
          return false;
      }
      return true;
    }
  }
}
