using System;

namespace Labs_5_STM
{
  public interface IStmRef: ICloneable
  {
    void SetAsObject(object value);
    object GetAsObject();
  }
}
