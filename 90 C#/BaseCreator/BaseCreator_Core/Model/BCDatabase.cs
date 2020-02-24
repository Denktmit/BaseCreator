using System;
using System.Collections.ObjectModel;
using System.Security;
using BaseCreator_Core.Helper;

namespace BaseCreator_Core.Model {

  public class BCDatabase : _BCClass, IComparable, IEquatable<BCDatabase> {

    #region fields and properties
    private BCFile _file;
    private ObservableCollection<BCTable> _tables;

    public BCFile File { get { return _file; } }
    public ObservableCollection<BCTable> Tables {
      get {
        if (_tables == null)
          LoadTables();
        return _tables;
      }
      set {
        _tables = value;
        OnPropertyChanged();
      }
    }
    public override string Prefix { get { return "db"; } }
    public int TableCount { get { return Tables.Count; } }
    #endregion fields and properties

    #region ViewModels
    #endregion ViewModels

    #region WPFCommands

    private void CreateCommands() {

    }
    #endregion WPFCommands

    #region constructors
    public BCDatabase(BCFile file, string darstellung) : base(darstellung) {
      // set properties
      _file = file;
      // set viewmodels
      // load commands
      CreateCommands();
      // load gui-texts
      // start init
      int init = 0;
      try {
        init = Init();
        if (init != 0) {
          throw new VerificationException("Error at creating <BCDatabase>. Error:" + init);
        }
      }
      catch (Exception e) {
        string className = "BCDatabase";
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
    private void LoadTables() {
      _tables = new ObservableCollection<BCTable>();

      OnPropertyChanged("Tables");
    }
    public void SortTables() {
      Tables.Sort();
    }
    #endregion Class-Methods

    #region Helper-Methods
    public override string ToString() {
      return "BCDatabase[" + Darstellung + "]";
    }
    public int CompareTo(object other) {
      if (other is BCDatabase) {
        BCDatabase odb = (BCDatabase)other;
        return this.Darstellung.CompareTo(odb.Darstellung);
      }
      return -1;
    }
    public bool Equals(BCDatabase other) {
      if(other is BCDatabase) {
        BCDatabase odb = (BCDatabase)other;
        if (this.File.Equals(odb.File) && this.Darstellung.Equals(odb.Darstellung))
          return true;
      }
      return false;
    }
    #endregion Helper-Methods

    #region GUI-Language
    #endregion GUI-Language

  }

}
