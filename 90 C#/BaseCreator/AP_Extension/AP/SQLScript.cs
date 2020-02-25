using System;
using System.Collections.Generic;
using VDUtils.Helper;

namespace AP_Extension.AP {

  public class SQLScript {

    public string ScriptName { get; set; }
    public DBType Type { get; set; }
    public bool IsSys { get; set; }
    public int Level { get; set; }
    public FileWriter FW { get; set; }
    public List<SQLScript> NeededScripts { get; set; }
    public CSV_Tabelle Tabelle { get; set; }

    public SQLScript(string inScriptName, DBType inType, bool inIsSys, CSV_Tabelle tabelle, FileWriter inFW) {
      Tabelle = tabelle;
      ScriptName = inScriptName;
      Type = inType;
      IsSys = inIsSys;
      Level = 1;
      FW = inFW;
      UpdateFW();
      NeededScripts = new List<SQLScript>();
    }

    public SQLScript(string dummyName) {
      ScriptName = "DUMMY_" + dummyName;
      Type = DBType.Dummy;
      IsSys = false;
      Level = 1;
      FW = new FileWriter("DUMMY", "DUMMY", "DUMMY");
      UpdateFW();
      NeededScripts = new List<SQLScript>();
    }

    private void UpdateFW() {
      switch (Type) {
        case DBType.Database:
          FW.FolderPath += "\\20_SQL\\00_AutoCreate\\10_Databases";
          break;
        case DBType.Table:
          FW.FolderPath += "\\20_SQL\\00_AutoCreate\\20_Tables";
          break;
        case DBType.Function:
          FW.FolderPath += "\\20_SQL\\00_AutoCreate\\30_Functions";
          break;
        case DBType.View:
          FW.FolderPath += "\\20_SQL\\00_AutoCreate\\40_Views";
          break;
        case DBType.SP:
          FW.FolderPath += "\\20_SQL\\00_AutoCreate\\50_SPs";
          break;
        case DBType.Dummy:
          FW.FolderPath += "\\20_SQL\\00_AutoCreate\\99_Dummys";
          break;
      }
    }

    public void AddDependency(string inScriptName) {
      if (!IsAlreadyDependend(inScriptName)) {
        SQLScript loaded = SQLDependencies.GetScript(inScriptName);
        NeededScripts.Add(loaded);
        if (loaded.Level >= Level) {
          Level = loaded.Level + 1;
        }
      }
    }

    public void UpdateLevel() {
      foreach (SQLScript s in NeededScripts) {
        if (s.Level >= Level) {
          Level = s.Level + 1;
        }
      }
    }

    public bool IsAlreadyDependend(string inScriptName) {
      bool ret = false;
      if (ScriptName == inScriptName) {
        //throw new Exception("SQLScript.IsAlreadyDependend() -> Same ScriptName old:<"+ScriptName+"> input:<"+inScriptName+">");
        return true;
      }
      foreach (SQLScript s in NeededScripts) {
        if (s.ScriptName == inScriptName) {
          return true;
        }
        if (s.IsAlreadyDependend(inScriptName)) {
          return true;
        }
      }
      return ret;
    }

    public void PrintToConsoleSimple() {
      Console.WriteLine(" + Script: " + ScriptName);
      Console.WriteLine("   - Type : " + Type);
      Console.WriteLine("   - SysAP: " + (IsSys ? "System" : "Aktivenplaner"));
      Console.WriteLine("   - Level: " + Level);
      string needed = "";
      foreach (SQLScript s in NeededScripts) {
        needed += s.ScriptName + "(L:" + s.Level + "), ";
      }
      Console.WriteLine("   - Needs: " + needed);
      Console.WriteLine(" ");
    }

    public override string ToString() {
      return ScriptName + "(Sys:" + IsSys + ", Level:" + Level + ", NeededScripts:" + NeededScripts.Count + ")";
    }

  }

}
