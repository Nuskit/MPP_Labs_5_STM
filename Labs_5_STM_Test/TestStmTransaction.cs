using Labs_5_STM;

namespace Labs_5_STM_Test
{
  class StmTransactionInformation
  {
    public int CallRollBack { get; set; }
  }

  class TestStmTransaction : IStmTransaction
  {
    public StmTransactionInformation stmTransactionInformation { get; private set; }

    private IStmTransaction stmTransation;

    public TestStmTransaction(IStmTransaction stmTransation)
    {
      this.stmTransation = stmTransation;
      this.stmTransactionInformation = new StmTransactionInformation();
    }

    public void Begin()
    {
      stmTransation.Begin();
    }

    public bool TryCommit()
    {
      return stmTransation.TryCommit();
    }

    public void Rollback()
    {
      ++stmTransactionInformation.CallRollBack;
      stmTransation.Rollback();
    }

    public void TryAddComponent(IStmRef stmRef, StmRefSavedState stmRefSavedState)
    {
      stmTransation.TryAddComponent(stmRef, stmRefSavedState);
    }
  }
}