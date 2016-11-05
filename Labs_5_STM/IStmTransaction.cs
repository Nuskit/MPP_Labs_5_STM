namespace Labs_5_STM
{
  public interface IStmTransaction
  {
    void Begin();
    void Commit();
    void Rollback();
    void TryAddComponent(IStmRef stmRef, StmRefSavedState stmRefSavedState);
    bool IsCorrectnessTransaction();
  }
}
