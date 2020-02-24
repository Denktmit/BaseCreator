using BaseCreator_Core.Helper;
using System;
using System.Security;

namespace BaseCreator_Core.Core.Tags {

  public abstract class _Tag {

    #region fields and properties
    private string _darstellung;
    private string _beschreibung;
    private string _content;

    public string Darstellung { get { return _darstellung; } }
    public string Beschreibung { get { return _beschreibung; } }
    public string Content {
      get { return _content; }
      set { _content = value; }
    }
    public abstract string Opening { get; }
    public abstract string Closing { get; }
    #endregion fields and properties

    #region constructors
    public _Tag(string darstellung, string beschreibung) {
      // set properties
      _darstellung = darstellung;
      _beschreibung = beschreibung;
      _content = "";
    }
    #endregion constructors

    #region Init-Methods
    #endregion Init-Methods
    
    #region Class-Methods
    #endregion Class-Methods

    #region Helper-Methods
    #endregion Helper-Methods
    
  }

}
