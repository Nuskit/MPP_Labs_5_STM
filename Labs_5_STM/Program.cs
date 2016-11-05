﻿using System;
using System.Threading;
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
        s.Get();
      });

      Stm.Do(() =>
      {
        s.Set(4);
      });

      Console.WriteLine();

      var taskRead = Task.Run(() =>
        {
          Stm.Do(() =>
          {
            s.Get();
            Thread.Sleep(1050);
            s.Get();
          });
        });

      var taskWrite= Task.Run(() =>
      {
        Stm.Do(() =>
        {
          Thread.Sleep(200);
          s.Set(2);
        });
      });

      taskRead.Wait();
      taskWrite.Wait();

      Console.WriteLine();
      Console.WriteLine();
      Console.WriteLine();
      Console.WriteLine("Hard test");

      var variableFirst = new StmRef<int>();
      var variableSecond = new StmRef<string>("Start");
      Task.Run(() => {
        Stm.Do(() => {
          variableFirst.Set(2);
          Thread.Sleep(50);
          variableFirst.Get();
          variableSecond.Set("Hello");
          variableFirst.Set(5);
          variableSecond.Set("Some");
        });
      });

      Task.Run(() => {
        Stm.Do(() => {
          variableSecond.Get();
          variableSecond.Get();
          variableFirst.Get();
          variableSecond.Get();
          variableFirst.Get();
        });
      });

      Task.Run(() => {
        Stm.Do(() => {
          variableFirst.Set(7);
          variableFirst.Set(8);
          Thread.Sleep(30);
          variableFirst.Set(9);
          variableSecond.Get();
          variableFirst.Set(10);
        });
      });



      Console.ReadKey();
    }
  }
}
