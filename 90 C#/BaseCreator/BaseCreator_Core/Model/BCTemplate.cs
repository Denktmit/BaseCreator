using System;
using System.Security;
using BaseCreator_Core.Core;
using BaseCreator_Core.Helper;

namespace BaseCreator_Core.Model {

  public class BCTemplate : _BCClass {

    #region fields and properties
    private string _content;
    private string _filePath;
    private string _bezeichnung;
    private string _dateiname;
    private TemplateTarget _target;

    public string Content {
      get { return _content; }
      set {
        _content = value;
        OnPropertyChanged();
        Dirty = true;
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
    public string Bezeichnung {
      get { return _bezeichnung; }
      set {
        _bezeichnung = value;
        OnPropertyChanged();
        Dirty = true;
        OnPropertyChanged("CurrentFilePath");
      }
    }
    public string Dateiname {
      get { return _dateiname; }
      set {
        _dateiname = value;
        OnPropertyChanged();
        Dirty = true;
      }
    }
    public string CurrentFilePath {
      get {
        string[] infos = FileManager.splitFilePath(FilePath);
        string newFileName = (String.IsNullOrWhiteSpace(Bezeichnung) ? ("_template_" + DateTime.Now.ToString("yyyyMMdd_HHmmss")) : Bezeichnung);
        return infos[0] + "\\" + newFileName + "." + infos[2];
      }
    }
    public override string Prefix { get { return ""; } }
    public TemplateTarget Target {
      get { return _target; }
      set {
        _target = value;
        OnPropertyChanged();
        Dirty = true;
      }
    }
    #endregion fields and properties

    #region ViewModels
    #endregion ViewModels

    #region WPFCommands

    private void CreateCommands() {

    }
    #endregion WPFCommands

    #region constructors
    public BCTemplate(string bezeichnung) : this(bezeichnung, "") { }
    public BCTemplate(string bezeichnung, string filePath) : base() {
      // set properties
      Darstellung = bezeichnung;
      _filePath = filePath;
      _content = "";
      _bezeichnung = bezeichnung;
      _target = BC_Core.GetTemplateTarget("Keins");
      // set viewmodels
      // load commands
      CreateCommands();
      // load gui-texts
      // start init
      int init = 0;
      try {
        init = Init();
        if (init != 0) {
          throw new VerificationException("Error at creating <BCTemplate>. Error:" + init);
        }
      }
      catch (Exception e) {
        string className = "BCTemplate";
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

      return true;
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
      return "BCTemplate[" + Darstellung + "]";
    }
    #endregion Helper-Methods

    #region GUI-Language
    #endregion GUI-Language

  }

}
