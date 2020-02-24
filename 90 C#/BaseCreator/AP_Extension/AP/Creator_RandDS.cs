using BaseCreator_Core.Helper;
using System;
using System.Security;

namespace AP_Extension.AP {

  public class Creator_RandDS {

    #region fields and properties
    #endregion fields and properties

    #region ViewModels
    #endregion ViewModels

    #region WPFCommands

    private void CreateCommands() {

    }
    #endregion WPFCommands

    #region constructors
    public Creator_RandDS() {
      // set properties
      // set viewmodels
      // load commands
      CreateCommands();
      // load gui-texts
      // start init
      int init = 0;
      try {
        init = Init();
        if (init != 0) {
          throw new VerificationException("Error at creating <Creator_RandDS>. Error:" + init);
        }
      }
      catch (Exception e) {
        string className = "Creator_RandDS";
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
    #endregion Helper-Methods

    #region GUI-Language
    #endregion GUI-Language

  }

}
