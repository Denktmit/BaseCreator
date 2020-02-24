using BaseCreator_Core.Helper;
using BaseCreator_Core.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCreator_Console {

  class Program {

    static void Main(string[] args) {

      Console.WriteLine("BaseCreator\n");


      double x = 10.0;
      double y = 0.0;

      int a = 10;
      int b = 0;

      BCFile file = null;

      Console.WriteLine("BC: " + file?.Darstellung.ToString());

      Console.WriteLine("x/y = " + (x/y));
      //Console.WriteLine("a/b = " + (a/b));

      Console.WriteLine("\nPress enter to exit");
      string s = Console.ReadLine();

    }

  }

}
