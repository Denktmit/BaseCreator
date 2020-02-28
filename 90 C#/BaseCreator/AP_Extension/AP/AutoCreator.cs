using BaseCreator_Model.Model;
using System;
using System.Collections.Generic;
using VDUtils.Helper;

namespace AP_Extension.AP {

  public class AutoCreator {

    #region fields
    private List<BCDatabase> _dBs;
    private List<string> _errors;
    //private DBConnection _conn;
    private List<CSV_Datenbank> _dbs;
    private List<CSV_Tabelle> _dbTables;
    private List<CSVEnum> _enums;
    private static string _appath;
    private DateTime _startDateTime;
    private Creator_SQL _csql;
    private Creator_CS _ccs;
    private Creator_PHP _cphp;
    private Creator_HTML _chtml;
    private Creator_RandDS _crds;
    private string _filePath;
    private bool _modusBC;
    #endregion fields

    #region properties
    public List<BCDatabase> DBs { get { return _dBs; } }
    public List<string> Errors { get { return _errors; } set { _errors = value; } }
    //public DBConnection Conn { get { return _conn; } set { _conn = value; } }
    public List<CSV_Datenbank> Dbs { get { return _dbs; } set { _dbs = value; } }
    public List<CSV_Tabelle> DbTables { get { return _dbTables; } set { _dbTables = value; } }
    public List<CSVEnum> Enums { get { return _enums; } set { _enums = value; } }
    public string APPath { get { return _appath; } set { _appath = value; } }
    public DateTime StartDateTime { get { return _startDateTime; } set { _startDateTime = value; } }
    public Creator_SQL CSQL { get { return _csql; } set { _csql = value; } }
    public Creator_CS CCS { get { return _ccs; } set { _ccs = value; } }
    public Creator_PHP CPHP { get { return _cphp; } set { _cphp = value; } }
    public Creator_HTML CHTML { get { return _chtml; } set { _chtml = value; } }
    public Creator_RandDS CRDS { get { return _crds; } set { _crds = value; } }
    public string FilePath {
      get { return _filePath; }
      set { _filePath = value; }
    }
    public bool ModusBC {
      get { return _modusBC; }
      set { _modusBC = value; }
    }
    #endregion properties

    #region konstruktor
    public AutoCreator() {
      StartDateTime = DateTime.Now;
      APPath = Configurations.GetValue("appath");
      Errors = new List<string>();
      //Conn = GetDefaultDBConn();
      //LoadConnection();
      //DbTables = DListToClassLists(CSVImport.getListList(APPath + "\\00_Documents\\DB-Aufbau", "DB-Tables", ";", true));
      //Enums = DListToCSVEnums(CSVImport.getListList(APPath + "\\00_Documents\\DB-Aufbau", "enums", ";", true));
      //CalculateDependenciesClass();
    }
    #endregion konstruktor

    #region class_methods
    public void run() {
      CreateSQL();
      CreateCS();
      CreatePHP();
      CreateHTML();
      CreateRandDS();
    }
    public bool CreateSQL() {
      try {
//        CSQL = new Creator_SQL(this);
//        CSQL.run();
        return true;
      } catch(Exception ex) {
        ErrorHandler.CreateErrorException(ex, ErrorType.AutoCreateError, "AutoCreator", "CreateSQL"
          , "Fehler beim Erstellen der SQL-Dateien.", "73");
        return false;
      }
    }
    public bool CreateCS() {
      try {
//        CCS = new Creator_CS(this);
//        CCS.run();
        return true;
      } catch (Exception ex) {
        ErrorHandler.CreateErrorException(ex, ErrorType.AutoCreateError, "AutoCreator", "CreateCS"
          , "Fehler beim Erstellen der C#-Dateien.", "84");
        return false;
      }
    }
    public bool CreatePHP() {
      try {
//        CPHP = new Creator_PHP(this);
//        CPHP.run();
        return true;
      } catch (Exception ex) {
        ErrorHandler.CreateErrorException(ex, ErrorType.AutoCreateError, "AutoCreator", "CreatePHP"
          , "Fehler beim Erstellen der PHP-Dateien.", "95");
        return false;
      }
    }
    public bool CreateHTML() {
      try {
//        CHTML = new Creator_HTML(this);
//        CHTML.run();
        return true;
      } catch (Exception ex) {
        ErrorHandler.CreateErrorException(ex, ErrorType.AutoCreateError, "AutoCreator", "CreateHTML"
          , "Fehler beim Erstellen der SQL-Dateien.", "106");
        return false;
      }
    }
    public bool CreateRandDS() {
      try {
        //CRDS = new Creator_RandDS(this);
        //CRDS.run();
        return true;
      } catch (Exception ex) {
        ErrorHandler.CreateErrorException(ex, ErrorType.AutoCreateError, "AutoCreator", "CreateRandDS"
          , "Fehler beim Erstellen der Zufallsdatensätze.", "117");
        return false;
      }
    }
    #endregion class_methods

