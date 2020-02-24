using System;
using System.Collections.Generic;
using System.Security;

namespace AP_Extension.AP {

  public class CDH {

    private AutoCreator _ac;
    private CSV_Tabelle _tabelle;
    private List<CSV_Tabelle> _dependencies;
    private int _level;

    public AutoCreator AC { get { return _ac; } }
    public CSV_Tabelle Tabelle { get { return _tabelle; } }
    public List<CSV_Tabelle> Dependencies { get { return _dependencies; } }
    public string TableName { get { return Tabelle == null ? "nA" : Tabelle.TableName; } }
    public int ClassDependency { get { return Tabelle == null ? -1 : Tabelle.ClassDependency; } set { if (Tabelle != null) { Tabelle.ClassDependency = value; } } }
    public int Level { get { return _level; } set { _level = value; } }

    public CDH(AutoCreator ac, CSV_Tabelle tabelle) {
      _ac = ac;
      _tabelle = tabelle;
      _dependencies = new List<CSV_Tabelle>();
      foreach (CSV_Spalte c in Tabelle.Columns) {
        if (c.ReferencesOtherTable) {
          CSV_Tabelle tmp = c.ReferencedTable(AC.DbTables);
          if (!Dependencies.Contains(tmp) && tmp.TableName != TableName) {
            Dependencies.Add(tmp);
          }
        }
      }
      _level = Dependencies.Count == 0 ? 1 : -1;
    }

    public CSV_Tabelle GetCSVTabelle(string tableName) {
      CSV_Tabelle tmp = null;


      return tmp;
    }

  }

}
