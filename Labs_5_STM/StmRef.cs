using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labs_5_STM
{
  public class StmRef<T>
    where T:struct
  {
    T Get();
    void Set<T>(T value);
  }
}
