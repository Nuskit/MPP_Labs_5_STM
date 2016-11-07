using System.Collections.Generic;
using System.Linq;

namespace Labs_5_STM
{
  public class StmTransaction : IStmTransaction
  {
    private Dictionary<IStmRef, Queue<StmRefSavedState>> components;
    private bool isNestedTransaction;

    public StmTransaction(bool isNestedTransaction=false)
    {
      this.isNestedTransaction = isNestedTransaction;
      components = new Dictionary<IStmRef, Queue<StmRefSavedState>>();
    }

    public void TryAddComponent(IStmRef stmRef, StmRefSavedState stmRefSavedState)
    {
      AddComponent(stmRef, stmRefSavedState);
    }

    private void AddComponent(IStmRef stmRef, StmRefSavedState stmRefSavedState)
    {
      Queue<StmRefSavedState> refSavedState;
      if (components.TryGetValue(stmRef, out refSavedState))
        refSavedState.Enqueue(stmRefSavedState);
      else
        components.Add(stmRef, new Queue<StmRefSavedState>(new[] { stmRefSavedState }));
    }

    public void Begin()
    {
      //not can checking decorator
      //Stm.NotifyBeginTransaction(this);
    }

    private void SaveCommit()
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
      {
        component.Key.SetAsObject(FindFirstСollisionOrGetLastCommitSavedState(component).GetAsObject());
      }
      ClearComponents();
      GiveOtherThreadTimeCompleteTransaction();
    }

    private void GiveOtherThreadTimeCompleteTransaction()
    {
      System.Threading.Thread.Sleep(0);
    }

    private IStmRef FindFirstСollisionOrGetLastCommitSavedState(KeyValuePair<IStmRef, Queue<StmRefSavedState>> stmSavedState)
    {
      bool isFoundCollision = false;
      StmRefSavedState lastSavedState = stmSavedState.Value.First();
      foreach (var currentSavedState in stmSavedState.Value.Skip(1))
      {
        if (!currentSavedState.SaveStmRef.Equals(lastSavedState.NextStmRef))
          isFoundCollision = true;

        lastSavedState = currentSavedState;
        if (isFoundCollision)
          break;
      }

      return isFoundCollision ? lastSavedState.SaveStmRef : Stm.GetLastCommitState(stmSavedState.Key).NextStmRef;
    }

    private bool IsLinearChanges()
    {
      foreach (var component in components.Values)
        if (!isCorrectStmRef(component))
        {
          return false;
        }
      return true;
    }

    private KeyValuePair<IStmRef, StmRefSavedState> GetFirstStmRefPair(IStmRef key,Queue<StmRefSavedState> value)
    {
      return new KeyValuePair<IStmRef, StmRefSavedState>(key,value.First());
    }

    public bool TryCommit()
    {
      bool isCorrectCommit = IsLinearChanges();
      if (isCorrectCommit)
      {
        lock (Stm.commitLock)
        {
          isCorrectCommit = TryCompleteCorrectCommit(isCorrectCommit);
          if (isCorrectCommit)
            SaveCommit();
        }
      }
      return isCorrectCommit;
    }

    private bool TryCompleteCorrectCommit(bool isCorrectCommit)
    {
      if (!isNestedTransaction)
        foreach (var component in components)
          if (!IsSyncronizeWithLastCommit(GetFirstStmRefPair(component.Key, component.Value)))
          {
            isCorrectCommit = false;
            break;
          }

      return isCorrectCommit;
    }

    private bool IsSyncronizeWithLastCommit(KeyValuePair<IStmRef, StmRefSavedState> keyValuePair)
    {
      return Stm.IsSyncronizeWithLastCommit(keyValuePair);
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
