using System;
using System.Security;
using System.Collections.Generic;

namespace AP_Extension.AP {

  public class CSV_Spalte {

    private string m_tabelle;
    private string m_Attribut;
    private string m_Art;
    private string m_csart;
    private string m_Groesse;
    private bool m_PrimKey;
    private bool m_NotNull;
    private bool m_AutoIncre;
    private string m_DefaultVal;
    private string m_Constraint;
    private string m_ForeignKeyTo;
    private string m_OnDelete;
    private string m_OnUpdate;
    private string m_Kommentar;
    private string m_MakeUnique;
    private int m_Idx;

    #region Konstruktor
    public CSV_Spalte(int idx, string tableName, string att, string art, string gro, bool pri, bool not, bool aut, string def, string con, string fkt, string ond, string onu, string kom, string mak) {
      m_Idx = idx;
      m_tabelle = tableName;
      Attribut = att;
      Art = art;
      Groesse = gro;
      PrimKey = pri;      // bool
      NotNull = not;      // bool
      AutoIncre = aut;    // bool
      DefaultVal = def;
      Constraint = con;
      ForeignKeyTo = fkt;
      OnDelete = ond;
      OnUpdate = onu;
      Kommentar = kom;
      MakeUnique = mak;
      m_csart = GetCSArt();
    }
    #endregion Konstruktor

    #region Properties
    public string Tabelle { get { return m_tabelle; } }
    public string Attribut { get => m_Attribut; set => m_Attribut = value; }
    public string AttributWOB { get { return GettAttributWithoutBeginning(); } }
    public string Art { get => m_Art; set => m_Art = value; }
    public string CSArt { get { return m_csart; } }
    public string Groesse { get => m_Groesse; set => m_Groesse = value; }
    public bool PrimKey { get => m_PrimKey; set => m_PrimKey = value; }
    public bool NotNull { get => m_NotNull; set => m_NotNull = value; }
    public bool AutoIncre { get => m_AutoIncre; set => m_AutoIncre = value; }
    public string DefaultVal { get => m_DefaultVal; set => m_DefaultVal = value; }
    public string Constraint { get => m_Constraint; set => m_Constraint = value; }
    public string ForeignKeyTo { get => m_ForeignKeyTo; set => m_ForeignKeyTo = value; }
    public string OnDelete { get => m_OnDelete; set => m_OnDelete = value; }
    public string OnUpdate { get => m_OnUpdate; set => m_OnUpdate = value; }
    public string Kommentar { get => m_Kommentar; set => m_Kommentar = value; }
    public string MakeUnique { get => m_MakeUnique; set => m_MakeUnique = value; }
    public bool ReferencesOtherTable { get { return IsClassReference(); } }
    public int Idx { get => m_Idx; set => m_Idx = value; }
    #endregion Properties

