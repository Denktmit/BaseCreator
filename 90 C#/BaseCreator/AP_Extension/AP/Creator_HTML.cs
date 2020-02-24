using System;
using System.Security;
using System.Collections.Generic;
using System.IO;

namespace AP_Extension.AP {

  public class Creator_HTML {

    #region fields
    private AutoCreator _ac;
    private List<string> _tempFileContent;
    /*private HtmlDocument htmlDocument;
    private List<HtmlElement> htmlElements;
    private List<string> _databases;*/
    #endregion fields

    #region properties
    public AutoCreator AC { get { return _ac; } set { _ac = value; } }
    public List<string> TempFileContent { get { return _tempFileContent; } set { _tempFileContent = value; } }
    #endregion properties

    #region konstruktor
    public Creator_HTML(AutoCreator inAC) {
      AC = inAC;
      TempFileContent = new List<string>();
    }
    #endregion konstruktor

    #region class_methods
    public void run() {

    }
    #endregion class_methods

    #region helper_methods
    private List<string> GetSimpleScriptHeader(string inDbObject) {
      List<string> newContent = new List<string>();

      Atf(GetHeaderRow("*1"));
      Atf(GetHeaderRow(""));
      if (inDbObject.StartsWith("tblAP")) {
        Atf(GetHeaderRow("Table: " + inDbObject));
      }
      else {
        Atf(GetHeaderRow("Database: " + inDbObject));
      }
      Atf(GetHeaderRow("History :"));
      Atf(GetHeaderRow("          - " + AC.GetStartDate() + ", VD, Creation of the script"));
      Atf(GetHeaderRow(""));
      Atf(GetHeaderRow("*2"));

      return newContent;
    }
    private List<string> GetSkriptHeader(string scriptName, string purpose, List<string> returns, List<string> errorCodes, List<string> neededDbObjects) {
      List<string> newContent = new List<string>();

      Atf(GetHeaderRow("*1"));
      Atf(GetHeaderRow(""));
      // Script-Name
      Atf(GetHeaderRow("Skript  : " + scriptName));
      // Purpose
      Atf(GetHeaderRow("Purpose : " + purpose));
      // returns
      if (returns == null) {
        Atf(GetHeaderRow("Returns : TODO"));
      }
      else {
        for (int i = 0; i < returns.Count; i++) {
          if (i == 0) {
            Atf(GetHeaderRow("Returns : " + returns[i]));
          }
          else {
            Atf(GetHeaderRow("           " + returns[i]));
          }
        }
      }
      // errorCodes
      if (errorCodes == null) {
        Atf(GetHeaderRow("nError  : -x  - Error at called procedure"));
        Atf(GetHeaderRow("          0   - Completed successfully"));
        Atf(GetHeaderRow("          1   - Incorrect input"));
        Atf(GetHeaderRow("          2   - Error at end of procedure"));
        Atf(GetHeaderRow("          3   - Error at called procedure"));
        Atf(GetHeaderRow("          4   - (1062)  Found duplicate key"));
        Atf(GetHeaderRow("          5   - (45000) User defined error"));
        Atf(GetHeaderRow("          6   - (1106)  Unknown procedure"));
        Atf(GetHeaderRow("          7   - (1122)  Unknown function"));
      }
      else {
        for (int i = 0; i < errorCodes.Count; i++) {
          if (i == 0) {
            Atf(GetHeaderRow("nError  : " + errorCodes[i]));
          }
          else {
            Atf(GetHeaderRow("           " + errorCodes[i]));
          }
        }
      }
      // needed Db Objects
      if (neededDbObjects == null) {
        Atf(GetHeaderRow("Needs following DB-Objects:"));
        Atf(GetHeaderRow("            ->  TODO"));
      }
      else {
        if (neededDbObjects.Count > 0) {
          Atf(GetHeaderRow("Needs following DB-Objects:"));
          for (int i = 0; i < neededDbObjects.Count; i++) {
            Atf(GetHeaderRow("            ->  " + neededDbObjects[i]));
          }
        }
      }
      // History
      Atf(GetHeaderRow("History :"));
      Atf(GetHeaderRow("          - " + AC.GetStartDate() + ", VD, Creation of the procedure"));
      Atf(GetHeaderRow(""));
      Atf(GetHeaderRow("*2"));

      return newContent;
    }
    private string GetHeaderRow(string inRow) {
      int width = 80;
      if (width < 60) {
        return "TODO";
      }
      string ret = "";
      if (inRow == "*1") {
        ret += "/";
        for (int i = 0; i < width - 1; i++) {
          ret += "*";
        }
        return ret;
      }
      if (inRow == "*2") {
        for (int i = 0; i < width - 1; i++) {
          ret += "*";
        }
        ret += "/";
        return ret;
      }
      if (inRow == "") {
        ret += "*";
        for (int i = 0; i < width - 2; i++) {
          ret += " ";
        }
        ret += "*";
        return ret;
      }
      if (inRow.Length > (width - 4)) {
        return "* " + inRow.Substring(0, (width - 5)) + "- *\r\n" + GetHeaderRow("          " + inRow.Substring((width - 5)));
      }
      if (inRow.Length == (width - 4)) {
        return "* " + inRow + " *";
      }
      ret = "* " + inRow;
      while (ret.Length < (width - 2)) {
        ret += " ";
      }
      return ret + " *";
    }
    private void Atf(string inLine) {
      if (TempFileContent == null) {
        throw new Exception("ERROR@Creator_SQL.Atf: TempFileContent was null!");
      }
      if (inLine == null) {
        throw new Exception("ERROR@Creator_SQL.Atf: Parameter inLine was null!");
      }
      TempFileContent.Add(inLine);
    }
    private void Atf_multi(List<string> inLines) {
      if (TempFileContent == null) {
        throw new Exception("ERROR@Creator_SQL.Atf: TempFileContent was null!");
      }
      if (inLines == null) {
        throw new Exception("ERROR@Creator_SQL.Atf: Parameter inLines was null!");
      }
      TempFileContent.AddRange(inLines);
    }
    private string GetDataTypeAsStringForSql(string inString, string inSize) {
      switch (inString) {
        case "int":
          return "INT";
        case "datetime":
          return "DATETIME";
        case "decimal":
          return "DECIMAL(" + inSize.Replace(",", ".") + ")";
        case "nvarchar":
          return "NVARCHAR(" + inSize.Replace(",", ".") + ")";
        case "date":
          return "DATE";
        default:
          return "TODO";
      }
    }
    private List<CSV_Tabelle> GetReferencingTables(string tableName) {
      List<CSV_Tabelle> ret = new List<CSV_Tabelle>();
      for (int i = 0; i < AC.DbTables.Count; i++) {
        CSV_Tabelle tempTable = AC.DbTables[i];
        foreach (var col in tempTable.Columns) {
          if (col.ForeignKeyTo.Length > 0 && col.ForeignKeyTo != "Enum") {
            if (tableName == col.ForeignKeyTo.Split('.')[0].Substring(3)) {
              if (!ret.Contains(tempTable)) {
                ret.Add(tempTable);
              }
            }
          }
        }
      }
      return ret;
    }
    private List<string> GetSqlFilesInDirectory(string inDir) {
      List<string> sqlFiles = new List<string>();
      // check if foldername is correct
      DirectoryInfo di = new DirectoryInfo(inDir);
      if (di.Name[0] < '0' || di.Name[0] > '9') {
        return sqlFiles;
      }
      try {
        foreach (string d in Directory.GetDirectories(inDir)) {
          sqlFiles.AddRange(GetSqlFilesInDirectory(d));
        }
        foreach (string f in Directory.GetFiles(inDir)) {
          sqlFiles.Add(f);
        }
      }
      catch (Exception e) {
        Console.WriteLine("ERROR@Creator_SQL.GetSqlFilesInDirectory(" + inDir + "): " + e.ToString());
        throw e;
      }
      return sqlFiles;
    }
    private string[] GetFilePathInfos(string inPath) {
      string[] fpi = new string[3];
      fpi[0] = Path.GetDirectoryName(inPath);
      fpi[1] = Path.GetFileNameWithoutExtension(inPath);
      fpi[2] = Path.GetExtension(inPath);
      return fpi;
    }
    #endregion helper_methods

  }

}
