using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Labs_5_STM
{
  public static class Stm
  {
    internal static object commitLock = new object();

    static ConcurrentDictionary<int, Stack<IStmTransaction>> threadTransactions = new ConcurrentDictionary<int, Stack<IStmTransaction>>();
    static ConcurrentDictionary<IStmRef, StmRefSavedState> stmRefList = new ConcurrentDictionary<IStmRef, StmRefSavedState>();

    static private IStmTransaction CreateNewTransaction()
    {
      bool isNestedTransaction=false;
      Stack<IStmTransaction> stackTransaction;
      if (threadTransactions.TryGetValue(CurrentThreadId, out stackTransaction))
        isNestedTransaction = true;
      return new LoggerStmTransaction(new StmTransaction(isNestedTransaction));
    }

    private static int CurrentThreadId
    {
      get
      {
        return Thread.CurrentThread.ManagedThreadId;
      }
    }

    internal static void RegisterRef(IStmRef stmRef)
    {
      stmRefList.TryAdd(stmRef, new StmRefSavedState(stmRef.StmUnmanagedClone() as IStmRef, null));
    }

    internal static void UnregisterRef(IStmRef stmRef)
    {
      StmRefSavedState removeSavedStmRef;
      stmRefList.TryRemove(stmRef, out removeSavedStmRef);
    }

    internal static void NotifyBeginTransaction(IStmTransaction transaction)
    {
      var stackTransaction = threadTransactions.GetOrAdd(CurrentThreadId, new Stack<IStmTransaction>());
      stackTransaction.Push(transaction);
    }

    internal static void NotifyEndTransaction(IEnumerable<KeyValuePair<IStmRef, StmRefSavedState>> commitKeyValues)
    {
      Stack<IStmTransaction> stackTransaction;
      if (threadTransactions.TryGetValue(CurrentThreadId, out stackTransaction))
      {
        stackTransaction.Pop();
        if (stackTransaction.Count != 0)
        {
          foreach (var commitBlock in commitKeyValues)
          {
            stackTransaction.Peek().TryAddComponent(commitBlock.Key, commitBlock.Value);
          }
        }
        else
        {
          threadTransactions.TryRemove(CurrentThreadId, out stackTransaction);
          CommitChangeInStmRef(commitKeyValues);
        }
      }
    }

    private static void CommitChangeInStmRef(IEnumerable<KeyValuePair<IStmRef, StmRefSavedState>> commitKeyValues)
    {
      foreach(var commitBlock in commitKeyValues)
        stmRefList.AddOrUpdate(commitBlock.Key, commitBlock.Value, (x, y) => { return commitBlock.Value; });
    }

    public static void Do(Action operation, IStmTransaction stmTransaction = null)
    {
      bool isCompleteTransaction = false;
      var transaction = stmTransaction ?? CreateNewTransaction();

      NotifyBeginTransaction(transaction);
      transaction.Begin();
      do
      {
        operation.Invoke();
        if (transaction.TryCommit())
          isCompleteTransaction = true;
        else
          transaction.Rollback();
      } while (!isCompleteTransaction);
    }

    internal static StmRefSavedState GetLastCommitState(IStmRef key)
    {
      StmRefSavedState value;
      stmRefList.TryGetValue(key, out value);
      return value;

    }

    internal static void RegisterStmOperation(IStmRef sourse, IStmRef oldStmRef, IStmRef newStmRef = null)
    {
      Stack<IStmTransaction> stackTransaction;
      if (threadTransactions.TryGetValue(CurrentThreadId, out stackTransaction))
        stackTransaction.Peek().TryAddComponent(sourse, new StmRefSavedState(oldStmRef, newStmRef));
    }

    internal static bool IsSyncronizeWithLastCommit(KeyValuePair<IStmRef, StmRefSavedState> checkSavedState)
    {
      StmRefSavedState lastCommitSavedState;
      bool isSyncronizeWithLastCommit = false;
      if ((stmRefList.TryGetValue(checkSavedState.Key, out lastCommitSavedState))
        && (lastCommitSavedState.NextStmRef.Equals(checkSavedState.Value.SaveStmRef)))
        isSyncronizeWithLastCommit = true;
      return isSyncronizeWithLastCommit;
    }
  }
}