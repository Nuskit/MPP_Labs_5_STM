using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labs_5_STM
{
  class Program
  {
    static void Main(string[] args)
    {
      var s = new StmRef<int>(5);
      Stm.Do(() =>
      {
        s.Set(6);
      });
    }
  }
}
