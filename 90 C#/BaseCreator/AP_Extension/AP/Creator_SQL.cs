using System;
using System.Security;
using System.Collections.Generic;
using System.IO;
using BaseCreator_Core.Helper;

namespace AP_Extension.AP {

  public class Creator_SQL {

    #region fields
    private AutoCreator _ac;
    #endregion fields

    #region properties
    public AutoCreator AC { get { return _ac; } set { _ac = value; } }
    #endregion properties

    #region konstruktor
    public Creator_SQL(AutoCreator inAC) {
      AC = inAC;
    }
    #endregion konstruktor

    #region class_methods
    public void run() {
      bool onlyBundle = false;
      if (!onlyBundle) {
        Create_Databases();
        Create_Tables();
        Create_Functions();
        Create_Views();
        Create_StoredProcedures();
        SQLDependencies.Finish();
      }
      Create_BundledSqlScript(AC.APPath + "\\20_SQL", true);
    }
    private void Create_Databases() {
      Create_Database("dbAktivenplaner");
      Create_Database("dbTest");
    }
    private void Create_Tables() {
      Create_AllTables();
    }
    private void Create_Functions() {
      Create_fnAPGetId();
    }
    private void Create_Views() {
      Create_AllViews();
    }
    private void Create_StoredProcedures() {
      Create_AllStoredProcedures();
      Create_spAPDataManipulation();
    }
    private List<string> Create_BundledSqlScript(string folderPath, bool isStartFolder) {
      if (isStartFolder) {
        Console.WriteLine("------------------------ -");
        Console.WriteLine(" + Create_BundledSqlScript()");
      }
      List<string> ret = new List<string>();
      // go through folders first
      foreach (string folder in GetFolderPathsInThisDirectory(folderPath)) {
        ret.AddRange(Create_BundledSqlScript(folder, false));
      }
      // bundle all SQL-scripts in this folder
      foreach (string pfad in GetFilePathsInThisDirectory(folderPath)) {
        string[] fpi = GetFilePathInfos(pfad);
        //Console.WriteLine("    fpi[0]: " + fpi[0]);
        //Console.WriteLine("    fpi[1]: " + fpi[1]);
        //Console.WriteLine("    fpi[2]: " + fpi[2]);
        ret.AddRange(FileManager.getContentAsList(fpi[0], fpi[1], fpi[2].Substring(1)));
      }
      FileManager.writeNewFileWithContent(folderPath, "_Bundled_SQL_Script", "sql", ret, true);
      if (isStartFolder) {
        FileManager.writeNewFileWithContent(AC.APPath + "\\10_Install", "30_SQL", "sql", ret, true);
      }
      return ret;
    }
    private List<string> GetFolderPathsInThisDirectory(string folderPath) {
      //Console.WriteLine("***   Folder: " + folderPath);
      List<string> ret = new List<string>();
      foreach (string d in Directory.GetDirectories(folderPath)) {
        DirectoryInfo di = new DirectoryInfo(d);
        if (di.Name[0] >= '0' && di.Name[0] <= '9') {
          //Console.WriteLine("#     di.FolderName = " + di.Name);
          ret.Add(d);
        }
      }
      return ret;
    }
    private List<string> GetFilePathsInThisDirectory(string folderPath) {
      List<string> ret = new List<string>();
      string[] files = Directory.GetFiles(folderPath);
      foreach (string f in files) {
        FileInfo di = new FileInfo(f);
        if (di.Name[0] >= '0' && di.Name[0] <= '9') {
          //Console.WriteLine("#     di.FileName = " + di.Name);
          ret.Add(f);
        }
      }
      return ret;
    }
    #endregion class_methods

    #region create_databases
    private void Create_Database(string inDb) {
      Console.WriteLine("------------------------ -");
      Console.WriteLine(" + Create_Database(" + inDb + ")");
      string fileName = inDb;
      FileWriter fw = new FileWriter(AC.APPath, fileName, "sql");
      SQLScript newScript = new SQLScript(fileName, DBType.Database, false, null, fw);
      //newScript.AddDependency("dummy");

      fw.Amltf(GetSimpleScriptHeader(inDb));
      fw.Altf("");
      fw.Altf("DROP DATABASE IF EXISTS " + inDb + ";");
      fw.Altf("");
      fw.Altf("CREATE DATABASE " + inDb + ";");
      fw.Altf("");

      // Script-End
      SQLDependencies.AddScript(newScript);
    }
    #endregion create_databases

    #region create_tables
    private void Create_AllTables() {
      foreach (var table in AC.DbTables) {
        Create_SingleTable(table);
      }
    }
    private void Create_SingleTable(CSV_Tabelle table) {
      string fileName = table.TableName;
      Console.WriteLine("  -  Create_SingleTable(" + fileName + ")");
      FileWriter fw = new FileWriter(AC.APPath, fileName, "sql");
      SQLScript newScript = new SQLScript(fileName, DBType.Table, table.IsSys, table, fw);
      newScript.AddDependency("dbAktivenplaner");
      // load all dependencies
      foreach (CSV_Spalte c in table.Columns) {
        if (c.Attribut.StartsWith("i")) {
          newScript.AddDependency("tbl" + (c.Art.StartsWith("Sys") ? "" : "AP") + c.Art);
        }
        if (c.Attribut.StartsWith("e")) {
          newScript.AddDependency("tblSysEnumElement");
        }
      }

      fw.Amltf(GetSimpleScriptHeader(table.TableName));
      fw.Altf("");
      fw.Altf("USE dbAktivenplaner;");
      fw.Altf("");
      fw.Altf("DROP TABLE IF EXISTS " + table.TableName + ";");
      fw.Altf("");
      // Table itself
      fw.Altf("CREATE TABLE " + table.TableName + " (");
      for (int i = 0; i < table.Columns.Count; i++) {
        CSV_Spalte column = table.Columns[i];
        // format and comma
        string s = "  ";
        if (i == 0) {
          s += "  ";
        }
        else {
          s += ", ";
        }
        // attribut name
        s += column.Attribut;
        // art
        s += " " + GetDataTypeAsStringForSql(column.Art, column.Groesse);
        // Primary Key
        if (column.PrimKey) {
          s += " PRIMARY KEY";
        }
        // Not null
        if (column.NotNull) {
          s += " NOT NULL";
        }
        // Auto increment
        if (column.AutoIncre) {
          s += " AUTO_INCREMENT";
        }
        // Default
        if (column.DefaultVal.Length > 0 && column.DefaultVal != "NOW") {
          s += " DEFAULT ";
          if (column.Art != "int" && column.Art != "decimal" && !column.Art.StartsWith("enum_")) { s += "N'"; }
          s += column.DefaultVal;
          if (column.Art != "int" && column.Art != "decimal" && !column.Art.StartsWith("enum_")) { s += "'"; }
        }
        else {
          if (column.Art == "datetime" && column.NotNull) {
            s += " DEFAULT NOW()";
          }
        }
        fw.Altf(s);
      }
      fw.Altf(");");
      fw.Altf("");
      // Table uniques
      //   Load all uniques (a, b, ...) and as collections
      List<string> uniques = new List<string>();
      List<List<string>> unCol = new List<List<string>>();
      foreach (var column in table.Columns) {
        if (column.MakeUnique.Length > 0) {
          bool found = false;
          for (int i = 0; i < uniques.Count; i++) {
            if (uniques[i] == column.MakeUnique) {
              unCol[i].Add(column.Attribut);
              found = true;
            }
          }
          if (!found) {
            uniques.Add(column.MakeUnique);
            List<string> temp = new List<string>();
            temp.Add(column.Attribut);
            unCol.Add(temp);
          }
        }
      }
      int help = 0;
      foreach (var uniqueCollection in unCol) {
        fw.Altf("ALTER TABLE " + fileName + "");
        fw.Altf("  ADD CONSTRAINT con_un_" + uniques[help]);
        string temp = "";
        for (int i = 0; i < uniqueCollection.Count; i++) {
          if (i == 0) {
            temp += uniqueCollection[i];
          }
          else {
            temp += ", " + uniqueCollection[i];
          }
        }
        fw.Altf("    UNIQUE (" + temp + ");");
        fw.Altf("");
        help++;
      }
      // Table constraints
      foreach (var column in table.Columns) {
        if (column.ForeignKeyTo.Length > 0) {
          if (column.ForeignKeyTo == "Enum") {
            // do nothing
          }
          else {
            fw.Altf("/* ALTER TABLE " + table.TableName);
            string constName = column.Constraint;
            if (constName.Length >= 30) {
              constName = constName.Substring(0, 29);
            }
            fw.Altf("  ADD CONSTRAINT " + constName + " FOREIGN KEY (" + column.Attribut + ")");
            fw.Altf("  REFERENCES " + column.ForeignKeyTo.Split('.')[0] + " (" + column.ForeignKeyTo.Split('.')[1] + ")");
            // sn=setnull, c=cascade, na=noaction, r=restrict
            switch (column.OnDelete) {
              case "sn":
                fw.Altf("  ON DELETE SET NULL");
                break;
              case "c":
                fw.Altf("  ON DELETE CASCADE");
                break;
              case "na":
                fw.Altf("  ON DELETE NO ACTION");
                break;
              case "r":
                fw.Altf("  ON DELETE RESTRICT");
                break;
              default:
                fw.Altf("  TODO");
                break;
            }
            switch (column.OnUpdate) {
              case "sn":
                fw.Altf("  ON UPDATE SET NULL");
                break;
              case "c":
                fw.Altf("  ON UPDATE CASCADE");
                break;
              case "na":
                fw.Altf("  ON UPDATE NO ACTION");
                break;
              case "r":
                fw.Altf("  ON UPDATE RESTRICT");
                break;
              default:
                fw.Altf("  TODO");
                break;
            }
            fw.Altf("; */");
            fw.Altf("");
          }
        }
      }

      // Script-End
      SQLDependencies.AddScript(newScript);
    }
    #endregion create_tables

    #region create_functions
    private void Create_fnAPGetId() {
      string functionName = "fnAPGetId";
      Console.WriteLine("  -  Create_" + functionName + "()");
      string fileName = functionName;
      FileWriter fw = new FileWriter(AC.APPath, fileName, "sql");
      SQLScript newScript = new SQLScript(fileName, DBType.Function, false, null, fw);
      newScript.AddDependency("dbAktivenplaner");

      string purpose = "Gibt die Id eines Datensatzes zurueck falls dieser existiert.";
      List<string> returns = new List<string>();            // should not be null
      List<string> errorCodes = new List<string>();         // null -> sp-standard
      List<string> neededDbObjects = new List<string>();    // should not be null
      returns.Add("INT");
      returns.Add("-11 falls kein passender Datensatz gefunden werden konnte.");
      foreach (var table in AC.DbTables) {
        neededDbObjects.Add(table.TableName);
        newScript.AddDependency(table.TableName);
      }
      fw.Amltf(GetSkriptHeader(functionName, purpose, returns, errorCodes, neededDbObjects));

      fw.Altf("");
      fw.Altf("USE dbAktivenplaner;");
      fw.Altf("DROP FUNCTION IF EXISTS " + functionName + ";");
      fw.Altf("");
      fw.Altf("DELIMITER //");
      fw.Altf("CREATE FUNCTION " + functionName + " (");
      fw.Altf("    in_szTable      nvarchar(50)");
      fw.Altf("  , in_param0       nvarchar(200)");
      fw.Altf("  , in_param1       nvarchar(200)");
      fw.Altf("  , in_param2       nvarchar(200)");
      fw.Altf("  , in_param3       nvarchar(200)");
      fw.Altf("  , in_param4       nvarchar(200)");
      fw.Altf("  , in_param5       nvarchar(200)");
      fw.Altf("  , in_param6       nvarchar(200)");
      fw.Altf("  , in_param7       nvarchar(200)");
      fw.Altf("  , in_param8       nvarchar(200)");
      fw.Altf("  , in_param9       nvarchar(200)");
      fw.Altf(")");
      fw.Altf("RETURNS INT");
      fw.Altf("BEGIN");
      fw.Altf("");
      fw.Altf("  DECLARE nReturnId INT DEFAULT -11;");
      fw.Altf("");
      foreach (var table in AC.DbTables) {
        if (table.IsSys) {
          continue;
        }
        string tblName = table.TableName;
        List<string> uniques = new List<string>();
        CSV_Spalte tmpCol = new CSV_Spalte(0, tblName, "", "", "", false, false, false, "", "", "", "", "", "", "");
        for (int i = 3; i < table.Columns.Count; i++) {
          tmpCol = table.Columns[i];
          if (tmpCol.MakeUnique.Length > 0) {
            if (!uniques.Contains(tmpCol.MakeUnique)) {
              uniques.Add(tmpCol.MakeUnique);
            }
          }
        }
        if (uniques.Count > 0) {
          fw.Altf("  -- " + tblName);
          fw.Altf("  IF in_szTable = N'" + tblName + "' THEN");
          for (int u = 0; u < uniques.Count; u++) {
            fw.Altf("    IF EXISTS (SELECT 1");
            fw.Altf("      FROM " + tblName);
            fw.Altf("      WHERE 1=1");
            for (int i = 3; i < table.Columns.Count; i++) {
              tmpCol = table.Columns[i];
              if (tmpCol.MakeUnique == uniques[u]) {
                string t = GetDataTypeAsStringForSql(tmpCol.Art, tmpCol.Groesse);
                if (t == "INT") {
                  t = "SIGNED";
                }
                string ts = " _TODO_1234_ ";
                if (t.StartsWith("NVARCHAR")) {
                  ts = "        AND " + tmpCol.Attribut + " = in_param" + (i - 3);
                }
                else {
                  ts = "        AND " + tmpCol.Attribut + " = CAST(in_param" + (i - 3) + " AS " + t + ")";
                }
                fw.Altf(ts);
              }
            }
            fw.Altf("      LIMIT 1");
            fw.Altf("    )");
            fw.Altf("    THEN");
            fw.Altf("      SET nReturnId = ( SELECT " + table.Columns[0].Attribut);
            fw.Altf("                        FROM " + table.TableName);
            fw.Altf("                        WHERE 1=1");
            for (int i = 3; i < table.Columns.Count; i++) {
              tmpCol = table.Columns[i];
              if (tmpCol.MakeUnique == uniques[u]) {
                string t = GetDataTypeAsStringForSql(tmpCol.Art, tmpCol.Groesse);
                if (t == "INT") {
                  t = "SIGNED";
                }
                string ts = " _TODO_1234_ ";
                if (t.StartsWith("NVARCHAR")) {
                  ts = "                        AND " + tmpCol.Attribut + " = in_param" + (i - 3);
                }
                else {
                  ts = "                        AND " + tmpCol.Attribut + " = CAST(in_param" + (i - 3) + " AS " + t + ")";
                }
                fw.Altf(ts);
              }
            }
            fw.Altf("                        LIMIT 1");
            fw.Altf("      );");
            fw.Altf("      RETURN nReturnId;");
            fw.Altf("    END IF;");
          }
          fw.Altf("  END IF;");
          fw.Altf("");
        }
      }
      fw.Altf("");
      fw.Altf("RETURN -11;");
      fw.Altf("");
      fw.Altf("END; //");
      fw.Altf("DELIMITER ;");
      fw.Altf("/* Test-Call");
      fw.Altf("select " + functionName + "(");
      fw.Altf("    N'tblAPPerson'                 -- szTable");
      fw.Altf("  , N'Roos'                        -- param00");
      fw.Altf("  , N'Janek Sheldon'               -- param01");
      fw.Altf("  , N'1993-02-01'                  -- param02");
      fw.Altf("  , N''                            -- param03");
      fw.Altf("  , N''                            -- param04");
      fw.Altf("  , N''                            -- param05");
      fw.Altf("  , N''                            -- param06");
      fw.Altf("  , N'janek.roos@uttenruthia.de'   -- param07");
      fw.Altf("  , N''                            -- param08");
      fw.Altf("  , N''                            -- param09");
      fw.Altf(") as Result;");
      fw.Altf("*/");
      fw.Altf("");

      // Script-End
      SQLDependencies.AddScript(newScript);
    }
    #endregion create_functions

    #region create_views
    private void Create_AllViews() {
      foreach (var table in AC.DbTables) {
        Create_SimpleView(table);
        Create_ExpandedView(table);
        Create_RecursiveView(table);
        Create_NormalView(table);
        Create_FullView(table);
        if (!table.IsSys)
          Create_InvalidView(table);
      }
      Create_OutdatedDatasetsView(AC.DbTables);
      Create_InvalidDatasetsView(AC.DbTables);
    }
    private void Create_SimpleView(CSV_Tabelle table) {
      string viewName = "TODO_Create_SimpleView";
      if (table.IsSys) {
        viewName = "vw" + table.DataName;
      }
      else {
        viewName = "vwAP" + table.DataName;
      }
      viewName += "_simple";
      Console.WriteLine("  -  Create_" + viewName + "()");
      string fileName = viewName;
      FileWriter fw = new FileWriter(AC.APPath, fileName, "sql");
      SQLScript newScript = new SQLScript(fileName, DBType.View, table.IsSys, table, fw);
      newScript.AddDependency(table.TableName);
      Get_ViewHeader(fw, viewName);


      string krz = "" + table.TableKuerzel;
      fw.Altf("  SELECT  " + krz + ".nId                  AS i" + table.DataName);
      for (int i = 1; i < table.Columns.Count; i++) {
        CSV_Spalte c = table.Columns[i];
        if (!table.IsSys && i < 3)
          continue;
        if (c.NotNull) {
          fw.Altf("        , " + krz + "." + c.Attribut + "                AS " + c.Attribut);
        }
      }
      fw.Altf("  FROM " + table.TableName + " " + krz);
      fw.Altf("  WHERE 1=1");
      if (!table.IsSys) {
        fw.Altf("    AND eDatenstatus = 2");
      }
      fw.Altf("  ORDER BY " + krz + ".nId");
      fw.Altf("  ;");


      Get_ViewFooter(fw, viewName);
      SQLDependencies.AddScript(newScript);
    }
    private void Create_ExpandedView(CSV_Tabelle table) {
      string viewName = "TODO_Create_ExpandedView";
      if (table.IsSys) {
        viewName = "vw" + table.DataName;
      }
      else {
        viewName = "vwAP" + table.DataName;
      }
      viewName += "_expanded";
      Console.WriteLine("  -  Create_" + viewName + "()");
      string fileName = viewName;
      FileWriter fw = new FileWriter(AC.APPath, fileName, "sql");
      SQLScript newScript = new SQLScript(fileName, DBType.View, table.IsSys, table, fw);
      newScript.AddDependency(table.TableName);
      Get_ViewHeader(fw, viewName);

      List<string> joins = new List<string>();
      string krz = "" + table.TableKuerzel;
      fw.Altf("  SELECT  " + krz + ".nId                  AS i" + table.DataName);
      for (int i = 1; i < table.Columns.Count; i++) {
        CSV_Spalte c = table.Columns[i];
        if (!table.IsSys && i < 3)
          continue;
        fw.Altf("        , " + krz + "." + c.Attribut + "                AS " + c.Attribut);
        if (c.ReferencesOtherTable) {
          CSV_Tabelle rt = c.ReferencedTable(AC.DbTables);
          string rtk = c.AttributWOB + "_" + rt.TableKuerzel;
          string rViewName = "";
          if (rt.IsSys) {
            rViewName = "vw" + rt.DataName;
          }
          else {
            rViewName = "vwAP" + rt.DataName;
          }
          newScript.AddDependency(rViewName);
          joins.Add("  JOIN " + rViewName + " " + rtk + " ON " + rtk + ".i" + rt.DataName + " = " + krz + "." + c.Attribut);
          for (int j = 0; j < rt.Columns.Count; j++) {
            CSV_Spalte rc = rt.Columns[j];
            if (rc.MakeUnique != "") {
              fw.Altf("        , " + rtk + "." + rc.Attribut + "                AS " + rc.Attribut + "_" + c.AttributWOB);
            }
          }
        }
      }
      fw.Altf("  FROM " + table.TableName + " " + krz);
      foreach (string j in joins) {
        fw.Altf("  LEFT");
        fw.Altf(j);
      }
      fw.Altf("  WHERE 1=1");
      if (!table.IsSys) {
        fw.Altf("    AND " + krz + ".eDatenstatus = 2");
      }
      fw.Altf("  ORDER BY " + krz + ".nId");
      fw.Altf("  ;");


      Get_ViewFooter(fw, viewName);
      SQLDependencies.AddScript(newScript);
    }
    private void Create_RecursiveView(CSV_Tabelle table) {
      string viewName = "TODO_Create_RecursiveView";
      if (table.IsSys) {
        viewName = "vw" + table.DataName;
      }
      else {
        viewName = "vwAP" + table.DataName;
      }
      viewName += "_recursive";
      Console.WriteLine("  -  Create_" + viewName + "()");
      string fileName = viewName;
      FileWriter fw = new FileWriter(AC.APPath, fileName, "sql");
      SQLScript newScript = new SQLScript(fileName, DBType.View, table.IsSys, table, fw);
      newScript.AddDependency(table.TableName);
      Get_ViewHeader(fw, viewName);

      List<string> joins = new List<string>();
      string krz = "" + table.TableKuerzel;
      fw.Altf("  SELECT  " + krz + ".nId                  AS i" + table.DataName);
      for (int i = 1; i < table.Columns.Count; i++) {
        CSV_Spalte c = table.Columns[i];
        if (!table.IsSys && i < 3)
          continue;
        fw.Altf("        , " + krz + "." + c.Attribut + "                AS " + c.Attribut);
        if (c.ReferencesOtherTable) {
          ViewRecursiveHelper(fw, c.ReferencedTable(AC.DbTables), joins, krz, c, 1, newScript);
        }
      }
      fw.Altf("  FROM " + table.TableName + " " + krz);
      foreach (string j in joins) {
        fw.Altf("  LEFT");
        fw.Altf(j);
      }
      fw.Altf("  WHERE 1=1");
      if (!table.IsSys) {
        fw.Altf("    AND " + krz + ".eDatenstatus = 2");
      }
      fw.Altf("  ORDER BY " + krz + ".nId");
      fw.Altf("  ;");

      Get_ViewFooter(fw, viewName);
      // Error Code: 1116. Too many tables; MariaDB can only use 61 tables in a join
      if (joins.Count <= 60)
        SQLDependencies.AddScript(newScript);
    }
    private void Create_NormalView(CSV_Tabelle table) {
      string viewName = "TODO_Create_View";
      if (table.IsSys) {
        viewName = "vw" + table.DataName;
      }
      else {
        viewName = "vwAP" + table.DataName;
      }
      viewName += "";
      Console.WriteLine("  -  Create_" + viewName + "()");
      string fileName = viewName;
      FileWriter fw = new FileWriter(AC.APPath, fileName, "sql");
      SQLScript newScript = new SQLScript(fileName, DBType.View, table.IsSys, table, fw);
      newScript.AddDependency(table.TableName);
      Get_ViewHeader(fw, viewName);


      string krz = "" + table.TableKuerzel;
      fw.Altf("  SELECT  " + krz + ".nId                  AS i" + table.DataName);
      for (int i = 1; i < table.Columns.Count; i++) {
        CSV_Spalte c = table.Columns[i];
        if (!table.IsSys && i < 3)
          continue;
        fw.Altf("        , " + krz + "." + c.Attribut + "                AS " + c.Attribut);
      }
      fw.Altf("  FROM " + table.TableName + " " + krz);
      fw.Altf("  WHERE 1=1");
      if (!table.IsSys) {
        fw.Altf("    AND eDatenstatus = 2");
      }
      fw.Altf("  ORDER BY " + krz + ".nId");
      fw.Altf("  ;");


      Get_ViewFooter(fw, viewName);
      SQLDependencies.AddScript(newScript);
    }
    private void Create_FullView(CSV_Tabelle table) {
      string viewName = "TODO_Create_View";
      if (table.IsSys) {
        viewName = "vw" + table.DataName;
      }
      else {
        viewName = "vwAP" + table.DataName;
      }
      viewName += "_full";
      Console.WriteLine("  -  Create_" + viewName + "()");
      string fileName = viewName;
      FileWriter fw = new FileWriter(AC.APPath, fileName, "sql");
      SQLScript newScript = new SQLScript(fileName, DBType.View, table.IsSys, table, fw);
      newScript.AddDependency(table.TableName);
      Get_ViewHeader(fw, viewName);


      string krz = "" + table.TableKuerzel;
      fw.Altf("  SELECT  " + krz + ".nId                  AS i" + table.DataName);
      for (int i = 1; i < table.Columns.Count; i++) {
        CSV_Spalte c = table.Columns[i];
        fw.Altf("        , " + krz + "." + c.Attribut + "                AS " + c.Attribut);
      }
      fw.Altf("  FROM " + table.TableName + " " + krz);
      fw.Altf("  WHERE 1=1");
      if (!table.IsSys) {
        fw.Altf("    AND eDatenstatus in (2, 3)");
      }
      fw.Altf("  ORDER BY " + krz + ".nId");
      fw.Altf("  ;");


      Get_ViewFooter(fw, viewName);
      SQLDependencies.AddScript(newScript);
    }
    private void Create_InvalidView(CSV_Tabelle table) {
      string viewName = "TODO_Create_View";
      if (table.IsSys) {
        viewName = "vw" + table.DataName;
      }
      else {
        viewName = "vwAP" + table.DataName;
      }
      viewName += "_invalid";
      Console.WriteLine("  -  Create_" + viewName + "()");
      string fileName = viewName;
      FileWriter fw = new FileWriter(AC.APPath, fileName, "sql");
      SQLScript newScript = new SQLScript(fileName, DBType.View, table.IsSys, table, fw);
      newScript.AddDependency(table.TableName);
      Get_ViewHeader(fw, viewName);


      string krz = "" + table.TableKuerzel;
      fw.Altf("  SELECT  " + krz + ".nId                  AS i" + table.DataName);
      for (int i = 1; i < table.Columns.Count; i++) {
        CSV_Spalte c = table.Columns[i];
        fw.Altf("        , " + krz + "." + c.Attribut + "                AS " + c.Attribut);
      }
      fw.Altf("  FROM " + table.TableName + " " + krz);
      fw.Altf("  WHERE 1=1");
      if (!table.IsSys) {
        fw.Altf("    AND eDatenstatus <> 2");
      }
      fw.Altf("  ORDER BY " + krz + ".nId");
      fw.Altf("  ;");


      Get_ViewFooter(fw, viewName);
      SQLDependencies.AddScript(newScript);
    }
    private void Get_ViewHeader(FileWriter fw, string viewName) {
      fw.Amltf(GetSimpleScriptHeader(viewName));
      fw.Altf("");
      fw.Altf("USE dbAktivenplaner;");
      fw.Altf("");
      fw.Altf("DROP VIEW IF EXISTS " + viewName + ";");
      fw.Altf("");
      fw.Altf("CREATE VIEW " + viewName + " AS");
      fw.Altf("");
    }
    private void Get_ViewFooter(FileWriter fw, string viewName) {
      fw.Altf("");
      fw.Altf("/* Test-Call");
      fw.Altf("");
      fw.Altf("SELECT * FROM " + viewName + ";");
      fw.Altf("");
      fw.Altf("*/");
      fw.Altf("");
    }
    private void ViewRecursiveHelper(FileWriter fw, CSV_Tabelle refTable, List<string> joins, string tableKuerzel, CSV_Spalte referencingColumn, int depth, SQLScript origScript) {
      string depthBlanks = "";
      for (int i = 0; i < depth; i++) {
        depthBlanks += "  ";
      }
      Console.WriteLine(depthBlanks + "" + referencingColumn.Tabelle + "." + referencingColumn.Attribut + "   ->   " + refTable.DataName);

      CSV_Spalte c = referencingColumn;
      string krz = tableKuerzel;
      CSV_Tabelle rt = refTable;
      string rtk = tableKuerzel + referencingColumn.Idx + "_" + rt.TableKuerzel;
      string rViewName = "";
      if (rt.IsSys) {
        rViewName = "vw" + rt.DataName;
      }
      else {
        rViewName = "vwAP" + rt.DataName;
      }
      origScript.AddDependency(rViewName);
      joins.Add("  JOIN " + rViewName + " " + rtk + " ON " + rtk + ".i" + rt.DataName + " = " + krz + "." + c.Attribut);
      for (int j = 0; j < rt.Columns.Count; j++) {
        CSV_Spalte rc = rt.Columns[j];
        if (rc.MakeUnique != "") {
          fw.Altf(depthBlanks + "        , " + rtk + "." + rc.Attribut + "                AS " + rc.Attribut + "_" + krz + "_" + c.Idx);
          if (rc.ReferencesOtherTable) {
            ViewRecursiveHelper(fw, rc.ReferencedTable(AC.DbTables), joins, rtk, rc, depth + 1, origScript);
          }
        }
      }



    }
    private void Create_OutdatedDatasetsView(List<CSV_Tabelle> tables) {
      string viewName = "vwAPOutdatedDatasets";
      Console.WriteLine("  -  Create_" + viewName + "()");
      string fileName = viewName;
      FileWriter fw = new FileWriter(AC.APPath, fileName, "sql");
      SQLScript newScript = new SQLScript(fileName, DBType.View, false, null, fw);
      Get_ViewHeader(fw, viewName);

      CSV_Tabelle t = null;
      for (int i = 0; i < tables.Count; i++) {
        t = tables[i];
        if (!t.IsSys) {
          newScript.AddDependency(t.TableName);
          string line = "(SELECT '" + t.TableName + "', nId, dtAendZeit, eDatenstatus";
          for (int c = 3; c < 20; c++) {
            if (c < t.Columns.Count) {
              line += ", CAST(" + t.Columns[c].Attribut + " AS CHAR(250)) AS c" + c;
            }
            else {
              line += ", CAST('' AS CHAR(250)) AS c" + c;
            }
          }
          line += " FROM " + t.TableName + " WHERE eDatenstatus = 3)";
          fw.Altf("  " + line);
          if (i < tables.Count - 1) {
            fw.Altf("  UNION");
          }
        }
      }
      fw.Altf("  ;");


      Get_ViewFooter(fw, viewName);
      SQLDependencies.AddScript(newScript);
    }
    private void Create_InvalidDatasetsView(List<CSV_Tabelle> tables) {
      string viewName = "vwAPInvalidDatasets";
      Console.WriteLine("  -  Create_" + viewName + "()");
      string fileName = viewName;
      FileWriter fw = new FileWriter(AC.APPath, fileName, "sql");
      SQLScript newScript = new SQLScript(fileName, DBType.View, false, null, fw);
      Get_ViewHeader(fw, viewName);

      CSV_Tabelle t = null;
      for (int i = 0; i < tables.Count; i++) {
        t = tables[i];
        if (!t.IsSys) {
          newScript.AddDependency(t.TableName);
          string line = "(SELECT '" + t.TableName + "', nId, dtAendZeit, eDatenstatus";
          for (int c = 3; c < 20; c++) {
            if (c < t.Columns.Count) {
              line += ", CAST(" + t.Columns[c].Attribut + " AS CHAR(250)) AS c" + c;
            }
            else {
              line += ", CAST('' AS CHAR(250)) AS c" + c;
            }
          }
          line += " FROM " + t.TableName + " WHERE eDatenstatus NOT IN (2, 3))";
          fw.Altf("  " + line);
          if (i < tables.Count - 1) {
            fw.Altf("  UNION");
          }
        }
      }
      fw.Altf("  ;");


      Get_ViewFooter(fw, viewName);
      SQLDependencies.AddScript(newScript);
    }
    #endregion create_views

