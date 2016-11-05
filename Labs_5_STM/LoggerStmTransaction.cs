using System;

namespace Labs_5_STM
{
  public class LoggerStmTransaction : IStmTransaction
  {
    private static int idTransaction;
    private IStmTransaction stmTransaction;
  
    public LoggerStmTransaction(IStmTransaction stmTransaction)
    {
      this.stmTransaction = stmTransaction;
    }

    public void Begin()
    {
      ++idTransaction;
      Console.WriteLine(String.Format("In thread {0}, id {1} begin transaction.", System.Threading.Thread.CurrentThread.ManagedThreadId, idTransaction));
      stmTransaction.Begin();
    }

    public void Commit()
    {
      Console.WriteLine(String.Format("In thread {0}, id {1} commit transaction.", System.Threading.Thread.CurrentThread.ManagedThreadId, idTransaction));
      stmTransaction.Commit();
    }

    public bool IsCorrectnessTransaction()
    {
      Console.WriteLine(String.Format("in thread {0}, id {1} check correctness transaction.", System.Threading.Thread.CurrentThread.ManagedThreadId, idTransaction));
      return stmTransaction.IsCorrectnessTransaction();
    }

    public void Rollback()
    {
      Console.WriteLine(String.Format("In thread {0}, id {1} rollback transaction.", System.Threading.Thread.CurrentThread.ManagedThreadId, idTransaction));
      stmTransaction.Rollback();
    }

    public void TryAddComponent(IStmRef stmRef, StmRefSavedState stmRefSavedState)
    {
      Console.WriteLine(String.Format("In thread {0}, id {1} add transaction component. SaveRef: {2}.", 
        System.Threading.Thread.CurrentThread.ManagedThreadId, idTransaction, stmRefSavedState.ToString()));
      stmTransaction.TryAddComponent(stmRef, stmRefSavedState);
    }
  }
}