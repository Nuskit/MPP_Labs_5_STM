using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Labs_5_STM
{
  public static class Stm
  {
    class ThreadInformation
    {
      public ThreadInformation(Action operation, int? parentThreadOperation = null)
      {
        this.Operation = operation;
        this.ParentThreadOperation = parentThreadOperation;
      }

      public Action Operation { get; set; }
      public int? ParentThreadOperation { get; set; }
    }
    
    static ConcurrentDictionary<int, Stack<IStmTransaction>> threadTransactions = new ConcurrentDictionary<int, Stack<IStmTransaction>>();

    static private IStmTransaction CreateNewTransaction()
    {
      return new LoggerStmTransaction(new StmTransaction());
    }

    private static int CurrentThreadId
    {
      get
      {
        return Thread.CurrentThread.ManagedThreadId;
      }
    }

    public static void NotifyBeginTransaction(IStmTransaction transaction)
    {
      var stackTransaction = threadTransactions.GetOrAdd(CurrentThreadId, new Stack<IStmTransaction>());
      stackTransaction.Push(transaction);
    }

    public static void NotifyEndTransaction(IEnumerable<KeyValuePair<IStmRef, StmRefSavedState>> commitKeyValues)
    {
      Stack<IStmTransaction> stackTransaction;
      threadTransactions.TryGetValue(CurrentThreadId, out stackTransaction);
      stackTransaction.Pop();
      if (stackTransaction.Count != 0)
      {
        foreach (var commitBlock in commitKeyValues)
          stackTransaction.Peek().TryAddComponent(commitBlock.Key, commitBlock.Value);
      }
      else
        threadTransactions.TryRemove(CurrentThreadId, out stackTransaction);
    }

    public static void Do(Action operation)
    {
      bool isCompleteTransaction = false;
      var transaction = CreateNewTransaction();

      NotifyBeginTransaction(transaction);
      transaction.Begin();
      do
      {
        operation.Invoke();
        if (transaction.IsCorrectnessTransaction())
          isCompleteTransaction = true;
        else
          transaction.Rollback();
      } while (!isCompleteTransaction);
      transaction.Commit();
    }

    public static void RegisterStmOperation(IStmRef sourse, IStmRef oldStmRef, IStmRef newStmRef = null)
    {
      Stack<IStmTransaction> stackTransaction;
      threadTransactions.TryGetValue(CurrentThreadId, out stackTransaction);
      stackTransaction.Peek().TryAddComponent(sourse, new StmRefSavedState(oldStmRef, newStmRef));
    }
  }
}