    #region create_storedProcedures
    private void Create_AllStoredProcedures() {
      foreach (var table in AC.DbTables) {
        Create_SingleProcedure(table);
      }
    }
    private void Create_SingleProcedure(CSV_Tabelle table) {
      Create_InsertProcedure(table);
      Create_UpdateProcedure(table);
      Create_DeleteProcedure(table);
    }
    private void Create_InsertProcedure(CSV_Tabelle table) {
      string procedureName = "spAPInsert" + table.DataName;
      if (table.DataName.StartsWith("Sys")) {
        procedureName = "spSysInsert" + table.DataName;
      }
      Console.WriteLine("  -  Create_" + procedureName + "()");
      string fileName = procedureName;
      FileWriter fw = new FileWriter(AC.APPath, fileName, "sql");
      SQLScript newScript = new SQLScript(fileName, DBType.SP, table.IsSys, table, fw);
      newScript.AddDependency("dbAktivenplaner");
      newScript.AddDependency(table.TableName);
      newScript.AddDependency("fnSysAddToHistory");
      newScript.AddDependency("fnSysPrintDebug");
      newScript.AddDependency("fnSysGetRight");
      newScript.AddDependency("fnSysCheckRight");

      string purpose = "Erstellt einen neuen Eintrag in der Tabelle " + table.TableName + ".";
      List<string> returns = new List<string>();            // should not be null
      List<string> errorCodes = null;                       // null -> sp-standard
      List<string> neededDbObjects = new List<string>();    // should not be null
      returns.Add("Resultset: Der neu eingefuegte Datensatz wird zurueck gegeben.");
      returns.Add("           -60 als Id falls die Prozedur fehlschlug.");
      neededDbObjects.Add("fnSysAddToHistory");
      neededDbObjects.Add("fnSysPrintDebug");
      neededDbObjects.Add("fnSysGetRight");
      neededDbObjects.Add("fnSysCheckRight");

      // Header
      fw.Amltf(GetSkriptHeader(procedureName, purpose, returns, errorCodes, neededDbObjects));
      fw.Altf("");
      fw.Altf("USE dbAktivenplaner;");
      fw.Altf("DROP PROCEDURE IF EXISTS " + procedureName + ";");
      fw.Altf("");
      fw.Altf("DELIMITER //");
      fw.Altf("CREATE PROCEDURE " + procedureName + " (");
      // Create_SPParameter
      Create_SPParameter(fw, table, "Insert");
      fw.Altf(") BEGIN");
      fw.Altf("  ");
      fw.Altf("  -- initialPart:BEGIN");
      fw.Altf("    -- Start: Declaration of local variables (only in procedure)");
      // Create_LocGlobVariables
      Create_LocGlobVariables(fw, table, "Insert", procedureName);
      fw.Altf("    -- End:   Setting of global variables (stay alive after procedure)");
      fw.Altf("    ");
      fw.Altf("    -- Start: Set default values");
      // Create_DefaultValues
      Create_DefaultValues(fw, table, "Insert");
      fw.Altf("    -- End:   Set default values");
      fw.Altf("    ");
      fw.Altf("    -- Start: Create Debug-table if necessary");
      // Create_StartDebug
      Create_StartDebug(fw, table, "Insert");
      fw.Altf("    -- End:   Debug output at procedure start");
      fw.Altf("    ");
      fw.Altf("    -- Start: Check inputs");
      // Create_InputCheck
      Create_InputCheck(fw, table, "Insert");
      fw.Altf("    -- End:   Check inputs");
      fw.Altf("  -- END initialPart;");
      fw.Altf("    ");
      fw.Altf("  -- --------------------------------- -");
      fw.Altf("  -- Main Part");
      fw.Altf("  -- --------------------------------- -");
      fw.Altf("  mainPart:BEGIN");
      fw.Altf("    -- Start: Declaration of handlers");
      // Create_MainPartStart
      Create_MainPartStart(fw, table, "Insert");
      fw.Altf("    -- -End-: Check right ");
      fw.Altf("    ");
      fw.Altf("    ");
      fw.Altf("    ");
      fw.Altf("    -- Start: Check if " + table.DataName + " already exists");
      // Create_CheckForTarget
      Create_CheckForTarget(fw, table, "Insert");
      fw.Altf("    -- End:   Check if " + table.DataName + " already exists");
      fw.Altf("    ");
      fw.Altf("    -- Start: Test if necessary datasets already exist");
      // Create_CheckNecessities
      Create_CheckNecessities(fw, table, "Insert");
      fw.Altf("    -- End:   Test if necessary datasets already exist");
      fw.Altf("    ");
      if (table.TableName == "tblSysEnumElement") {
        fw.Altf("    -- Start: Get next nId");
        // Create_InsertNewIdSysEnumElement
        Create_InsertNewIdSysEnumElement(fw, table, "Insert");
        fw.Altf("    -- End:   Get next nId");
        fw.Altf("    ");
      }
      fw.Altf("    -- Start: Insert new " + table.DataName);
      // Create_Insert
      Create_Insert(fw, table, "Insert");
      fw.Altf("    -- End:   Insert new " + table.DataName);
      fw.Altf("    ");
      fw.Altf("    -- Start: Evaluation");
      // Create_Evaluation
      Create_Evaluation(fw, table, "Insert");
      fw.Altf("    -- End:   Evaluation");
      fw.Altf("    ");
      fw.Altf("    ");
      fw.Altf("    ");
      fw.Altf("    -- Start: Error check at end of main part");
      // Create_MainPartEnd
      Create_MainPartEnd(fw, table, "Insert");
      fw.Altf("    -- End:   End of procedure -> everything is ok");
      fw.Altf("    ");
      fw.Altf("  END mainPart;");
      fw.Altf("    ");
      fw.Altf("  -- --------------------------------- -");
      fw.Altf("  -- Error Part");
      fw.Altf("  -- --------------------------------- -");
      // Create_ErrorPart
      Create_ErrorPart(fw, table, "Insert");
      fw.Altf("    ");
      fw.Altf("  -- --------------------------------- -");
      fw.Altf("  -- End Part");
      fw.Altf("  -- --------------------------------- -");
      // Create_EndPart
      Create_EndPart(fw, table, "Insert");
      fw.Altf("    ");
      fw.Altf("END; //");
      fw.Altf("DELIMITER ;");
      fw.Altf("    ");
      fw.Altf("/* Start: Test-Call");
      fw.Altf("");
      // Create_TestCall
      Create_TestCall(fw, table, "Insert", procedureName);
      fw.Altf("");
      fw.Altf("End:   Test-Call */");
      fw.Altf("");

      // Script-End
      SQLDependencies.AddScript(newScript);
    }
    private void Create_UpdateProcedure(CSV_Tabelle table) {
      string procedureName = "spAPUpdate" + table.DataName;
      if (table.DataName.StartsWith("Sys")) {
        procedureName = "spSysUpdate" + table.DataName;
      }
      Console.WriteLine("  -  Create_" + procedureName + "()");
      string fileName = procedureName;
      FileWriter fw = new FileWriter(AC.APPath, fileName, "sql");
      SQLScript newScript = new SQLScript(fileName, DBType.SP, table.IsSys, table, fw);
      newScript.AddDependency("dbAktivenplaner");
      newScript.AddDependency(table.TableName);
      newScript.AddDependency("fnSysAddToHistory");
      newScript.AddDependency("fnSysPrintDebug");
      newScript.AddDependency("fnSysGetRight");
      newScript.AddDependency("fnSysCheckRight");

      string purpose = "Aendert einen Eintrag in der Tabelle " + table.TableName + ".";
      List<string> returns = new List<string>();            // should not be null
      List<string> errorCodes = null;                       // null -> sp-standard
      List<string> neededDbObjects = new List<string>();    // should not be null
      returns.Add("Resultset: Der geaenderte Datensatz wird zurueck gegeben.");
      returns.Add("           -60 als Id falls die Prozedur fehlschlug.");
      neededDbObjects.Add("fnSysAddToHistory");
      neededDbObjects.Add("fnSysPrintDebug");
      neededDbObjects.Add("fnSysGetRight");
      neededDbObjects.Add("fnSysCheckRight");

      // Header
      fw.Amltf(GetSkriptHeader(procedureName, purpose, returns, errorCodes, neededDbObjects));
      fw.Altf("");
      fw.Altf("USE dbAktivenplaner;");
      fw.Altf("DROP PROCEDURE IF EXISTS " + procedureName + ";");
      fw.Altf("");
      fw.Altf("DELIMITER //");
      fw.Altf("CREATE PROCEDURE " + procedureName + " (");
      // Create_SPParameter
      Create_SPParameter(fw, table, "Update");
      fw.Altf(") BEGIN");
      fw.Altf("  ");
      fw.Altf("  -- initialPart:BEGIN");
      fw.Altf("    -- Start: Declaration of local variables (only in procedure)");
      // Create_LocGlobVariables
      Create_LocGlobVariables(fw, table, "Update", procedureName);
      fw.Altf("    -- End:   Setting of global variables (stay alive after procedure)");
      fw.Altf("    ");
      fw.Altf("    -- Start: Set default values");
      // Create_DefaultValues
      Create_DefaultValues(fw, table, "Update");
      fw.Altf("    -- End:   Set default values");
      fw.Altf("    ");
      fw.Altf("    -- Start: Create Debug-table if necessary");
      // Create_StartDebug
      Create_StartDebug(fw, table, "Update");
      fw.Altf("    -- End:   Debug output at procedure start");
      fw.Altf("    ");
      fw.Altf("    -- Start: Check inputs");
      // Create_InputCheck
      Create_InputCheck(fw, table, "Update");
      fw.Altf("    -- End:   Check inputs");
      fw.Altf("  -- END initialPart;");
      fw.Altf("    ");
      fw.Altf("  -- --------------------------------- -");
      fw.Altf("  -- Main Part");
      fw.Altf("  -- --------------------------------- -");
      fw.Altf("  mainPart:BEGIN");
      fw.Altf("    -- Start: Declaration of handlers");
      // Create_MainPartStart
      Create_MainPartStart(fw, table, "Update");
      fw.Altf("    -- -End-: Check right ");
      fw.Altf("    ");
      fw.Altf("    ");
      fw.Altf("    ");
      fw.Altf("    -- Start: Check if " + table.DataName + " even exists");
      // Create_CheckForTarget
      Create_CheckForTarget(fw, table, "Update");
      fw.Altf("    -- End:   Check if " + table.DataName + " even exists");
      fw.Altf("    ");
      fw.Altf("    -- Start: Check for unique-constraints");
      // Create_CheckForUniqueConstraints
      Create_CheckForUniqueConstraints(fw, table, "Update");
      fw.Altf("    -- End:   Check for unique-constraints");
      fw.Altf("    ");
      fw.Altf("    -- Start: Test if necessary datasets already exist");
      // Create_CheckNecessities
      Create_CheckNecessities(fw, table, "Update");
      fw.Altf("    -- End:   Test if necessary datasets already exist");
      fw.Altf("    ");
      fw.Altf("    -- Start: Update " + table.DataName);
      // Create_Update
      Create_Update(fw, table, "Update");
      fw.Altf("    -- End:   Update " + table.DataName);
      fw.Altf("    ");
      fw.Altf("    -- Start: Evaluation");
      // Create_Evaluation
      Create_Evaluation(fw, table, "Update");
      fw.Altf("    -- End:   Evaluation");
      fw.Altf("    ");
      fw.Altf("    ");
      fw.Altf("    ");
      fw.Altf("    -- Start: Error check at end of main part");
      // Create_MainPartEnd
      Create_MainPartEnd(fw, table, "Update");
      fw.Altf("    -- End:   End of procedure -> everything is ok");
      fw.Altf("    ");
      fw.Altf("  END mainPart;");
      fw.Altf("    ");
      fw.Altf("  -- --------------------------------- -");
      fw.Altf("  -- Error Part");
      fw.Altf("  -- --------------------------------- -");
      // Create_ErrorPart
      Create_ErrorPart(fw, table, "Update");
      fw.Altf("    ");
      fw.Altf("  -- --------------------------------- -");
      fw.Altf("  -- End Part");
      fw.Altf("  -- --------------------------------- -");
      // Create_EndPart
      Create_EndPart(fw, table, "Update");
      fw.Altf("    ");
      fw.Altf("END; //");
      fw.Altf("DELIMITER ;");
      fw.Altf("    ");
      fw.Altf("/* Start: Test-Call");
      fw.Altf("");
      // Create_TestCall
      Create_TestCall(fw, table, "Update", procedureName);
      fw.Altf("");
      fw.Altf("End:   Test-Call */");
      fw.Altf("");

      // Script-End
      SQLDependencies.AddScript(newScript);
    }
    private void Create_DeleteProcedure(CSV_Tabelle table) {
      string procedureName = "spAPDelete" + table.DataName;
      if (table.DataName.StartsWith("Sys")) {
        procedureName = "spSysDelete" + table.DataName;
      }
      Console.WriteLine("  -  Create_" + procedureName + "()");
      string fileName = procedureName;
      FileWriter fw = new FileWriter(AC.APPath, fileName, "sql");
      SQLScript newScript = new SQLScript(fileName, DBType.SP, table.IsSys, table, fw);
      newScript.AddDependency("dbAktivenplaner");
      newScript.AddDependency(table.TableName);
      newScript.AddDependency("fnSysAddToHistory");
      newScript.AddDependency("fnSysPrintDebug");
      newScript.AddDependency("fnSysGetRight");
      newScript.AddDependency("fnSysCheckRight");

      string purpose = "Loescht einen Eintrag aus der Tabelle " + table.TableName + ".";
      List<string> returns = new List<string>();            // should not be null
      List<string> errorCodes = null;                       // null -> sp-standard
      List<string> neededDbObjects = new List<string>();    // should not be null
      returns.Add("Resultset: Der geloeschte Datensatz wird zurueck gegeben.");
      returns.Add("           -60 als Id falls die Prozedur fehlschlug.");
      neededDbObjects.Add("fnSysAddToHistory");
      neededDbObjects.Add("fnSysPrintDebug");
      neededDbObjects.Add("fnSysGetRight");
      neededDbObjects.Add("fnSysCheckRight");

      foreach (CSV_Tabelle tab in AC.GetReferencingTables(table)) {
        string spName = "" + (tab.IsSys ? "spSysUpdate" : "spAPUpdate") + tab.DataName;
        newScript.AddDependency(spName);
        neededDbObjects.Add(spName);
        spName = "" + (tab.IsSys ? "spSysDelete" : "spAPDelete") + tab.DataName;
        newScript.AddDependency(spName);
        neededDbObjects.Add(spName);
      }

      // Header
      fw.Amltf(GetSkriptHeader(procedureName, purpose, returns, errorCodes, neededDbObjects));
      fw.Altf("");
      fw.Altf("USE dbAktivenplaner;");
      fw.Altf("DROP PROCEDURE IF EXISTS " + procedureName + ";");
      fw.Altf("");
      fw.Altf("DELIMITER //");
      fw.Altf("CREATE PROCEDURE " + procedureName + " (");
      // Create_SPParameter
      Create_SPParameter(fw, table, "Delete");
      fw.Altf(") BEGIN");
      fw.Altf("  ");
      fw.Altf("  -- initialPart:BEGIN");
      fw.Altf("    -- Start: Declaration of local variables (only in procedure)");
      // Create_LocGlobVariables
      Create_LocGlobVariables(fw, table, "Delete", procedureName);
      fw.Altf("    -- End:   Setting of global variables (stay alive after procedure)");
      fw.Altf("    ");
      fw.Altf("    -- Start: Set default values");
      // Create_DefaultValues
      Create_DefaultValues(fw, table, "Delete");
      fw.Altf("    -- End:   Set default values");
      fw.Altf("    ");
      fw.Altf("    -- Start: Create Debug-table if necessary");
      // Create_StartDebug
      Create_StartDebug(fw, table, "Delete");
      fw.Altf("    -- End:   Debug output at procedure start");
      fw.Altf("    ");
      fw.Altf("    -- Start: Check inputs");
      // Create_InputCheck
      Create_InputCheck(fw, table, "Delete");
      fw.Altf("    -- End:   Check inputs");
      fw.Altf("  -- END initialPart;");
      fw.Altf("    ");
      fw.Altf("  -- --------------------------------- -");
      fw.Altf("  -- Main Part");
      fw.Altf("  -- --------------------------------- -");
      fw.Altf("  mainPart:BEGIN");
      fw.Altf("    -- Start: Declaration of handlers");
      // Create_MainPartStart
      Create_MainPartStart(fw, table, "Delete");
      fw.Altf("    -- -End-: Check right ");
      fw.Altf("    ");
      fw.Altf("    ");
      fw.Altf("    ");
      fw.Altf("    -- Start: Check if " + table.DataName + " even exists");
      // Create_CheckForTarget
      Create_CheckForTarget(fw, table, "Delete");
      fw.Altf("    -- End:   Check if " + table.DataName + " even exists");
      fw.Altf("    ");
      fw.Altf("    -- Start: Load original values");
      // Create_LoadOrigValues
      Create_LoadOrigValues(fw, table, "Delete");
      fw.Altf("    -- End:   Load original values");
      fw.Altf("    ");
      fw.Altf("    -- Start: DeleteMode handling (in_nDeleteMode= 0:deleteSave, 1:deleteSetDefault, 2:deleteCascade)");
      // Create_Delete
      Create_Delete(fw, table, "Delete");
      fw.Altf("    -- End:   DeleteMode handling (in_nDeleteMode= 0:deleteSave, 1:deleteSetDefault, 2:deleteCascade)");
      fw.Altf("    ");
      fw.Altf("    -- Start: Evaluation");
      // Create_Evaluation
      Create_Evaluation(fw, table, "Delete");
      fw.Altf("    -- End:   Evaluation");
      fw.Altf("    ");
      fw.Altf("    ");
      fw.Altf("    ");
      fw.Altf("    -- Start: Error check at end of main part");
      // Create_MainPartEnd
      Create_MainPartEnd(fw, table, "Delete");
      fw.Altf("    -- End:   End of procedure -> everything is ok");
      fw.Altf("    ");
      fw.Altf("  END mainPart;");
      fw.Altf("    ");
      fw.Altf("  -- --------------------------------- -");
      fw.Altf("  -- Error Part");
      fw.Altf("  -- --------------------------------- -");
      // Create_ErrorPart
      Create_ErrorPart(fw, table, "Delete");
      fw.Altf("    ");
      fw.Altf("  -- --------------------------------- -");
      fw.Altf("  -- End Part");
      fw.Altf("  -- --------------------------------- -");
      // Create_EndPart
      Create_EndPart(fw, table, "Delete");
      fw.Altf("    ");
      fw.Altf("END; //");
      fw.Altf("DELIMITER ;");
      fw.Altf("    ");
      fw.Altf("/* Start: Test-Call");
      fw.Altf("");
      // Create_TestCall
      Create_TestCall(fw, table, "Delete", procedureName);
      fw.Altf("");
      fw.Altf("End:   Test-Call */");
      fw.Altf("");

      // Script-End
      SQLDependencies.AddScript(newScript);
    }
    private void Create_spAPDataManipulation() {
      string procedureName = "spAPDataManipulation";
      Console.WriteLine("  -  Create_" + procedureName + "()");
      string fileName = procedureName;
      FileWriter fw = new FileWriter(AC.APPath, fileName, "sql");
      SQLScript newScript = new SQLScript(fileName, DBType.SP, false, null, fw);
      newScript.AddDependency("fnSysPrintDebug");
      newScript.AddDependency("fnSysAddToHistory");

      string purpose = "Erstellt, aendert oder loescht einen Datensatz in einer der bestehenden AP-Tabellen.";
      List<string> returns = new List<string>();            // should not be null
      List<string> errorCodes = null;         // null -> sp-standard
      List<string> neededDbObjects = new List<string>();    // should not be null
      returns.Add("Resultset: Gibt den erstellten, geaenderten oder geloeschten Datensatz zurueck.");
      neededDbObjects.Add("fnSysPrintDebug");
      neededDbObjects.Add("fnSysAddToHistory");
      foreach (var table in AC.DbTables) {
        neededDbObjects.Add("sp" + (table.IsSys ? "Sys" : "AP") + "Insert" + table.DataName);
        neededDbObjects.Add("sp" + (table.IsSys ? "Sys" : "AP") + "Update" + table.DataName);
        neededDbObjects.Add("sp" + (table.IsSys ? "Sys" : "AP") + "Delete" + table.DataName);
        newScript.AddDependency("sp" + (table.IsSys ? "Sys" : "AP") + "Insert" + table.DataName);
        newScript.AddDependency("sp" + (table.IsSys ? "Sys" : "AP") + "Update" + table.DataName);
        newScript.AddDependency("sp" + (table.IsSys ? "Sys" : "AP") + "Delete" + table.DataName);
      }

      fw.Amltf(GetSkriptHeader(procedureName, purpose, returns, errorCodes, neededDbObjects));

      fw.Altf("");
      fw.Altf("-- __TODO__");
      fw.Altf("");
      fw.Altf("USE dbAktivenplaner;");
      fw.Altf("DROP PROCEDURE IF EXISTS " + procedureName + ";");
      fw.Altf("");
      fw.Altf("-- __TODO__");
      fw.Altf("");
      //fw.Altf("");
      //fw.Altf("DELIMITER //");
      //fw.Altf("CREATE PROCEDURE "+procedureName+" (");
      //fw.Altf("    IN    in_nCallerId            INT             --  Who started the SP");
      //fw.Altf("  , IN    in_nResultset         INT             --  <>0:  select a resultset");
      //fw.Altf("  , IN    in_nDebug             INT             --  <>0:  create debug-output, 1: select and delete debug at the end");
      //fw.Altf("  , IN    in_nDebugDepth        INT             --  indentation of debug inserts");
      //fw.Altf("  , INOUT inout_nError          INT             --  stores the error-code");
      //fw.Altf("  , INOUT inout_szError         NVARCHAR(1000)  --  stores the error-message");
      //fw.Altf("  , IN    in_nDeleteMode        INT             --  0:deleteSave, 1:deleteSetDefault, 2:deleteCascade");
      //fw.Altf("  -- -");
      //fw.Altf("  , IN    in_szTable            NVARCHAR(100)   --  table to manipulate");
      //fw.Altf("  , IN    in_nMode              NVARCHAR(100)   --  8:insert, 9:update, 10:delete");
      //fw.Altf("  -- -");
      //fw.Altf("  , IN    in_nXId               INT             --  dataset to alter");
      //fw.Altf("  , OUT   out_nXId              INT             --  dataset that was altered");
      //fw.Altf("  , IN    in_eDatenstatus       INT             --  new data status");
      //fw.Altf("  -- -");
      //fw.Altf("  , IN    in_szParam00          NVARCHAR(1000)  --  param00");
      //fw.Altf("  , IN    in_szParam01          NVARCHAR(1000)  --  param01");
      //fw.Altf("  , IN    in_szParam02          NVARCHAR(1000)  --  param02");
      //fw.Altf("  , IN    in_szParam03          NVARCHAR(1000)  --  param03");
      //fw.Altf("  , IN    in_szParam04          NVARCHAR(1000)  --  param04");
      //fw.Altf("  , IN    in_szParam05          NVARCHAR(1000)  --  param05");
      //fw.Altf("  , IN    in_szParam06          NVARCHAR(1000)  --  param06");
      //fw.Altf("  , IN    in_szParam07          NVARCHAR(1000)  --  param07");
      //fw.Altf("  , IN    in_szParam08          NVARCHAR(1000)  --  param08");
      //fw.Altf("  , IN    in_szParam09          NVARCHAR(1000)  --  param09");
      //fw.Altf("  , IN    in_szParam10          NVARCHAR(1000)  --  param10");
      //fw.Altf("  , IN    in_szParam11          NVARCHAR(1000)  --  param11");
      //fw.Altf("  , IN    in_szParam12          NVARCHAR(1000)  --  param12");
      //fw.Altf("  , IN    in_szParam13          NVARCHAR(1000)  --  param13");
      //fw.Altf("  , IN    in_szParam14          NVARCHAR(1000)  --  param14");
      //fw.Altf("  , IN    in_szParam15          NVARCHAR(1000)  --  param15");
      //fw.Altf("  , IN    in_szParam16          NVARCHAR(1000)  --  param16");
      //fw.Altf("  , IN    in_szParam17          NVARCHAR(1000)  --  param17");
      //fw.Altf("  , IN    in_szParam18          NVARCHAR(1000)  --  param18");
      //fw.Altf("  , IN    in_szParam19          NVARCHAR(1000)  --  param19");
      //fw.Altf("  , IN    in_szParam20          NVARCHAR(1000)  --  param20");
      //fw.Altf("  , IN    in_szParam21          NVARCHAR(1000)  --  param21");
      //fw.Altf("  , IN    in_szParam22          NVARCHAR(1000)  --  param22");
      //fw.Altf("  , IN    in_szParam23          NVARCHAR(1000)  --  param23");
      //fw.Altf("  , IN    in_szParam24          NVARCHAR(1000)  --  param24");
      //fw.Altf("  , IN    in_szParam25          NVARCHAR(1000)  --  param25");
      //fw.Altf("  , IN    in_szParam26          NVARCHAR(1000)  --  param26");
      //fw.Altf("  , IN    in_szParam27          NVARCHAR(1000)  --  param27");
      //fw.Altf("  , IN    in_szParam28          NVARCHAR(1000)  --  param28");
      //fw.Altf("  , IN    in_szParam29          NVARCHAR(1000)  --  param29");
      //fw.Altf(") BEGIN");
      //fw.Altf("");
      //fw.Altf("  -- initialPart:BEGIN");
      //fw.Altf("");
      //fw.Altf("    -- Start: Declaration of local variables (only in procedure)");
      //fw.Altf("    DECLARE szProcedureName   NVARCHAR(50)    DEFAULT N'"+procedureName+"';");
      //fw.Altf("    DECLARE nRowCount         INT             DEFAULT 0;");
      //fw.Altf("    DECLARE nSuccess          INT             DEFAULT 1;");
      //fw.Altf("    DECLARE dtProcedureStart  DATETIME        DEFAULT NOW();");
      //fw.Altf("    --  -End-: Declaration of local variables (only in procedure)");
      //fw.Altf("");
      //fw.Altf("    --  Start: Setting of global variables (stay alive after procedure)");
      //fw.Altf("    SET @nResultId    = 0;");
      //fw.Altf("    SET @nErrorCall   = 0;");
      //fw.Altf("    SET @szErrorCall  = N'';");
      //fw.Altf("    --  -End-: Setting of global variables (stay alive after procedure)");
      //fw.Altf("");
      //fw.Altf("    --  Start: Set default values");
      //fw.Altf("    -- ! needs to be set: in_nCallerId");
      //fw.Altf("    SET in_nResultset   = IFNULL(in_nResultset, 0);");
      //fw.Altf("    SET in_nDebug       = IFNULL(in_nDebug, 0);");
      //fw.Altf("    SET in_nDebugDepth  = IFNULL(in_nDebugDepth, 0);");
      //fw.Altf("    SET inout_nError    = IFNULL(inout_nError, 0);");
      //fw.Altf("    SET inout_szError   = IFNULL(inout_szError, N'');");
      //fw.Altf("    SET in_nDeleteMode  = IFNULL(in_nDeleteMode, 0);");
      //fw.Altf("    -- -");
      //fw.Altf("    -- ! needs to be set: in_szTable");
      //fw.Altf("    -- ! needs to be set: in_nMode");
      //fw.Altf("    -- -");
      //fw.Altf("    SET in_nXId         = IFNULL(in_nXId          ,-10);");
      //fw.Altf("    SET out_nXId        = IFNULL(out_nXId         ,-10);");
      //fw.Altf("    SET in_eDatenstatus = IFNULL(in_eDatenstatus, 3);");
      //fw.Altf("    -- -");
      //fw.Altf("    -- null-values are permitted: SET in_szParam00    = IFNULL(in_szParam00, N'');");
      //fw.Altf("    -- null-values are permitted: SET in_szParam01    = IFNULL(in_szParam01, N'');");
      //fw.Altf("    -- null-values are permitted: SET in_szParam02    = IFNULL(in_szParam02, N'');");
      //fw.Altf("    -- null-values are permitted: SET in_szParam03    = IFNULL(in_szParam03, N'');");
      //fw.Altf("    -- null-values are permitted: SET in_szParam04    = IFNULL(in_szParam04, N'');");
      //fw.Altf("    -- null-values are permitted: SET in_szParam05    = IFNULL(in_szParam05, N'');");
      //fw.Altf("    -- null-values are permitted: SET in_szParam06    = IFNULL(in_szParam06, N'');");
      //fw.Altf("    -- null-values are permitted: SET in_szParam07    = IFNULL(in_szParam07, N'');");
      //fw.Altf("    -- null-values are permitted: SET in_szParam08    = IFNULL(in_szParam08, N'');");
      //fw.Altf("    -- null-values are permitted: SET in_szParam09    = IFNULL(in_szParam09, N'');");
      //fw.Altf("    -- null-values are permitted: SET in_szParam10    = IFNULL(in_szParam10, N'');");
      //fw.Altf("    -- null-values are permitted: SET in_szParam11    = IFNULL(in_szParam11, N'');");
      //fw.Altf("    -- null-values are permitted: SET in_szParam12    = IFNULL(in_szParam12, N'');");
      //fw.Altf("    -- null-values are permitted: SET in_szParam13    = IFNULL(in_szParam13, N'');");
      //fw.Altf("    -- null-values are permitted: SET in_szParam14    = IFNULL(in_szParam14, N'');");
      //fw.Altf("    -- null-values are permitted: SET in_szParam15    = IFNULL(in_szParam15, N'');");
      //fw.Altf("    -- null-values are permitted: SET in_szParam16    = IFNULL(in_szParam16, N'');");
      //fw.Altf("    -- null-values are permitted: SET in_szParam17    = IFNULL(in_szParam17, N'');");
      //fw.Altf("    -- null-values are permitted: SET in_szParam18    = IFNULL(in_szParam18, N'');");
      //fw.Altf("    -- null-values are permitted: SET in_szParam19    = IFNULL(in_szParam19, N'');");
      //fw.Altf("    -- null-values are permitted: SET in_szParam20    = IFNULL(in_szParam20, N'');");
      //fw.Altf("    -- null-values are permitted: SET in_szParam21    = IFNULL(in_szParam21, N'');");
      //fw.Altf("    -- null-values are permitted: SET in_szParam22    = IFNULL(in_szParam22, N'');");
      //fw.Altf("    -- null-values are permitted: SET in_szParam23    = IFNULL(in_szParam23, N'');");
      //fw.Altf("    -- null-values are permitted: SET in_szParam24    = IFNULL(in_szParam24, N'');");
      //fw.Altf("    -- null-values are permitted: SET in_szParam25    = IFNULL(in_szParam25, N'');");
      //fw.Altf("    -- null-values are permitted: SET in_szParam26    = IFNULL(in_szParam26, N'');");
      //fw.Altf("    -- null-values are permitted: SET in_szParam27    = IFNULL(in_szParam27, N'');");
      //fw.Altf("    -- null-values are permitted: SET in_szParam28    = IFNULL(in_szParam28, N'');");
      //fw.Altf("    -- null-values are permitted: SET in_szParam29    = IFNULL(in_szParam29, N'');");
      //fw.Altf("    --  -End-: Set default values");
      //fw.Altf("");
      //fw.Altf("    --  Start: Create Debug-table if necessary");
      //fw.Altf("    IF in_nDebug <> 0 THEN");
      //fw.Altf("      CREATE TABLE IF NOT EXISTS tblSysDebug (");
      //fw.Altf("          nId       INT             NOT NULL PRIMARY KEY AUTO_INCREMENT");
      //fw.Altf("        , dtTime    TIMESTAMP       NOT NULL DEFAULT CURRENT_TIMESTAMP");
      //fw.Altf("        , szComment NVARCHAR(1000)  NOT NULL DEFAULT N'Missing debug comment!'");
      //fw.Altf("      );");
      //fw.Altf("    END IF;");
      //fw.Altf("    --  -End-: Create Debug-table if necessary");
      //fw.Altf("");
      //fw.Altf("    --  Start: Debug output at procedure start");
      //fw.Altf("    IF in_nDebug <> 0 THEN");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, N'---------------------------------------------------');");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'Starting Procedure ', szProcedureName, N':'));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_nCallerId      = ', in_nCallerId));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_nResultset   = ', in_nResultset));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_nDebug       = ', in_nDebug));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_nDebugDepth  = ', in_nDebugDepth));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with inout_nError    = ', inout_nError));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with inout_szError   = ', inout_szError));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_nDeleteMode  = ', in_nDeleteMode));");
      //fw.Altf("      --                                                                                           ");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_szTable      = ', in_szTable));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_nMode        = ', in_nMode));");
      //fw.Altf("      --                                                                                           ");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_nXId         = ', in_nXId));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_eDatenstatus = ', in_eDatenstatus));");
      //fw.Altf("      --                                                                                           ");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_szParam00    = ', in_szParam00));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_szParam01    = ', in_szParam01));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_szParam02    = ', in_szParam02));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_szParam03    = ', in_szParam03));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_szParam04    = ', in_szParam04));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_szParam05    = ', in_szParam05));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_szParam06    = ', in_szParam06));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_szParam07    = ', in_szParam07));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_szParam08    = ', in_szParam08));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_szParam09    = ', in_szParam09));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_szParam10    = ', in_szParam10));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_szParam11    = ', in_szParam11));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_szParam12    = ', in_szParam12));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_szParam13    = ', in_szParam13));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_szParam14    = ', in_szParam14));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_szParam15    = ', in_szParam15));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_szParam16    = ', in_szParam16));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_szParam17    = ', in_szParam17));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_szParam18    = ', in_szParam18));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_szParam19    = ', in_szParam19));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_szParam20    = ', in_szParam20));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_szParam21    = ', in_szParam21));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_szParam22    = ', in_szParam22));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_szParam23    = ', in_szParam23));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_szParam24    = ', in_szParam24));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_szParam25    = ', in_szParam25));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_szParam26    = ', in_szParam26));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_szParam27    = ', in_szParam27));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_szParam28    = ', in_szParam28));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_szParam29    = ', in_szParam29));");
      //fw.Altf("      ");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, N'---------------------------------------------------');");
      //fw.Altf("    END IF;");
      //fw.Altf("    --  -End-: Debug output at procedure start");
      //fw.Altf("");
      //fw.Altf("    --  Start: Check inputs");
      //fw.Altf("    SET inout_szError = N'ERROR at input: ';");
      //fw.Altf("    IF IFNULL(in_nCallerId, -1) < 0 THEN");
      //fw.Altf("      SET inout_nError  = 1;");
      //fw.Altf("      SET inout_szError = CONCAT_WS(N'', inout_szError, N'Input in_nCallerId was incorrect. (', in_nCallerId, N')');");
      //fw.Altf("    END IF;");
      //fw.Altf("    IF IFNULL(in_szTable, N'') = N'' THEN");
      //fw.Altf("      SET inout_nError  = 1;");
      //fw.Altf("      SET inout_szError = CONCAT_WS(N'', inout_szError, N'Input in_szTable was incorrect. (', in_szTable, N')');");
      //fw.Altf("    END IF;");
      //fw.Altf("    IF IFNULL(in_nMode, -1) NOT IN (8,9,10) THEN");
      //fw.Altf("      SET inout_nError  = 1;");
      //fw.Altf("      SET inout_szError = CONCAT_WS(N'', inout_szError, N'Input in_nMode was incorrect. (', in_nMode, N')');");
      //fw.Altf("    END IF;");
      //fw.Altf("    IF IFNULL(in_nDeleteMode, -1) NOT IN (0,1,2) THEN");
      //fw.Altf("      SET inout_nError  = 1;");
      //fw.Altf("      SET inout_szError = CONCAT_WS(N'', inout_szError, N'Input in_nDeleteMode was incorrect. (', in_nDeleteMode, N')');");
      //fw.Altf("    END IF;");
      //fw.Altf("    IF IFNULL(in_eDatenstatus, 3) NOT IN (0,1,2,3,4) THEN");
      //fw.Altf("      SET inout_nError  = 1;");
      //fw.Altf("      SET inout_szError = CONCAT_WS(N'', inout_szError, N'Input in_eDatenstatus was incorrect. (', in_eDatenstatus, N')');");
      //fw.Altf("    END IF;");
      //fw.Altf("    --  -End-: Check inputs");
      //fw.Altf("");
      //fw.Altf("  -- END initialPart;");
      //fw.Altf("");
      //fw.Altf("  -- ---------------------------------- -");
      //fw.Altf("  --  Main part");
      //fw.Altf("  -- ---------------------------------- -");
      //fw.Altf("  mainPart:BEGIN");
      //fw.Altf("");
      //fw.Altf("    --  Start: Declaration of handlers");
      //fw.Altf("    DECLARE EXIT      HANDLER FOR 1062      -- ER_DUP_ENTRY");
      //fw.Altf("        SET inout_nError = 4;");
      //fw.Altf("    DECLARE EXIT      HANDLER FOR 45000     -- User defined error");
      //fw.Altf("        SET inout_nError = 5;");
      //fw.Altf("    DECLARE EXIT      HANDLER FOR 1106      -- ER_UNKNOWN_PROCEDURE");
      //fw.Altf("        SET inout_nError = 6;");
      //fw.Altf("    DECLARE EXIT      HANDLER FOR 1122      -- ER_CANT_FIND_UDF");
      //fw.Altf("        SET inout_nError = 7;");
      //fw.Altf("    --  -End-: Declaration of handlers");
      //fw.Altf("");
      //fw.Altf("    --  Start: Error check at beginning of main part");
      //fw.Altf("    IF IFNULL(inout_nError, 0) <> 0 THEN");
      //fw.Altf("      SET nSuccess = 0;");
      //fw.Altf("      LEAVE mainPart;");
      //fw.Altf("    END IF;");
      //fw.Altf("    --  -End-: Error check at beginning of main part");
      //fw.Altf("    SET inout_nError  = 0;");
      //fw.Altf("    SET inout_szError = N'';");
      //fw.Altf("");
      //fw.Altf("");
      //fw.Altf("");
      //fw.Altf("");
      //fw.Altf("");
      //fw.Altf("    SET @sql = N'CALL spAP';");
      //fw.Altf("");
      //// Insert dataset
      //fw.Altf("-- ---------------------------------- -- -");
      //fw.Altf("-- ---       INSERT DATASET       --- -- -");
      //fw.Altf("-- ---------------------------------- -- -");
      //fw.Altf("    IF (in_nMode = 8) THEN");
      //fw.Altf("");
      //fw.Altf("      SET @sql = CONCAT_WS(N'', @sql, N'Insert', in_szTable, N' (');");
      //fw.Altf("      CASE in_szTable");
      //foreach(var table in AC.DbTables) {
      //  fw.Altf("        WHEN N'"+table.TableName+"' THEN BEGIN");
      //  fw.Altf("          SET @sql = CONCAT_WS(N'', @sql, in_nCallerId);                 -- in_nCallerId");
      //  fw.Altf("          SET @sql = CONCAT_WS(N'', @sql, N', ', in_eDatenstatus);     -- in_eDatenstatus");
      //  for(int i=3; i< table.Columns.Count; i++) {
      //    string param = ""+(i-3)+"";
      //    if (i < 13) {
      //      param = "0" + param;
      //    }
      //    if(table.Columns[i].Art == "nvarchar") {
      //      fw.Altf("          SET @sql = CONCAT_WS(N'', @sql, N', ', in_szParam" + param + ", N'' );     -- " + table.Columns[i].Attribut);
      //    }
      //    else {
      //      fw.Altf("          SET @sql = CONCAT_WS(N'', @sql, N', ', CAST(in_szParam" + param + " as " + GetDataTypeAsStringForSql(table.Columns[i].Art, table.Columns[i].Groesse) + "), N'' );     -- " + table.Columns[i].Attribut);
      //    }
      //  }
      //  fw.Altf("          SET @sql = CONCAT_WS(N'', @sql, N', ', out_nXId);            -- out_nId");
      //  fw.Altf("          SET @sql = CONCAT_WS(N'', @sql, N', ', in_nResultset);       -- in_nResultset");
      //  fw.Altf("          SET @sql = CONCAT_WS(N'', @sql, N', ', (in_nDebug+1));       -- in_nDebug");
      //  fw.Altf("          SET @sql = CONCAT_WS(N'', @sql, N', ', (in_nDebugDepth+1));  -- in_nDebugDepth");
      //  fw.Altf("          SET @sql = CONCAT_WS(N'', @sql, N', ', @nErrorCall);         -- inout_nError");
      //  fw.Altf("          SET @sql = CONCAT_WS(N'', @sql, N', ', @szErrorCall);        -- inout_szError");
      //  fw.Altf("        END;");
      //}
      //fw.Altf("        ELSE BEGIN");
      //fw.Altf("          SET @sql = N'ERROR';");
      //fw.Altf("        END;");
      //fw.Altf("      END CASE;");
      //fw.Altf("");
      //fw.Altf("    END IF;");
      //fw.Altf("");
      //// Update dataset
      //fw.Altf("-- ---------------------------------- -- -");
      //fw.Altf("-- ---       UPDATE DATASET       --- -- -");
      //fw.Altf("-- ---------------------------------- -- -");
      //fw.Altf("    IF (in_nMode = 9) THEN");
      //fw.Altf("");
      //fw.Altf("      SET @sql = CONCAT_WS(N'', @sql, N'Update', in_szTable, N' (');");
      //fw.Altf("      CASE in_szTable");
      //foreach (var table in AC.DbTables) {
      //  fw.Altf("        WHEN N'" + table.TableName + "' THEN BEGIN");
      //  fw.Altf("          SET @sql = CONCAT_WS(N'', @sql, in_nCallerId);                 -- in_nCallerId");
      //  fw.Altf("          SET @sql = CONCAT_WS(N'', @sql, in_nXId);                    -- dataset-Id");
      //  fw.Altf("          SET @sql = CONCAT_WS(N'', @sql, N', ', in_eDatenstatus);     -- in_eDatenstatus");
      //  for (int i = 3; i < table.Columns.Count; i++) {
      //    string param = "" + (i - 3) + "";
      //    if (i < 13) {
      //      param = "0" + param;
      //    }
      //    if (table.Columns[i].Art == "nvarchar") {
      //      fw.Altf("          SET @sql = CONCAT_WS(N'', @sql, N', N\\'', in_szParam" + param + ", N'\\'' );     -- " + table.Columns[i].Attribut);
      //    }
      //    else {
      //      fw.Altf("          SET @sql = CONCAT_WS(N'', @sql, N', ', CAST(in_szParam" + param + " as " + GetDataTypeAsStringForSql(table.Columns[i].Art, table.Columns[i].Groesse) + "), N'' );     -- " + table.Columns[i].Attribut);
      //    }
      //  }
      //  fw.Altf("          SET @sql = CONCAT_WS(N'', @sql, N', ', out_nXId);            -- out_nId");
      //  fw.Altf("          SET @sql = CONCAT_WS(N'', @sql, N', ', in_nResultset);       -- in_nResultset");
      //  fw.Altf("          SET @sql = CONCAT_WS(N'', @sql, N', ', (in_nDebug+1));       -- in_nDebug");
      //  fw.Altf("          SET @sql = CONCAT_WS(N'', @sql, N', ', (in_nDebugDepth+1));  -- in_nDebugDepth");
      //  fw.Altf("          SET @sql = CONCAT_WS(N'', @sql, N', ', @nErrorCall);         -- inout_nError");
      //  fw.Altf("          SET @sql = CONCAT_WS(N'', @sql, N', ', @szErrorCall);        -- inout_szError");
      //  fw.Altf("        END;");
      //}
      //fw.Altf("        ELSE BEGIN");
      //fw.Altf("          SET @sql = N'ERROR';");
      //fw.Altf("        END;");
      //fw.Altf("      END CASE;");
      //fw.Altf("");
      //fw.Altf("    END IF;");
      //fw.Altf("");
      //// Delete dataset
      //fw.Altf("-- ---------------------------------- -- -");
      //fw.Altf("-- ---       DELETE DATASET       --- -- -");
      //fw.Altf("-- ---------------------------------- -- -");
      //fw.Altf("    IF (in_nMode = 10) THEN");
      //fw.Altf("");
      //fw.Altf("      SET @sql = CONCAT_WS(N'', @sql, N'Delete', in_szTable, N' (');");
      //fw.Altf("      CASE in_szTable");
      //foreach (var table in AC.DbTables) {
      //  fw.Altf("        WHEN N'" + table.TableName + "' THEN BEGIN");
      //  fw.Altf("          SET @sql = CONCAT_WS(N'', @sql, in_nCallerId);                 -- in_nCallerId");
      //  fw.Altf("          SET @sql = CONCAT_WS(N'', @sql, in_nXId);                    -- dataset-Id");
      //  fw.Altf("          SET @sql = CONCAT_WS(N'', @sql, N', ', out_nXId);            -- out_nId");
      //  fw.Altf("          SET @sql = CONCAT_WS(N'', @sql, N', ', in_nResultset);       -- in_nResultset");
      //  fw.Altf("          SET @sql = CONCAT_WS(N'', @sql, N', ', (in_nDebug+1));       -- in_nDebug");
      //  fw.Altf("          SET @sql = CONCAT_WS(N'', @sql, N', ', (in_nDebugDepth+1));  -- in_nDebugDepth");
      //  fw.Altf("          SET @sql = CONCAT_WS(N'', @sql, N', ', @nErrorCall);         -- inout_nError");
      //  fw.Altf("          SET @sql = CONCAT_WS(N'', @sql, N', ', @szErrorCall);        -- inout_szError");
      //  fw.Altf("          SET @sql = CONCAT_WS(N'', @sql, N', ', in_nDeleteMode);      -- delete mode");
      //  fw.Altf("        END;");
      //}
      //fw.Altf("");
      //fw.Altf("        ELSE BEGIN");
      //fw.Altf("          SET @sql = N'ERROR';");
      //fw.Altf("        END;");
      //fw.Altf("      END CASE;");
      //fw.Altf("    END IF;");
      //fw.Altf("");
      //// execute statement
      //fw.Altf("-- ---------------------------------- -- -");
      //fw.Altf("-- ---     EXECUTE STATEMENT      --- -- -");
      //fw.Altf("-- ---------------------------------- -- -");
      //fw.Altf("    IF IFNULL(@sql, N'ERROR') = N'ERROR' THEN");
      //fw.Altf("      SET nSuccess = 0;");
      //fw.Altf("      SET inout_nError  = 5;");
      //fw.Altf("      SET inout_szError = N'Could not create sql statement.';");
      //fw.Altf("      LEAVE mainPart;");
      //fw.Altf("    END IF;");
      //fw.Altf("    PREPARE stmt FROM @sql;");
      //fw.Altf("    EXECUTE stmt;");
      //fw.Altf("    DEALLOCATE PREPARE stmt;");
      //fw.Altf("");
      //fw.Altf("");
      //fw.Altf("");
      //fw.Altf("    --  Start: Error check at end of main part");
      //fw.Altf("    -- IF IFNULL(nRowCount, -1) < 1 THEN");
      //fw.Altf("    --   SET inout_nError = 2;");
      //fw.Altf("    --   SET inout_szError = N'Couldnt manipulate table. RowCount to low.';");
      //fw.Altf("    --   SET nSuccess  = 0;");
      //fw.Altf("    --   LEAVE mainPart;");
      //fw.Altf("    -- END IF;");
      //fw.Altf("    --  -End-: Error check at end of main part");
      //fw.Altf("");
      //fw.Altf("    -- Start: Secure and evaluate errors");
      //fw.Altf("    IF IFNULL(out_nXId, -1) < 0 then");
      //fw.Altf("      SET inout_nError  = 2;");
      //fw.Altf("      SET inout_szError = N'Id out_nXId couldnt be resolved';");
      //fw.Altf("      SET nSuccess  = 0;");
      //fw.Altf("      LEAVE mainPart;");
      //fw.Altf("    END IF;");
      //fw.Altf("    -- -End-: Secure and evaluate errors");
      //fw.Altf("");
      //fw.Altf("    -- Start: End of procedure -> everything is ok");
      //fw.Altf("    SET inout_nError  = 0;");
      //fw.Altf("    SET inout_szError = CONCAT_WS(N'', inout_szError, N' -> No error occured.');");
      //fw.Altf("    -- -End-: End of procedure -> everything is ok");
      //fw.Altf("");
      //fw.Altf("  END mainPart;");
      //fw.Altf("");
      //fw.Altf("  -- ---------------------------------- -");
      //fw.Altf("  -- Error part");
      //fw.Altf("  -- ---------------------------------- -");
      //fw.Altf("  errorPart:BEGIN");
      //fw.Altf("");
      //fw.Altf("    --  Start: Check for errors");
      //fw.Altf("    IF inout_nError = 0 THEN");
      //fw.Altf("      LEAVE errorPart;");
      //fw.Altf("    END IF;");
      //fw.Altf("    --  -End-: Check for errors");
      //fw.Altf("");
      //fw.Altf("    set nSuccess = 0;");
      //fw.Altf("");
      //fw.Altf("    --  Start: Call fnSysAddToHistory");
      //fw.Altf("    set @func = fnSysAddToHistory (");
      //fw.Altf("    /* in_nCallerId     */    0       -- 0:System, if anything else, function wont work");
      //fw.Altf("    /* in_szTable     */  , in_szTable");
      //fw.Altf("    /* in_nTableId    */  , out_nXId");
      //fw.Altf("    /* in_eDMLArt     */  , in_nMode");
      //fw.Altf("    /* in_szFunction  */  , N'Datamanipulation'");
      //fw.Altf("    /* in_nError      */  , inout_nError");
      //fw.Altf("    /* in_szKommentar */  , CONCAT_WS(N'', N'Failed to manipulate table: ', inout_szError)");
      //fw.Altf("    /* in_nNutzerId   */  , in_nCallerId");
      //fw.Altf("    );");
      //fw.Altf("    --  -End-: Call fnSysAddToHistory");
      //fw.Altf("");
      //fw.Altf("    --  Start: Debug");
      //fw.Altf("    IF in_nDebug <> 0 THEN");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, N'');");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, N'---------------------------------------------------');");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'Finished with errors', N'.'));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'   inout_nError :  ', inout_nError));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'   inout_szError:  ', inout_szError));");
      //fw.Altf("    END IF;");
      //fw.Altf("    --  -End-: Debug");
      //fw.Altf("");
      //fw.Altf("  END errorPart;");
      //fw.Altf("");
      //fw.Altf("  -- ---------------------------------- -");
      //fw.Altf("  -- End part");
      //fw.Altf("  -- ---------------------------------- -");
      //fw.Altf("  endPart:BEGIN");
      //fw.Altf("");
      //fw.Altf("    --  Start: Call fnSysAddToHistory");
      //fw.Altf("    IF  inout_nError   = 0");
      //fw.Altf("    AND nSuccess = 1 THEN");
      //fw.Altf("      set @func = fnSysAddToHistory (");
      //fw.Altf("      /* in_nCallerId     */    0       -- 0:System, if anything else, function wont work");
      //fw.Altf("      /* in_szTable     */  , in_szTable");
      //fw.Altf("      /* in_nTableId    */  , out_nXId");
      //fw.Altf("      /* in_eDMLArt     */  , in_nMode");
      //fw.Altf("      /* in_szFunction  */  , N'Datamanipulation'");
      //fw.Altf("      /* in_nError      */  , inout_nError");
      //fw.Altf("      /* in_szKommentar */  , CONCAT_WS(N'', N'Successfull manipulation: ', inout_szError)");
      //fw.Altf("      /* in_nNutzerId   */  , in_nCallerId");
      //fw.Altf("      );");
      //fw.Altf("    END IF;");
      //fw.Altf("    --  -End-: Call fnSysAddToHistory");
      //fw.Altf("");
      //fw.Altf("    --  Start: Build resultset");
      //fw.Altf("    --  -End-: Build resultset");
      //fw.Altf("");
      //fw.Altf("    --  Start: Debug");
      //fw.Altf("    IF in_nDebug <> 0 THEN");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, N'');");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, N'---------------------------------------------------');");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'Ending Procedure ', szProcedureName, N'.'));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N' - at ', NOW()));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N' - time: ', TIMEDIFF(NOW(), dtProcedureStart)));");
      //fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, N'---------------------------------------------------');");
      //fw.Altf("      IF in_nDebug = 1 THEN");
      //fw.Altf("        SELECT * FROM tblSysDebug;");
      //fw.Altf("        TRUNCATE TABLE tblSysDebug;");
      //fw.Altf("      END IF;");
      //fw.Altf("    END IF;");
      //fw.Altf("    --  -End-: Debug");
      //fw.Altf("");
      //fw.Altf("  END endPart;");
      //fw.Altf("");
      //fw.Altf("END; //");
      //fw.Altf("DELIMITER ;");
      //fw.Altf("");
      //fw.Altf("");
      //fw.Altf("");
      //fw.Altf("/* Test-Call");
      //fw.Altf("use dbAktivenplaner;");
      //fw.Altf("-- -");
      //fw.Altf("set @in_nCallerId            = 492;     --  Who started the SP");
      //fw.Altf("set @in_nResultset         = 1;       --  <>0:  select a resultset");
      //fw.Altf("set @in_nDebug             = 1;       --  <>0:  create debug-output, 1: select and delete debug at the end");
      //fw.Altf("set @in_nDebugDepth        = 0;       --  indentation of debug inserts");
      //fw.Altf("set @inout_nError          = 0;       --  stores the error-code");
      //fw.Altf("set @inout_szError         = N'';     --  stores the error-message");
      //fw.Altf("set @in_nDeleteMode        = 0;       --  0:deleteSave, 1:deleteSetDefault, 2:deleteCascade");
      //fw.Altf("-- -");
      //fw.Altf("set @in_szTable            = N'Verwaltung';     --  table to manipulate");
      //fw.Altf("set @in_nMode              = 8;       --  8:insert, 9:update, 10:delete");
      //fw.Altf("-- -");
      //fw.Altf("set @in_nXId               = 0;       --  dataset to alter");
      //fw.Altf("set @out_nXId              = 0;       --  dataset that was altered");
      //fw.Altf("set @in_eDatenstatus       = 1;       --  new data status");
      //fw.Altf("-- -");
      //fw.Altf("set @in_szParam00          = 492;    --  param00");
      //fw.Altf("set @in_szParam01          = 1;    --  param01");
      //fw.Altf("set @in_szParam02          = N'VD';    --  param02");
      //fw.Altf("set @in_szParam03          = N'TODO';    --  param03");
      //fw.Altf("set @in_szParam04          = null;    --  param04");
      //fw.Altf("set @in_szParam05          = null;    --  param05");
      //fw.Altf("set @in_szParam06          = null;    --  param06");
      //fw.Altf("set @in_szParam07          = null;    --  param07");
      //fw.Altf("set @in_szParam08          = null;    --  param08");
      //fw.Altf("set @in_szParam09          = null;    --  param09");
      //fw.Altf("set @in_szParam10          = null;    --  param10");
      //fw.Altf("set @in_szParam11          = null;    --  param11");
      //fw.Altf("set @in_szParam12          = null;    --  param12");
      //fw.Altf("set @in_szParam13          = null;    --  param13");
      //fw.Altf("set @in_szParam14          = null;    --  param14");
      //fw.Altf("set @in_szParam15          = null;    --  param15");
      //fw.Altf("set @in_szParam16          = null;    --  param16");
      //fw.Altf("set @in_szParam17          = null;    --  param17");
      //fw.Altf("set @in_szParam18          = null;    --  param18");
      //fw.Altf("set @in_szParam19          = null;    --  param19");
      //fw.Altf("set @in_szParam20          = null;    --  param20");
      //fw.Altf("set @in_szParam21          = null;    --  param21");
      //fw.Altf("set @in_szParam22          = null;    --  param22");
      //fw.Altf("set @in_szParam23          = null;    --  param23");
      //fw.Altf("set @in_szParam24          = null;    --  param24");
      //fw.Altf("set @in_szParam25          = null;    --  param25");
      //fw.Altf("set @in_szParam26          = null;    --  param26");
      //fw.Altf("set @in_szParam27          = null;    --  param27");
      //fw.Altf("set @in_szParam28          = null;    --  param28");
      //fw.Altf("set @in_szParam29          = null;    --  param29");
      //fw.Altf("-- -");
      //fw.Altf("CALL spAPDataManipulation(");
      //fw.Altf("    @in_nCallerId");
      //fw.Altf("  , @in_nResultset  ");
      //fw.Altf("  , @in_nDebug      ");
      //fw.Altf("  , @in_nDebugDepth ");
      //fw.Altf("  , @inout_nError   ");
      //fw.Altf("  , @inout_szError  ");
      //fw.Altf("  , @in_nDeleteMode ");
      //fw.Altf("-- -");
      //fw.Altf("  , @in_szTable     ");
      //fw.Altf("  , @in_nMode       ");
      //fw.Altf("-- -");
      //fw.Altf("  , @in_nXId        ");
      //fw.Altf("  , @out_nXId       ");
      //fw.Altf("  , @in_eDatenstatus");
      //fw.Altf("-- -");
      //fw.Altf("  , @in_szParam00   ");
      //fw.Altf("  , @in_szParam01   ");
      //fw.Altf("  , @in_szParam02   ");
      //fw.Altf("  , @in_szParam03   ");
      //fw.Altf("  , @in_szParam04   ");
      //fw.Altf("  , @in_szParam05   ");
      //fw.Altf("  , @in_szParam06   ");
      //fw.Altf("  , @in_szParam07   ");
      //fw.Altf("  , @in_szParam08   ");
      //fw.Altf("  , @in_szParam09   ");
      //fw.Altf("  , @in_szParam10   ");
      //fw.Altf("  , @in_szParam11   ");
      //fw.Altf("  , @in_szParam12   ");
      //fw.Altf("  , @in_szParam13   ");
      //fw.Altf("  , @in_szParam14   ");
      //fw.Altf("  , @in_szParam15   ");
      //fw.Altf("  , @in_szParam16   ");
      //fw.Altf("  , @in_szParam17   ");
      //fw.Altf("  , @in_szParam18   ");
      //fw.Altf("  , @in_szParam19   ");
      //fw.Altf("  , @in_szParam20   ");
      //fw.Altf("  , @in_szParam21   ");
      //fw.Altf("  , @in_szParam22   ");
      //fw.Altf("  , @in_szParam23   ");
      //fw.Altf("  , @in_szParam24   ");
      //fw.Altf("  , @in_szParam25   ");
      //fw.Altf("  , @in_szParam26   ");
      //fw.Altf("  , @in_szParam27   ");
      //fw.Altf("  , @in_szParam28   ");
      //fw.Altf("  , @in_szParam29   ");
      //fw.Altf(");");
      //fw.Altf("select * from tblSysHistory order by nId desc;");
      //fw.Altf("*/");
      fw.Altf("");

      // Script-End
      SQLDependencies.AddScript(newScript);
    }
    private void Create_SPParameter(FileWriter fw, CSV_Tabelle table, string spType) {
      fw.Altf("    IN      in_nCallerId    INT                 -- Is mandatory");
      fw.Altf("  -- Table data");
      // insert or update
      if (spType == "Insert" || spType == "Update") {
        if (spType == "Update") {
          fw.Altf("  , IN      in_nId          INT");
        }
        CSV_Spalte tmpCol = null;
        for (int i = table.IsSys ? 1 : 2; i < table.Columns.Count; i++) {
          tmpCol = table.Columns[i];
          fw.Altf("  , IN      in_" + tmpCol.Attribut + "        " + tmpCol.GetSQLType());
        }
      }
      // delete
      else {
        fw.Altf("  , IN      in_nId          INT");
        fw.Altf("  , IN      in_nDeleteMode  INT");
      }
      fw.Altf("  -- Standard in-/outputs");
      fw.Altf("  , OUT     out_nId         INT");
      fw.Altf("  , IN      in_nResultset   INT                 -- <>0: select a resultset");
      fw.Altf("  , IN      in_nDebug       INT                 -- <>0: create debug-output, 1: select and delete debug at the end");
      fw.Altf("  , IN      in_nDebugDepth  INT                 -- indentation of debug inserts");
      fw.Altf("  , INOUT   inout_nError    INT                 -- stores the error-code");
      fw.Altf("  , INOUT   inout_szError   NVARCHAR(1000)      -- stores the error-message");
    }
    private void Create_LocGlobVariables(FileWriter fw, CSV_Tabelle table, string spType, string procedureName) {
      fw.Altf("    DECLARE szProcedureName     NVARCHAR(50)    DEFAULT N'" + procedureName + "';");
      fw.Altf("    DECLARE iRight              INT             DEFAULT 0;");
      fw.Altf("    DECLARE szRightDescription  NVARCHAR(100)   DEFAULT N'action_sp_" + procedureName + "';");
      fw.Altf("    DECLARE nRowCount           INT             DEFAULT 0;");
      fw.Altf("    DECLARE nSuccess            INT             DEFAULT 1;");
      fw.Altf("    DECLARE dtProcedureStart    DATETIME        DEFAULT NOW();");
      if (spType == "Delete") {
        fw.Altf("    DECLARE nResultId           INT             DEFAULT 0;");
        fw.Altf("    DECLARE nErrorCall          INT             DEFAULT 0;");
        fw.Altf("    DECLARE szErrorCall         NVARCHAR(1000)  DEFAULT N'';");
        fw.Altf("    DECLARE nDebugCall          INT             DEFAULT 0;");
      }
      if (spType == "Insert" && table.TableName == "tblSysEnumElement") {
        fw.Altf("    DECLARE nNewId              INT             DEFAULT -1;");
      }
      fw.Altf("    --  End:   Declaration of local variables (only in procedure)");
      fw.Altf("    ");
      if (spType == "Delete") {
        fw.Altf("    --  Start: Declare cursors");
        fw.Altf("    DECLARE done                INT             DEFAULT FALSE;");
        fw.Altf("    DECLARE c_Id                INT             DEFAULT 0;");
        // Start: Cursors
        foreach (CSV_Spalte col in AC.GetReferencingColumns(table)) {
          fw.Altf("    DECLARE cur_" + col.Tabelle + "_" + col.Attribut + " CURSOR FOR");
          fw.Altf("        SELECT nId");
          fw.Altf("        FROM  " + col.Tabelle + "");
          fw.Altf("        WHERE " + col.Attribut + " = in_nId");
          fw.Altf("        ;");
        }
        // End:   Cursors
        fw.Altf("    DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = TRUE;");
        fw.Altf("    --  -End-: Declare cursors");
        fw.Altf("    ");
      }
      if (spType != "Delete") {
        fw.Altf("    --  Start: Setting of global variables (stay alive after procedure)");
        fw.Altf("    -- SET @nResultId    = 0;");
        fw.Altf("    -- SET @nErrorCall   = 0;");
        fw.Altf("    -- SET @szErrorCall  = N'';");
      }
      else {
        fw.Altf("    --  Start: Setting of global variables (stay alive after procedure)");
        fw.Altf("    CREATE TEMPORARY TABLE IF NOT EXISTS tblTdDelete" + table.DataName + " (");
        string line = "";
        CSV_Spalte tmpCol = null;
        for (int i = 0; i < table.Columns.Count; i++) {
          tmpCol = table.Columns[i];
          line = i == 0 ? "        " : "      , ";
          line += tmpCol.GetColumnDescription();
          fw.Altf(line);
        }
        fw.Altf("    );");
        fw.Altf("    TRUNCATE tblTdDelete" + table.DataName + ";");
      }
    }
    private void Create_DefaultValues(FileWriter fw, CSV_Tabelle table, string spType) {
      if (spType == "Insert" || spType == "Update") {
        CSV_Spalte tmpCol = null;
        for (int i = table.IsSys ? 1 : 2; i < table.Columns.Count; i++) {
          tmpCol = table.Columns[i];
          if (tmpCol.DefaultVal.Length > 0) {
            fw.Altf("    SET in_" + tmpCol.Attribut + " = IFNULL(in_" + tmpCol.Attribut + ", " + tmpCol.GetSQLDefaultValue() + ");");
          }
        }
      }
      else {
        fw.Altf("    SET in_nDeleteMode        = IFNULL(in_nDeleteMode,   0);");
      }
      fw.Altf("    SET out_nId               = IFNULL(out_nId, -10);");
      fw.Altf("    SET in_nResultset         = IFNULL(in_nResultset,    0);");
      fw.Altf("    SET in_nDebug             = IFNULL(in_nDebug,        0);");
      fw.Altf("    SET in_nDebugDepth        = IFNULL(in_nDebugDepth,   0);");
      fw.Altf("    SET inout_nError          = IFNULL(inout_nError,     0);");
      fw.Altf("    SET inout_szError         = IFNULL(inout_szError,    N'');");
      if (spType == "Delete") {
        fw.Altf("    SET nDebugCall = in_nDebug;");
        fw.Altf("    IF in_nDebug = 1 THEN");
        fw.Altf("      SET nDebugCall = 2;");
        fw.Altf("    END IF;");
      }
    }
    private void Create_StartDebug(FileWriter fw, CSV_Tabelle table, string spType) {
      fw.Altf("    IF in_nDebug <> 0 THEN");
      fw.Altf("      CREATE TABLE IF NOT EXISTS tblDebug (");
      fw.Altf("          nId       INT             NOT NULL PRIMARY KEY AUTO_INCREMENT");
      fw.Altf("        , dtTime    TIMESTAMP       NOT NULL DEFAULT CURRENT_TIMESTAMP");
      fw.Altf("        , szComment NVARCHAR(1000)  NOT NULL DEFAULT N'Missing debug comment!'");
      fw.Altf("      );");
      fw.Altf("    END IF;");
      fw.Altf("    --  -End-: Create Debug-table if necessary");
      fw.Altf("");
      fw.Altf("    --  Start: Debug output at procedure start");
      fw.Altf("    IF in_nDebug <> 0 THEN");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, N'---------------------------------------------------');");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'Starting Procedure ', szProcedureName, N':'));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_nCallerId    = ', in_nCallerId));");
      if (spType != "Insert") {
        fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_nId          = ', in_nId));");
      }
      if (spType == "Delete") {
        fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_nDeleteMode  = ', in_nDeleteMode));");
      }
      else {
        CSV_Spalte tmpCol = null;
        for (int i = table.IsSys ? 1 : 2; i < table.Columns.Count; i++) {
          tmpCol = table.Columns[i];
          fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_" + tmpCol.Attribut + "  = ', in_" + tmpCol.Attribut + "));");
        }
      }
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_nCallerId    = ', in_nCallerId));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_nResultset = ', in_nResultset));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_nDebug     = ', in_nDebug));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, N'---------------------------------------------------');");
      fw.Altf("    END IF;");
    }
    private void Create_InputCheck(FileWriter fw, CSV_Tabelle table, string spType) {
      int nErrorHelper = -10;
      fw.Altf("    SET inout_szError = N'ERROR at input: ';");
      fw.Altf("    IF IFNULL(in_nCallerId, -1) < 0 THEN");
      fw.Altf("      SET inout_nError  = " + nErrorHelper + ";");
      nErrorHelper--;
      fw.Altf("      SET inout_szError = CONCAT_WS(N'', inout_szError, N'Input in_nCallerId was incorrect. (', in_nCallerId, N')');");
      fw.Altf("    END IF;");
      if (spType != "Insert") {
        fw.Altf("    IF IFNULL(in_nId, -1) < 0 THEN");
        fw.Altf("      SET inout_nError  = " + nErrorHelper + ";");
        nErrorHelper--;
        fw.Altf("      SET inout_szError = CONCAT_WS(N'', inout_szError, N'Input in_nId was incorrect. (', in_nId, N')');");
        fw.Altf("    END IF;");
      }
      if (spType == "Update") {
        // Inputs muessen nicht ueberprueft werden. Falls NULL werden sie spaeter durch die alten Werte ersetzt.
        return;
      }
      if (spType == "Delete") {
        fw.Altf("    IF IFNULL(in_nDeleteMode, -1) < 0 THEN");
        fw.Altf("      SET inout_nError  = " + nErrorHelper + ";");
        nErrorHelper--;
        fw.Altf("      SET inout_szError = CONCAT_WS(N'', inout_szError, N'Input in_nDeleteMode was incorrect. (', in_nDeleteMode, N')');");
        fw.Altf("    END IF;");
      }
      else {
        CSV_Spalte tmpCol = null;
        for (int i = table.IsSys ? 1 : 2; i < table.Columns.Count; i++) {
          tmpCol = table.Columns[i];
          if (tmpCol.NotNull) {
            fw.Altf("    IF IFNULL (in_" + tmpCol.Attribut + ", " + tmpCol.GetSQLInputCheckComparison() + " THEN");
            fw.Altf("      SET inout_nError  = " + nErrorHelper + ";");
            fw.Altf("      SET inout_szError = CONCAT_WS(N'', inout_szError, N'Input in_" + tmpCol.Attribut + " was incorrect. (', in_" + tmpCol.Attribut + ", N')');");
            fw.Altf("    END IF;");
          }
          nErrorHelper--;
        }
      }
      fw.Altf("IF inout_szError = 'ERROR at input: ' THEN");
      fw.Altf("  SET inout_szError = N'';");
      fw.Altf("END IF;");
    }
    private void Create_MainPartStart(FileWriter fw, CSV_Tabelle table, string spType) {
      fw.Altf("    DECLARE EXIT      HANDLER FOR 1062      -- ER_DUP_ENTRY");
      fw.Altf("        SET inout_nError = -140;");
      fw.Altf("    DECLARE EXIT      HANDLER FOR 45000     -- User defined error");
      fw.Altf("        SET inout_nError = -141;");
      fw.Altf("    DECLARE EXIT      HANDLER FOR 1106      -- ER_UNKNOWN_PROCEDURE");
      fw.Altf("        SET inout_nError = -142;");
      fw.Altf("    DECLARE EXIT      HANDLER FOR 1122      -- ER_CANT_FIND_UDF");
      fw.Altf("        SET inout_nError = -143;");
      fw.Altf("    --  -End-: Declaration of handlers");
      fw.Altf("");
      fw.Altf("    --  Start: Error check at beginning of main part");
      fw.Altf("    IF IFNULL(inout_nError, 0) <> 0 THEN");
      fw.Altf("      SET nSuccess = 0;");
      fw.Altf("      LEAVE mainPart;");
      fw.Altf("    END IF;");
      fw.Altf("    SET inout_nError  = 0;");
      fw.Altf("    SET inout_szError = N'';");
      fw.Altf("    -- End:   Error check at beginning of main part");
      fw.Altf("");
      fw.Altf("    -- Start: Check right");
      fw.Altf("    -- Start: Debug");
      fw.Altf("    IF in_nDebug <> 0 THEN");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'Ueberpruefen ob der Nutzer mit der Id ', in_nCallerId, N' das benoetigte Recht besitzt.'));");
      fw.Altf("    END IF;");
      fw.Altf("    -- -End-: Debug");
      fw.Altf("    SET iRight = fnSysGetRight(szRightDescription);");
      fw.Altf("    IF iRight IS NULL THEN");
      fw.Altf("      -- Start: Debug");
      fw.Altf("      IF in_nDebug <> 0 THEN");
      fw.Altf("        SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'Das Recht ', szRightDescription, N' wird neu erstellt.', N''));");
      fw.Altf("      END IF;");
      fw.Altf("      -- -End-: Debug");
      fw.Altf("      -- create right");
      fw.Altf("      INSERT INTO tblSysText (szDescription) VALUES (szRightDescription);");
      fw.Altf("      SET iRight = LAST_INSERT_ID();");
      fw.Altf("      INSERT INTO tblSysLanguageText (iDescription, iLanguage, szText, szToken) VALUES (iRight, 1, CONCAT_WS(N'', N'Das Recht fuer die Prozedur ', szProcedureName, N''), N'act_" + spType + "SP');");
      fw.Altf("      INSERT INTO tblSysAction (iDescription, iParentAction, szComment) VALUES (iRight, NULL, CONCAT_WS(N'', N'Das Recht fuer die Prozedur ', szProcedureName, N''));");
      fw.Altf("      SET iRight = LAST_INSERT_ID();");
      fw.Altf("    END IF;");
      fw.Altf("    IF (fnSysCheckRight(in_nCallerId, iRight)) <> 1 THEN");
      fw.Altf("      -- Start: Debug");
      fw.Altf("      IF in_nDebug <> 0 THEN");
      fw.Altf("        SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'Der angegebene Benutzer besitzt nicht das Recht fuer diese Prozedur.', N''));");
      fw.Altf("      END IF;");
      fw.Altf("      -- -End-: Debug");
      fw.Altf("      SET nSuccess = 0;");
      fw.Altf("      SET inout_nError = -109;");
      fw.Altf("      SET inout_szError = CONCAT_WS(N'', N'Der angegebene Benutzer (Id: ', in_nCallerId, N') besitzt nicht das Recht fuer die Prozedur <', szRightDescription, N'>.');");
      fw.Altf("      LEAVE mainPart;");
      fw.Altf("    END IF;");
    }
    private void Create_CheckForTarget(FileWriter fw, CSV_Tabelle table, string spType) {
      if (spType == "Insert") {
        List<string> uniques = new List<string>();
        CSV_Spalte tmpCol = null;
        for (int i = table.DataName.StartsWith("Sys") ? 1 : 2; i < table.Columns.Count; i++) {
          tmpCol = table.Columns[i];
          if (tmpCol.MakeUnique.Length > 0) {
            if (!uniques.Contains(tmpCol.MakeUnique)) {
              uniques.Add(tmpCol.MakeUnique);
            }
          }
        }
        for (int u = 0; u < uniques.Count; u++) {
          fw.Altf("    IF EXISTS (SELECT 1");
          fw.Altf("               FROM " + table.TableName);
          fw.Altf("               WHERE 1=1");
          for (int i = table.DataName.StartsWith("Sys") ? 1 : 2; i < table.Columns.Count; i++) {
            tmpCol = table.Columns[i];
            if (tmpCol.MakeUnique == uniques[u]) {
              if (tmpCol.NotNull) {
                fw.Altf("                 AND " + tmpCol.Attribut + " = in_" + tmpCol.Attribut);
              }
              else {
                fw.Altf("                 AND ( " + tmpCol.Attribut + " = in_" + tmpCol.Attribut);
                fw.Altf("                    OR " + tmpCol.Attribut + " IS NULL)");
              }
            }
          }
          fw.Altf("               LIMIT 1");
          fw.Altf("    )");
          fw.Altf("    THEN");
          fw.Altf("      SET inout_nError    = -106;");
          fw.Altf("      SET inout_szError   = N'" + table.DataName + " already exists.';");
          fw.Altf("      SET nSuccess        = 0;");
          fw.Altf("      SET out_nId = (SELECT nId");
          fw.Altf("        FROM " + table.TableName);
          fw.Altf("        WHERE 1=1");
          for (int i = table.DataName.StartsWith("Sys") ? 1 : 2; i < table.Columns.Count; i++) {
            tmpCol = table.Columns[i];
            if (tmpCol.MakeUnique == uniques[u]) {
              if (tmpCol.NotNull) {
                fw.Altf("                 AND " + tmpCol.Attribut + " = in_" + tmpCol.Attribut);
              }
              else {
                fw.Altf("                 AND ( " + tmpCol.Attribut + " = in_" + tmpCol.Attribut);
                fw.Altf("                    OR " + tmpCol.Attribut + " IS NULL)");
              }
            }
          }
          fw.Altf("        LIMIT 1");
          fw.Altf("      );");
          fw.Altf("      -- Start: Debug");
          fw.Altf("      IF in_nDebug <> 0 THEN");
          fw.Altf("        SET @func = fnSysPrintDebug(in_nDebugDepth, N'');");
          fw.Altf("        SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'" + table.DataName + " already exists.', N''));");
          for (int i = table.DataName.StartsWith("Sys") ? 1 : 2; i < table.Columns.Count && table.Columns[i].MakeUnique == uniques[u]; i++) {
            tmpCol = table.Columns[i];
            fw.Altf("        SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_" + tmpCol.Attribut + " = ', in_" + tmpCol.Attribut + "));");
          }
          fw.Altf("      END IF;");
          fw.Altf("      -- -End-: Debug");
          fw.Altf("      LEAVE mainPart;");
          fw.Altf("    END IF;");
        }
      }
      else {
        fw.Altf("    IF NOT EXISTS (SELECT 1");
        fw.Altf("      FROM " + table.TableName);
        fw.Altf("      WHERE nId = in_nId");
        fw.Altf("    )");
        fw.Altf("    THEN");
        fw.Altf("      SET inout_nError    = -105;");
        fw.Altf("      SET inout_szError   = CONCAT_WS(N'', N'" + table.DataName + "<Id:', in_nId, N'> does not exist.', N'');");
        fw.Altf("      SET nSuccess        = 0;");
        fw.Altf("      SET out_nId = -105;");
        fw.Altf("      -- Start: Debug");
        fw.Altf("      IF in_nDebug <> 0 THEN");
        fw.Altf("        SET @func = fnSysPrintDebug(in_nDebugDepth, N'');");
        fw.Altf("        SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'" + table.DataName + "<Id:', in_nId, N'> does not exist.', N''));");
        fw.Altf("      END IF;");
        fw.Altf("      -- -End-: Debug");
        fw.Altf("      LEAVE mainPart;");
        fw.Altf("    END IF;");
      }
    }
    private void Create_CheckForUniqueConstraints(FileWriter fw, CSV_Tabelle table, string spType) {
      if (spType != "Update") {
        throw new Exception("Falscher parameter");
      }
      List<string> uniques = new List<string>();
      CSV_Spalte tmpCol = null;
      for (int i = table.DataName.StartsWith("Sys") ? 1 : 2; i < table.Columns.Count; i++) {
        tmpCol = table.Columns[i];
        if (tmpCol.MakeUnique.Length > 0) {
          if (!uniques.Contains(tmpCol.MakeUnique)) {
            uniques.Add(tmpCol.MakeUnique);
          }
        }
      }
      for (int u = 0; u < uniques.Count; u++) {
        fw.Altf("    IF EXISTS (SELECT 1");
        fw.Altf("               FROM " + table.TableName);
        fw.Altf("               WHERE 1=1");
        for (int i = table.DataName.StartsWith("Sys") ? 1 : 2; i < table.Columns.Count; i++) {
          tmpCol = table.Columns[i];
          if (tmpCol.MakeUnique == uniques[u]) {
            if (tmpCol.NotNull) {
              fw.Altf("                 AND " + tmpCol.Attribut + " = in_" + tmpCol.Attribut);
            }
            else {
              fw.Altf("                 AND ( " + tmpCol.Attribut + " = in_" + tmpCol.Attribut);
              fw.Altf("                    OR " + tmpCol.Attribut + " IS NULL)");
            }
          }
        }
        fw.Altf("                 AND nId <> in_nId");
        fw.Altf("               LIMIT 1");
        fw.Altf("    )");
        fw.Altf("    THEN");
        fw.Altf("      SET inout_nError    = -106;");
        fw.Altf("      SET inout_szError   = N'" + table.DataName + " with the given constraints already exists.';");
        fw.Altf("      SET nSuccess        = 0;");
        fw.Altf("      SET out_nId = (SELECT nId");
        fw.Altf("        FROM " + table.TableName);
        fw.Altf("        WHERE 1=1");
        for (int i = table.DataName.StartsWith("Sys") ? 1 : 2; i < table.Columns.Count; i++) {
          tmpCol = table.Columns[i];
          if (tmpCol.MakeUnique == uniques[u]) {
            if (tmpCol.NotNull) {
              fw.Altf("                 AND " + tmpCol.Attribut + " = in_" + tmpCol.Attribut);
            }
            else {
              fw.Altf("                 AND ( " + tmpCol.Attribut + " = in_" + tmpCol.Attribut);
              fw.Altf("                    OR " + tmpCol.Attribut + " IS NULL)");
            }
          }
        }
        fw.Altf("          AND nId <> in_nId");
        fw.Altf("        LIMIT 1");
        fw.Altf("      );");
        fw.Altf("      -- Start: Debug");
        fw.Altf("      IF in_nDebug <> 0 THEN");
        fw.Altf("        SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'" + table.DataName + " with the given constraints already exists.', N''));");
        for (int i = table.DataName.StartsWith("Sys") ? 1 : 2; i < table.Columns.Count && table.Columns[i].MakeUnique == uniques[u]; i++) {
          tmpCol = table.Columns[i];
          fw.Altf("        SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_" + tmpCol.Attribut + " = ', in_" + tmpCol.Attribut + "));");
        }
        fw.Altf("      END IF;");
        fw.Altf("      -- -End-: Debug");
        fw.Altf("      LEAVE mainPart;");
        fw.Altf("    END IF;");
      }
    }
    private void Create_InsertNewIdSysEnumElement(FileWriter fw, CSV_Tabelle table, string spType) {
      if (spType != "Insert") {
        throw new Exception("Falscher parameter");
      }
      fw.Altf("    SET nNewId = (SELECT  CASE  WHEN see.nId IS NULL         THEN enu.nOffset");
      fw.Altf("                                WHEN see.nId < enu.nOffset   THEN enu.nOffset");
      fw.Altf("                                ELSE see.nId + 1");
      fw.Altf("                          END");
      fw.Altf("                  FROM tblSysEnum         enu");
      fw.Altf("                  LEFT");
      fw.Altf("                  JOIN tblSysEnumElement  see ON enu.nId = see.iEnum");
      fw.Altf("                  WHERE enu.nId = in_iEnum");
      fw.Altf("                  ORDER BY see.nId DESC");
      fw.Altf("                  LIMIT 1);");
      fw.Altf("    IF nNewId >= (SELECT nOffset+nMaxElements FROM tblSysEnum WHERE nId=in_iEnum LIMIT 1) THEN");
      fw.Altf("      SET inout_nError = -9998;");
      fw.Altf("      SET inout_szError = CONCAT_WS(N'', N'Die Enumeration nutzt bereits ihre hoechste Enumerationselementsid. Die neue Id waere ', nNewId, N'. MaxId: ', (SELECT nOffset+nMaxElements FROM tblSysEnum WHERE nId=in_iEnum LIMIT 1));");
      fw.Altf("      SET nSuccess = 0;");
      fw.Altf("      -- Start: Debug");
      fw.Altf("      IF in_nDebug <> 0 THEN");
      fw.Altf("        SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'Die Enumeration nutzt bereits ihre hoechste Enumerationselementsid. Die neue Id waere ', nNewId, N'.'));");
      fw.Altf("      END IF;");
      fw.Altf("      -- -End-: Debug");
      fw.Altf("      LEAVE mainPart;");
      fw.Altf("    END IF;");
      fw.Altf("    IF EXISTS ( SELECT 1 FROM tblSysEnum WHERE nId <> in_iEnum AND nOffset <= nNewId AND (nOffset+nMaxElements) >= (nNewId+1)) THEN");
      fw.Altf("      SET inout_nError = -9999;");
      fw.Altf("      SET inout_szError = CONCAT_WS(N'', N'Die neue Id wuerde sich mit einer anderen Enumeration ueberschneiden. Die neue Id waere ', nNewId, N'.');");
      fw.Altf("      SET nSuccess = 0;");
      fw.Altf("      -- Start: Debug");
      fw.Altf("      IF in_nDebug <> 0 THEN");
      fw.Altf("        SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'Die neue Id wuerde sich mit einer anderen Enumeration ueberschneiden. Die neue Id waere ', nNewId, N'.'));");
      fw.Altf("      END IF;");
      fw.Altf("      -- -End-: Debug");
      fw.Altf("      LEAVE mainPart;");
      fw.Altf("    END IF;");
    }
    private void Create_CheckNecessities(FileWriter fw, CSV_Tabelle table, string spType) {
      if (spType != "Insert" && spType != "Update") {
        throw new Exception("Falscher parameter");
      }
      CSV_Spalte tmpCol = null;
      for (int i = table.DataName.StartsWith("Sys") ? 1 : 2; i < table.Columns.Count; i++) {
        tmpCol = table.Columns[i];
        if (tmpCol.ForeignKeyTo.Length > 0 && tmpCol.NotNull) {
          string foreign = tmpCol.ForeignKeyTo;
          string fTable = foreign.Split('.')[0];
          string fColumn = foreign.Split('.')[1];
          string errMsg = "CONCAT_WS(N'', N'Kein gueltiger Datenpunkt in Tabelle " + fTable + " fuer " + fColumn + "=', in_" + tmpCol.Attribut + ", N'.')";
          fw.Altf("    -- Reference to " + fTable);
          fw.Altf("    IF NOT EXISTS ( SELECT 1 FROM " + fTable);
          fw.Altf("                    WHERE " + fColumn + " = in_" + tmpCol.Attribut);
          fw.Altf("                    LIMIT 1");
          fw.Altf("    ) THEN");
          fw.Altf("      SET inout_nError = -105;");
          fw.Altf("      SET inout_szError = " + errMsg + ";");
          fw.Altf("      SET nSuccess = 0;");
          fw.Altf("      -- Start: Debug");
          fw.Altf("      IF in_nDebug <> 0 THEN");
          fw.Altf("        SET @func = fnSysPrintDebug(in_nDebugDepth, " + errMsg + ");");
          fw.Altf("      END IF;");
          fw.Altf("      -- -End-: Debug");
          fw.Altf("      LEAVE mainPart;");
          fw.Altf("    END IF;");
        }
      }
    }
    private void Create_LoadOrigValues(FileWriter fw, CSV_Tabelle table, string spType) {
      if (spType != "Delete") {
        throw new Exception("Falscher parameter");
      }
      fw.Altf("    INSERT tblTdDelete" + table.DataName + " (");
      CSV_Spalte tmpCol = null;
      string line;
      for (int i = 0; i < table.Columns.Count; i++) {
        tmpCol = table.Columns[i];
        line = "      " + (i == 0 ? "  " : ", ");
        line += tmpCol.Attribut;
        fw.Altf(line);
      }
      fw.Altf("    )");
      fw.Altf("    SELECT  nId");
      for (int i = 1; i < table.Columns.Count; i++) {
        tmpCol = table.Columns[i];
        fw.Altf("          , " + tmpCol.Attribut);
      }
      fw.Altf("    FROM " + table.TableName);
      fw.Altf("    WHERE nId = in_nId;");
    }
    private void Create_Insert(FileWriter fw, CSV_Tabelle table, string spType) {
      if (spType != "Insert") {
        throw new Exception("Falscher parameter");
      }
      fw.Altf("    INSERT INTO " + table.TableName + " (");
      if (table.TableName != "tblSysEnumElement") {
        CSV_Spalte tmpCol = null;
        string line;
        for (int i = table.IsSys ? 1 : 2; i < table.Columns.Count; i++) {
          tmpCol = table.Columns[i];
          line = "      " + (i == (table.IsSys ? 1 : 2) ? "  " : ", ");
          line += tmpCol.Attribut;
          fw.Altf(line);
        }
        fw.Altf("    )");
        fw.Altf("    VALUES (");
        for (int i = table.IsSys ? 1 : 2; i < table.Columns.Count; i++) {
          tmpCol = table.Columns[i];
          line = "      " + (i == (table.IsSys ? 1 : 2) ? "  " : ", ");
          line += "in_" + tmpCol.Attribut;
          fw.Altf(line);
        }
      }
      else {
        fw.Altf("        nId");
        fw.Altf("      , iEnum");
        fw.Altf("      , iDescription");
        fw.Altf("    )");
        fw.Altf("    VALUES (");
        fw.Altf("        nNewId");
        fw.Altf("      , in_iEnum");
        fw.Altf("      , in_iDescription");
      }
      fw.Altf("    );");
    }
    private void Create_Update(FileWriter fw, CSV_Tabelle table, string spType) {
      if (spType != "Update") {
        throw new Exception("Falscher parameter");
      }
      fw.Altf("    -- Load original values");
      CSV_Spalte tmpCol = null;
      for (int i = table.IsSys ? 1 : 2; i < table.Columns.Count; i++) {
        tmpCol = table.Columns[i];
        fw.Altf("    SET @old_" + tmpCol.Attribut + " = (SELECT " + tmpCol.Attribut + " FROM " + table.TableName + " WHERE nId = in_nId);");
      }
      fw.Altf("");
      fw.Altf("    -- Set empty input parameters");
      tmpCol = null;
      string tmp = "";
      for (int i = table.IsSys ? 1 : 2; i < table.Columns.Count; i++) {
        tmpCol = table.Columns[i];
        tmp = "    SET in_" + tmpCol.Attribut + " = IFNULL(in_" + tmpCol.Attribut + ", @old_" + tmpCol.Attribut + ");";
        if (tmpCol.NotNull) {
          fw.Altf(tmp);
        }
        else {
          fw.Altf("-- NULLABLE: " + tmp);
        }
      }
      fw.Altf("");
      fw.Altf("    -- Update " + table.DataName);
      fw.Altf("    UPDATE " + table.TableName);
      if (table.IsSys) {
        tmpCol = null;
        string line = "";
        for (int i = 1; i < table.Columns.Count; i++) {
          tmpCol = table.Columns[i];
          line = i == 1 ? "    SET " : "      , ";
          line += tmpCol.Attribut + " = in_" + tmpCol.Attribut;
          fw.Altf(line);
        }
      }
      else {
        fw.Altf("    SET dtAendZeit = NOW()");
        tmpCol = null;
        for (int i = 2; i < table.Columns.Count; i++) {
          tmpCol = table.Columns[i];
          fw.Altf("      , " + tmpCol.Attribut + " = in_" + tmpCol.Attribut);
        }
      }
      fw.Altf("    WHERE nId = in_nId;");
    }
    private void Create_Delete(FileWriter fw, CSV_Tabelle table, string spType) {
      List<CSV_Spalte> referencingColumns = AC.GetReferencingColumns(table);
      if (spType != "Delete") {
        throw new Exception("Falscher parameter");
      }
      // DeleteMode: 0
      fw.Altf("    -- DeleteMode: 0 -> deleteSave (dont delete if reference exists)");
      if (referencingColumns.Count > 0) {
        fw.Altf("    IF in_nDeleteMode = 0 THEN");
        foreach (CSV_Spalte col in referencingColumns) {
          fw.Altf("      -- check if reference exists in " + col.Tabelle + " on column " + col.Attribut);
          fw.Altf("      IF EXISTS ( SELECT 1 FROM vw" + col.Tabelle.Substring(3) + "_full WHERE " + col.Attribut + " = in_nId LIMIT 1) THEN");
          fw.Altf("        SET inout_nError = -111;");
          fw.Altf("        SET inout_szError = N'At least one reference on this dataset exists in " + col.Tabelle + ".';");
          fw.Altf("        IF in_nDebug <> 0 THEN");
          fw.Altf("          SET @func = fnSysPrintDebug(in_nDebugDepth, N'At least one reference on this dataset exists in " + col.Tabelle + ".');");
          fw.Altf("          SET @func = fnSysPrintDebug(in_nDebugDepth, N'    -> quit deleting');");
          fw.Altf("        END IF;");
          fw.Altf("        LEAVE mainPart;");
          fw.Altf("      END IF;");
        }
        fw.Altf("    END IF;");
      }
      else {
        fw.Altf("    -- No referencing columns on table " + table.TableName);
      }
      fw.Altf("");
      fw.Altf("    -- DeleteMode: 1 -> deleteSetDefault");
      if (referencingColumns.Count > 0) {
        fw.Altf("    IF in_nDeleteMode = 1 THEN");
        foreach (CSV_Spalte col in referencingColumns) {
          fw.Altf("      -- update referencing column " + col.Attribut + " from " + col.Tabelle + "");
          fw.Altf("      IF in_nDebug <> 0 THEN");
          fw.Altf("        SET @func = fnSysPrintDebug(in_nDebugDepth, N'---------------------------------------------------');");
          fw.Altf("        SET @func = fnSysPrintDebug(in_nDebugDepth, N'Update referencing column " + col.Attribut + " from " + col.Tabelle + "');");
          fw.Altf("      END IF;");
          fw.Altf("      UPDATE " + col.Tabelle);
          if (col.Tabelle.Substring(3).StartsWith("AP")) {
            fw.Altf("      SET eDatenstatus = 5                -- Datenstatus auf <Verweis geloescht> setzen, da ein ungueltiger Verweis entsteht");
          }
          else {
            fw.Altf("      SET " + col.Attribut + " = -1           -- Verweis auf den zu loeschenden Datensatz verwerfen");
          }
          fw.Altf("      WHERE " + col.Attribut + " = in_nId;");
          fw.Altf("      IF in_nDebug <> 0 THEN");
          fw.Altf("        SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'Updated rows: ', ROW_COUNT()));");
          fw.Altf("      END IF;");
          //  ---- darf nicht funktionieren, da die aufgerufene
          //       SP auf vorhandensein des datensatzes mit der Id -1 schaut
          //fw.Altf("      OPEN cur_" + col.Tabelle + "_" + col.Attribut + ";");
          //fw.Altf("      update_" + col.Tabelle + "_" + col.Attribut + ": LOOP");
          //fw.Altf("        FETCH cur_" + col.Tabelle + "_" + col.Attribut + " INTO c_Id;");
          //fw.Altf("        IF done THEN");
          //fw.Altf("          LEAVE update_" + col.Tabelle + "_" + col.Attribut + ";");
          //fw.Altf("        END IF;");
          //fw.Altf("        IF in_nDebug <> 0 THEN");
          //fw.Altf("          SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'  -> Updating "+col.Tabelle+" with Id ', c_Id, N''));");
          //fw.Altf("        END IF;");
          //fw.Altf("        UPDATE "+col.Tabelle);
          //fw.Altf("        SET " + col.Attribut + " = -1");
          //string spName = col.Tabelle.Substring(3);
          //if (spName.StartsWith("Sys")) {
          //  spName = "spSysUpdate"+spName;
          //}
          //else {
          //  spName = "spAPUpdate"+spName.Substring(2);
          //}
          //CSV_Tabelle ftab = AC.GetTable(col.Tabelle);
          //fw.Altf("        CALL "+spName+"(");
          //fw.Altf("              in_nCallerId");
          //fw.Altf("            , c_Id");
          //CSV_Spalte tmpForeignCol = null;
          //for (int i = table.IsSys ? 1 : 2; i < ftab.Columns.Count; i++) {
          //  tmpForeignCol = ftab.Columns[i];
          //  string line = "            , ";
          //  if(tmpForeignCol.Attribut == "eDatenstatus") {
          //    line += "4           -- Datensatz ist unbestimmt, da ungueltiger Wert";
          //  }
          //  else if(tmpForeignCol.Art == table.DataName) {
          //    line += "-1          -- <- das ist das update";
          //  }
          //  else {
          //    line += "NULL        -- Der alte Wert wird weiter genutzt";
          //  }
          //  fw.Altf(line);
          //}
          //fw.Altf("            , nResultId           -- ");
          //fw.Altf("            , 0                   -- no resultset");
          //fw.Altf("            , nDebugCall          -- ");
          //fw.Altf("            , (in_nDebugDepth+1)  -- ");
          //fw.Altf("            , nErrorCall          -- ");
          //fw.Altf("            , szErrorCall         -- ");
          //fw.Altf("        );");
          //fw.Altf("        IF in_nDebug <> 0 THEN");
          //fw.Altf("          IF nErrorCall <> 0 THEN");
          //fw.Altf("            SET inout_nError  = nErrorCall;");
          //fw.Altf("            SET inout_szError = szErrorCall;");
          //fw.Altf("            SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'    Error: ',nErrorCall, N' -> ', szErrorCall, N''));");
          //fw.Altf("            LEAVE update_" + col.Tabelle + "_" + col.Attribut + ";");
          //fw.Altf("          END IF;");
          //fw.Altf("          IF nErrorCall = 0 THEN");
          //fw.Altf("            SET inout_nError  = nErrorCall;");
          //fw.Altf("            SET inout_szError = szErrorCall;");
          //fw.Altf("            SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'    Success: ', szErrorCall, N''));");
          //fw.Altf("          END IF;");
          //fw.Altf("        END IF;");
          //fw.Altf("      END LOOP;");
          //fw.Altf("      CLOSE cur_" + col.Tabelle + "_" + col.Attribut + ";");
          //fw.Altf("      IF in_nDebug <> 0 THEN");
          //fw.Altf("        SET @func = fnSysPrintDebug(in_nDebugDepth, N'');");
          //fw.Altf("      END IF;");
          //fw.Altf("      IF nErrorCall <> 0 THEN");
          //fw.Altf("        SET inout_nError  = nErrorCall;");
          //fw.Altf("        SET inout_szError = szErrorCall;");
          //fw.Altf("        LEAVE mainPart;");
          //fw.Altf("      END IF;");
        }
        fw.Altf("    END IF;");
      }
      else {
        fw.Altf("    -- No referencing columns on table " + table.TableName);
      }
      fw.Altf("");
      fw.Altf("    -- DeleteMode: 2 -> deleteCascade");
      if (referencingColumns.Count > 0) {
        fw.Altf("    IF in_nDeleteMode = 2 THEN");
        foreach (CSV_Spalte col in referencingColumns) {
          string cursorName = "cur_" + col.Tabelle + "_" + col.Attribut;
          string loopName = "delete_" + col.Tabelle + "_" + col.Attribut;
          fw.Altf("      -- delete dataset with referencing column " + col.Attribut + " from " + col.Tabelle);
          fw.Altf("      IF in_nDebug <> 0 THEN");
          fw.Altf("        SET @func = fnSysPrintDebug(in_nDebugDepth, N'---------------------------------------------------');");
          fw.Altf("        SET @func = fnSysPrintDebug(in_nDebugDepth, N'Delete referencing datasets from " + col.Tabelle + "');");
          fw.Altf("      END IF;");
          fw.Altf("      SET done = FALSE;");
          fw.Altf("      OPEN " + cursorName + ";");
          fw.Altf("      " + loopName + ": LOOP");
          fw.Altf("        FETCH " + cursorName + " INTO c_Id;");
          fw.Altf("        IF done THEN");
          fw.Altf("          IF in_nDebug <> 0 THEN SET @func = fnSysPrintDebug(in_nDebugDepth, N'Done'); END IF;");
          fw.Altf("          LEAVE " + loopName + ";");
          fw.Altf("        END IF;");
          fw.Altf("        IF in_nDebug <> 0 THEN");
          fw.Altf("          SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'Deleting at " + col.Tabelle + " with nId ', c_Id, N''));");
          fw.Altf("        END IF;");
          string spName = col.Tabelle.Substring(3);
          if (spName.StartsWith("Sys")) {
            spName = "spSysDelete" + spName;
          }
          else {
            spName = "spAPDelete" + spName.Substring(2);
          }
          CSV_Tabelle ftab = AC.GetTable(col.Tabelle);
          fw.Altf("        CALL " + spName + "(");
          fw.Altf("              in_nCallerId");
          fw.Altf("            , c_Id");
          fw.Altf("            , in_nDeleteMode");
          fw.Altf("            , nResultId           -- ");
          fw.Altf("            , 0                   -- no resultset");
          fw.Altf("            , nDebugCall          -- ");
          fw.Altf("            , (in_nDebugDepth+1)  -- ");
          fw.Altf("            , nErrorCall          -- ");
          fw.Altf("            , szErrorCall         -- ");
          fw.Altf("        );");
          fw.Altf("        IF in_nDebug <> 0 THEN");
          fw.Altf("          IF nErrorCall <> 0 THEN");
          fw.Altf("            SET inout_nError  = nErrorCall;");
          fw.Altf("            SET inout_szError = szErrorCall;");
          fw.Altf("            SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'    Error: ',nErrorCall, N' -> ', szErrorCall, N''));");
          fw.Altf("            LEAVE " + loopName + ";");
          fw.Altf("          END IF;");
          fw.Altf("          IF nErrorCall = 0 THEN");
          fw.Altf("            SET inout_nError  = nErrorCall;");
          fw.Altf("            SET inout_szError = szErrorCall;");
          fw.Altf("            SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'    Success: ', szErrorCall, N''));");
          fw.Altf("          END IF;");
          fw.Altf("        END IF;");
          fw.Altf("        IF nErrorCall <> 0 THEN");
          fw.Altf("          SET inout_nError  = nErrorCall;");
          fw.Altf("          SET inout_szError = szErrorCall;");
          fw.Altf("          SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'    Error: ',nErrorCall, N' -> ', szErrorCall, N''));");
          fw.Altf("          LEAVE " + loopName + ";");
          fw.Altf("        END IF;");
          fw.Altf("      END LOOP;");
          fw.Altf("      CLOSE " + cursorName + ";");
          fw.Altf("      IF in_nDebug <> 0 THEN");
          fw.Altf("        SET @func = fnSysPrintDebug(in_nDebugDepth, N'');");
          fw.Altf("      END IF;");
          fw.Altf("      IF nErrorCall <> 0 THEN");
          fw.Altf("        SET inout_nError  = nErrorCall;");
          fw.Altf("        SET inout_szError = szErrorCall;");
          fw.Altf("        LEAVE mainPart;");
          fw.Altf("      END IF;");
        }
        fw.Altf("    END IF;");
      }
      else {
        fw.Altf("    -- No referencing columns on table " + table.TableName);
      }
      fw.Altf("");
      fw.Altf("    -- Delete dataset from " + table.TableName);
      fw.Altf("    DELETE FROM " + table.TableName);
      fw.Altf("    WHERE nId = in_nId;");
    }
    private void Create_Evaluation(FileWriter fw, CSV_Tabelle table, string spType) {
      if (spType == "Delete") {
        fw.Altf("    SET nRowCount = ROW_COUNT();");
        fw.Altf("    SET out_nId = in_nId;");
        fw.Altf("    SET nSuccess = 1;");
        fw.Altf("    SET inout_szError = CONCAT_WS(N'', N'Deletion of " + table.DataName + " with nId = ', in_nId);");
        CSV_Spalte tmpCol = null;
        for (int i = table.IsSys ? 1 : 2; i < table.Columns.Count; i++) {
          tmpCol = table.Columns[i];
          string line = "    SET inout_szError = CONCAT_WS(N'', inout_szError,";
          line += " N', " + tmpCol.Attribut + " = ',";
          line += " (SELECT " + tmpCol.Attribut + " FROM tblTdDelete" + table.DataName + " WHERE nId = in_nId LIMIT 1) );";
          fw.Altf(line);
        }
        fw.Altf("    SET inout_szError = CONCAT_WS(N'', inout_szError, N'.');");
        fw.Altf("    --  Start: Debug");
        fw.Altf("    IF in_nDebug <> 0 THEN");
        fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, N'');");
        fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'Deleted " + table.DataName + "', N''));");
        fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'     nRowCount: ', nRowCount));");
        fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'     out_nId:   ', out_nId));");
        fw.Altf("    END IF;");
        fw.Altf("    --  -End-: Debug");
      }
      if (spType == "Update") {
        fw.Altf("    SET nRowCount = ROW_COUNT();");
        fw.Altf("    SET nSuccess = 1;");
        fw.Altf("    SET out_nId = in_nId;");
        fw.Altf("    SET inout_szError = CONCAT_WS(N'', N'Update of " + table.DataName + " with nId = ', out_nId);");
        CSV_Spalte tmpCol = null;
        for (int i = table.IsSys ? 1 : 2; i < table.Columns.Count; i++) {
          tmpCol = table.Columns[i];
          string line = "    IF in_" + tmpCol.Attribut + " != @old_" + tmpCol.Attribut + " OR (in_" + tmpCol.Attribut + " IS NULL XOR @old_" + tmpCol.Attribut + " IS NULL) THEN SET inout_szError = CONCAT_WS(N'', inout_szError,";
          line += " N', " + tmpCol.Attribut + ": ',";
          line += " @old_" + tmpCol.Attribut + ", N' -> ',";
          line += " in_" + tmpCol.Attribut + "); END IF;";
          fw.Altf(line);
        }
        fw.Altf("    SET inout_szError = CONCAT_WS(N'', inout_szError, N'.');");
        fw.Altf("    --  Start: Debug");
        fw.Altf("    IF in_nDebug <> 0 THEN");
        fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'Updated " + table.DataName + "', N''));");
        fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'     nRowCount: ', nRowCount));");
        fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'     out_nId:   ', out_nId));");
        fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'     Content:   ', inout_szError));");
        fw.Altf("    END IF;");
        fw.Altf("    --  -End-: Debug");
      }
      if (spType == "Insert") {
        fw.Altf("    SET nRowCount = ROW_COUNT();");
        fw.Altf("    SET nSuccess = 1;");
        fw.Altf("    SET out_nId = ( SELECT nId");
        fw.Altf("                    FROM " + table.TableName);
        fw.Altf("                    WHERE 1=1");
        CSV_Spalte tmpCol = null;
        for (int i = table.IsSys ? 1 : 2; i < table.Columns.Count; i++) {
          tmpCol = table.Columns[i];
          if (tmpCol.MakeUnique.Length > 0) {
            if (tmpCol.NotNull) {
              fw.Altf("                    AND " + tmpCol.Attribut + " = in_" + tmpCol.Attribut);
            }
            else {
              fw.Altf("                    AND ( " + tmpCol.Attribut + " = in_" + tmpCol.Attribut);
              fw.Altf("                       OR " + tmpCol.Attribut + " IS NULL)");
            }
          }
        }
        fw.Altf("                    LIMIT 1");
        fw.Altf("    );");
        fw.Altf("    SET inout_szError = CONCAT_WS(N'', N'Insert of " + table.DataName + " with nId = ', out_nId);");
        tmpCol = null;
        for (int i = table.IsSys ? 1 : 2; i < table.Columns.Count; i++) {
          tmpCol = table.Columns[i];
          string line = "    SET inout_szError = CONCAT_WS(N'', inout_szError,";
          line += " N', " + tmpCol.Attribut + " = ', in_" + tmpCol.Attribut + ");";
          fw.Altf(line);
        }
        fw.Altf("    SET inout_szError = CONCAT_WS(N'', inout_szError, N'.');");
        fw.Altf("    --  Start: Debug");
        fw.Altf("    IF in_nDebug <> 0 THEN");
        fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'Inserted " + table.DataName + "', N''));");
        fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'     nRowCount: ', nRowCount));");
        fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'     out_nId:   ', out_nId));");
        fw.Altf("    END IF;");
        fw.Altf("    --  -End-: Debug");
      }
    }
    private void Create_MainPartEnd(FileWriter fw, CSV_Tabelle table, string spType) {
      fw.Altf("    IF IFNULL(nRowCount, -1) < 1 THEN");
      fw.Altf("      SET inout_nError = -107;");
      fw.Altf("      SET inout_szError = N'" + spType + " of " + table.DataName + " failed. RowCount to low.';");
      fw.Altf("      SET nSuccess  = 0;");
      fw.Altf("      LEAVE mainPart;");
      fw.Altf("    END IF;");
      fw.Altf("    --  -End-: Error check at end of main part");
      fw.Altf("    ");
      fw.Altf("    -- Start: Secure and evaluate errors");
      fw.Altf("    IF IFNULL(out_nId, -1) < 0 THEN");
      fw.Altf("      SET inout_nError  = -108;");
      fw.Altf("      SET inout_szError = N'Id out_nId couldnt be resolved';");
      fw.Altf("      SET nSuccess  = 0;");
      fw.Altf("      LEAVE mainPart;");
      fw.Altf("    END IF;");
      fw.Altf("    -- -End-: Secure and evaluate errors");
      fw.Altf("    ");
      fw.Altf("    -- Start: End of procedure -> everything is ok");
      fw.Altf("    SET inout_nError  = 0;");
      fw.Altf("    SET inout_szError = CONCAT_WS(N'', inout_szError, N' -> No error occured.');");
    }
    private void Create_ErrorPart(FileWriter fw, CSV_Tabelle table, string spType) {
      string action = "1";
      if (spType == "Update") {
        action = "2";
      }
      if (spType == "Delete") {
        action = "3";
      }
      fw.Altf("  errorPart:BEGIN");
      fw.Altf("");
      fw.Altf("    --  Start: Check for errors");
      fw.Altf("    IF inout_nError = 0 THEN");
      fw.Altf("      LEAVE errorPart;");
      fw.Altf("    END IF;");
      fw.Altf("    --  -End-: Check for errors");
      fw.Altf("");
      fw.Altf("    set nSuccess = 0;");
      fw.Altf("");
      fw.Altf("    --  Start: Call fnSysAddToHistory");
      fw.Altf("    SET @func = fnSysAddToHistory (");
      fw.Altf("    /* in_nCallerId   */    0       -- 0:System, if anything else, function wont work");
      fw.Altf("    /* in_szTable     */  , N'" + table.TableName + "'");
      if (spType == "Insert") {
        fw.Altf("    /* in_nTableId    */  , out_nId");
      }
      else {
        fw.Altf("    /* in_nTableId    */  , in_nId");
      }
      fw.Altf("    /* in_iAction     */  , " + action + "");
      fw.Altf("    /* in_szFunction  */  , N'" + spType + " " + table.DataName + "'");
      fw.Altf("    /* in_nError      */  , inout_nError");
      fw.Altf("    /* in_szComment   */  , CONCAT_WS(N'', N'" + spType + " at " + table.TableName + " failed: ', inout_szError)");
      fw.Altf("    /* in_nUserId     */  , in_nCallerId");
      fw.Altf("    );");
      fw.Altf("    --  -End-: Call fnSysAddToHistory");
      fw.Altf("");
      fw.Altf("    --  Start: Debug");
      fw.Altf("    IF in_nDebug <> 0 THEN");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, N'');");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, N'---------------------------------------------------');");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'Finished with errors', N'.'));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'   inout_nError :  ', inout_nError));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'   inout_szError:  ', inout_szError));");
      fw.Altf("    END IF;");
      fw.Altf("    --  -End-: Debug");
      fw.Altf("");
      fw.Altf("  END errorPart;");
    }
    private void Create_EndPart(FileWriter fw, CSV_Tabelle table, string spType) {
      string action = "1";
      if (spType == "Update") {
        action = "2";
      }
      if (spType == "Delete") {
        action = "3";
      }
      fw.Altf("  endPart:BEGIN");
      fw.Altf("");
      fw.Altf("    --  Start: Call fnSysAddToHistory");
      fw.Altf("    IF  inout_nError   = 0");
      fw.Altf("    AND nSuccess = 1 THEN");
      fw.Altf("      SET @func = fnSysAddToHistory (");
      fw.Altf("      /* in_nCallerId   */    0       -- 0:System, if anything else, function wont work");
      fw.Altf("      /* in_szTable     */  , N'" + table.TableName + "'");
      fw.Altf("      /* in_nTableId    */  , out_nId");
      fw.Altf("      /* in_iAction     */  , " + action + "");
      fw.Altf("      /* in_szFunction  */  , N'" + spType + " " + table.DataName + "'");
      fw.Altf("      /* in_nError      */  , inout_nError");
      fw.Altf("      /* in_szComment   */  , CONCAT_WS(N'', N'" + spType + " at " + table.TableName + " was successful: ', inout_szError)");
      fw.Altf("      /* in_nUserId     */  , in_nCallerId");
      fw.Altf("      );");
      fw.Altf("    END IF;");
      fw.Altf("    --  -End-: Call fnSysAddToHistory");
      fw.Altf("");
      string tblName = table.TableName;
      if (spType == "Delete") {
        tblName = "tblTdDelete" + table.DataName;
      }
      fw.Altf("    --  Start: Build resultset");
      fw.Altf("    IF in_nResultset <> 0 THEN");
      fw.Altf("      IF out_nId < 0 OR out_nId IS NULL THEN");
      fw.Altf("        SELECT -60;");
      fw.Altf("      ELSE");
      fw.Altf("        SELECT * FROM " + tblName + " WHERE nId = out_nId;");
      fw.Altf("      END IF;");
      fw.Altf("    END IF;");
      fw.Altf("    --  -End-: Build resultset");
      fw.Altf("");
      fw.Altf("    --  Start: Debug");
      fw.Altf("    IF in_nDebug <> 0 THEN");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, N'');");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, N'---------------------------------------------------');");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'Ending Procedure ', szProcedureName, N'.'));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N' - at ', NOW()));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N' - time: ', TIMEDIFF(NOW(), dtProcedureStart)));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, N'---------------------------------------------------');");
      fw.Altf("      IF in_nDebug = 1 THEN");
      fw.Altf("        SELECT * FROM tblSysDebug;");
      fw.Altf("        TRUNCATE TABLE tblSysDebug;");
      fw.Altf("      END IF;");
      fw.Altf("    END IF;");
      fw.Altf("    --  -End-: Debug");
      fw.Altf("");
      fw.Altf("  END endPart;");
    }
    private void Create_TestCall(FileWriter fw, CSV_Tabelle table, string spType, string procedureName) {
      fw.Altf("USE dbAktivenplaner;");
      fw.Altf("SET @in_nCallerId = 4206942;");
      fw.Altf("-- -");
      // insert or update
      if (spType == "Insert" || spType == "Update") {
        if (spType == "Update") {
          fw.Altf("SET @in_nId = 100;");
        }
        CSV_Spalte tmpCol = null;
        for (int i = table.IsSys ? 1 : 2; i < table.Columns.Count; i++) {
          tmpCol = table.Columns[i];
          fw.Altf("SET @in_" + tmpCol.Attribut + " = " + tmpCol.GetTestValue() + ";");
        }
      }
      // delete
      else {
        fw.Altf("SET @in_nId = 100;");
        fw.Altf("SET @in_nDeleteMode = 0;");
      }
      fw.Altf("  -- -");
      fw.Altf("SET @out_nId         = -42;");
      fw.Altf("SET @in_nResultset   = 1;");
      fw.Altf("SET @in_nDebug       = 1;");
      fw.Altf("SET @in_nDebugDepth  = 0;");
      fw.Altf("SET @inout_nError    = 0;");
      fw.Altf("SET @inout_szError   = N'';");
      fw.Altf("CALL " + procedureName + "(");
      fw.Altf("    @in_nCallerId");
      // insert or update
      if (spType == "Insert" || spType == "Update") {
        if (spType == "Update") {
          fw.Altf("  , @in_nId");
        }
        CSV_Spalte tmpCol = null;
        for (int i = table.IsSys ? 1 : 2; i < table.Columns.Count; i++) {
          tmpCol = table.Columns[i];
          fw.Altf("  , @in_" + tmpCol.Attribut + "");
        }
      }
      // delete
      else {
        fw.Altf("  , @in_nId");
        fw.Altf("  , @in_nDeleteMode");
      }
      fw.Altf("  , @out_nId");
      fw.Altf("  , @in_nResultset");
      fw.Altf("  , @in_nDebug");
      fw.Altf("  , @in_nDebugDepth");
      fw.Altf("  , @inout_nError");
      fw.Altf("  , @inout_szError");
      fw.Altf(");");
      fw.Altf("SELECT * FROM tblSysHistory ORDER BY nId DESC;");
      fw.Altf("SELECT * FROM " + table.TableName + " ORDER BY nId DESC;");
    }
    private void Create_InsertProcedure_old(CSV_Tabelle table) {
      string procedureName = "spAPInsert" + table.DataName;
      if (table.IsSys) {
        procedureName = "spSysInsert" + table.DataName;
      }
      Console.WriteLine("  -  Create_" + procedureName + "()");
      string fileName = procedureName;
      FileWriter fw = new FileWriter(AC.APPath, fileName, "sql");
      SQLScript newScript = new SQLScript(fileName, DBType.SP, table.IsSys, table, fw);
      newScript.AddDependency("dbAktivenplaner");
      newScript.AddDependency(table.TableName);
      newScript.AddDependency("fnSysAddToHistory");
      newScript.AddDependency("fnSysPrintDebug");

      string purpose = "Erstellt einen neuen Eintrag in der Tabelle " + table.TableName + ".";
      List<string> returns = new List<string>();            // should not be null
      List<string> errorCodes = null;                       // null -> sp-standard
      List<string> neededDbObjects = new List<string>();    // should not be null
      returns.Add("Resultset: Der neu eingefuegte Datensatz wird zurueck gegeben.");
      returns.Add("           -60 als Id falls die Prozedur fehlschlug.");
      neededDbObjects.Add("fnSysAddToHistory");
      neededDbObjects.Add("fnSysPrintDebug");

      fw.Amltf(GetSkriptHeader(procedureName, purpose, returns, errorCodes, neededDbObjects));

      CSV_Spalte tmpCol = new CSV_Spalte(0, table.TableName, "", "", "", false, false, false, "", "", "", "", "", "", "");
      string tableId = table.Columns[0].Attribut;

      fw.Altf("");
      fw.Altf("USE dbAktivenplaner;");
      fw.Altf("DROP PROCEDURE IF EXISTS " + procedureName + ";");
      fw.Altf("");
      fw.Altf("DELIMITER //");
      fw.Altf("CREATE PROCEDURE " + procedureName + " (");
      fw.Altf("    IN    in_nCallerId       INT             --  Is mandatory");
      fw.Altf("  -- -");
      // each input value
      #region assign each input value
      for (int i = table.DataName.StartsWith("Sys") ? 1 : 2; i < table.Columns.Count; i++) {
        tmpCol = table.Columns[i];
        fw.Altf("  , IN    in_" + tmpCol.Attribut + "   " + GetDataTypeAsStringForSql(tmpCol.Art, tmpCol.Groesse));
      }
      #endregion assign each input value
      fw.Altf("  -- -");
      fw.Altf("  , OUT   out_nId   INT");
      fw.Altf("  -- -");
      fw.Altf("  , IN    in_nResultset    INT             --  <>0:  select a resultset");
      fw.Altf("  , IN    in_nDebug        INT             --  <>0:  create debug-output, 1: select and delete debug at the end");
      fw.Altf("  , IN    in_nDebugDepth   INT             --  indentation of debug inserts");
      fw.Altf("  , INOUT inout_nError     INT             --  stores the error-code");
      fw.Altf("  , INOUT inout_szError    NVARCHAR(1000)  --  stores the error-message");
      fw.Altf(") BEGIN");
      fw.Altf("");
      fw.Altf("  -- initialPart:BEGIN");
      fw.Altf("");
      fw.Altf("    -- Start: Declaration of local variables (only in procedure)");
      fw.Altf("    DECLARE szProcedureName   NVARCHAR(50)    DEFAULT N'" + procedureName + "';");
      fw.Altf("    DECLARE nRowCount         INT             DEFAULT 0;");
      fw.Altf("    DECLARE nSuccess          INT             DEFAULT 1;");
      fw.Altf("    DECLARE dtProcedureStart  DATETIME        DEFAULT NOW();");
      fw.Altf("    --  -End-: Declaration of local variables (only in procedure)");
      fw.Altf("");
      fw.Altf("    --  Start: Setting of global variables (stay alive after procedure)");
      fw.Altf("    -- SET @nResultId    = 0;");
      fw.Altf("    -- SET @nErrorCall   = 0;");
      fw.Altf("    -- SET @szErrorCall  = N'';");
      fw.Altf("    --  -End-: Setting of global variables (stay alive after procedure)");
      fw.Altf("");
      fw.Altf("    --  Start: Set default values");
      // each default value
      #region set default values
      for (int i = table.DataName.StartsWith("Sys") ? 1 : 2; i < table.Columns.Count; i++) {
        tmpCol = table.Columns[i];
        if (tmpCol.DefaultVal.Length > 0) {
          string s = "    SET in_" + tmpCol.Attribut + " = IFNULL(in_" + tmpCol.Attribut + ", ";
          if (tmpCol.Art == "datetime" || tmpCol.Art == "nvarchar" || tmpCol.Art == "date" || tmpCol.Art == "time") {
            s += "N'";
          }
          if (tmpCol.Art == "decimal") {
            s += tmpCol.DefaultVal.Replace(",", ".");
          }
          else {
            s += tmpCol.DefaultVal;
          }
          if (tmpCol.Art == "datetime" || tmpCol.Art == "nvarchar" || tmpCol.Art == "date") {
            s += "'";
          }
          s += ");";
          fw.Altf(s);
        }
      }
      #endregion set default values
      fw.Altf("    -- -");
      fw.Altf("    SET out_" + tableId + " = IFNULL(out_" + tableId + ", -60);");
      fw.Altf("    -- -");
      fw.Altf("    SET in_nResultset         = IFNULL(in_nResultset,    0);");
      fw.Altf("    SET in_nDebug             = IFNULL(in_nDebug,        0);");
      fw.Altf("    SET in_nDebugDepth        = IFNULL(in_nDebugDepth,   0);");
      fw.Altf("    SET inout_nError          = IFNULL(inout_nError,     0);");
      fw.Altf("    SET inout_szError         = IFNULL(inout_szError,    N'');");
      fw.Altf("    --  -End-: Set default values");
      fw.Altf("");
      fw.Altf("    --  Start: Create Debug-table if necessary");
      fw.Altf("    IF in_nDebug <> 0 THEN");
      fw.Altf("      CREATE TABLE IF NOT EXISTS tblSysDebug (");
      fw.Altf("          nId       INT             NOT NULL PRIMARY KEY AUTO_INCREMENT");
      fw.Altf("        , dtTime    TIMESTAMP       NOT NULL DEFAULT CURRENT_TIMESTAMP");
      fw.Altf("        , szComment NVARCHAR(1000)  NOT NULL DEFAULT N'Missing debug comment!'");
      fw.Altf("      );");
      fw.Altf("    END IF;");
      fw.Altf("    --  -End-: Create Debug-table if necessary");
      fw.Altf("");
      fw.Altf("    --  Start: Debug output at procedure start");
      fw.Altf("    IF in_nDebug <> 0 THEN");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, N'---------------------------------------------------');");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'Starting Procedure ', szProcedureName, N':'));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_nCallerId    = ', in_nCallerId));");
      // print each input value
      #region print into debug each input value
      for (int i = table.DataName.StartsWith("Sys") ? 1 : 2; i < table.Columns.Count; i++) {
        tmpCol = table.Columns[i];
        fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_" + tmpCol.Attribut + " = ', in_" + tmpCol.Attribut + "));");
      }
      #endregion print into debug each input value
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_nResultset = ', in_nResultset));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_nDebug     = ', in_nDebug));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, N'---------------------------------------------------');");
      fw.Altf("    END IF;");
      fw.Altf("    --  -End-: Debug output at procedure start");
      fw.Altf("");
      fw.Altf("    --  Start: Check inputs");
      fw.Altf("    SET inout_szError = N'ERROR at input: ';");
      fw.Altf("    IF IFNULL(in_nCallerId, -1) < 0 THEN");
      fw.Altf("      SET inout_nError  = 1;");
      fw.Altf("      SET inout_szError = CONCAT_WS(N'', inout_szError, N'Input in_nCallerId was incorrect. (', in_nCallerId, N')');");
      fw.Altf("    END IF;");
      // check each input value
      #region check each input value for correctness if NOT NULL
      int helper = 0;
      for (int i = table.DataName.StartsWith("Sys") ? 1 : 2; i < table.Columns.Count; i++) {
        tmpCol = table.Columns[i];
        if (tmpCol.NotNull) {
          string s = "    IF IFNULL (in_" + tmpCol.Attribut + ", ";
          if (tmpCol.Art == "datetime" || tmpCol.Art == "nvarchar" || tmpCol.Art == "date") {
            s += "N'";
          }
          switch (tmpCol.Art) {
            case "int":
              s += "-1234567891";
              break;
            case "decimal":
              s += "-1234567891.0";
              break;
            case "nvarchar":
              s += "";
              break;
            case "datetime":
              s += "";
              break;
            case "date":
              s += "";
              break;
            case "bool":
              s += "-1";
              break;
            default:
              bool found = false;
              if (tmpCol.Art.StartsWith("enum")) {
                found = true;
              }
              foreach (var c in AC.DbTables) {
                if (c.DataName == tmpCol.Art) {
                  found = true;
                }
              }
              if (found) {
                s += "-1234567891";
              }
              else {
                s += "TODO";
              }
              break;
          }
          if (tmpCol.Art == "datetime" || tmpCol.Art == "nvarchar" || tmpCol.Art == "date") {
            s += "'";
          }
          s += ") ";
          switch (tmpCol.Art) {
            case "int":
              s += "< ";
              break;
            case "decimal":
              s += "< ";
              break;
            case "nvarchar":
              s += "= ";
              break;
            case "datetime":
              s += "= ";
              break;
            case "date":
              s += "= ";
              break;
            case "bool":
              s += "< ";
              break;
            default:
              bool found = false;
              if (tmpCol.Art.StartsWith("enum")) {
                found = true;
              }
              foreach (var c in AC.DbTables) {
                if (c.DataName == tmpCol.Art) {
                  found = true;
                }
              }
              if (found) {
                s += "< ";
              }
              else {
                s += "TODO";
              }
              break;
          }
          if (tmpCol.Art == "datetime" || tmpCol.Art == "nvarchar" || tmpCol.Art == "date") {
            s += "N'";
          }
          switch (tmpCol.Art) {
            case "int":
              s += "-1234567890";
              break;
            case "decimal":
              s += "-1234567890.0";
              break;
            case "nvarchar":
              s += "";
              break;
            case "datetime":
              s += "";
              break;
            case "date":
              s += "";
              break;
            case "bool":
              s += "0";
              break;
            default:
              bool found = false;
              if (tmpCol.Art.StartsWith("enum")) {
                found = true;
              }
              foreach (var c in AC.DbTables) {
                if (c.DataName == tmpCol.Art) {
                  found = true;
                }
              }
              if (found) {
                s += "-1234567890";
              }
              else {
                s += "TODO";
              }
              break;
          }
          if (tmpCol.Art == "datetime" || tmpCol.Art == "nvarchar" || tmpCol.Art == "date") {
            s += "'";
          }
          s += " THEN";
          if (tmpCol.Attribut.StartsWith("i")) {
            s = "    IF IFNULL (in_" + tmpCol.Attribut + ", -1) < 0 THEN";
          }
          fw.Altf(s);
          s = "      SET inout_nError  = " + ((10 + helper) * -1) + ";";
          fw.Altf(s);
          s = "      SET inout_szError = CONCAT_WS(N'', inout_szError, N'Input in_" + tmpCol.Attribut + " was incorrect. (', in_" + tmpCol.Attribut + ", N')');";
          fw.Altf(s);
          fw.Altf("    END IF;");
          helper++;
        }
      }
      #endregion check each input value for correctness if NOT NULL
      fw.Altf("    --  -End-: Check inputs");
      fw.Altf("");
      fw.Altf("  -- END initialPart;");
      fw.Altf("");
      fw.Altf("  -- ---------------------------------- -");
      fw.Altf("  --  Main part");
      fw.Altf("  -- ---------------------------------- -");
      fw.Altf("  mainPart:BEGIN");
      fw.Altf("");
      fw.Altf("    --  Start: Declaration of handlers");
      fw.Altf("    DECLARE EXIT      HANDLER FOR 1062      -- ER_DUP_ENTRY");
      fw.Altf("        SET inout_nError = -140;");
      fw.Altf("    DECLARE EXIT      HANDLER FOR 45000     -- User defined error");
      fw.Altf("        SET inout_nError = -141;");
      fw.Altf("    DECLARE EXIT      HANDLER FOR 1106      -- ER_UNKNOWN_PROCEDURE");
      fw.Altf("        SET inout_nError = -142;");
      fw.Altf("    DECLARE EXIT      HANDLER FOR 1122      -- ER_CANT_FIND_UDF");
      fw.Altf("        SET inout_nError = -143;");
      fw.Altf("    --  -End-: Declaration of handlers");
      fw.Altf("");
      fw.Altf("    --  Start: Error check at beginning of main part");
      fw.Altf("    IF IFNULL(inout_nError, 0) <> 0 THEN");
      fw.Altf("      SET nSuccess = 0;");
      fw.Altf("      LEAVE mainPart;");
      fw.Altf("    END IF;");
      fw.Altf("    --  -End-: Error check at beginning of main part");
      fw.Altf("    SET inout_nError  = 0;");
      fw.Altf("    SET inout_szError = N'';");
      fw.Altf("");
      fw.Altf("");
      fw.Altf("");
      fw.Altf("");
      fw.Altf("");
      // check if objects that are linked here already exist
      #region check for existing foreign keys
      fw.Altf("    -- Test if necessary datasets already exist");
      for (int i = table.DataName.StartsWith("Sys") ? 1 : 3; i < table.Columns.Count; i++) {
        tmpCol = table.Columns[i];
        if (tmpCol.ForeignKeyTo.Length > 0 && tmpCol.NotNull) {
          string foreign = tmpCol.ForeignKeyTo;
          string fTable = foreign.Split('.')[0];
          string fColumn = foreign.Split('.')[1];
          string errMsg = "CONCAT_WS(N'', N'Kein gueltiger Datenpunkt in Tabelle " + fTable + " fuer " + fColumn + "=', in_" + tmpCol.Attribut + ", N'.')";
          fw.Altf("    -- Reference in " + fTable);
          fw.Altf("    IF NOT EXISTS (SELECT 1 FROM " + fTable);
          fw.Altf("      WHERE " + fColumn + " = in_" + tmpCol.Attribut);
          fw.Altf("      LIMIT 1");
          fw.Altf("    )");
          fw.Altf("    THEN");
          fw.Altf("      SET inout_nError = -105;");
          fw.Altf("      SET inout_szError = " + errMsg + ";");
          fw.Altf("      SET nSuccess = 0;");
          fw.Altf("      -- Start: Debug");
          fw.Altf("      IF in_nDebug <> 0 THEN");
          fw.Altf("        SET @func = fnSysPrintDebug(in_nDebugDepth, " + errMsg + ");");
          fw.Altf("      END IF;");
          fw.Altf("      -- -End-: Debug");
          fw.Altf("      LEAVE mainPart;");
          fw.Altf("    END IF;");
          fw.Altf("    ");
        }
      }
      #endregion check for existing foreign keys
      // check if dataset already exists
      #region test if dataset already exists
      fw.Altf("    -- Test if " + table.DataName + " already exists");
      List<string> uniques = new List<string>();
      for (int i = table.DataName.StartsWith("Sys") ? 1 : 2; i < table.Columns.Count; i++) {
        tmpCol = table.Columns[i];
        if (tmpCol.MakeUnique.Length > 0) {
          if (!uniques.Contains(tmpCol.MakeUnique)) {
            uniques.Add(tmpCol.MakeUnique);
          }
        }
      }
      for (int u = 0; u < uniques.Count; u++) {
        fw.Altf("    IF EXISTS (SELECT 1");
        fw.Altf("               FROM " + table.TableName);
        fw.Altf("               WHERE 1=1");
        for (int i = table.DataName.StartsWith("Sys") ? 1 : 2; i < table.Columns.Count; i++) {
          tmpCol = table.Columns[i];
          if (tmpCol.MakeUnique == uniques[u]) {
            fw.Altf("               AND   " + tmpCol.Attribut + " = in_" + tmpCol.Attribut);
          }
        }
        fw.Altf("      LIMIT 1");
        fw.Altf("    )");
        fw.Altf("    THEN");
        fw.Altf("      SET inout_nError    = -106;");
        fw.Altf("      SET inout_szError   = N'" + table.DataName + " already exists.';");
        fw.Altf("      SET nSuccess        = 0;");
        fw.Altf("      SET out_" + tableId + " = (SELECT " + tableId);
        fw.Altf("        FROM " + table.TableName);
        fw.Altf("        WHERE 1=1");
        for (int i = table.DataName.StartsWith("Sys") ? 1 : 2; i < table.Columns.Count; i++) {
          tmpCol = table.Columns[i];
          if (tmpCol.MakeUnique == uniques[u]) {
            fw.Altf("      AND   " + tmpCol.Attribut + " = in_" + tmpCol.Attribut);
          }
        }
        fw.Altf("        LIMIT 1");
        fw.Altf("      );");
        fw.Altf("      -- Start: Debug");
        fw.Altf("      IF in_nDebug <> 0 THEN");
        fw.Altf("        SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'" + table.DataName + " already exists.', N''));");
        for (int i = table.DataName.StartsWith("Sys") ? 1 : 2; i < table.Columns.Count && table.Columns[i].MakeUnique == uniques[u]; i++) {
          tmpCol = table.Columns[i];
          fw.Altf("        SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_" + tmpCol.Attribut + " = ', in_" + tmpCol.Attribut + "));");
        }
        fw.Altf("      END IF;");
        fw.Altf("      -- -End-: Debug");
        fw.Altf("      LEAVE mainPart;");
        fw.Altf("    END IF;");
        fw.Altf("");
      }
      #endregion test if dataset already exists
      // actually insert dataset
      #region insert dataset
      fw.Altf("    -- Insert new " + table.DataName + "");
      fw.Altf("    INSERT INTO " + table.TableName + " (");
      int start = table.DataName.StartsWith("Sys") ? 1 : 2;
      for (int i = start; i < table.Columns.Count; i++) {
        tmpCol = table.Columns[i];
        if (i == start) {
          fw.Altf("        " + tmpCol.Attribut);
        }
        else {
          fw.Altf("      , " + tmpCol.Attribut);
        }
      }
      fw.Altf("    )");
      fw.Altf("    VALUES (");
      for (int i = start; i < table.Columns.Count; i++) {
        tmpCol = table.Columns[i];
        if (i == start) {
          fw.Altf("        in_" + tmpCol.Attribut);
        }
        else {
          fw.Altf("      , in_" + tmpCol.Attribut);
        }
      }
      #endregion insert dataset
      fw.Altf("    );");
      fw.Altf("    ");
      fw.Altf("    -- Start: Evaluation");
      fw.Altf("    SET nRowCount = ROW_COUNT();");
      fw.Altf("    SET out_" + tableId + " = (SELECT " + tableId + "");
      fw.Altf("      FROM " + table.TableName + "");
      fw.Altf("      WHERE 1=1");
      for (int i = table.DataName.StartsWith("Sys") ? 1 : 2; i < table.Columns.Count; i++) {
        tmpCol = table.Columns[i];
        if (tmpCol.NotNull) {
          fw.Altf("        AND " + tmpCol.Attribut + " = in_" + tmpCol.Attribut);
        }
      }
      fw.Altf("      LIMIT 1");
      fw.Altf("    );");
      fw.Altf("    SET nSuccess = 1;");
      fw.Altf("    SET inout_szError = CONCAT_WS(N'', N'Insert of " + table.DataName + " with " + tableId + " = ', out_" + tableId + ");");
      for (int i = table.DataName.StartsWith("Sys") ? 1 : 2; i < table.Columns.Count; i++) {
        tmpCol = table.Columns[i];
        fw.Altf("    SET inout_szError = CONCAT_WS(N'', inout_szError, N', " + tmpCol.Attribut + " = ', in_" + tmpCol.Attribut + ");");
      }
      fw.Altf("    SET inout_szError = CONCAT_WS(N'', inout_szError, N'.');");
      fw.Altf("    --  -End-: Evaluation");
      fw.Altf("");
      fw.Altf("    --  Start: Debug");
      fw.Altf("    IF in_nDebug <> 0 THEN");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'Inserted " + table.DataName + "', N''));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'     nRowCount: ', nRowCount));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'     " + tableId + ": ', out_" + tableId + "));");
      for (int i = table.DataName.StartsWith("Sys") ? 1 : 2; i < table.Columns.Count; i++) {
        tmpCol = table.Columns[i];
        fw.Altf("      SET inout_szError = CONCAT_WS(N'', inout_szError, N', " + tmpCol.Attribut + " = ', in_" + tmpCol.Attribut + ");");
      }
      fw.Altf("    END IF;");
      fw.Altf("    --  -End-: Debug");
      fw.Altf("");
      fw.Altf("");
      fw.Altf("");
      fw.Altf("");
      fw.Altf("");
      fw.Altf("");
      fw.Altf("    --  Start: Error check at end of main part");
      fw.Altf("    IF IFNULL(nRowCount, -1) < 1 THEN");
      fw.Altf("      SET inout_nError = -107;");
      fw.Altf("      SET inout_szError = N'Couldnt insert " + table.DataName + ". RowCount to low.';");
      fw.Altf("      SET nSuccess  = 0;");
      fw.Altf("      LEAVE mainPart;");
      fw.Altf("    END IF;");
      fw.Altf("    --  -End-: Error check at end of main part");
      fw.Altf("");
      fw.Altf("    -- Start: Secure and evaluate errors");
      fw.Altf("    if ifnull(out_" + tableId + ", -1) < 0 then");
      fw.Altf("      SET inout_nError  = -108;");
      fw.Altf("      SET inout_szError = N'Id out_" + tableId + " couldnt be resolved';");
      fw.Altf("      SET nSuccess  = 0;");
      fw.Altf("      LEAVE mainPart;");
      fw.Altf("    END IF;");
      fw.Altf("    -- -End-: Secure and evaluate errors");
      fw.Altf("");
      fw.Altf("    -- Start: End of procedure -> everything is ok");
      fw.Altf("    SET inout_nError  = 0;");
      fw.Altf("    SET inout_szError = CONCAT_WS(N'', inout_szError, N' -> No error occured.');");
      fw.Altf("    -- -End-: End of procedure -> everything is ok");
      fw.Altf("");
      fw.Altf("  END mainPart;");
      fw.Altf("");
      fw.Altf("  -- ---------------------------------- -");
      fw.Altf("  -- Error part");
      fw.Altf("  -- ---------------------------------- -");
      fw.Altf("  errorPart:BEGIN");
      fw.Altf("");
      fw.Altf("    --  Start: Check for errors");
      fw.Altf("    IF inout_nError = 0 THEN");
      fw.Altf("      LEAVE errorPart;");
      fw.Altf("    END IF;");
      fw.Altf("    --  -End-: Check for errors");
      fw.Altf("");
      fw.Altf("    set nSuccess = 0;");
      fw.Altf("");
      fw.Altf("    --  Start: Call fnSysAddToHistory");
      fw.Altf("    SET @func = fnSysAddToHistory (");
      fw.Altf("    /* in_nCallerId   */    0       -- 0:System, if anything else, function wont work");
      fw.Altf("    /* in_szTable     */  , N'" + table.TableName + "'");
      fw.Altf("    /* in_nTableId    */  , -10");
      fw.Altf("    /* in_iAction     */  , 1");
      fw.Altf("    /* in_szFunction  */  , N'Insert " + table.DataName + "'");
      fw.Altf("    /* in_nError      */  , inout_nError");
      fw.Altf("    /* in_szComment   */  , CONCAT_WS(N'', N'Failed to insert at " + table.TableName + ": ', inout_szError)");
      fw.Altf("    /* in_nUserId     */  , in_nCallerId");
      fw.Altf("    );");
      fw.Altf("    --  -End-: Call fnSysAddToHistory");
      fw.Altf("");
      fw.Altf("    --  Start: Debug");
      fw.Altf("    IF in_nDebug <> 0 THEN");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, N'');");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, N'---------------------------------------------------');");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'Finished with errors', N'.'));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'   inout_nError :  ', inout_nError));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'   inout_szError:  ', inout_szError));");
      fw.Altf("    END IF;");
      fw.Altf("    --  -End-: Debug");
      fw.Altf("");
      fw.Altf("  END errorPart;");
      fw.Altf("");
      fw.Altf("  -- ---------------------------------- -");
      fw.Altf("  -- End part");
      fw.Altf("  -- ---------------------------------- -");
      fw.Altf("  endPart:BEGIN");
      fw.Altf("");
      fw.Altf("    --  Start: Call fnSysAddToHistory");
      fw.Altf("    IF  inout_nError   = 0");
      fw.Altf("    AND nSuccess = 1 THEN");
      fw.Altf("      SET @func = fnSysAddToHistory (");
      fw.Altf("      /* in_nCallerId   */    0       -- 0:System, if anything else, function wont work");
      fw.Altf("      /* in_szTable     */  , N'" + table.TableName + "'");
      fw.Altf("      /* in_nTableId    */  , out_" + tableId + "");
      fw.Altf("      /* in_iAction     */  , 1");
      fw.Altf("      /* in_szFunction  */  , N'Insert " + table.DataName + "'");
      fw.Altf("      /* in_nError      */  , inout_nError");
      fw.Altf("      /* in_szComment   */  , CONCAT_WS(N'', N'Successfull insert at " + table.TableName + ": ', inout_szError)");
      fw.Altf("      /* in_nUserId     */  , in_nCallerId");
      fw.Altf("      );");
      fw.Altf("    END IF;");
      fw.Altf("    --  -End-: Call fnSysAddToHistory");
      fw.Altf("");
      fw.Altf("    --  Start: Build resultset");
      fw.Altf("    IF in_nResultset <> 0 THEN");
      fw.Altf("      SELECT * FROM " + table.TableName + " WHERE " + tableId + " = out_" + tableId + ";");
      fw.Altf("    END IF;");
      fw.Altf("    --  -End-: Build resultset");
      fw.Altf("");
      fw.Altf("    --  Start: Debug");
      fw.Altf("    IF in_nDebug <> 0 THEN");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, N'');");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, N'---------------------------------------------------');");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'Ending Procedure ', szProcedureName, N'.'));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N' - at ', NOW()));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N' - time: ', TIMEDIFF(NOW(), dtProcedureStart)));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, N'---------------------------------------------------');");
      fw.Altf("      IF in_nDebug = 1 THEN");
      fw.Altf("        SELECT * FROM tblSysDebug;");
      fw.Altf("        TRUNCATE TABLE tblSysDebug;");
      fw.Altf("      END IF;");
      fw.Altf("    END IF;");
      fw.Altf("    --  -End-: Debug");
      fw.Altf("");
      fw.Altf("  END endPart;");
      fw.Altf("");
      fw.Altf("END; //");
      fw.Altf("DELIMITER ;");
      fw.Altf("");
      // Test Call
      #region test call
      fw.Altf("/* Test-Call");
      fw.Altf("use dbAktivenplaner;");
      fw.Altf("set @in_nCallerId = 4206942;");
      fw.Altf("-- -");
      for (int i = table.DataName.StartsWith("Sys") ? 1 : 2; i < table.Columns.Count; i++) {
        tmpCol = table.Columns[i];
        string s = "set @in_" + tmpCol.Attribut + " = ";
        if (tmpCol.NotNull) {
          if (tmpCol.Attribut.StartsWith("i") || tmpCol.Attribut.StartsWith("e")) {
            s += "1;";
          }
          else {
            switch (tmpCol.Art) {
              case "int":
                s += "1;";
                break;
              case "decimal":
                s += "1.0;";
                break;
              case "nvarchar":
                s += "N'test-input';";
                break;
              case "datetime":
                s += "N'2000-01-01 01:01:01';";
                break;
              case "date":
                s += "N'2000-01-01';";
                break;
              default:
                bool found = false;
                if (tmpCol.Art.StartsWith("enum")) {
                  found = true;
                }
                foreach (var c in AC.DbTables) {
                  if (c.TableName == tmpCol.Art) {
                    found = true;
                  }
                }
                if (found) {
                  s += "1";
                }
                else {
                  s += "TODO";
                }
                break;
            }
          }
        }
        else {
          s += " NULL ;";
        }
        fw.Altf(s);
      }
      fw.Altf("-- -");
      fw.Altf("set @out_Id = -42;");
      fw.Altf("-- -");
      fw.Altf("set @in_nResultset    = 1;");
      fw.Altf("set @in_nDebug        = 1;");
      fw.Altf("set @in_nDebugDepth   = 0;");
      fw.Altf("set @inout_nError     = 0;");
      fw.Altf("set @inout_szError    = N'';");
      fw.Altf("CALL " + procedureName + "(");
      fw.Altf("    @in_nCallerId");
      for (int i = table.DataName.StartsWith("Sys") ? 1 : 2; i < table.Columns.Count; i++) {
        tmpCol = table.Columns[i];
        fw.Altf("  , @in_" + tmpCol.Attribut);
      }
      fw.Altf("  , @out_" + tableId + "");
      fw.Altf("  , @in_nResultset");
      fw.Altf("  , @in_nDebug");
      fw.Altf("  , @in_nDebugDepth");
      fw.Altf("  , @inout_nError");
      fw.Altf("  , @inout_szError");
      fw.Altf(");");
      fw.Altf("select * from tblSysHistory order by nId desc;");
      fw.Altf("select * from " + table.TableName + " order by nId desc;");
      fw.Altf("*/");
      #endregion test call
      fw.Altf("");

      // Script-End
      SQLDependencies.AddScript(newScript);
    }
    private void Create_UpdateProcedure_old(CSV_Tabelle table) {
      string procedureName = "spAPUpdate" + table.DataName;
      if (table.IsSys) {
        procedureName = "spSysUpdate" + table.DataName;
      }
      Console.WriteLine("  -  Create_" + procedureName + "()");
      string fileName = procedureName;
      FileWriter fw = new FileWriter(AC.APPath, fileName, "sql");
      SQLScript newScript = new SQLScript(fileName, DBType.SP, table.IsSys, table, fw);
      newScript.AddDependency("dbAktivenplaner");
      newScript.AddDependency(table.TableName);
      newScript.AddDependency("fnSysAddToHistory");
      newScript.AddDependency("fnSysPrintDebug");

      string purpose = "Veraendert einen Eintrag in der Tabelle " + table.TableName + ".";
      List<string> returns = new List<string>();            // should not be null
      List<string> errorCodes = null;         // null -> sp-standard
      List<string> neededDbObjects = new List<string>();    // should not be null
      returns.Add("Resultset: Der neue, geaenderte Datensatz wird zurueck gegeben.");
      returns.Add("           -60 als Id falls die Prozedur fehlschlug.");
      neededDbObjects.Add("fnSysAddToHistory");
      neededDbObjects.Add("fnSysPrintDebug");

      fw.Amltf(GetSkriptHeader(procedureName, purpose, returns, errorCodes, neededDbObjects));

      CSV_Spalte tmpCol = new CSV_Spalte(0, table.TableName, "", "", "", false, false, false, "", "", "", "", "", "", "");
      string tableId = table.Columns[0].Attribut;

      fw.Altf("");
      fw.Altf("USE dbAktivenplaner;");
      fw.Altf("DROP PROCEDURE IF EXISTS " + procedureName + ";");
      fw.Altf("");
      fw.Altf("DELIMITER //");
      fw.Altf("CREATE PROCEDURE " + procedureName + " (");
      fw.Altf("    IN    in_nCallerId       INT             --  Is mandatory");
      fw.Altf("  -- -");
      //each input value
      #region input values
      for (int i = table.DataName.StartsWith("Sys") ? 0 : 2; i < table.Columns.Count; i++) {
        tmpCol = table.Columns[i];
        fw.Altf("  , IN    in_" + tmpCol.Attribut + "   " + GetDataTypeAsStringForSql(tmpCol.Art, tmpCol.Groesse));
      }
      #endregion input values
      fw.Altf("  -- -");
      fw.Altf("  , OUT   out_nId " + GetDataTypeAsStringForSql(table.Columns[0].Art, table.Columns[0].Groesse));
      fw.Altf("  -- -");
      fw.Altf("  , IN    in_nResultset    INT             --  <>0:  select a resultset");
      fw.Altf("  , IN    in_nDebug        INT             --  <>0:  create debug-output, 1: select and delete debug at the end");
      fw.Altf("  , IN    in_nDebugDepth   INT             --  indentation of debug inserts");
      fw.Altf("  , INOUT inout_nError     INT             --  stores the error-code");
      fw.Altf("  , INOUT inout_szError    NVARCHAR(1000)  --  stores the error-message");
      fw.Altf(") BEGIN");
      fw.Altf("");
      fw.Altf("  -- initialPart:BEGIN");
      fw.Altf("");
      fw.Altf("    -- Start: Declaration of local variables (only in procedure)");
      fw.Altf("    DECLARE szProcedureName   NVARCHAR(50)    DEFAULT N'" + procedureName + "';");
      fw.Altf("    DECLARE nRowCount         INT             DEFAULT 0;");
      fw.Altf("    DECLARE nSuccess          INT             DEFAULT 1;");
      fw.Altf("    DECLARE dtProcedureStart  DATETIME        DEFAULT NOW();");
      fw.Altf("    DECLARE szChangedData     NVARCHAR(500)   DEFAULT N'';");
      fw.Altf("    --  -End-: Declaration of local variables (only in procedure)");
      fw.Altf("");
      fw.Altf("    --  Start: Setting of global variables (stay alive after procedure)");
      fw.Altf("    -- SET @nResultId    = 0;");
      fw.Altf("    -- SET @nErrorCall   = 0;");
      fw.Altf("    -- SET @szErrorCall  = N'';");
      fw.Altf("    --  -End-: Setting of global variables (stay alive after procedure)");
      fw.Altf("");
      // set default values
      #region set default values
      fw.Altf("    --  Start: Set default values");
      for (int i = !table.IsSys ? 2 : 1; i < table.Columns.Count; i++) {
        tmpCol = table.Columns[i];
        if (tmpCol.DefaultVal.Length > 0) {
          string s = "SET in_" + tmpCol.Attribut + " = IFNULL(in_" + tmpCol.Attribut + ", ";
          if (tmpCol.Art == "datetime" || tmpCol.Art == "nvarchar" || tmpCol.Art == "date") {
            s += "N'";
          }
          if (tmpCol.Art == "decimal") {
            s += tmpCol.DefaultVal.Replace(",", ".");
          }
          else {
            s += tmpCol.DefaultVal;
          }
          if (tmpCol.Art == "datetime" || tmpCol.Art == "nvarchar" || tmpCol.Art == "date") {
            s += "'";
          }
          s += ");";
          fw.Altf(s);
        }
      }
      #endregion set default values
      fw.Altf("    -- -");
      fw.Altf("    SET out_" + tableId + " = IFNULL(out_" + tableId + ", -10);");
      fw.Altf("    -- -");
      fw.Altf("    SET in_nResultset         = IFNULL(in_nResultset,    0);");
      fw.Altf("    SET in_nDebug             = IFNULL(in_nDebug,        0);");
      fw.Altf("    SET in_nDebugDepth        = IFNULL(in_nDebugDepth,   0);");
      fw.Altf("    SET inout_nError          = IFNULL(inout_nError,     0);");
      fw.Altf("    SET inout_szError         = IFNULL(inout_szError,    N'');");
      fw.Altf("    --  -End-: Set default values");
      fw.Altf("");
      fw.Altf("    --  Start: Create Debug-table if necessary");
      fw.Altf("    IF in_nDebug <> 0 THEN");
      fw.Altf("      CREATE TABLE IF NOT EXISTS tblSysDebug (");
      fw.Altf("          nId       INT             NOT NULL PRIMARY KEY AUTO_INCREMENT");
      fw.Altf("        , dtTime    TIMESTAMP       NOT NULL DEFAULT CURRENT_TIMESTAMP");
      fw.Altf("        , szComment NVARCHAR(1000)  NOT NULL DEFAULT N'Missing debug comment!'");
      fw.Altf("      );");
      fw.Altf("    END IF;");
      fw.Altf("    --  -End-: Create Debug-table if necessary");
      fw.Altf("");
      fw.Altf("    --  Start: Debug output at procedure start");
      fw.Altf("    IF in_nDebug <> 0 THEN");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, N'---------------------------------------------------');");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'Starting Procedure ', szProcedureName, N':'));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_nCallerId    = ', in_nCallerId));");
      // print input values to debug
      #region print input values to debug
      for (int i = 0; i < table.Columns.Count; i++) {
        if (i != 1 && !table.IsSys) {
          tmpCol = table.Columns[i];
          fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_" + tmpCol.Attribut + " = ', in_" + tmpCol.Attribut + "));");
        }
      }
      #endregion print input values to debug
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_nResultset = ', in_nResultset));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_nDebug     = ', in_nDebug));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, N'---------------------------------------------------');");
      fw.Altf("    END IF;");
      fw.Altf("    --  -End-: Debug output at procedure start");
      fw.Altf("");
      fw.Altf("    --  Start: Check inputs");
      fw.Altf("    SET inout_szError = N'ERROR at input: ';");
      fw.Altf("    IF IFNULL(in_nCallerId, -1) < 0 THEN");
      fw.Altf("      SET inout_nError  = 1;");
      fw.Altf("      SET inout_szError = CONCAT_WS(N'', inout_szError, N'Input in_nCallerId was incorrect. (', in_nCallerId, N')');");
      fw.Altf("    END IF;");
      fw.Altf("    IF IFNULL(in_" + tableId + ", -1) < 0 THEN");
      fw.Altf("      SET inout_nError  = 1;");
      fw.Altf("      SET inout_szError = CONCAT_WS(N'', inout_szError, N'Input in_" + tableId + " was incorrect. (', in_" + tableId + ", N')');");
      fw.Altf("    END IF;");
      // check each input value
      #region check each input value
      int helper = 0;
      for (int i = table.IsSys ? 1 : 2; i < table.Columns.Count; i++) {
        tmpCol = table.Columns[i];
        if (tmpCol.NotNull) {
          string s = "    IF IFNULL (in_" + tmpCol.Attribut + ", ";
          if (tmpCol.Art == "datetime" || tmpCol.Art == "nvarchar" || tmpCol.Art == "date") {
            s += "N'";
          }
          switch (tmpCol.Art) {
            case "int":
              s += "-1234567891";
              break;
            case "decimal":
              s += "-1234567891.0";
              break;
            case "nvarchar":
              s += "";
              break;
            case "datetime":
              s += "";
              break;
            case "date":
              s += "";
              break;
            case "bool":
              s += "-1";
              break;
            default:
              bool found = false;
              if (tmpCol.Art.StartsWith("enum")) {
                found = true;
              }
              foreach (var c in AC.DbTables) {
                if (c.DataName == tmpCol.Art) {
                  found = true;
                }
              }
              if (found) {
                s += "-1234567891";
              }
              else {
                s += "TODO";
              }
              break;
          }
          if (tmpCol.Art == "datetime" || tmpCol.Art == "nvarchar" || tmpCol.Art == "date") {
            s += "'";
          }
          s += ") ";
          switch (tmpCol.Art) {
            case "int":
              s += "< ";
              break;
            case "decimal":
              s += "< ";
              break;
            case "nvarchar":
              s += "= ";
              break;
            case "datetime":
              s += "= ";
              break;
            case "date":
              s += "= ";
              break;
            case "bool":
              s += "< ";
              break;
            default:
              bool found = false;
              if (tmpCol.Art.StartsWith("enum")) {
                found = true;
              }
              foreach (var c in AC.DbTables) {
                if (c.DataName == tmpCol.Art) {
                  found = true;
                }
              }
              if (found) {
                s += "< ";
              }
              else {
                s += "TODO";
              }
              break;
          }
          if (tmpCol.Art == "datetime" || tmpCol.Art == "nvarchar" || tmpCol.Art == "date") {
            s += "N'";
          }
          switch (tmpCol.Art) {
            case "int":
              s += "-1234567890";
              break;
            case "decimal":
              s += "-1234567890.0";
              break;
            case "nvarchar":
              s += "";
              break;
            case "datetime":
              s += "";
              break;
            case "date":
              s += "";
              break;
            case "bool":
              s += "0";
              break;
            default:
              bool found = false;
              if (tmpCol.Art.StartsWith("enum")) {
                found = true;
              }
              foreach (var c in AC.DbTables) {
                if (c.DataName == tmpCol.Art) {
                  found = true;
                }
              }
              if (found) {
                s += "-1234567890";
              }
              else {
                s += "TODO";
              }
              break;
          }
          if (tmpCol.Art == "datetime" || tmpCol.Art == "nvarchar" || tmpCol.Art == "date") {
            s += "'";
          }
          s += " THEN";
          fw.Altf(s);
          s = "      SET inout_nError  = " + ((10 + helper) * -1) + ";";
          fw.Altf(s);
          s = "      SET inout_szError = CONCAT_WS(N'', inout_szError, N'Input in_" + tmpCol.Attribut + " was incorrect. (', in_" + tmpCol.Attribut + ", N')');";
          fw.Altf(s);
          fw.Altf("    END IF;");
          helper++;
        }
      }
      #endregion check each input value
      fw.Altf("    --  -End-: Check inputs");
      fw.Altf("");
      fw.Altf("  -- END initialPart;");
      fw.Altf("");
      fw.Altf("  -- ---------------------------------- -");
      fw.Altf("  --  Main part");
      fw.Altf("  -- ---------------------------------- -");
      fw.Altf("  mainPart:BEGIN");
      fw.Altf("");
      fw.Altf("    --  Start: Declaration of handlers");
      fw.Altf("    DECLARE EXIT      HANDLER FOR 1062      -- ER_DUP_ENTRY");
      fw.Altf("        SET inout_nError = -140;");
      fw.Altf("    DECLARE EXIT      HANDLER FOR 45000     -- User defined error");
      fw.Altf("        SET inout_nError = -141;");
      fw.Altf("    DECLARE EXIT      HANDLER FOR 1106      -- ER_UNKNOWN_PROCEDURE");
      fw.Altf("        SET inout_nError = -142;");
      fw.Altf("    DECLARE EXIT      HANDLER FOR 1122      -- ER_CANT_FIND_UDF");
      fw.Altf("        SET inout_nError = -143;");
      fw.Altf("    --  -End-: Declaration of handlers");
      fw.Altf("");
      fw.Altf("    --  Start: Error check at beginning of main part");
      fw.Altf("    IF IFNULL(inout_nError, 0) <> 0 THEN");
      fw.Altf("      SET nSuccess = 0;");
      fw.Altf("      LEAVE mainPart;");
      fw.Altf("    END IF;");
      fw.Altf("    --  -End-: Error check at beginning of main part");
      fw.Altf("    SET inout_nError  = 0;");
      fw.Altf("    SET inout_szError = N'';");
      fw.Altf("");
      fw.Altf("");
      fw.Altf("");
      fw.Altf("");
      fw.Altf("");
      fw.Altf("    -- Test if " + table.DataName + " already exists");
      fw.Altf("    IF NOT EXISTS (SELECT 1");
      fw.Altf("      FROM " + table.TableName + "");
      fw.Altf("      WHERE " + tableId + " = in_" + tableId + "");
      fw.Altf("    )");
      fw.Altf("    THEN");
      fw.Altf("      SET inout_nError    = -105;");
      fw.Altf("      SET inout_szError   = N'" + table.DataName + " does not exist.';");
      fw.Altf("      SET nSuccess        = 0;");
      fw.Altf("      SET out_" + tableId + " = -10;");
      fw.Altf("      -- Start: Debug");
      fw.Altf("      IF in_nDebug <> 0 THEN");
      fw.Altf("        SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'" + table.DataName + "<Id:', in_nId, N'> does not exist.', N''));");
      fw.Altf("      END IF;");
      fw.Altf("      -- -End-: Debug");
      fw.Altf("      LEAVE mainPart;");
      fw.Altf("    END IF;");
      fw.Altf("");
      // Load original values
      #region load original values
      fw.Altf("  -- Load original values");
      for (int i = 1; i < table.Columns.Count; i++) {
        if (!table.IsSys && i == 1) {
          tmpCol = table.Columns[i];
          fw.Altf("  SET @old_" + tmpCol.Attribut + " = (SELECT " + tmpCol.Attribut);
          fw.Altf("      FROM " + table.TableName);
          fw.Altf("      WHERE " + tableId + " = in_" + tableId + ");");
        }
      }
      #endregion load original values
      fw.Altf("");
      // set empty input parameters
      #region set empty parameters
      #endregion set empty parameters
      fw.Altf("  -- Set emtpy input parameters");
      for (int i = 2; i < table.Columns.Count; i++) {
        if (!table.IsSys && i == 1) {
          tmpCol = table.Columns[i];
          fw.Altf("  IF IFNULL(in_" + tmpCol.Attribut + ", NULL) = NULL THEN");
          fw.Altf("    SET szChangedData = CONCAT_WS(N'', N'; " + tmpCol.Attribut + ": ', in_" + tmpCol.Attribut + ", ' -> ', @old_" + tmpCol.Attribut + " );");
          fw.Altf("    SET in_" + tmpCol.Attribut + " = @old_" + tmpCol.Attribut + ";");
          fw.Altf("  END IF;");
        }

      }








      // TODO












      fw.Altf("");
      fw.Altf("    -- Test if necessary datasets already exist");
      for (int i = 3; i < table.Columns.Count; i++) {
        tmpCol = table.Columns[i];
        if (tmpCol.ForeignKeyTo.Length > 0 && tmpCol.NotNull) {
          string foreign = tmpCol.ForeignKeyTo;
          string fTable = foreign.Split('.')[0];
          string fColumn = foreign.Split('.')[1];
          fw.Altf("    IF NOT EXISTS (SELECT 1 FROM " + fTable);
          fw.Altf("      WHERE " + fColumn + " = in_" + tmpCol.Attribut);
          fw.Altf("      LIMIT 1");
          fw.Altf("    )");
          fw.Altf("    THEN");
          fw.Altf("      SET inout_nError = 1;");
          fw.Altf("      SET inout_szError = CONCAT_WS(N'', N'Kein gueltiger Datenpunkt in Tabelle " + fTable + " fuer " + fColumn + "=', in_" + tmpCol.Attribut + ",N'.');");
          fw.Altf("      SET nSuccess = 0;");
          fw.Altf("      -- Start: Debug");
          fw.Altf("      IF in_nDebug <> 0 THEN");
          fw.Altf("        SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'Kein gueltiger Datenpunkt in Tabelle " + fTable + " fuer " + fColumn + "=', in_" + tmpCol.Attribut + ",N'.'));");
          fw.Altf("      END IF;");
          fw.Altf("      -- -End-: Debug");
          fw.Altf("      LEAVE mainPart;");
          fw.Altf("    END IF;");
          fw.Altf("    ");
        }
      }
      fw.Altf("  -- Update " + table.TableName);
      fw.Altf("  UPDATE " + table.TableName);
      fw.Altf("  SET dtAendZeit = NOW()");
      for (int i = 2; i < table.Columns.Count; i++) {
        tmpCol = table.Columns[i];
        fw.Altf("      , " + tmpCol.Attribut + " = in_" + tmpCol.Attribut);
      }
      fw.Altf("  WHERE " + tableId + " = in_" + tableId + ";");
      fw.Altf("");
      fw.Altf("    -- Start: Evaluation");
      fw.Altf("    SET nRowCount = ROW_COUNT();");
      fw.Altf("    SET out_" + tableId + " = (SELECT " + tableId + "");
      fw.Altf("      FROM " + table.TableName + "");
      fw.Altf("      WHERE 1=1");
      for (int i = 2; i < table.Columns.Count; i++) {
        tmpCol = table.Columns[i];
        if (tmpCol.NotNull) {
          fw.Altf("        AND " + tmpCol.Attribut + " = in_" + tmpCol.Attribut);
        }
      }
      fw.Altf("      LIMIT 1");
      fw.Altf("    );");
      fw.Altf("    SET nSuccess = 1;");
      fw.Altf("    SET inout_szError = CONCAT_WS(N'', N'Update of " + table.TableName + " with " + tableId + " = ', out_" + tableId + ");");
      for (int i = 2; i < table.Columns.Count; i++) {
        tmpCol = table.Columns[i];
        fw.Altf("    SET inout_szError = CONCAT_WS(N'', inout_szError, N', " + tmpCol.Attribut + ": ', @old_" + tmpCol.Attribut + ", N' -> ', in_" + tmpCol.Attribut + ");");
      }
      fw.Altf("    SET inout_szError = CONCAT_WS(N'', inout_szError, N'.');");
      fw.Altf("    --  -End-: Evaluation");
      fw.Altf("");
      fw.Altf("    --  Start: Debug");
      fw.Altf("    IF in_nDebug <> 0 THEN");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'Updated " + table.TableName + "', N''));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'     nRowCount: ', nRowCount));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'     " + tableId + ": ', out_" + tableId + "));");
      for (int i = 2; i < table.Columns.Count; i++) {
        tmpCol = table.Columns[i];
        fw.Altf("      SET inout_szError = CONCAT_WS(N'', inout_szError, N', " + tmpCol.Attribut + " = ', in_" + tmpCol.Attribut + ");");
      }
      fw.Altf("    END IF;");
      fw.Altf("    --  -End-: Debug");
      fw.Altf("");
      fw.Altf("");
      fw.Altf("");
      fw.Altf("");
      fw.Altf("");
      fw.Altf("");
      fw.Altf("    --  Start: Error check at end of main part");
      fw.Altf("    IF IFNULL(nRowCount, -1) < 1 THEN");
      fw.Altf("      SET inout_nError = 2;");
      fw.Altf("      SET inout_szError = N'Couldnt update " + table.TableName + ". RowCount to low.';");
      fw.Altf("      SET nSuccess  = 0;");
      fw.Altf("      LEAVE mainPart;");
      fw.Altf("    END IF;");
      fw.Altf("    --  -End-: Error check at end of main part");
      fw.Altf("");
      fw.Altf("    -- Start: Secure and evaluate errors");
      fw.Altf("    if ifnull(out_" + tableId + ", -1) < 0 then");
      fw.Altf("      SET inout_nError  = 2;");
      fw.Altf("      SET inout_szError = N'Id out_" + tableId + " couldnt be resolved';");
      fw.Altf("      SET nSuccess  = 0;");
      fw.Altf("      LEAVE mainPart;");
      fw.Altf("    END IF;");
      fw.Altf("    -- -End-: Secure and evaluate errors");
      fw.Altf("");
      fw.Altf("    -- Start: End of procedure -> everything is ok");
      fw.Altf("    SET inout_nError  = 0;");
      fw.Altf("    SET inout_szError = CONCAT_WS(N'', inout_szError, N' -> No error occured.');");
      fw.Altf("    -- -End-: End of procedure -> everything is ok");
      fw.Altf("");
      fw.Altf("  END mainPart;");
      fw.Altf("");
      fw.Altf("  -- ---------------------------------- -");
      fw.Altf("  -- Error part");
      fw.Altf("  -- ---------------------------------- -");
      fw.Altf("  errorPart:BEGIN");
      fw.Altf("");
      fw.Altf("    --  Start: Check for errors");
      fw.Altf("    IF inout_nError = 0 THEN");
      fw.Altf("      LEAVE errorPart;");
      fw.Altf("    END IF;");
      fw.Altf("    --  -End-: Check for errors");
      fw.Altf("");
      fw.Altf("    set nSuccess = 0;");
      fw.Altf("");
      fw.Altf("    --  Start: Call fnSysAddToHistory");
      fw.Altf("    SET @func = fnSysAddToHistory (");
      fw.Altf("    /* in_nCallerId   */    0       -- 0:System, if anything else, function wont work");
      fw.Altf("    /* in_szTable     */  , N'" + table.TableName + "'");
      fw.Altf("    /* in_nTableId    */  , -10");
      fw.Altf("    /* in_eDMLArt     */  , 123               -- 123=update     __TODO__ was ist die nummer fuer update?");
      fw.Altf("    /* in_szFunction  */  , N'Update'");
      fw.Altf("    /* in_nError      */  , inout_nError");
      fw.Altf("    /* in_szKommentar */  , CONCAT_WS(N'', N'Failed to update at " + table.TableName + ": ', inout_szError)");
      fw.Altf("    /* in_nArt        */  , 9");
      fw.Altf("    /* in_nNutzerId   */  , in_nCallerId");
      fw.Altf("    );");
      fw.Altf("    --  -End-: Call fnSysAddToHistory");
      fw.Altf("");
      fw.Altf("    --  Start: Debug");
      fw.Altf("    IF in_nDebug <> 0 THEN");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, N'');");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, N'---------------------------------------------------');");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'Finished with errors', N'.'));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'   inout_nError :  ', inout_nError));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'   inout_szError:  ', inout_szError));");
      fw.Altf("    END IF;");
      fw.Altf("    --  -End-: Debug");
      fw.Altf("");
      fw.Altf("  END errorPart;");
      fw.Altf("");
      fw.Altf("  -- ---------------------------------- -");
      fw.Altf("  -- End part");
      fw.Altf("  -- ---------------------------------- -");
      fw.Altf("  endPart:BEGIN");
      fw.Altf("");
      fw.Altf("    --  Start: Call fnSysAddToHistory");
      fw.Altf("    IF  inout_nError   = 0");
      fw.Altf("    AND nSuccess = 1 THEN");
      fw.Altf("      SET @func = fnSysAddToHistory (");
      fw.Altf("      /* in_nCallerId   */    0       -- 0:System, if anything else, function wont work");
      fw.Altf("      /* in_szTable     */  , N'" + table.TableName + "'");
      fw.Altf("      /* in_nTableId    */  , out_" + tableId + "");
      fw.Altf("      /* in_eDMLArt     */  , 123               -- 123=update     __TODO__ was ist die nummer fuer update?");
      fw.Altf("      /* in_szFunction  */  , N'Update'");
      fw.Altf("      /* in_nError      */  , inout_nError");
      fw.Altf("      /* in_szKommentar */  , CONCAT_WS(N'', N'Successfull update at " + table.TableName + ": ', inout_szError)");
      fw.Altf("      /* in_nNutzerId   */  , in_nCallerId");
      fw.Altf("      );");
      fw.Altf("    END IF;");
      fw.Altf("    --  -End-: Call fnSysAddToHistory");
      fw.Altf("");
      fw.Altf("    --  Start: Build resultset");
      fw.Altf("    IF in_nResultset <> 0 THEN");
      fw.Altf("      SELECT * FROM " + table.TableName + " WHERE " + tableId + " = out_" + tableId + ";");
      fw.Altf("    END IF;");
      fw.Altf("    --  -End-: Build resultset");
      fw.Altf("");
      fw.Altf("    --  Start: Debug");
      fw.Altf("    IF in_nDebug <> 0 THEN");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, N'');");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, N'---------------------------------------------------');");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'Ending Procedure ', szProcedureName, N'.'));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N' - at ', NOW()));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N' - time: ', TIMEDIFF(NOW(), dtProcedureStart)));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, N'---------------------------------------------------');");
      fw.Altf("      IF in_nDebug = 1 THEN");
      fw.Altf("        SELECT * FROM tblSysDebug;");
      fw.Altf("        TRUNCATE TABLE tblSysDebug;");
      fw.Altf("      END IF;");
      fw.Altf("    END IF;");
      fw.Altf("    --  -End-: Debug");
      fw.Altf("");
      fw.Altf("  END endPart;");
      fw.Altf("");
      fw.Altf("END; //");
      fw.Altf("DELIMITER ;");
      fw.Altf("");
      fw.Altf("/* Test-Call");
      fw.Altf("use dbAktivenplaner;");
      fw.Altf("set @in_nCallerId = 492;");
      fw.Altf("-- -");
      for (int i = 0; i < table.Columns.Count; i++) {
        if (i != 1) {
          tmpCol = table.Columns[i];
          string s = "set @in_" + tmpCol.Attribut + " = ";
          if (tmpCol.NotNull) {
            if (tmpCol.Attribut.StartsWith("e") || tmpCol.Attribut.StartsWith("i")) {
              s += "1;";
            }
            else {
              switch (tmpCol.Art) {
                case "int":
                  s += "1;";
                  break;
                case "decimal":
                  s += "1.0;";
                  break;
                case "nvarchar":
                  s += "N'test-input';";
                  break;
                case "datetime":
                  s += "N'2000-01-01 01:01:01';";
                  break;
                case "date":
                  s += "N'2000-01-01';";
                  break;
                case "bool":
                  s += "0;";
                  break;
                default:
                  bool found = false;
                  if (tmpCol.Art.StartsWith("enum")) {
                    found = true;
                  }
                  foreach (var c in AC.DbTables) {
                    if (c.TableName == tmpCol.Art) {
                      found = true;
                    }
                  }
                  if (found) {
                    s += "1";
                  }
                  else {
                    s += "TODO";
                  }
                  break;
              }
            }
          }
          else {
            s += " NULL ;";
          }
          fw.Altf(s);
        }
      }
      fw.Altf("-- -");
      fw.Altf("set @out_" + tableId + " = -42;");
      fw.Altf("-- -");
      fw.Altf("set @in_nResultset    = 1;");
      fw.Altf("set @in_nDebug        = 1;");
      fw.Altf("set @in_nDebugDepth   = 0;");
      fw.Altf("set @inout_nError     = 0;");
      fw.Altf("set @inout_szError    = N'';");
      fw.Altf("CALL " + procedureName + "(");
      fw.Altf("    @in_nCallerId");
      for (int i = 0; i < table.Columns.Count; i++) {
        if (i != 1) {
          tmpCol = table.Columns[i];
          fw.Altf("  , @in_" + tmpCol.Attribut);
        }
      }
      fw.Altf("  , @out_" + tableId + "");
      fw.Altf("  , @in_nResultset");
      fw.Altf("  , @in_nDebug");
      fw.Altf("  , @in_nDebugDepth");
      fw.Altf("  , @inout_nError");
      fw.Altf("  , @inout_szError");
      fw.Altf(");");
      fw.Altf("select * from tblSysHistory order by nId desc;");
      fw.Altf("select * from " + table.TableName + " order by " + tableId + " desc;");
      fw.Altf("*/");
      fw.Altf("");

      // Script-End
      SQLDependencies.AddScript(newScript);
    }
    private void Create_DeleteProcedure_old(CSV_Tabelle table) {
      string procedureName = "spAPDelete" + table.DataName;
      if (table.IsSys) {
        procedureName = "spSysDelete" + table.DataName;
      }
      Console.WriteLine("  -  Create_" + procedureName + "()");
      string fileName = procedureName;
      FileWriter fw = new FileWriter(AC.APPath, fileName, "sql");
      SQLScript newScript = new SQLScript(fileName, DBType.SP, table.IsSys, table, fw);
      newScript.AddDependency("dbAktivenplaner");
      newScript.AddDependency(table.TableName);
      newScript.AddDependency("fnSysAddToHistory");
      newScript.AddDependency("fnSysPrintDebug");

      List<CSV_Tabelle> refTables = GetReferencingTables(table.TableName);

      string purpose = "Loescht einen Eintrag aus der Tabelle " + table.TableName + ".";
      List<string> returns = new List<string>();            // should not be null
      List<string> errorCodes = null;         // null -> sp-standard
      List<string> neededDbObjects = new List<string>();    // should not be null
      returns.Add("Resultset: Der geloeschte Datensatz wird zurueck gegeben.");
      returns.Add("           -10 als Id falls die Prozedur fehlschlug.");
      neededDbObjects.Add("fnSysAddToHistory");
      neededDbObjects.Add("fnSysPrintDebug");
      foreach (var tab in refTables) {
        neededDbObjects.Add("sp" + (tab.IsSys ? "Sys" : "AP") + "Update" + tab.DataName);
        neededDbObjects.Add("sp" + (tab.IsSys ? "Sys" : "AP") + "Delete" + tab.DataName);
        newScript.AddDependency("sp" + (tab.IsSys ? "Sys" : "AP") + "Update" + tab.DataName);
        newScript.AddDependency("sp" + (tab.IsSys ? "Sys" : "AP") + "Delete" + tab.DataName);
      }

      fw.Amltf(GetSkriptHeader(procedureName, purpose, returns, errorCodes, neededDbObjects));

      CSV_Spalte tmpCol = new CSV_Spalte(0, table.TableName, "", "", "", false, false, false, "", "", "", "", "", "", "");
      string tableId = table.Columns[0].Attribut;

      fw.Altf("");
      fw.Altf("USE dbAktivenplaner;");
      fw.Altf("DROP PROCEDURE IF EXISTS " + procedureName + ";");
      fw.Altf("");
      fw.Altf("DELIMITER //");
      fw.Altf("CREATE PROCEDURE " + procedureName + " (");
      fw.Altf("    IN    in_nCallerId       INT             --  Is mandatory");
      fw.Altf("  -- -");
      fw.Altf("  , IN    in_" + tableId + "    INT");
      fw.Altf("  -- -");
      fw.Altf("  , OUT   out_" + tableId + "   " + GetDataTypeAsStringForSql(table.Columns[0].Art, table.Columns[0].Groesse));
      fw.Altf("  -- -");
      fw.Altf("  , IN    in_nResultset    INT             --  <>0:  select a resultset");
      fw.Altf("  , IN    in_nDebug        INT             --  <>0:  create debug-output, 1: select and delete debug at the end");
      fw.Altf("  , IN    in_nDebugDepth   INT             --  indentation of debug inserts");
      fw.Altf("  , INOUT inout_nError     INT             --  stores the error-code");
      fw.Altf("  , INOUT inout_szError    NVARCHAR(1000)  --  stores the error-message");
      fw.Altf("  , IN    in_nDeleteMode   INT             --  0:deleteSave, 1:deleteSetDefault, 2:deleteCascade");
      fw.Altf(") BEGIN");
      fw.Altf("");
      fw.Altf("  -- initialPart:BEGIN");
      fw.Altf("");
      fw.Altf("    -- Start: Declaration of local variables (only in procedure)");
      fw.Altf("    DECLARE szProcedureName   NVARCHAR(50)    DEFAULT N'" + procedureName + "';");
      fw.Altf("    DECLARE nRowCount         INT             DEFAULT 0;");
      fw.Altf("    DECLARE nSuccess          INT             DEFAULT 1;");
      fw.Altf("    DECLARE dtProcedureStart  DATETIME        DEFAULT NOW();");
      fw.Altf("    DECLARE nResultId         INT             DEFAULT 0;");
      fw.Altf("    DECLARE nErrorCall        INT             DEFAULT 0;");
      fw.Altf("    DECLARE szErrorCall       NVARCHAR(1000)  DEFAULT N'';");
      fw.Altf("    DECLARE nDebugCall        INT             DEFAULT 0;");
      fw.Altf("    --  -End-: Declaration of local variables (only in procedure)");
      fw.Altf("");
      fw.Altf("    --  Start: Declare cursors");
      fw.Altf("    DECLARE done              INT             DEFAULT FALSE;");
      fw.Altf("    DECLARE c_Id              INT             DEFAULT 0;");
      foreach (var refTable in refTables) {
        foreach (var col in refTable.Columns) {
          if (col.ForeignKeyTo.Length > 0 && col.ForeignKeyTo != "Enum") {
            if (table.TableName == col.ForeignKeyTo.Split('.')[0].Substring(3)) {
              // Start: Do stuff for one referencing tableColumn
              fw.Altf("    DECLARE cur_" + refTable.DataName + "_" + col.Attribut + " CURSOR FOR");
              fw.Altf("        SELECT " + refTable.Columns[0].Attribut + "");
              fw.Altf("        FROM " + refTable.TableName + "");
              fw.Altf("        WHERE " + col.Attribut + " = in_" + tableId);
              fw.Altf("        ;");
              // End: Do stuff for one referencing tableColumn
            }
          }
        }
      }
      fw.Altf("    DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = TRUE;");
      fw.Altf("    --  -End-: Declare cursors");
      fw.Altf("");
      fw.Altf("    --  Start: Setting of global variables (stay alive after procedure)");
      fw.Altf("    SET @nResultId    = 0;");
      fw.Altf("    SET @nErrorCall   = 0;");
      fw.Altf("    SET @szErrorCall  = N'';");
      fw.Altf("    CREATE TEMPORARY TABLE IF NOT EXISTS tblTdDelete" + table.DataName + " (");
      for (int i = 0; i < table.Columns.Count; i++) {
        CSV_Spalte column = table.Columns[i];
        // format and comma
        string s = "  ";
        if (i == 0) {
          s += "  ";
        }
        else {
          s += ", ";
        }
        // attribut name
        s += column.Attribut;
        // art
        s += " " + GetDataTypeAsStringForSql(column.Art, column.Groesse);
        // Primary Key
        if (column.PrimKey) {
          s += " PRIMARY KEY";
        }
        // Not null
        if (column.NotNull) {
          s += " NOT NULL";
        }
        // Auto increment
        if (column.AutoIncre) {
          s += " AUTO_INCREMENT";
        }
        // Default
        if (column.DefaultVal.Length > 0) {
          s += " DEFAULT ";
          if (column.Art != "int" && column.Art != "decimal") { s += "N'"; }
          s += column.DefaultVal;
          if (column.Art != "int" && column.Art != "decimal") { s += "'"; }
        }
        else {
          if (column.Art == "datetime" && column.NotNull) {
            s += " DEFAULT NOW()";
          }
        }
        fw.Altf("    " + s);
      }
      fw.Altf("    );");
      fw.Altf("    TRUNCATE tblTdDelete" + table.DataName + ";");
      fw.Altf("    --  -End-: Setting of global variables (stay alive after procedure)");
      fw.Altf("");
      fw.Altf("    --  Start: Set default values");
      fw.Altf("    SET out_" + tableId + " = IFNULL(out_" + tableId + ", -10);");
      fw.Altf("    -- -");
      fw.Altf("    SET in_nResultset         = IFNULL(in_nResultset,    0);");
      fw.Altf("    SET in_nDebug             = IFNULL(in_nDebug,        0);");
      fw.Altf("    SET in_nDebugDepth        = IFNULL(in_nDebugDepth,   0);");
      fw.Altf("    SET inout_nError          = IFNULL(inout_nError,     0);");
      fw.Altf("    SET inout_szError         = IFNULL(inout_szError,    N'');");
      fw.Altf("    SET in_nDeleteMode        = IFNULL(in_nDeleteMode,   0);");
      fw.Altf("    SET nDebugCall = in_nDebug;");
      fw.Altf("    IF in_nDebug = 1 THEN");
      fw.Altf("      SET nDebugCall = 2;");
      fw.Altf("    END IF;");
      fw.Altf("    --  -End-: Set default values");
      fw.Altf("");
      fw.Altf("    --  Start: Create Debug-table if necessary");
      fw.Altf("    IF in_nDebug <> 0 THEN");
      fw.Altf("      CREATE TABLE IF NOT EXISTS tblSysDebug (");
      fw.Altf("          nId       INT             NOT NULL PRIMARY KEY AUTO_INCREMENT");
      fw.Altf("        , dtTime    TIMESTAMP       NOT NULL DEFAULT CURRENT_TIMESTAMP");
      fw.Altf("        , szComment NVARCHAR(1000)  NOT NULL DEFAULT N'Missing debug comment!'");
      fw.Altf("      );");
      fw.Altf("    END IF;");
      fw.Altf("    --  -End-: Create Debug-table if necessary");
      fw.Altf("");
      fw.Altf("    --  Start: Debug output at procedure start");
      fw.Altf("    IF in_nDebug <> 0 THEN");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, N'---------------------------------------------------');");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'Starting Procedure ', szProcedureName, N':'));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_nCallerId    = ', in_nCallerId));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_" + tableId + " = ', in_" + tableId + "));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_nResultset = ', in_nResultset));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'       with in_nDebug     = ', in_nDebug));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, N'---------------------------------------------------');");
      fw.Altf("    END IF;");
      fw.Altf("    --  -End-: Debug output at procedure start");
      fw.Altf("");
      fw.Altf("    --  Start: Check inputs");
      fw.Altf("    SET inout_szError = N'ERROR at input: ';");
      fw.Altf("    IF IFNULL(in_nCallerId, -1) < 0 THEN");
      fw.Altf("      SET inout_nError  = 1;");
      fw.Altf("      SET inout_szError = CONCAT_WS(N'', inout_szError, N'Input in_nCallerId was incorrect. (', in_nCallerId, N')');");
      fw.Altf("    END IF;");
      fw.Altf("    IF IFNULL(in_" + tableId + ", -1) < 0 THEN");
      fw.Altf("      SET inout_nError  = 1;");
      fw.Altf("      SET inout_szError = CONCAT_WS(N'', inout_szError, N'Input in_" + tableId + " was incorrect. (', in_" + tableId + ", N')');");
      fw.Altf("    END IF;");
      fw.Altf("    --  -End-: Check inputs");
      fw.Altf("");
      fw.Altf("  -- END initialPart;");
      fw.Altf("");
      fw.Altf("  -- ---------------------------------- -");
      fw.Altf("  --  Main part");
      fw.Altf("  -- ---------------------------------- -");
      fw.Altf("  mainPart:BEGIN");
      fw.Altf("");
      fw.Altf("    --  Start: Declaration of handlers");
      fw.Altf("    DECLARE EXIT      HANDLER FOR 1062      -- ER_DUP_ENTRY");
      fw.Altf("        SET inout_nError = 4;");
      fw.Altf("    DECLARE EXIT      HANDLER FOR 45000     -- User defined error");
      fw.Altf("        SET inout_nError = 5;");
      fw.Altf("    DECLARE EXIT      HANDLER FOR 1106      -- ER_UNKNOWN_PROCEDURE");
      fw.Altf("        SET inout_nError = 6;");
      fw.Altf("    DECLARE EXIT      HANDLER FOR 1122      -- ER_CANT_FIND_UDF");
      fw.Altf("        SET inout_nError = 7;");
      fw.Altf("    --  -End-: Declaration of handlers");
      fw.Altf("");
      fw.Altf("    --  Start: Error check at beginning of main part");
      fw.Altf("    IF IFNULL(inout_nError, 0) <> 0 THEN");
      fw.Altf("      SET nSuccess = 0;");
      fw.Altf("      LEAVE mainPart;");
      fw.Altf("    END IF;");
      fw.Altf("    --  -End-: Error check at beginning of main part");
      fw.Altf("    SET inout_nError  = 0;");
      fw.Altf("    SET inout_szError = N'';");
      fw.Altf("");
      fw.Altf("");
      fw.Altf("");
      fw.Altf("");
      fw.Altf("");
      fw.Altf("    -- Test if " + table.TableName + " already exists");
      fw.Altf("    IF NOT EXISTS (SELECT 1");
      fw.Altf("      FROM " + table.TableName + "");
      fw.Altf("      WHERE " + tableId + " = in_" + tableId + "");
      fw.Altf("    )");
      fw.Altf("    THEN");
      fw.Altf("      SET inout_nError    = 1;");
      fw.Altf("      SET inout_szError   = N'" + table.DataName + " does not exist.';");
      fw.Altf("      SET nSuccess        = 0;");
      fw.Altf("      SET out_" + tableId + " = -10;");
      fw.Altf("      -- Start: Debug");
      fw.Altf("      IF in_nDebug <> 0 THEN");
      fw.Altf("        SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'" + table.DataName + " does not exist.', N''));");
      fw.Altf("      END IF;");
      fw.Altf("      -- -End-: Debug");
      fw.Altf("      LEAVE mainPart;");
      fw.Altf("    END IF;");
      fw.Altf("");
      fw.Altf("  -- Load original values");
      fw.Altf("    INSERT tblTdDelete" + table.DataName + " (");
      for (int i = 0; i < table.Columns.Count; i++) {
        tmpCol = table.Columns[i];
        if (i == 0) {
          fw.Altf("            " + tmpCol.Attribut + "");
        }
        else {
          fw.Altf("          , " + tmpCol.Attribut + "");
        }
      }
      fw.Altf("      )");
      fw.Altf("      SELECT  " + tableId + "");
      for (int i = 0; i < table.Columns.Count; i++) {
        tmpCol = table.Columns[i];
        fw.Altf("            , " + tmpCol.Attribut + "");
      }
      fw.Altf("      FROM " + table.TableName);
      fw.Altf("      WHERE " + tableId + " = in_" + tableId + ";");
      fw.Altf("");
      fw.Altf("    -- Start: DeleteMode handling (in_nDeleteMode= 0:deleteSave, 1:deleteSetDefault, 2:deleteCascade)");
      if (refTables.Count > 0) {
        fw.Altf("    --   DeleteMode: 0 -> deleteSave (dont delete if reference exists)");
        fw.Altf("    IF in_nDeleteMode = 0 THEN");
        foreach (var refTable in refTables) {
          foreach (var col in refTable.Columns) {
            if (col.ForeignKeyTo.Length > 0 && col.ForeignKeyTo != "Enum") {
              if (table.TableName == col.ForeignKeyTo.Split('.')[0].Substring(3)) {
                // Start: Do stuff for one referencing tableColumn
                fw.Altf("      -- check if reference exists in " + refTable.TableName + " on column " + col.Attribut);
                fw.Altf("      IF EXISTS ( SELECT " + refTable.Columns[0].Attribut + " FROM " + refTable.TableName);
                fw.Altf("                   WHERE " + col.Attribut + " = in_" + tableId);
                fw.Altf("                   LIMIT 1");
                fw.Altf("      )");
                fw.Altf("      THEN");
                fw.Altf("        SET inout_nError = 1;");
                fw.Altf("        SET inout_szError = N'At least one reference on this dataset exists. (found at " + refTable.TableName + ")';");
                fw.Altf("        SET nSuccess = 0;");
                fw.Altf("        IF in_nDebug <> 0 THEN");
                fw.Altf("          SET @func = fnSysPrintDebug(in_nDebugDepth, N'At least one reference on this dataset exists. (found at " + refTable.TableName + ")');");
                fw.Altf("          SET @func = fnSysPrintDebug(in_nDebugDepth, N'    -> quit deleting');");
                fw.Altf("        END IF;");
                fw.Altf("        LEAVE mainPart;");
                fw.Altf("      END IF;");
                // End: Do stuff for one referencing tableColumn
              }
            }
          }
        }
        fw.Altf("    END IF;");
        fw.Altf("");
        fw.Altf("    --   DeleteMode: 1 -> deleteSetDefault");
        fw.Altf("    IF in_nDeleteMode = 1 THEN");
        foreach (var refTable in refTables) {
          foreach (var col in refTable.Columns) {
            if (col.ForeignKeyTo.Length > 0 && col.ForeignKeyTo != "Enum") {
              if (table.TableName == col.ForeignKeyTo.Split('.')[0].Substring(3)) {
                // Start: Do stuff for one referencing tableColumn
                fw.Altf("      -- update referencing column " + col.Attribut + " from " + refTable.TableName);
                fw.Altf("      IF in_nDebug <> 0 THEN");
                fw.Altf("        SET @func = fnSysPrintDebug(in_nDebugDepth, N'---------------------------------------------------');");
                fw.Altf("      END IF;");
                fw.Altf("      OPEN cur_" + refTable.DataName + "_" + col.Attribut + ";");
                fw.Altf("      update_" + refTable.TableName + "_" + col.Attribut + ": LOOP");
                fw.Altf("        FETCH cur_" + refTable.DataName + "_" + col.Attribut + " INTO c_Id;");
                fw.Altf("        IF done THEN");
                fw.Altf("          LEAVE update_" + refTable.DataName + "_" + col.Attribut + ";");
                fw.Altf("        END IF;");
                fw.Altf("        IF in_nDebug <> 0 THEN");
                fw.Altf("          SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'Updating " + refTable.TableName + " with Id ', c_Id, N''));");
                fw.Altf("        END IF;");
                fw.Altf("        CALL spAPUpdate" + refTable.DataName + "(");
                fw.Altf("              in_nCallerId");
                fw.Altf("            , c_Id");
                fw.Altf("            , 4");
                for (int i = 3; i < refTable.Columns.Count; i++) {
                  if (col.Attribut == refTable.Columns[i].Attribut) {
                    fw.Altf("            , -1         -- <- this is the update");
                  }
                  else {
                    fw.Altf("            , null");
                  }
                }
                fw.Altf("            , nResultId");
                fw.Altf("            , 0");
                fw.Altf("            , nDebugCall");
                fw.Altf("            , (in_nDebugDepth+1)");
                fw.Altf("            , nErrorCall");
                fw.Altf("            , szErrorCall");
                fw.Altf("        );");
                fw.Altf("        IF nErrorCall <> 0 THEN");
                fw.Altf("          SET inout_nError  = nErrorCall;");
                fw.Altf("          SET inout_szError = szErrorCall;");
                fw.Altf("          SET nSuccess = 0;");
                fw.Altf("          IF in_nDebug <> 0 THEN");
                fw.Altf("            SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'    Error: ',nErrorCall, N' -> ', szErrorCall, N''));");
                fw.Altf("          END IF;");
                fw.Altf("          SET @func = fnSysAddToHistory (");
                fw.Altf("          /* in_nCallerId     */    0       -- 0:System, if anything else, function wont work");
                fw.Altf("          /* in_szTable     */  , N'" + table.TableName + "'");
                fw.Altf("          /* in_nTableId    */  , in_" + tableId);
                fw.Altf("          /* in_eDMLArt     */  , 123               -- 123=update     __TODO__ was ist die nummer fuer update?");
                fw.Altf("          /* in_szFunction  */  , N'Delete'");
                fw.Altf("          /* in_nError      */  , inout_nError");
                fw.Altf("          /* in_szKommentar */  , CONCAT_WS(N'', N'ERROR AT DELETE (mode=', in_nDeleteMode, N'): Failed to delete at " + table.TableName + ": ', inout_szError)");
                fw.Altf("          /* in_nNutzerId   */  , in_nCallerId");
                fw.Altf("          );");
                fw.Altf("          LEAVE update_" + refTable.DataName + "_" + col.Attribut + ";");
                fw.Altf("        END IF;");
                fw.Altf("        IF in_nDebug <> 0 THEN");
                fw.Altf("          SET inout_nError  = nErrorCall;");
                fw.Altf("          SET inout_szError = szErrorCall;");
                fw.Altf("          SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'    Success: ', szErrorCall, N''));");
                fw.Altf("        END IF;");
                fw.Altf("      END LOOP;");
                fw.Altf("      CLOSE cur_" + refTable.DataName + "_" + col.Attribut + ";");
                fw.Altf("      IF in_nDebug <> 0 THEN");
                fw.Altf("        SET @func = fnSysPrintDebug(in_nDebugDepth, N'');");
                fw.Altf("      END IF;");
                fw.Altf("      IF nErrorCall <> 0 THEN");
                fw.Altf("        SET inout_nError  = nErrorCall;");
                fw.Altf("        SET inout_szError = szErrorCall;");
                fw.Altf("        LEAVE mainPart;");
                fw.Altf("      END IF;");
                // End: Do stuff for one referencing tableColumn
              }
            }
          }
        }
        fw.Altf("    END IF;");
        fw.Altf("");
        fw.Altf("    --   DeleteMode: 2 -> deleteCascade");
        fw.Altf("    IF in_nDeleteMode = 2 THEN");
        foreach (var refTable in refTables) {
          foreach (var col in refTable.Columns) {
            if (col.ForeignKeyTo.Length > 0 && col.ForeignKeyTo != "Enum") {
              if (table.TableName == col.ForeignKeyTo.Split('.')[0].Substring(3)) {
                // Start: Do stuff for one referencing tableColumn
                fw.Altf("      -- delete dataset with referencing column " + col.Attribut + " from " + refTable.TableName);
                fw.Altf("      IF in_nDebug <> 0 THEN");
                fw.Altf("        SET @func = fnSysPrintDebug(in_nDebugDepth, N'---------------------------------------------------');");
                fw.Altf("      END IF;");
                fw.Altf("      OPEN cur_" + refTable.DataName + "_" + col.Attribut + ";");
                fw.Altf("      delete_" + refTable.DataName + "_" + col.Attribut + ": LOOP");
                fw.Altf("        FETCH cur_" + refTable.DataName + "_" + col.Attribut + " INTO c_Id;");
                fw.Altf("        IF done THEN");
                fw.Altf("          LEAVE delete_" + refTable.DataName + "_" + col.Attribut + ";");
                fw.Altf("        END IF;");
                fw.Altf("        IF in_nDebug <> 0 THEN");
                fw.Altf("          SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'Deleting " + refTable.DataName + " with Id ', c_Id, N''));");
                fw.Altf("        END IF;");
                fw.Altf("        CALL spAPDelete" + refTable.DataName + "(");
                fw.Altf("              in_nCallerId");
                fw.Altf("            , c_Id");
                fw.Altf("            , nResultId");
                fw.Altf("            , 0");
                fw.Altf("            , nDebugCall");
                fw.Altf("            , (in_nDebug+1)");
                fw.Altf("            , nErrorCall");
                fw.Altf("            , szErrorCall");
                fw.Altf("            , in_nDeleteMode");
                fw.Altf("        );");
                fw.Altf("        IF nErrorCall <> 0 THEN");
                fw.Altf("          SET inout_nError  = nErrorCall;");
                fw.Altf("          SET inout_szError = szErrorCall;");
                fw.Altf("          SET nSuccess = 0;");
                fw.Altf("          IF in_nDebug <> 0 THEN");
                fw.Altf("            SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'    Error: ',nErrorCall, N' -> ', szErrorCall, N''));");
                fw.Altf("          END IF;");
                fw.Altf("          SET @func = fnSysAddToHistory (");
                fw.Altf("          /* in_nCallerId     */    0       -- 0:System, if anything else, function wont work");
                fw.Altf("          /* in_szTable     */  , N'" + table.TableName + "'");
                fw.Altf("          /* in_nTableId    */  , in_" + tableId);
                fw.Altf("          /* in_eDMLArt     */  , 123               -- 123=update     __TODO__ was ist die nummer fuer update?");
                fw.Altf("          /* in_szFunction  */  , N'Delete'");
                fw.Altf("          /* in_nError      */  , inout_nError");
                fw.Altf("          /* in_szKommentar */  , CONCAT_WS(N'', N'ERROR AT DELETE (mode=', in_nDeleteMode, N'): Failed to delete at " + table.TableName + ": ', inout_szError)");
                fw.Altf("          /* in_nNutzerId   */  , in_nCallerId");
                fw.Altf("          );");
                fw.Altf("          LEAVE delete_" + refTable.DataName + "_" + col.Attribut + ";");
                fw.Altf("        END IF;");
                fw.Altf("        IF in_nDebug <> 0 THEN");
                fw.Altf("          SET inout_nError  = nErrorCall;");
                fw.Altf("          SET inout_szError = szErrorCall;");
                fw.Altf("          SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'    Success: ', szErrorCall, N''));");
                fw.Altf("        END IF;");
                fw.Altf("      END LOOP;");
                fw.Altf("      CLOSE cur_" + refTable.DataName + "_" + col.Attribut + ";");
                fw.Altf("      IF in_nDebug <> 0 THEN");
                fw.Altf("        SET @func = fnSysPrintDebug(in_nDebugDepth, N'');");
                fw.Altf("      END IF;");
                fw.Altf("      IF nErrorCall <> 0 THEN");
                fw.Altf("        SET inout_nError  = nErrorCall;");
                fw.Altf("        SET inout_szError = szErrorCall;");
                fw.Altf("        LEAVE mainPart;");
                fw.Altf("      END IF;");
                // End: Do stuff for one referencing tableColumn
              }
            }
          }
        }
        fw.Altf("    END IF;");
      }
      fw.Altf("    -- -End-: DeleteMode handling (in_nDeleteMode= 0:deleteSave, 1:deleteSetDefault, 2:deleteCascade)");
      fw.Altf("");
      // delete dataset
      fw.Altf("    -- Delete from " + table.TableName);
      fw.Altf("    DELETE FROM " + table.TableName);
      fw.Altf("    WHERE " + tableId + " = in_" + tableId + ";");
      fw.Altf("");
      fw.Altf("    -- Start: Evaluation");
      fw.Altf("    SET nRowCount = ROW_COUNT();");
      fw.Altf("    SET out_" + tableId + " = in_" + tableId + ";");
      fw.Altf("    SET nSuccess = 1;");
      fw.Altf("    SET inout_szError = CONCAT_WS(N'', N'Delete of " + table.DataName + " with " + tableId + " = ', out_" + tableId + ");");
      for (int i = 3; i < table.Columns.Count; i++) {
        tmpCol = table.Columns[i];
        fw.Altf("    SET inout_szError = CONCAT_WS(N'', inout_szError, N', " + tmpCol.Attribut + " = ', (SELECT " + tmpCol.Attribut + " FROM tblTdDelete" + table.DataName + " WHERE " + tableId + " = in_" + tableId + " LIMIT 1) );");
      }
      fw.Altf("    SET inout_szError = CONCAT_WS(N'', inout_szError, N'.');");
      fw.Altf("    --  -End-: Evaluation");
      fw.Altf("");
      fw.Altf("    --  Start: Debug");
      fw.Altf("    IF in_nDebug <> 0 THEN");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'Deleted " + table.DataName + "', N''));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'     nRowCount: ', nRowCount));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'     " + tableId + ": ', out_" + tableId + "));");
      for (int i = 2; i < table.Columns.Count; i++) {
        tmpCol = table.Columns[i];
        fw.Altf("      SET inout_szError = CONCAT_WS(N'', inout_szError, N', " + tmpCol.Attribut + " = ', in_" + tmpCol.Attribut + ");");
      }
      fw.Altf("    END IF;");
      fw.Altf("    --  -End-: Debug");
      fw.Altf("");
      fw.Altf("");
      fw.Altf("");
      fw.Altf("");
      fw.Altf("");
      fw.Altf("");
      fw.Altf("    --  Start: Error check at end of main part");
      fw.Altf("    IF IFNULL(nRowCount, -1) < 1 THEN");
      fw.Altf("      SET inout_nError = 2;");
      fw.Altf("      SET inout_szError = N'Couldnt update " + table.TableName + ". RowCount to low.';");
      fw.Altf("      SET nSuccess  = 0;");
      fw.Altf("      LEAVE mainPart;");
      fw.Altf("    END IF;");
      fw.Altf("    --  -End-: Error check at end of main part");
      fw.Altf("");
      fw.Altf("    -- Start: Secure and evaluate errors");
      fw.Altf("    if ifnull(out_" + tableId + ", -1) < 0 then");
      fw.Altf("      SET inout_nError  = 2;");
      fw.Altf("      SET inout_szError = N'Id out_" + tableId + " couldnt be resolved';");
      fw.Altf("      SET nSuccess  = 0;");
      fw.Altf("      LEAVE mainPart;");
      fw.Altf("    END IF;");
      fw.Altf("    -- -End-: Secure and evaluate errors");
      fw.Altf("");
      fw.Altf("    -- Start: End of procedure -> everything is ok");
      fw.Altf("    SET inout_nError  = 0;");
      fw.Altf("    SET inout_szError = CONCAT_WS(N'', inout_szError, N' -> No error occured.');");
      fw.Altf("    -- -End-: End of procedure -> everything is ok");
      fw.Altf("");
      fw.Altf("  END mainPart;");
      fw.Altf("");
      fw.Altf("  -- ---------------------------------- -");
      fw.Altf("  -- Error part");
      fw.Altf("  -- ---------------------------------- -");
      fw.Altf("  errorPart:BEGIN");
      fw.Altf("");
      fw.Altf("    --  Start: Check for errors");
      fw.Altf("    IF inout_nError = 0 THEN");
      fw.Altf("      LEAVE errorPart;");
      fw.Altf("    END IF;");
      fw.Altf("    --  -End-: Check for errors");
      fw.Altf("");
      fw.Altf("    set nSuccess = 0;");
      fw.Altf("");
      fw.Altf("    --  Start: Call fnSysAddToHistory");
      fw.Altf("    SET @func = fnSysAddToHistory (");
      fw.Altf("    /* in_nCallerId     */    0       -- 0:System, if anything else, function wont work");
      fw.Altf("    /* in_szTable     */  , N'" + table.TableName + "'");
      fw.Altf("    /* in_nTableId    */  , -10");
      fw.Altf("    /* in_eDMLArt     */  , 123               -- 123=update     __TODO__ was ist die nummer fuer update?");
      fw.Altf("    /* in_szFunction  */  , N'delete'");
      fw.Altf("    /* in_nError      */  , inout_nError");
      fw.Altf("    /* in_szKommentar */  , CONCAT_WS(N'', N'Failed to delete at " + table.TableName + ": ', inout_szError)");
      fw.Altf("    /* in_nNutzerId   */  , in_nCallerId");
      fw.Altf("    );");
      fw.Altf("    --  -End-: Call fnSysAddToHistory");
      fw.Altf("");
      fw.Altf("    --  Start: Debug");
      fw.Altf("    IF in_nDebug <> 0 THEN");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, N'');");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, N'---------------------------------------------------');");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'Finished with errors', N'.'));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'   inout_nError :  ', inout_nError));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'   inout_szError:  ', inout_szError));");
      fw.Altf("    END IF;");
      fw.Altf("    --  -End-: Debug");
      fw.Altf("");
      fw.Altf("  END errorPart;");
      fw.Altf("");
      fw.Altf("  -- ---------------------------------- -");
      fw.Altf("  -- End part");
      fw.Altf("  -- ---------------------------------- -");
      fw.Altf("  endPart:BEGIN");
      fw.Altf("");
      fw.Altf("    --  Start: Call fnSysAddToHistory");
      fw.Altf("    IF  inout_nError   = 0");
      fw.Altf("    AND nSuccess = 1 THEN");
      fw.Altf("      SET @func = fnSysAddToHistory (");
      fw.Altf("      /* in_nCallerId     */    0       -- 0:System, if anything else, function wont work");
      fw.Altf("      /* in_szTable     */  , N'" + table.TableName + "'");
      fw.Altf("      /* in_nTableId    */  , out_" + tableId + "");
      fw.Altf("      /* in_eDMLArt     */  , 123               -- 123=update     __TODO__ was ist die nummer fuer update?");
      fw.Altf("      /* in_szFunction  */  , N'delete'");
      fw.Altf("      /* in_nError      */  , inout_nError");
      fw.Altf("      /* in_szKommentar */  , CONCAT_WS(N'', N'Successfull delete at " + table.TableName + ": ', inout_szError)");
      fw.Altf("      /* in_nNutzerId   */  , in_nCallerId");
      fw.Altf("      );");
      fw.Altf("    END IF;");
      fw.Altf("    --  -End-: Call fnSysAddToHistory");
      fw.Altf("");
      fw.Altf("    --  Start: Build resultset");
      fw.Altf("    IF in_nResultset <> 0 THEN");
      fw.Altf("      SELECT * FROM tblTdDelete" + table.DataName + ";");
      fw.Altf("    END IF;");
      fw.Altf("    --  -End-: Build resultset");
      fw.Altf("");
      fw.Altf("    --  Start: Debug");
      fw.Altf("    IF in_nDebug <> 0 THEN");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, N'');");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, N'---------------------------------------------------');");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N'Ending Procedure ', szProcedureName, N'.'));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N' - at ', NOW()));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, CONCAT_WS(N'', N' - time: ', TIMEDIFF(NOW(), dtProcedureStart)));");
      fw.Altf("      SET @func = fnSysPrintDebug(in_nDebugDepth, N'---------------------------------------------------');");
      fw.Altf("      IF in_nDebug = 1 THEN");
      fw.Altf("        SELECT * FROM tblSysDebug;");
      fw.Altf("        TRUNCATE TABLE tblSysDebug;");
      fw.Altf("      END IF;");
      fw.Altf("    END IF;");
      fw.Altf("    --  -End-: Debug");
      fw.Altf("");
      fw.Altf("  END endPart;");
      fw.Altf("");
      fw.Altf("END; //");
      fw.Altf("DELIMITER ;");
      fw.Altf("");
      fw.Altf("/* Test-Call");
      fw.Altf("use dbAktivenplaner;");
      fw.Altf("set @in_nCallerId = 492;");
      fw.Altf("-- -");
      fw.Altf("set @in_" + tableId + " = 10;");
      fw.Altf("-- -");
      fw.Altf("set @out_" + tableId + " = -42;");
      fw.Altf("-- -");
      fw.Altf("set @in_nResultset    = 1;");
      fw.Altf("set @in_nDebug        = 1;");
      fw.Altf("set @in_nDebugDepth   = 0;");
      fw.Altf("set @inout_nError     = 0;");
      fw.Altf("set @inout_szError    = N'';");
      fw.Altf("set @in_nDeleteMode   = 0;");
      fw.Altf("CALL " + procedureName + "(");
      fw.Altf("    @in_nCallerId");
      fw.Altf("  , @in_" + tableId + "");
      fw.Altf("  , @out_" + tableId + "");
      fw.Altf("  , @in_nResultset");
      fw.Altf("  , @in_nDebug");
      fw.Altf("  , @in_nDebugDepth");
      fw.Altf("  , @inout_nError");
      fw.Altf("  , @inout_szError");
      fw.Altf("  , @in_nDeleteMode");
      fw.Altf(");");
      fw.Altf("select * from tblSysHistory order by nId desc;");
      fw.Altf("select * from " + table.TableName + " order by " + tableId + " desc;");
      fw.Altf("*/");
      fw.Altf("");

      // Script-End
      SQLDependencies.AddScript(newScript);
    }
    #endregion create_storedProcedures

    #region helper_methods
    private List<string> GetSimpleScriptHeader(string inDbObject) {
      List<string> newContent = new List<string>();

      newContent.Add(GetHeaderRow("*1"));
      newContent.Add(GetHeaderRow(""));
      if (inDbObject.StartsWith("tblAP") || inDbObject.StartsWith("tblSys")) {
        newContent.Add(GetHeaderRow("Table: " + inDbObject));
      }
      else {
        newContent.Add(GetHeaderRow("Database: " + inDbObject));
      }
      newContent.Add(GetHeaderRow("History :"));
      newContent.Add(GetHeaderRow("          - " + AC.GetStartDate() + ", VD, Creation of the script"));
      newContent.Add(GetHeaderRow(""));
      newContent.Add(GetHeaderRow("*2"));

      return newContent;
    }
    private List<string> GetSkriptHeader(string scriptName, string purpose, List<string> returns, List<string> errorCodes, List<string> neededDbObjects) {
      List<string> newContent = new List<string>();

      newContent.Add(GetHeaderRow("*1"));
      newContent.Add(GetHeaderRow(""));
      // Script-Name
      newContent.Add(GetHeaderRow("Skript  : " + scriptName));
      // Purpose
      newContent.Add(GetHeaderRow("Purpose : " + purpose));
      // returns
      if (returns == null) {
        newContent.Add(GetHeaderRow("Returns : TODO"));
      }
      else {
        for (int i = 0; i < returns.Count; i++) {
          if (i == 0) {
            newContent.Add(GetHeaderRow("Returns : " + returns[i]));
          }
          else {
            newContent.Add(GetHeaderRow("           " + returns[i]));
          }
        }
      }
      // errorCodes
      if (errorCodes == null) {
        newContent.Add(GetHeaderRow("nError  : -x  - Error at called procedure"));
        newContent.Add(GetHeaderRow("          0   - Completed successfully"));
        newContent.Add(GetHeaderRow("          1   - Incorrect input"));
        newContent.Add(GetHeaderRow("          2   - Error at end of procedure"));
        newContent.Add(GetHeaderRow("          3   - Error at called procedure"));
        newContent.Add(GetHeaderRow("          4   - (1062)  Found duplicate key"));
        newContent.Add(GetHeaderRow("          5   - (45000) User defined error"));
        newContent.Add(GetHeaderRow("          6   - (1106)  Unknown procedure"));
        newContent.Add(GetHeaderRow("          7   - (1122)  Unknown function"));
      }
      else {
        for (int i = 0; i < errorCodes.Count; i++) {
          if (i == 0) {
            newContent.Add(GetHeaderRow("nError  : " + errorCodes[i]));
          }
          else {
            newContent.Add(GetHeaderRow("           " + errorCodes[i]));
          }
        }
      }
      // needed Db Objects
      if (neededDbObjects == null) {
        newContent.Add(GetHeaderRow("Needs following DB-Objects:"));
        newContent.Add(GetHeaderRow("            ->  TODO"));
      }
      else {
        if (neededDbObjects.Count > 0) {
          newContent.Add(GetHeaderRow("Needs following DB-Objects:"));
          for (int i = 0; i < neededDbObjects.Count; i++) {
            newContent.Add(GetHeaderRow("            ->  " + neededDbObjects[i]));
          }
        }
      }
      // History
      newContent.Add(GetHeaderRow("History :"));
      newContent.Add(GetHeaderRow("          - " + AC.GetStartDate() + ", VD, Creation of the script"));
      newContent.Add(GetHeaderRow(""));
      newContent.Add(GetHeaderRow("*2"));

      return newContent;
    }
    private string GetHeaderRow(string inRow) {
      int width = 80;
      if (width < 60) {
        return "TODO";
      }
      string ret = "";
      if (inRow == "*1") {
        ret += "/* ";
        for (int i = 0; i < width - 3; i++) {
          ret += "*";
        }
        return ret;
      }
      if (inRow == "*2") {
        for (int i = 0; i < width - 3; i++) {
          ret += "*";
        }
        ret += " */";
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
    private string GetDataTypeAsStringForSql(string inArt, string inSize) {
      if (inArt.StartsWith("enum_")) {
        return "INT";
      }
      foreach (var c in AC.DbTables) {
        if (c.DataName == inArt) {
          return "INT";
        }
      }
      switch (inArt) {
        case "int":
          return "INT";
        case "datetime":
          return "DATETIME";
        case "decimal":
          //return "DECIMAL("+inSize.Replace(",",".")+")";
          return "DECIMAL(10,2)";
        case "nvarchar":
          return "NVARCHAR(" + inSize.Replace(",", ".") + ")";
        case "date":
          return "DATE";
        case "bool":
          return "BOOLEAN";
        default:
          return " _TODO_ ";
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
    private int References(CSV_Spalte column) {
      if (column.Attribut.StartsWith("i")) {
        return 2;
      }
      if (column.Attribut.StartsWith("e")) {
        return 1;
      }
      return 0;
    }
    #endregion helper_methods

  }

}
