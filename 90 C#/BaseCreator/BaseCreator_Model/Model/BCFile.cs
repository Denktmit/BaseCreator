using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security;
using VDUtils.Helper;

namespace BaseCreator_Model.Model {

  public class BCFile : _BCClass, IComparable, IEquatable<BCFile> {

    #region fields and properties
    private ObservableCollection<BCDatabase> _databases;
    private string _filePath;
    private List<List<string>> _origContent;

    public ObservableCollection<BCDatabase> Databases {
      get {
        if (_databases == null)
          LoadDatabases();
        return _databases;
      }
      set {
        _databases = value;
        OnPropertyChanged();
      }
    }
    public string FilePath {
      get { return _filePath; }
      set {
        _filePath = value;
        OnPropertyChanged();
        Dirty = true;
      }
    }
    public List<List<string>> OrigContent { get { return _origContent; } }
    public override string Prefix { get { return ""; } }
    #endregion fields and properties

    #region ViewModels
    #endregion ViewModels

    #region WPFCommands

    private void CreateCommands() {

    }
    #endregion WPFCommands

    #region constructors
    public BCFile(string filePath) : this(filePath, null) { }
    public BCFile(string filePath, List<List<string>> origContent) : base() {
      // set properties
      _filePath = filePath;
      _origContent = origContent;
      // set viewmodels
      // load commands
      CreateCommands();
      // load gui-texts
      // start init
      int init = 0;
      try {
        init = Init();
        if (init != 0) {
          throw new VerificationException("Error at creating <BCFile>. Error:" + init);
        }
      }
      catch (Exception e) {
        string className = "BCFile";
        string methodName = "Constructor";
        string errMsg = "Error at constructor of <" + className + ">. Error:" + init;
        throw ErrorHandler.GetErrorException(e, ErrorType.CreationError, className, methodName, errMsg, "XYZ", true);
      }
    }
    #endregion constructors

    #region Init-Methods
    public int Init() {
      // load (test) data
      if (!LoadContent())
        return 1;
      // do stuff
      return 0;
    }
    private bool LoadContent() {
      Darstellung = FileManager.splitFilePath(FilePath)[1];
      return true;
    }
    #endregion Init-Methods

    #region Btn-Methods
    #endregion Btn-Methods

    #region Database-Methods
    #endregion Database-Methods

    #region Class-Methods
    private void LoadDatabases() {
      _databases = new ObservableCollection<BCDatabase>();

      OnPropertyChanged("Databases");
    }
    public void SortDatabases() {
      Databases.Sort();
    }
    #endregion Class-Methods

    #region Helper-Methods
    public override string ToString() {
      return "BCFile["+Darstellung+"]";
    }
    public int CompareTo(object other) {
      if (other is BCFile) {
        BCFile odb = (BCFile)other;
        return this.Darstellung.CompareTo(odb.Darstellung);
      }
      return -1;
    }
    public bool Equals(BCFile other) {
      if (this.FilePath.Equals(other.FilePath))
        return true;
      return false;
    }
    #endregion Helper-Methods

    #region GUI-Language
    #endregion GUI-Language

  }

}
