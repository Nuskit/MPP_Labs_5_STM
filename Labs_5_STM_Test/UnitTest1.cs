using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

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
        variable.Set(2);
      });

      var task2 = Task.Run(() =>
      {
        variable.Set(3);
      });
      task1.Wait();
      task2.Wait();
      Assert.IsTrue(variable == 2 || variable == 3);
    }
  }
}
