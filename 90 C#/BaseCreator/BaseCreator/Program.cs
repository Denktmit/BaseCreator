using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCreator {

  class Program {

    static void Main(string[] args) {

      Console.WriteLine("BaseCreator");

      Console.WriteLine("Parameters:");
      
      var arguments = new Dictionary<string, string>();

      foreach (string argument in args) {
        string[] splitted = argument.Split('=');

        if (splitted.Length == 2) {
          arguments[splitted[0]] = splitted[1];
        }
        else {
          Console.WriteLine("! " + argument);
        }
      }

      foreach(KeyValuePair<string,string> arg in arguments) {
        Console.WriteLine(" - " + arg);
        Console.WriteLine("   -> Key: " + arg.Key);
        Console.WriteLine("   -> Val: " + arg.Value);
      }




      Console.WriteLine("\nPress enter to exit");
      string s = Console.ReadLine();
    }

  }

}
