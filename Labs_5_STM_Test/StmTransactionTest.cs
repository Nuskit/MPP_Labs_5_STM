using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Labs_5_STM;
using System.Threading;

namespace Labs_5_STM_Test
{
  [TestClass]
  public class StmTransactionTest
  {
    [TestMethod]
    public void TestSerialSet()
    {
      var variable = new StmRef<int>();
      var taskFirst = Task.Run(() =>
      {
        Stm.Do(() =>
        {
          variable.Set(2);
        });
      });

      var taskSecond = Task.Run(() =>
      {
        Stm.Do(() =>
        {
          Thread.Sleep(100);
          variable.Set(3);
        });
      });

      taskFirst.Wait();
      taskSecond.Wait();
      Assert.IsTrue(variable.Get() == 3);
    }

    [TestMethod]
    public void TestGet()
    {
      var variable = new StmRef<int>(5);
      int [] checkValue = new int[2];
      var task1 = Task.Run(() =>
      {
        Stm.Do(() =>
        {
          checkValue[0] = variable.Get();
          variable.Set(7);
        });
      });

      var task2 = Task.Run(() =>
      {

        Stm.Do(() =>
        {
          Thread.Sleep(100);
          checkValue[1] = variable.Get();
          variable.Set(2);
        });
      });
      task1.Wait();
      task2.Wait();
      Assert.IsTrue((variable.Get() == 2) && (checkValue[0] == 5) && (checkValue[1] == 7));
    }

    [TestMethod]
    public void TestNestedStm()
    {
      var variable = new StmRef<int>(5);
      int [] checkValue = new int[3];
      var taskFirst = Task.Run(() =>
      {

        Stm.Do(() =>
        {
          checkValue[0] = variable.Get();
          variable.Set(7);
          Stm.Do(() =>
          {
            checkValue[1] = variable.Get();
            variable.Set(1);
          });
          checkValue[2] = variable.Get();
          variable.Set(2);
        });
      });
      taskFirst.Wait();
      Assert.IsTrue((variable.Get() == 2) && (checkValue[0] == 5) && (checkValue[1] == 7) && (checkValue[2] == 1));
    }

    [TestMethod]
    public void TestRollBackStm()
    {
      var variable = new StmRef<int>(5);
      int[] checkValue = new int[2];
      var testStmTransaciton=new TestStmTransaction[2] {
        new TestStmTransaction(new StmTransaction()),
        new TestStmTransaction(new StmTransaction())
      };
      var taskFirst = Task.Run(() =>
      {
      Stm.Do(() =>
      {
        checkValue[testStmTransaciton[0].stmTransactionInformation.CallRollBack] = variable.Get();
        Thread.Sleep(300);
        variable.Get();
      }, testStmTransaciton[0]);
      });

      var taskSecond = Task.Run(() =>
      {
        Stm.Do(() =>
        {
          Thread.Sleep(50);
          variable.Set(1);
        },testStmTransaciton[1]);
      });
      taskFirst.Wait();
      taskSecond.Wait();
      Assert.IsTrue((variable.Get() == 1) && (checkValue[0] == 5) && (checkValue[1] == 1) 
        && (testStmTransaciton[0].stmTransactionInformation.CallRollBack== 1)
        && (testStmTransaciton[1].stmTransactionInformation.CallRollBack == 0)
        );
    }
  }
}
