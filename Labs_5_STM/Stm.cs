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

    static ConcurrentDictionary<int, IStmCompositeTransaction> threadTransactions = new ConcurrentDictionary<int, IStmCompositeTransaction>();
    static ConcurrentDictionary<IStmRef, ConcurrentStack<StmRefSavedState>> stackSavedState = new ConcurrentDictionary<IStmRef, ConcurrentStack<StmRefSavedState>>();

    static private IStmCompositeTransaction CreateNewTransaction()
    {
      return new StmTransaction();
    }

    private static int CurrentThreadId
    {
      get
      {
        return Thread.CurrentThread.ManagedThreadId;
      }
    }

    public static void BeginTransaction(IStmCompositeTransaction transaction, int? parentThreadOperation)
    {
      var listTrancation = threadTransactions.GetOrAdd(CurrentThreadId, transaction);
      if (parentThreadOperation != null)
        threadTransactions.Single(x => x.Key == parentThreadOperation).Value.TryAddComponent(transaction);
    }

    public static void RemoveSavedState(IStmCompositeTransaction transaction, IStmRef component)
    {
      ConcurrentStack<StmRefSavedState> stackTransactionSavedState;
      stackSavedState.TryGetValue(component, out stackTransactionSavedState);
      stackTransactionSavedState.TryPopRange(stackTransactionSavedState.Where(x => x.ParentTransaction.Equals(transaction)).ToArray());
    }

    private static void threadCall(object state)
    {
      ThreadInformation threadInformation = state as ThreadInformation;
      bool isCompleteTransaction = false;
      var transaction = CreateNewTransaction();
      BeginTransaction(transaction, threadInformation.ParentThreadOperation);
      //
      //use as method and call in Begin      
      //

      //
      transaction.Begin();
      do
      {
        threadInformation.Operation.Invoke();
        if (transaction.IsCorrectnessTransaction())
          isCompleteTransaction = true;
        else
          transaction.Rollback();
      } while (!isCompleteTransaction);
      if (threadInformation.ParentThreadOperation == null)
        transaction.Commit();
    }

    public static void Do(Action operation)
    {
      ThreadPool.QueueUserWorkItem(threadCall, new ThreadInformation(operation, isNestedOperation() ? CurrentThreadId : (int?)null));
    }

    private static bool isNestedOperation()
    {
      IStmCompositeTransaction checkedTransaction;
      return threadTransactions.TryGetValue(CurrentThreadId, out checkedTransaction);
    }

    public static void RegisterStmOperation(IStmCompositeRef sourse, StmOperationType operationType)
    {
      IStmCompositeTransaction currentTransaction;
      threadTransactions.TryGetValue(CurrentThreadId, out currentTransaction);
      currentTransaction.TryAddComponent(sourse);
      var currentStmRefStack = stackSavedState.GetOrAdd(sourse, new ConcurrentStack<StmRefSavedState>());
      currentStmRefStack.Push(new StmRefSavedState(sourse, currentTransaction, operationType));
    }

    private static IEnumerable<StmRefSavedState> GetFirstSavedState(ConcurrentStack<StmRefSavedState> stack, IStmTransaction transaction)
    {
      return stack.SkipWhile(x => x.ParentTransaction.Equals(transaction));
    }

    private static StmRefSavedState GetLastSavedState(ConcurrentStack<StmRefSavedState> stack, IStmTransaction transaction)
    {
      return stack.Last(x => x.ParentTransaction.Equals(transaction));
    }

    public static bool IsCorrectRefState(IStmRef stmRef)
    {
      var currentStackSavedState = stackSavedState.Single(x => x.Key.Equals(stmRef)).Value;
      var currentTransaction = threadTransactions.Single(x => x.Key == CurrentThreadId).Value;
      var firstSavedState = GetFirstSavedState(currentStackSavedState, currentTransaction);
      var lastSavedState = GetLastSavedState(currentStackSavedState, currentTransaction);
      foreach (var savedState in firstSavedState)
      {
        if (!savedState.ParentTransaction.Equals(currentTransaction) && savedState.OperationType == StmOperationType.SET)
          return false;
        else if (savedState.Equals(lastSavedState))
          break;
      }
      return true;
    }

    public static void RollBackTransaction(IStmTransaction transaction)
    {
      var s=stackSavedState.Select(x => x.Value.Where(y => y.ParentTransaction.Equals(transaction)));
      //s.First(x=>x.)
    }
  }
}