    #region db_connection
    //public static DBConnection LoadConnection() {
    //  return ServerManager.Connection;
    //}
    #endregion db_connection

    #region test_methods
    //public static void TestCallSP(DBConnection conn) {
    //  Console.WriteLine("--------------");
    //  /*Console.WriteLine(" + CallSP(...)");
    //
    //  eAP_Datenstatus datStat = eAP_Datenstatus.gueltig;
    //  int npers = 1;
    //  DateTime aendTime = DateTime.Now;
    //  string nname = "Schulze";
    //  string vname = "Peter";
    //  string titel = "";
    //  string beruf = "Azubi";
    //  string nrmob = "0147568925365";
    //  string nrpri = "";
    //  string email = "schulze.peter@gmail.com";
    //  Person p = new Person(npers, aendTime, datStat, nname, vname, titel, beruf, nrmob, nrpri, email);
    //
    //  DBSPContent cont = p.GetInsertDBSPContent();
    //
    //  List<List<String>> ergebnis = DBCalls.CallSP(conn, cont);
    //  foreach (var e in ergebnis) {
    //    foreach (var e2 in e) {
    //      Console.Write(e2 + ", ");
    //    }
    //    Console.WriteLine("");
    //  }
    //
    //  int nerror = cont.GetIndexOf("@inout_nError");
    //  Console.WriteLine("nError: " + cont.Values[nerror]);
    //  int szerror = cont.GetIndexOf("@inout_szError");
    //  Console.WriteLine("szError: " + cont.Values[szerror]);
    //  */
    //}
    //public static void ReadTest(DBConnection conn) {
    //  Console.WriteLine("----------------");
    //  Console.WriteLine(" + ReadTest(...)");
    //  string query = "select * from vwPerson";
    //
    //  DBResult ergebnis = DBCalls.ExecQuery(conn, query);
    //  foreach (var e in ergebnis.Result) {
    //    foreach (var e2 in e) {
    //      Console.Write(e2 + ", ");
    //    }
    //    Console.WriteLine("");
    //  }
    //}
    //public static void WriteTest(DBConnection conn) {
    //  Console.WriteLine("-----------------");
    //  Console.WriteLine(" + WriteTest(...)");
    //  string nname = "Testopod";
    //  string vname = "Vordermann";
    //  string tit = "Prof. Tester";
    //  string ber = "Tester";
    //  string nrm = "1242154214";
    //  string npr = "23523523523";
    //  string nge = "23523";
    //  string ema = "sajf@safg.de";
    //  string query = "";
    //  query += "insert tblPerson (nDatenstatus";
    //  query += "  , szNachname";
    //  query += "  , szVorname";
    //  query += "  , szTitel";
    //  query += "  , szBeruf";
    //  query += "  , szNummerMobil";
    //  query += "  , szNummerPrivat";
    //  query += "  , szNummerGeschaeft";
    //  query += "  , szEmail";
    //  query += ")";
    //  query += "values ( 1";
    //  query += "  , N'" + nname;
    //  query += "'  , N'" + vname;
    //  query += "'  , N'" + tit;
    //  query += "'  , N'" + ber;
    //  query += "'  , N'" + nrm;
    //  query += "'  , N'" + npr;
    //  query += "'  , N'" + nge;
    //  query += "'  , N'" + ema;
    //  query += "');";
    //  Console.WriteLine("query:");
    //  Console.WriteLine(query);
    //  DBResult ergebnis = DBCalls.ExecQuery(conn, query);
    //  foreach (var e in ergebnis.Result) {
    //    foreach (var e2 in e) {
    //      Console.Write(e2 + ", ");
    //    }
    //    Console.WriteLine("");
    //  }
    //}
    #endregion test_methods

