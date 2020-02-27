using System;
using System.Security;
using VDUtils.Helper;

namespace BaseCreator_Model.Model {

  public class BCColumn : _BCClass, IComparable, IEquatable<BCColumn> {

    #region fields and properties
    private BCTable _table;
    private DataType _dataType;
    private string _verweis;
    private string _default;
    private bool _notNull;
    private bool _autoIncrement;
    private bool _primaryKey;
    private string _makeUnique;
    private string _kommentar;
    private BCTable _reference;

    public BCTable Table { get { return _table; } }
    public DataType DataType {
      get { return _dataType; }
      set {
        _dataType = value;
        OnPropertyChanged();
        OnPropertyChanged("References");
        OnPropertyChanged("Reference");
        Dirty = true;
        if(value==null || value.Darstellung != "Verweis") {
          Verweis = "-";
        }
      }
    }
    public string Verweis {
      get { return _verweis; }
      set {
        _verweis = value;
        OnPropertyChanged();
      }
    }
    public string Default {
      get { return _default; }
      set {
        _default = value;
        OnPropertyChanged();
        Dirty = true;
      }
    }
    public bool NotNull {
      get { return _notNull; }
      set {
        _notNull = value;
        OnPropertyChanged();
        Dirty = true;
      }
    }
    public bool AutoIncrement {
      get { return _autoIncrement; }
      set {
        _autoIncrement = value;
        OnPropertyChanged();
        Dirty = true;
      }
    }
    public bool PrimaryKey {
      get { return _primaryKey; }
      set {
        _primaryKey = value;
        OnPropertyChanged();
        Dirty = true;
      }
    }
    public string MakeUnique {
      get { return _makeUnique; }
      set {
        _makeUnique = value;
        OnPropertyChanged();
        Dirty = true;
      }
    }
    public string Kommentar {
      get { return _kommentar; }
      set {
        _kommentar = value;
        OnPropertyChanged();
        Dirty = true;
      }
    }
    public bool References {
      get {
        if (DataType == null)
          return false;
        return DataType.Darstellung == "Verweis";
      }
    }
    public BCTable Reference {
      get { return _reference; }
      set {
        _reference = value;
        OnPropertyChanged();
        Dirty = true;
        if (value != null) {
          Verweis = value.Database.Darstellung+"."+value.Darstellung+".nId";
        }
        else {
          Verweis = "-";
        }
        Table.RefreshDependencyLevel();
      }
    }
    public override string Prefix { get { return DataType?.Prefix; } }
    public string DarstellungWPrefix { get { return Prefix + Darstellung; } }
    #endregion fields and properties

    #region ViewModels
    #endregion ViewModels

    #region WPFCommands

    private void CreateCommands() {

    }
    #endregion WPFCommands

    #region constructors
    public BCColumn(BCTable table, string darstellung) : base(darstellung) {
      // set properties
      _table = table;
      _dataType = DataType.GetDataType("Zahl");
      _verweis = "-";
      _default = "";
      _notNull = false;
      _autoIncrement = false;
      _primaryKey = false;
      _makeUnique = "";
      _kommentar = "";
      _reference = null;
      // set viewmodels
      // load commands
      CreateCommands();
      // load gui-texts
      // start init
      int init = 0;
      try {
        init = Init();
        if (init != 0) {
          throw new VerificationException("Error at creating <BCColumn>. Error:" + init);
        }
      }
      catch (Exception e) {
        string className = "BCColumn";
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
    #endregion Class-Methods

    #region Helper-Methods
    public override string ToString() {
      return "BCColumn[" + Darstellung + "]";
    }
    public int CompareTo(object other) {
      if (other is BCColumn) {
        BCColumn odb = (BCColumn)other;
        if (this.PrimaryKey && !odb.PrimaryKey)
          return -1;
        if (!this.PrimaryKey && odb.PrimaryKey)
          return 1;
        return this.Darstellung.CompareTo(odb.Darstellung);
      }
      return -1;
    }
    public bool Equals(BCColumn other) {
      if (this.Table.Equals(other.Table) && this.Darstellung.Equals(other.Darstellung))
        return true;
      return false;
    }
    #endregion Helper-Methods

    #region GUI-Language
    #endregion GUI-Language

  }

}
