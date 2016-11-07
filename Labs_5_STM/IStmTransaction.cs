namespace Labs_5_STM
{
  public interface IStmTransaction
  {
    void Begin();
    bool TryCommit();
    void Rollback();
    void TryAddComponent(IStmRef stmRef, StmRefSavedState stmRefSavedState);
  }
}
