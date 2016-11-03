using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labs_5_STM
{
  public interface IStmRef: ICloneable
  {
    void SetAsObject(object value);
    object GetAsObject();
  }
}
