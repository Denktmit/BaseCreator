using BaseCreator_Core.Helper;
using BaseCreator_Core.Model;
using System;
using System.Security;

namespace BaseCreator_Core.Core.Tags {

  public class fe_Tag : _Tag {

    #region fields and properties
    private TemplateTarget _templateTarget;

    public TemplateTarget TemplateTarget {
      get { return _templateTarget; }
      set { _templateTarget = value; }
    }
    public override string Opening { get { return "<fe:"+TemplateTarget.TargetClass+">"; } }
    public override string Closing { get { return "</fe:" + TemplateTarget.TargetClass + ">"; } }
    #endregion fields and properties

    #region constructors
    public fe_Tag(TemplateTarget templateTarget) : base("fe", "ForEach") {
      // set properties
      _templateTarget = templateTarget;
      // start init
      int init = 0;
      try {
        init = Init();
        if (init != 0) {
          throw new VerificationException("Error at creating <fe_Tag>. Error:" + init);
        }
      }
      catch (Exception e) {
        string className = "fe_Tag";
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