    #region Helper_methods
    public CSV_Tabelle GetTable(string tableName) {
      foreach (CSV_Tabelle tab in DbTables) {
        if (tab.TableName == tableName) {
          return tab;
        }
      }
      throw new Exception("Die Tabelle <" + tableName + "> kann nicht geladen werden.");
    }
    public string GetStartDate() {
      return StartDateTime.Date.ToString("d");
    }
    //public string GetHashSum(string instring) {
    //  return PasswordManager.GetHashCode(instring);
    //}
    public string GetDateTimeString(string inString) {
      string ret = "";
      if (inString.Length != 10) {
        Console.WriteLine("Error@GetDateTimeString(" + inString + ")->1");
        throw new Exception("Error@GetDateTimeString(" + inString + ")->1");
      }
      if (inString[4] != '-' || inString[7] != '-') {
        Console.WriteLine("Error@GetDateTimeString(" + inString + ")->2");
        throw new Exception("Error@GetDateTimeString(" + inString + ")->2");
      }
      string y = inString.Substring(0, 4);
      string m = inString.Substring(5, 2);
      string d = inString.Substring(8, 2);
      int year = 0;
      int month = 0;
      int day = 0;
      if (!Int32.TryParse(y, out year) || !Int32.TryParse(m, out month) || !Int32.TryParse(d, out day)) {
        Console.WriteLine("Error@GetDateTimeString(" + inString + ")->3");
        throw new Exception("Error@GetDateTimeString(" + inString + ")->3");
      }
      ret = year + ", " + month + ", " + day;
      return ret;
    }
    public string GetStringWithoutBeginning(string inString) {
      if (inString == null || inString.Length <= 0) {
        Console.WriteLine("ERROR@AutoCreator.GetStringWithoutBeginning(...): Should not occur. in String was null or empty.");
        return "__TODO__";
      }
      // hier das n, sz, dt am anfang entfernen
      switch (inString[0]) {
        case 'n':
          return inString.Substring(1, inString.Length - 1);
        case 'd':
          if (inString.StartsWith("dt")) {
            return inString.Substring(2, inString.Length - 2);
          }
          else {
            return inString.Substring(1, inString.Length - 1);
          }
        case 's':
          return inString.Substring(2, inString.Length - 2);
        case 'b':
          return inString.Substring(1, inString.Length - 1);
        case 'e':
          return inString.Substring(1, inString.Length - 1);
        case 'i':
          return inString.Substring(1, inString.Length - 1);
        case 'f':
          return inString.Substring(1, inString.Length - 1);
        case 't':
          return inString.Substring(1, inString.Length - 1);
        default:
          return " _ TODO _ ";
      }
    }
    public List<CSV_Tabelle> GetReferencingTables(CSV_Tabelle forTable) {
      List<CSV_Tabelle> refTables = new List<CSV_Tabelle>();
      foreach (CSV_Tabelle tab in DbTables) {
        if (tab != forTable) {
          foreach (CSV_Spalte column in tab.Columns) {
            if (column.Art == forTable.DataName) {
              if (!refTables.Contains(tab)) {
                refTables.Add(tab);
              }
            }
            if (column.Art.StartsWith("enum") && forTable.DataName == "SysEnumElement") {
              if (!refTables.Contains(tab)) {
                refTables.Add(tab);
              }
            }
          }
        }
      }
      return refTables;
    }
    public List<CSV_Spalte> GetReferencingColumns(CSV_Tabelle forTable) {
      List<CSV_Spalte> refColumns = new List<CSV_Spalte>();
      foreach (CSV_Tabelle tab in GetReferencingTables(forTable)) {
        if (tab != forTable) {
          foreach (CSV_Spalte column in tab.Columns) {
            if (column.Art == forTable.DataName) {
              if (!refColumns.Contains(column)) {
                refColumns.Add(column);
              }
            }
            if (column.Art.StartsWith("enum") && forTable.DataName == "SysEnumElement") {
              if (!refColumns.Contains(column)) {
                refColumns.Add(column);
              }
            }
          }
        }
      }
      return refColumns;
    }
    private void CalculateDependenciesClass() {
      List<CDH> openClasses = new List<CDH>();
      List<CDH> closedClasses = new List<CDH>();
      foreach (CSV_Tabelle table in DbTables) {
        CDH tmp = new CDH(this, table);
        if (tmp.Level == 1)
          closedClasses.Add(tmp);
        else
          openClasses.Add(tmp);
      }
      int helper = 10000;
      while (openClasses.Count > 0 && helper > 0) {
        List<CDH> newlyClosed = new List<CDH>();
        foreach (CDH c in openClasses) {
          int found = 0;
          foreach (CSV_Tabelle t in c.Dependencies) {
            foreach (CDH closed in closedClasses) {
              if (closed.TableName == t.TableName) {
                found++;
                if (closed.Level >= c.Level) {
                  c.Level = closed.Level + 1;
                }
                break;
              }
            }
          }
          if (found == c.Dependencies.Count) {
            newlyClosed.Add(c);
            closedClasses.Add(c);
          }
        }
        foreach (CDH nc in newlyClosed) {
          openClasses.Remove(nc);
        }
        helper--;
      }
      Console.WriteLine("open:    " + openClasses.Count);
      Console.WriteLine("closed:  " + closedClasses.Count);
      foreach (CDH c in closedClasses) {
        c.ClassDependency = c.Level;
      }
      Console.WriteLine("Debug");
    }
    public CSV_Datenbank GetDatenbank(string dbname, bool createIfNull) {
      foreach (CSV_Datenbank db in Dbs) {
        if (db.DBName == dbname)
          return db;
      }
      if (createIfNull) {
        CSV_Datenbank ndb = new CSV_Datenbank(dbname);
        Dbs.Add(ndb);
        return ndb;
      }
      return null;
    }
    public CSV_Tabelle GetTabelle(CSV_Datenbank db, string tabname, string tabtoken, bool createIfNull) {
      foreach (CSV_Tabelle tab in db.Tabellen) {
        if (tab.DataName == tabname)
          return tab;
      }
      if (createIfNull) {
        CSV_Tabelle ntab = new CSV_Tabelle(tabname, tabtoken);
        db.Tabellen.Add(ntab);
        return ntab;
      }
      return null;
    }
    public CSV_Spalte GetSpalte(CSV_Tabelle tab, string spaltenname, bool createIfNull) {
      foreach (CSV_Spalte col in tab.Columns) {
        if (col.Attribut == spaltenname)
          return col;
      }
      if (createIfNull) {
        CSV_Spalte ncol = new CSV_Spalte(-1, null, null, null, null, false, false, false, null, null, null, null, null, null, null);
        tab.Columns.Add(ncol);
        return ncol;
      }
      return null;
    }
    #endregion Helper_methods

