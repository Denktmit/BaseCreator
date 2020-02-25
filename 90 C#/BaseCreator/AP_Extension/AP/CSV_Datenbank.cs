using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDUtils.Helper;

namespace AP_Extension.AP {

  public class CSV_Datenbank {

    #region fields and properties
    private string _dBName;
    private List<CSV_Tabelle> _tabellen;

    public string DBName {
      get { return _dBName; }
      set { _dBName = value; }
    }
    public List<CSV_Tabelle> Tabellen {
      get {
        if (_tabellen == null)
          _tabellen = new List<CSV_Tabelle>();
        return _tabellen;
      }
    }
    #endregion fields and properties

    #region ViewModels
    #endregion ViewModels

    #region WPFCommands
    #endregion WPFCommands

    #region constructors
    public CSV_Datenbank(string name) {
      // set properties
      // set viewmodels
      // load commands
      // load gui-texts
      // start init
      int init = 0;
      try {
        init = Init();
      } catch (Exception e) {
        throw e;
      }
      if (init != 0) {
        string className = "CSV_Datenbank";
        string methodName = "Constructor";
        string errMsg = "Error at constructor of <" + className + ">. Error:" + init + ".";
        throw ErrorHandler.GetErrorException(ErrorType.CreationError, className, methodName, errMsg, "XYZ", true);
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
    #endregion Helper-Methods

    #region GUI-Language
    #endregion GUI-Language

  }

}
