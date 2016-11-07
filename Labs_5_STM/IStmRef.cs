using System;

namespace Labs_5_STM
{
  public interface IStmRef: IStmUmanagedClone
  {
    void SetAsObject(object value);
    object GetAsObject();
  }
}
