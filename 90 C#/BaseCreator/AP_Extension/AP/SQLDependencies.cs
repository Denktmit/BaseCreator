using System;
using System.Security;
using System.Collections.Generic;

namespace AP_Extension.AP {

  public static class SQLDependencies {

    private static List<SQLScript> _allScripts = new List<SQLScript>();

    public static List<SQLScript> AllScripts { get { return _allScripts; } set { _allScripts = value; } }

    private static bool ScriptNameExists(string inScriptName) {
      foreach (SQLScript s in AllScripts) {
        if (s.ScriptName == inScriptName) {
          return true;
        }
      }
      return false;
    }

    public static bool ScriptExists(string inScriptName) {
      SQLScript loaded = GetScript(inScriptName);
      if (loaded == null) {
        return false;
      }
      if (loaded.Type == DBType.Dummy) {
        return false;
      }
      return true;
    }

    public static SQLScript GetScript(string inScriptName) {
      foreach (SQLScript s in AllScripts) {
        if (s.ScriptName == inScriptName) {
          return s;
        }
      }
      SQLScript dummy = new SQLScript(inScriptName);
      try {
        AddScript(dummy);
        Console.WriteLine("! Dummy-Script created: " + inScriptName);
      }
      catch (Exception e) {
        //Console.WriteLine("EXCEPTION: " + e.Message);
      }
      return dummy;
    }

    public static void AddScript(SQLScript inScript) {
      if (ScriptNameExists(inScript.ScriptName)) {
        throw new Exception("SQLDependencies.AddScript(" + inScript.ScriptName + ") -> Script already exists.");
      }
      AllScripts.Add(inScript);
    }

    public static bool Finish() {
      Console.WriteLine(" + SQLDependencies.Finish()");
      bool ret = true;

      // Update Levels
      foreach (SQLScript s in AllScripts) {
        s.Level = s.Tabelle == null ? 999 : s.Tabelle.ClassDependency;
      }

      // remove dummy-scripts
      List<SQLScript> dummys = new List<SQLScript>();
      foreach (SQLScript s in AllScripts) {
        if (s.Type == DBType.Dummy) {
          dummys.Add(s);
        }
      }
      foreach (SQLScript d in dummys) {
        AllScripts.Remove(d);
      }

      // Check for dummy-scripts
      foreach (SQLScript s in AllScripts) {
        if (s.Type == DBType.Dummy) {
          throw new Exception("SQLDependencies.Finish() -> Dummy-Entry left over <" + s.ScriptName + ">");
        }
      }

      // Get scripts in right order
      int actRuns = 0;
      int maxRuns = 1000000;
      List<SQLScript> tmp = new List<SQLScript>();
      SQLScript script = null;
      int lastLevel = 10000;
      while (AllScripts.Count > 0 && actRuns < maxRuns) {
        lastLevel = 10000;
        foreach (SQLScript s in AllScripts) {
          if (s.Level <= lastLevel) {
            script = s;
            lastLevel = s.Level;
          }
        }
        tmp.Add(script);
        AllScripts.Remove(script);
        actRuns++;
      }
      if (actRuns >= maxRuns) {
        throw new Exception("SQLDependencies.Finish() -> Error at getting scripts in right order");
      }
      AllScripts = tmp;

      // Print to test
      // PrintSQLScriptNames();

      //Create files
      foreach (SQLScript s in AllScripts) {
        string level = "";
        if (s.Level / 1000 > 0) {
          level = "" + s.Level;
        }
        else {
          if (s.Level / 100 > 0) {
            level = "0" + s.Level;
          }
          else {
            if (s.Level / 10 > 0) {
              level = "00" + s.Level;
            }
            else {
              if (s.Level / 1 > 0) {
                level = "000" + s.Level;
              }
              else {
                level = "0000";
              }
            }
          }
        }
        s.FW.FileName = level + " " + s.FW.FileName;
        s.FW.CreateFile();
      }

      return ret;
    }

    private static void PrintSQLScriptNames() {
      Console.WriteLine("------------------------------------------");
      Console.WriteLine("   SQLDependencies.PrintSQLScriptNames()");
      Console.WriteLine("");
      foreach (SQLScript s in AllScripts) {
        s.PrintToConsoleSimple();
      }
      Console.WriteLine("");
      Console.WriteLine("------------------------------------------");
    }

  }

}
