using System;
using System.Security;
using System.Collections.Generic;

namespace AP_Extension.AP {

  public class CSV_Tabelle {

    #region fields
    private string m_TableName;
    private string m_DataName;
    private string m_ViewName;
    private string m_TableKuerzel;
    private bool _isSys;
    private bool _refItself;
    private List<CSV_Spalte> m_Columns = new List<CSV_Spalte>();
    private int _classDependency;
    #endregion fields

    #region Properties
    public string TableName { get => m_TableName; set => m_TableName = value; }
    public string DataName { get => m_DataName; set => m_DataName = value; }
    public string ViewName { get => m_ViewName; set => m_ViewName = value; }
    public string TableKuerzel { get => m_TableKuerzel; set => m_TableKuerzel = value; }
    public bool IsSys { get { return _isSys; } set { _isSys = value; } }
    public bool ReferencesItself { get { return _refItself; } set { _refItself = value; } }
    public bool HasSysTextReference { get { return CheckForSysTextReference(); } }
    public List<CSV_Spalte> Columns {
      get { return m_Columns; }
      set { m_Columns = value; }
    }
    public int ClassDependency { get { return _classDependency; } set { _classDependency = value; } }
    #endregion Properties

    #region Konstruktor
    public CSV_Tabelle(string dataName, string tableKrz) {
      if (dataName.StartsWith("Sys")) {
        TableName = "tbl" + dataName;
        ViewName = "vw" + dataName;
        IsSys = true;
      }
      else {
        TableName = "tblAP" + dataName;
        ViewName = "vwAP" + dataName;
        IsSys = false;
      }
      DataName = dataName;
      TableKuerzel = tableKrz;
      ReferencesItself = false;
      _classDependency = 0;
    }
    public CSV_Tabelle(string tableName, string tableKrz, List<CSV_Spalte> columns)
      : this(tableName, tableKrz) {
      Columns = columns;
    }
    #endregion Konstruktor

    #region class_methods
    public void CheckStuff() {
      foreach (CSV_Spalte c in Columns) {
        if (c.Art == DataName) {
          ReferencesItself = true;
        }
      }
    }
    public override string ToString() {
      string s = "[" + TableName + "(" + Columns.Count + "):";
      foreach (var c in Columns) {
        s += "<";
        s += c.Attribut + ":";
        s += c.Art;
        s += ">";
      }
      s += "]";
      return s;
    }
    private bool CheckForSysTextReference() {
      foreach (CSV_Spalte c in Columns) {
        if (c.Art == "SysText") {
          return true;
        }
      }
      return false;
    }
    public List<List<CSV_Spalte>> GetMakeUniqueCollections() {
      List<List<CSV_Spalte>> ret = new List<List<CSV_Spalte>>();
      List<CSV_Spalte> tmp = new List<CSV_Spalte>();
      // Id as single Uniquer
      tmp.Add(Columns[0]);
      ret.Add(tmp);
      // Get all Uniquer-Collections-Identifiers
      List<string> mus = new List<string>();
      foreach (CSV_Spalte col in Columns) {
        if (col.MakeUnique != null && col.MakeUnique != "" && !mus.Contains(col.MakeUnique)) {
          mus.Add(col.MakeUnique);
        }
      }
      // Create Uniquer-Collections
      foreach (string mu in mus) {
        tmp = new List<CSV_Spalte>();
        foreach (CSV_Spalte col in Columns) {
          if (col.MakeUnique != null && col.MakeUnique != "" && col.MakeUnique == mu) {
            tmp.Add(col);
          }
        }
        ret.Add(tmp);
      }
      return ret;
    }
    public bool OnlyNotNull() {
      foreach (CSV_Spalte col in Columns) {
        if (!col.NotNull) {
          return false;
        }
      }
      return true;
    }
    #endregion class_methods

  }

}
