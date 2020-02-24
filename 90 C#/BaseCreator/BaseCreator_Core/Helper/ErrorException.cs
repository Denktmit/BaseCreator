using System;

namespace BaseCreator_Core.Helper {

  public class ErrorException : Exception {

    #region fields and properties
    // public string Source { get; set; }
    // public string HelpLink { get; set; }
    // public string StackTrace { get; }
    // public MethodBase TargetSite { get; }
    public new Exception InnerException { get; private set; }
    // public string Message { get; }
    // public int HResult { get; protected set; }
    // public virtual IDictionary Data { get; }
    public DateTime DateTime { get; protected set; }
    public string Key { get; protected set; }
    public ErrorType ErrorType { get; set; }
    public string ClassWithError { get; protected set; }
    public string MethodWithError { get; protected set; }
    public bool Thrown { get; set; }
    #endregion fields and properties

    #region Events
    // protected event EventHandler<SafeSerializationEventArgs> SerializeObjectState;
    #endregion Events

    #region constructors
    public ErrorException() : base() {
      DateTime = DateTime.Now;
      Key = Miscellaneous.GetRandomSeed(23);
      ErrorType = ErrorType.OtherError;
      ClassWithError = "";
      MethodWithError = "";
      Thrown = false;
      Source = "";
      HelpLink = ErrorType + "-0";
    }
    // public ErrorException(string message) { }
    // public ErrorException(string message, Exception innerException) { }
    // protected ErrorException(SerializationInfo info, StreamingContext context) { }
    public ErrorException(ErrorType inType, string inClassWithError, string inMethodWithError, string inMessage, string inHelpLinkExt, bool inThrown) : base(inMessage) {
      DateTime = DateTime.Now;
      Key = Miscellaneous.GetRandomSeed(23);
      ErrorType = inType;
      ClassWithError = inClassWithError;
      MethodWithError = inMethodWithError;
      Thrown = inThrown;
      Source = inClassWithError + "." + inMethodWithError + "()";
      HelpLink = ErrorType + "-" + inHelpLinkExt;
      InnerException = new Exception("-");
      InnerException.Source = "-";
    }
    public ErrorException(ErrorType inType, string inClassWithError, string inMethodWithError, string inMessage, Exception inInnerException, string inHelpLinkExt, bool inThrown) : base(inMessage, inInnerException) {
      DateTime = DateTime.Now;
      Key = Miscellaneous.GetRandomSeed(23);
      ErrorType = inType;
      ClassWithError = inClassWithError;
      MethodWithError = inMethodWithError;
      Thrown = inThrown;
      InnerException = inInnerException;
      Source = inClassWithError + "." + inMethodWithError + "()" + ((InnerException == null || InnerException.Source == null || InnerException.Source == "") ? "" : (" <- " + InnerException.Source));
      HelpLink = ErrorType + "-" + inHelpLinkExt + ((InnerException == null || InnerException.HelpLink == null || InnerException.HelpLink == "") ? "" : (" / " + InnerException.HelpLink));
    }

    #endregion constructors

    #region Class-Methods
    // public virtual Exception GetBaseException() { }
    // public virtual void GetObjectData(SerializationInfo info, StreamingContext context) { }
    // public Type GetType() { }
    public override string ToString() {
      string ret = "ErrorException <" + ErrorType.ToString() + ">";
      return ret;
    }
    #endregion Class-Methods

    #region Helper-Methods

    #endregion Helper-Methods

  }

  public enum ErrorType {
    SystemException = 1
    , DataSetNotFound = 2
    , LanguageTextNotFound = 3
    , CreationError = 4
    , OtherError = 5
    , DatabaseError = 6
    , DBResultError = 7
    , ListIdToBig = 8
    , FileNotFound = 9
    , CorruptedFile = 10
    , AutoCreateError = 11
    , InputError = 12
    , ParseError = 13
    , GetterError = 14
    , SetterError = 15
    , PropertyWrongFormat = 16
    , PropertyMissing = 17
    , DBConstraintViolation = 18
    , DBSPFailure = 19
    , MissingRight = 20
    , AssemblyError = 21
    , FileError = 22
    , LoadError = 23
    , ConnectionError = 24
    , EmailError = 25
    , AddinException = 26
    , DownloadException = 27
  }

}
