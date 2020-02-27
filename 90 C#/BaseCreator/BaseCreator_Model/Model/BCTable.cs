using System;
using System.Collections.ObjectModel;
using System.Security;
using VDUtils.Helper;

namespace BaseCreator_Model.Model {

  public class BCTable : _BCClass, IComparable, IEquatable<BCTable> {

    #region fields and properties
    private BCDatabase _database;
    private ObservableCollection<BCColumn> _columns;
    private string _kuerzel;
    private int _dependencyLevel;

    public BCDatabase Database { get { return _database; } }
    public ObservableCollection<BCColumn> Columns {
      get {
        if (_columns == null)
          LoadColumns();
        return _columns;
      }
    }
    public string Kuerzel {
      get { return _kuerzel; }
      set {
        _kuerzel = value;
        OnPropertyChanged();
        Dirty = true;
      }
    }
    public override string Prefix { get { return "tbl"; } }
    public int ColumnCount { get { return Columns.Count; } }
    public int DependencyLevel { get { return _dependencyLevel; } }
    public string DDL { get { return DependencyLevel + " " + Darstellung; } }
    #endregion fields and properties

    #region ViewModels
    #endregion ViewModels

    #region WPFCommands

    private void CreateCommands() {

    }
    #endregion WPFCommands

    #region constructors
    public BCTable(BCDatabase database, string darstellung) : base(darstellung) {
      // set properties
      _database = database;
      _dependencyLevel = -1;
      // set viewmodels
      // load commands
      CreateCommands();
      // load gui-texts
      // start init
      int init = 0;
      try {
        init = Init();
        if (init != 0) {
          throw new VerificationException("Error at creating <BCTable>. Error:" + init);
        }
      }
      catch (Exception e) {
        string className = "BCTable";
        string methodName = "Constructor";
        string errMsg = "Error at constructor of <" + className + ">. Error:" + init;
        throw ErrorHandler.GetErrorException(e, ErrorType.CreationError, className, methodName, errMsg, "XYZ", true);
      }
    }
    #endregion constructors

    #region Init-Methods
    public int Init() {
      // load (test) data
      // do stuff
      return 0;
    }
    #endregion Init-Methods

    #region Btn-Methods
    #endregion Btn-Methods

    #region Database-Methods
    #endregion Database-Methods

    #region Class-Methods
    private void LoadColumns() {
      _columns = new ObservableCollection<BCColumn>();

      OnPropertyChanged("Columns");
    }
    public void RefreshDependencyLevel() {
      _dependencyLevel = 0;
      foreach (BCColumn col in Columns) {
        if (col.DataType.Darstellung == "Verweis") {
          if (col != null && col.Reference != null)
            if (col.Reference.DependencyLevel >= _dependencyLevel)
              _dependencyLevel = col.Reference.DependencyLevel + 1;
        }
      }
      OnPropertyChanged("DependencyLevel");
    }
    #endregion Class-Methods

    #region Helper-Methods
    public override string ToString() {
      return "BCTable[" + Darstellung + "]";
    }
    public int CompareTo(object other) {
      if (other is BCTable) {
        BCTable odb = (BCTable)other;
        return this.DependencyLevel.CompareTo(odb.DependencyLevel);
      }
      return -1;
    }
    public bool Equals(BCTable other) {
      if (this.Database.Equals(other.Database) && this.Darstellung.Equals(other.Darstellung))
        return true;
      return false;
    }
    #endregion Helper-Methods

    #region GUI-Language
    #endregion GUI-Language

  }

}
