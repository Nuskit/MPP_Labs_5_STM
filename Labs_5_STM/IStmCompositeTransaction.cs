using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labs_5_STM
{
  public interface IStmCompositeTransaction: IStmTransaction, IStmComposite
  {
    void TryAddComponent(IStmComposite sourse);
  }
}
