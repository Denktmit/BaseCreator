using BaseCreator_Core.Helper;
using System;
using System.Security;

namespace BaseCreator_Core.Core {

  public class Step {

    #region fields and properties
    #endregion fields and properties
    
    #region constructors
    public Step() {
      // set properties
      // start init
      int init = 0;
      try {
        init = Init();
        if (init != 0) {
          throw new VerificationException("Error at creating <Step>. Error:" + init);
        }
      }
      catch (Exception e) {
        string className = "Step";
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
    
    #region Class-Methods
    #endregion Class-Methods

    #region Helper-Methods
    #endregion Helper-Methods
    
  }

}