    #region class-methods
    public bool IsSomewhereElseNullable(List<CSV_Tabelle> tabellen) {
      foreach (CSV_Tabelle tab in tabellen) {
        foreach (CSV_Spalte col in tab.Columns) {
          if (col.Art == Tabelle) {
            return true;
          }
        }
      }
      return false;
    }
    public CSV_Tabelle ReferencedTable(List<CSV_Tabelle> tabellen) {
      if (!IsClassReference()) {
        return null;
      }
      foreach (CSV_Tabelle tab in tabellen) {
        if (Attribut.StartsWith("e") && tab.DataName == "SysEnumElement") {
          return tab;
        }
        if (tab.DataName == Art) {
          return tab;
        }
      }
      return null;
    }
    public bool IsClassReference() {
      if (Attribut.StartsWith("i") || Attribut.StartsWith("e")) {
        return true;
      }
      return false;
    }
    public bool NeedsQuote() {
      if (Art == "nvarchar" || Art == "date" || Art == "time" || Art == "datetime") {
        return true;
      }
      return false;
    }
    private string GetCSArt() {
      if (m_tabelle == "tblAPKorpoVerbindung" && Attribut == "nAktivenNr") {
        Console.WriteLine("");
      }
      string ret = "";
      switch (Art) {
        case "nvarchar":
          ret = "string";
          break;
        case "int":
          ret = "int";
          break;
        case "bool":
          ret = "bool";
          break;
        case "decimal":
          ret = "double";
          break;
        case "date":
          ret = "DateTime";
          break;
        case "time":
          ret = "DateTime";
          break;
        case "datetime":
          ret = "DateTime";
          break;
        default:
          if (Art.StartsWith("enum")) {
            ret = "SysEnumElement";
            break;
          }
          else {
            return Art;
          }
      }
      if (!NotNull && Attribut != "nId" && Attribut != "dtAendZeit" && Attribut != "eDatenstatus" && (ret == "int" || ret == "DateTime" || ret == "double")) {
        ret += "?";
      }
      return ret;
    }
    private string GettAttributWithoutBeginning() {
      if (Attribut == null || Attribut.Length <= 0) {
        Console.WriteLine("ERROR@CSV-Spalte.GettAttributWithoutBeginning(...): Should not occur. in String was null or empty.");
        return "__TODO__";
      }
      // hier das n, sz, dt am anfang entfernen
      switch (Attribut[0]) {
        case 'n':
          return Attribut.Substring(1, Attribut.Length - 1);
        case 'd':
          if (Attribut.StartsWith("dt")) {
            return Attribut.Substring(2, Attribut.Length - 2);
          }
          else {
            return Attribut.Substring(1, Attribut.Length - 1);
          }
        case 's':
          return Attribut.Substring(2, Attribut.Length - 2);
        case 'b':
          return Attribut.Substring(1, Attribut.Length - 1);
        case 'e':
          return Attribut.Substring(1, Attribut.Length - 1);
        case 'i':
          return Attribut.Substring(1, Attribut.Length - 1);
        case 'f':
          return Attribut.Substring(1, Attribut.Length - 1);
        case 't':
          return Attribut.Substring(1, Attribut.Length - 1);
        default:
          return " _ TODO _ ";
      }
    }
    public bool IsDateDateTimeTime() {
      if (Art == "datetime")
        return true;
      if (Art == "time")
        return true;
      if (Art == "date")
        return true;
      return false;
    }
    public string GetSQLType() {
      switch (Art) {
        case "nvarchar":
          return "NVARCHAR(" + Groesse + ")";
        case "int":
          return "INT";
        case "bool":
          return "BOOL";
        case "decimal":
          return "DECIMAL(" + Groesse.Replace('.', ',') + ")";
        case "date":
          return "DATE";
        case "time":
          return "TIME";
        case "datetime":
          return "DATETIME";
        default:
          if (Attribut.StartsWith("e")) {
            return "INT";
          }
          else if (Attribut.StartsWith("i")) {
            return "INT";
          }
          else {
            throw new Exception("Konnte den SQL-Typen nicht ermitteln. Attribut:<" + Attribut + ">, Art:<" + Art + ">");
          }
      }
    }
    public string GetSQLDefaultValue() {
      if (DefaultVal == "NOW" || DefaultVal == "now") {
        return "NOW()";
      }
      if (DefaultVal.Length <= 0) {
        return "";
      }
      string ret = "";
      if (NeedsQuote())
        ret += "'";
      ret += DefaultVal;
      if (NeedsQuote())
        ret += "'";
      return ret;
    }
    public string GetColumnDescription() {
      string ret = Attribut + " " + GetSQLType() + " ";
      if (PrimKey)
        ret += "PRIMARY KEY ";
      if (NotNull)
        ret += "NOT NULL ";
      if (AutoIncre)
        ret += "AUTO_INCREMENT ";
      if (DefaultVal.Length > 0) {
        ret += "DEFAULT " + GetSQLDefaultValue();
      }
      return ret;
    }
    public string GetSQLInputCheckComparison() {
      // -1234567891) < -1234567890
      string s = "";
      if (Art == "datetime" || Art == "nvarchar" || Art == "date") {
        s += "N'";
      }
      switch (Art) {
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
          if (Art.StartsWith("enum")) {
            found = true;
          }
          if (Attribut.StartsWith("i")) {
            found = true;
          }
          if (found) {
            s += "-1234567891";
          }
          else {
            s += "TODO";
          }
          break;
      }
      if (Art == "datetime" || Art == "nvarchar" || Art == "date") {
        s += "'";
      }
      s += ") ";
      switch (Art) {
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
          if (Art.StartsWith("enum")) {
            found = true;
          }
          if (Attribut.StartsWith("i")) {
            found = true;
          }
          if (found) {
            s += "< ";
          }
          else {
            s += "TODO";
          }
          break;
      }
      if (Art == "datetime" || Art == "nvarchar" || Art == "date") {
        s += "N'";
      }
      switch (Art) {
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
          if (Art.StartsWith("enum")) {
            found = true;
          }
          if (Attribut.StartsWith("i")) {
            found = true;
          }
          if (found) {
            s += "-1234567890";
          }
          else {
            s += "TODO";
          }
          break;
      }
      if (Art == "datetime" || Art == "nvarchar" || Art == "date") {
        s += "'";
      }
      return s;
    }
    public string GetTestValue() {
      if (Attribut == "eDatenstatus") {
        return "2";
      }
      string s = "";
      if (NotNull) {
        if (Attribut.StartsWith("i") || Attribut.StartsWith("e")) {
          s += "1";
        }
        else {
          switch (Art) {
            case "bool":
              s += "1";
              break;
            case "int":
              s += "1";
              break;
            case "decimal":
              s += "1.0";
              break;
            case "nvarchar":
              s += "N'test-input'";
              break;
            case "datetime":
              s += "N'2000-01-01 01:01:01'";
              break;
            case "date":
              s += "N'2000-01-01'";
              break;
            case "time":
              s += "N'01:01:01'";
              break;
            default:
              bool found = false;
              if (Art.StartsWith("enum")) {
                found = true;
              }
              if (Attribut.StartsWith("i")) {
                found = true;
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
        s += " NULL ";
      }
      return s;
    }
    #endregion class-methods

    public override string ToString() {
      return Tabelle + "." + Attribut;
    }

  }

}
