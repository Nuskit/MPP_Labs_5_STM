using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Labs_5_STM;

namespace Labs_5_STM_Test
{
  [TestClass]
  public class UnitTest1
  {
    [TestMethod]
    public void TestMethod1()
    {
      var variable = new StmRef<int>();
      var task1 = Task.Run(() =>
      {
        variable.Value = 2;
      });

      var task2 = Task.Run(() =>
      {
        variable.Value = 3;
      });
      task1.Wait();
      task2.Wait();
      Assert.IsTrue(variable.Value == 2 || variable.Value == 3);
    }
  }
}
