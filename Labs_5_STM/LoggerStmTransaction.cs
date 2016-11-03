using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
      Console.WriteLine(String.Format("Begin transaction in thread {0}, id {1}.",System.Threading.Thread.CurrentThread.ManagedThreadId, idTransaction));
      stmTransaction.Begin();
    }

    public void Commit()
    {
      Console.WriteLine(String.Format("Commit transaction in thread {0}, id {2}.", System.Threading.Thread.CurrentThread.ManagedThreadId, idTransaction));
      stmTransaction.Commit();
    }

    public void Rollback()
    {
      Console.WriteLine(String.Format("Commit transaction in thread {0}, id {2}.", System.Threading.Thread.CurrentThread.ManagedThreadId, idTransaction));
      stmTransaction.Commit();
    }

    public void TryAddComponent(IStmRef stmRef, StmRefSavedState stmRefSavedState)
    {
      Console.WriteLine(String.Format("Add transaction component in thread {0}, id {2}. Ref: {3}", 
        System.Threading.Thread.CurrentThread.ManagedThreadId, idTransaction,stmRef.ToString()));
      stmTransaction.TryAddComponent(stmRef, stmRefSavedState);
    }
  }
}