    #region converter_methods
    public string ConvertStringToConformity(string inString) {
      string ret = "";
      for (int i = 0; i < inString.Length; i++) {
        switch (inString[i]) {
          case ' ':
            ret += "_";
            break;
          case '-':
            ret += "_";
            break;
          case '/':
            ret += "_";
            break;
          case 'ß':
            ret += "ss";
            break;
          case 'ä':
            ret += "ae";
            break;
          case 'Ä':
            ret += "Ae";
            break;
          case 'ö':
            ret += "oe";
            break;
          case 'Ö':
            ret += "Oe";
            break;
          case 'ü':
            ret += "ue";
            break;
          case 'Ü':
            ret += "Ue";
            break;
          default:
            ret += inString[i];
            break;
        }
      }
      return ret;
    }
    public List<CSVEnum> DListToCSVEnums(List<List<String>> dList) {
      Console.WriteLine("-------------------------");
      Console.WriteLine(" + DListToCSVEnums(...)");
      List<CSVEnum> ret = new List<CSVEnum>();
      // CSV-Tabelle einlesen
      if (dList == null) {
        string errMsg = "! ERROR@DListToCSVEnums -> Uebergebene Liste war null.";
        Console.WriteLine(errMsg);
        throw new Exception(errMsg);
      }
      if (dList.Count <= 0) {
        string errMsg = "! ERROR@DListToCSVEnums -> Uebergebene Liste war leer.";
        Console.WriteLine(errMsg);
        throw new Exception(errMsg);
      }
      // Ausgeben:
      // CSVImport.printDList(dList);
      //
      CSVEnum tempEnum = new CSVEnum("nA", -1);
      // alle zeilen durchlaufen
      List<string> enumNames = new List<string>();
      bool headerRow = true;
      foreach (List<string> row in dList) {
        if (headerRow) {
          headerRow = false;
          continue;
        }
        // enum einlesen und ggf erstellen
        string tmpEnumName = ConvertStringToConformity("enum_" + row[1] + "_" + row[2].Substring(1));
        if (!enumNames.Contains(tmpEnumName)) {
          enumNames.Add(tmpEnumName);
          tempEnum = new CSVEnum(tmpEnumName, -1);
          ret.Add(tempEnum);
        }
        // passendes enum finden
        else {
          bool found = false;
          foreach (var en in ret) {
            if (en.Description == tmpEnumName) {
              tempEnum = en;
              found = true;
            }
          }
          if (!found) {
            throw new Exception("ERROR @ DListToCSVEnums -> konnte kein passendes enum finden");
          }
        }
        // enumelement erstellen und hinzufuegen
        int tmpId = Int32.Parse(row[3]);
        string tmpDtKuerzel = row[4];
        string tmpDtText = row[5];
        string tmpDesc = tempEnum.Description + "_" + ConvertStringToConformity(tmpDtKuerzel);
        tempEnum.AppendElement(tmpId, tmpDesc, tmpDtText, tmpDtKuerzel);
      }
      return ret;
    }
    public List<CSV_Tabelle> DListToClassLists(List<List<string>> dList) {
      Console.WriteLine("-------------------------");
      Console.WriteLine(" + DListToClassLists(...)");
      List<string> uniqueKuerzel = new List<string>();
      // CSV-Tabelle einlesen
      if (dList == null) {
        string errorMsg = "! ERROR@DListToClassLists -> Uebergebene Liste war null.";
        Console.WriteLine(errorMsg);
        throw new Exception(errorMsg);
      }
      if (dList.Count <= 0) {
        string errMsg = "! ERROR@DListToClassLists -> Uebergebene Liste war leer.";
        Console.WriteLine(errMsg);
        throw new Exception(errMsg);
      }
      // Tabellen durchlaufen
      List<CSV_Tabelle> ret = new List<CSV_Tabelle>();
      int columns = dList[0].Count;
      int rows = dList.Count;
      int anf = 1;
      int end = 1;
      int t = 1;
      while (t < dList.Count) {
        anf = t;
        end = t + 1;
        while (end < dList.Count && string.IsNullOrWhiteSpace(dList[end][0])) {
          end++;
        }
        t = end;
        end--;
        int constraintHelper = 0;
        string dataName = dList[anf][0];
        string tableKrz = dList[anf][1];
        if (uniqueKuerzel.Contains(tableKrz)) {
          throw new Exception("! Das Tabellenkuerzel <" + tableKrz + "> existiert bereits. Passe die CSV-Datei an!");
        } else {
          uniqueKuerzel.Add(tableKrz);
        }
        CSV_Tabelle tempTable = new CSV_Tabelle(dataName, tableKrz);
        bool firstId = true;
        // Standard-Werte versorgen
        int idx = 0;
        if (!dList[anf][0].StartsWith("Sys")) {
          //                                             Idx   tablename            Attribut,         Art,                Groesse,    PK,     NN,   AI,     Def,    Const,  FKT,  ODel, OUp, Kommentar,                                           MakeUnique
          CSV_Spalte idSpalte = new CSV_Spalte(0, tempTable.TableName, "nId", "int", "-", true, true, true, "", "", "", "", "", "Eindeutige Id der Tabelle " + dataName, "");
          CSV_Spalte aendzeitSpalte = new CSV_Spalte(1, tempTable.TableName, "dtAendZeit", "datetime", "-", false, true, false, "NOW", "", "", "", "", "Letzte Änderung", "");
          CSV_Spalte datenstatusSpalte = new CSV_Spalte(2, tempTable.TableName, "eDatenstatus", "enum_Datenstatus", "-", false, true, false, "4", "", "", "", "", "Datenstatus: 2-gueltig, 3-veraltet, 4-unbestimmt", "");
          tempTable.Columns.Add(idSpalte);
          tempTable.Columns.Add(aendzeitSpalte);
          tempTable.Columns.Add(datenstatusSpalte);
          idx = 3;
        }
        // Spalten durchlaufen
        for (int r = anf; r <= end; r++) {
          string att = dList[r][2];
          string art = dList[r][3];
          string gro = dList[r][4];
          if (art == "decimal") {
            gro = "10.2";
          }
          bool pri = false;
          bool not = (dList[r][5] == "x") ? true : false;
          bool aut = false;
          if (att.EndsWith("Id") && firstId) {
            pri = true;
            aut = true;
            not = true;
            firstId = false;
          }
          string def = dList[r][6];
          string con = "";
          string fkt = "";
          string ond = "";
          string onu = "";
          if (dList[r][2].StartsWith("i")) {
            con = "fk" + constraintHelper + dList[r][3] + dList[r][2].Substring(1);
            constraintHelper++;
            fkt = "tblAP" + dList[r][3] + ".nId";
            ond = "na";
            onu = "na";
          }
          if (dList[r][2].StartsWith("e")) {
            con = "fk" + constraintHelper + dList[anf][0] + dList[r][2].Substring(1);
            constraintHelper++;
            fkt = "tblSysEnumElement.nId";
          }
          if (dList[r][3].StartsWith("Sys")) {
            fkt = "tbl" + dList[r][3] + ".nId";
          }
          string kom = dList[r][8];
          string mak = dList[r][7];
          CSV_Spalte tempCol = new CSV_Spalte(idx, tempTable.TableName, att, art, gro, pri, not, aut, def, con, fkt, ond, onu, kom, mak);
          idx++;
          tempTable.Columns.Add(tempCol);
          tempTable.CheckStuff();   // references itself
        }
        ret.Add(tempTable);
      }
      foreach (var e in ret) {
        //Console.WriteLine(e.ToString());
      }
      return ret;
    }
    public List<CSV_Tabelle> BCListToClassLists(List<List<string>> dList) {
      List<CSV_Tabelle> ret = new List<CSV_Tabelle>();
      foreach(CSV_Datenbank db in BCListToDBList(dList)) {
        ret.AddRange(db.Tabellen);
      }
      return ret;
    }
    public List<CSV_Datenbank> BCListToDBList(List<List<string>> dList) {
      Console.WriteLine("-------------------------");
      Console.WriteLine(" + BCListToDBList(...)");
      Dbs = new List<CSV_Datenbank>();
      List<string> uniqueKuerzel = new List<string>();
      // CSV-Tabelle einlesen
      if (dList == null) {
        string errorMsg = "! ERROR@DListToClassLists -> Uebergebene Liste war null.";
        Console.WriteLine(errorMsg);
        throw new Exception(errorMsg);
      }
      if (dList.Count <= 0) {
        string errMsg = "! ERROR@DListToClassLists -> Uebergebene Liste war leer.";
        Console.WriteLine(errMsg);
        throw new Exception(errMsg);
      }
      // Tabellen durchlaufen
      foreach (List<string> row in dList) {
        string r = "";
        foreach (string f in row) {
          r += f + " ; ";
        }
        Console.WriteLine(r);
        CSV_Datenbank db = GetDB(Dbs, row[0], true);
        CSV_Tabelle tab = GetTabelle(db.Tabellen, row[1], row[2], true);



        // ...




      }
      return Dbs;
    }
    public CSV_Datenbank GetDB(List<CSV_Datenbank> liste, string dbname, bool create) {
      foreach (CSV_Datenbank db in liste) {
        if (db.DBName == dbname)
          return db;
      }
      if (create) {
        CSV_Datenbank dbret = new CSV_Datenbank(dbname);
        liste.Add(dbret);
        return dbret;
      }
      return null;
    }
    public CSV_Tabelle GetTabelle(List<CSV_Tabelle> liste, string tabname, string tabtoken, bool create) {
      foreach (CSV_Tabelle o in liste) {
        if (o.DataName == tabname && o.TableKuerzel == tabtoken)
          return o;
      }
      if (create) {
        CSV_Tabelle ret = new CSV_Tabelle(tabname, tabtoken);
        liste.Add(ret);
        return ret;
      }
      return null;
    }
    public CSV_Spalte GetSpalte(List<CSV_Spalte> liste, string colname, bool create) {
      foreach (CSV_Spalte o in liste) {
        if (o.Attribut == colname)
          return o;
      }
      if (create) {
        CSV_Spalte dbret = new CSV_Spalte(colname);
        liste.Add(dbret);
        return dbret;
      }
      return null;
    }
    public void ImportData(List<List<string>> data, string filePath) {
      FilePath = filePath;
      DbTables = BCListToClassLists(data);
      ModusBC = false;
    }
    public void ImportData(BCFile file) {
      FilePath = file.FilePath;
      _dBs = new List<BCDatabase>();
      foreach(BCDatabase db in file.Databases) {
        _dBs.Add(db);
      }
      ModusBC = true;
    }
    #endregion converter_methods

  }

}
