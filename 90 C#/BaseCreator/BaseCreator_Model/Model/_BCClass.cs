using System;
using VDUtils.Helper;

namespace BaseCreator_Model.Model {

  public abstract class _BCClass : ModelBase {

    #region fields and properties
    private bool _dirty;
    private bool _invalid;
    private bool _isSelected;
    private string _darstellung;

    public bool Dirty {
      get { return _dirty; }
      set {
        _dirty = value;
        OnPropertyChanged();
        OnPropertyChanged("ExtendedDarstellung");
      }
    }
    public bool Invalid {
      get { return _invalid; }
      set {
        _invalid = value;
        OnPropertyChanged();
      }
    }
    public bool IsSelected {
      get { return _isSelected; }
      set {
        _isSelected = value;
        OnPropertyChanged();
      }
    }
    public string Darstellung {
      get { return _darstellung; }
      set {
        _darstellung = value;
        OnPropertyChanged();
        Dirty = true;
        OnPropertyChanged("ExtendedDarstellung");
      }
    }
    public string ExtendedDarstellung {
      get { return (Invalid ? "!" : "") + (Dirty ? "*" : "") + Darstellung; }
      set { /* do nothing */ }
    }
    public string DarstellungWP {
      get { return Prefix + Darstellung; }
      set { /* do nothing */ }
    }
    public abstract string Prefix { get; }
    #endregion fields and properties

    #region ViewModels
    #endregion ViewModels

    #region WPFCommands
    #endregion WPFCommands

    #region constructors
    public _BCClass() : this("ToDo") { }
    public _BCClass(string darstellung) {
      _dirty = true;
      _invalid = false;
      _isSelected = false;
      _darstellung = darstellung;
    }
    #endregion constructors

    #region Init-Methods
    #endregion Init-Methods

    #region Btn-Methods
    #endregion Btn-Methods

    #region Database-Methods
    #endregion Database-Methods

    #region Class-Methods
    #endregion Class-Methods

    #region Helper-Methods
    #endregion Helper-Methods

    #region GUI-Language
    #endregion GUI-Language

  }

}
