using System;
using System.Collections.Generic;

namespace VDUtils.Helper {

  public static class ErrorHandler {

    #region fields and properties
    private static List<ErrorException> _errors = new List<ErrorException>();
    private static List<ErrorException> _dataerrors = new List<ErrorException>();
    private static long _maxSizeInKB = 1000 * 1;
    private static int _maxLogFiles = 50;

    public static List<ErrorException> Errors { get { return _errors; } set { _errors = value; } }
    public static List<ErrorException> DataErrors { get { return _dataerrors; } set { _dataerrors = value; } }
    #endregion fields and properties

    #region exception
    public static ErrorException GetErrorException(Exception ex, ErrorType inType, string inClass, string inMethod, string inMsg, string inHelpLinkExt, bool inThrown) {
      ErrorType newType = inType;
      if (ex == null) {
        //throw new ArgumentNullException("Exception at ErrorHandler.GetErrorException() -> Given Exception was null.");
      }
      if (ex is ErrorException) {
        newType = ((ErrorException)ex).ErrorType;
      }
      if (ex is ErrorException) {
        ((ErrorException)ex).Thrown = false;
      }
      ErrorException ret = new ErrorException(newType, inClass, inMethod, inMsg, ex, inHelpLinkExt, inThrown);
      AddError(ret, inThrown);
      return ret;
    }
    public static void CreateErrorException(Exception ex, ErrorType inType, string inClass, string inMethod, string inMsg, string inHelpLinkExt) {
      GetErrorException(ex, inType, inClass, inMethod, inMsg, inHelpLinkExt, false);
    }
    #endregion exception

    #region error
    public static ErrorException GetErrorException(ErrorType inType, string inClass, string inMethod, string inMsg, string inHelpLinkExt, bool inThrown) {
      ErrorException ret = new ErrorException(inType, inClass, inMethod, inMsg, inHelpLinkExt, inThrown);
      AddError(ret, inThrown);
      return ret;
    }
    public static void CreateErrorException(ErrorType inType, string inClass, string inMethod, string inMsg, string inHelpLinkExt) {
      GetErrorException(inType, inClass, inMethod, inMsg, inHelpLinkExt, false);
    }
    #endregion error

    #region logging
    public static void AddError(ErrorException error, bool inThrown) {
      if (inThrown) {
        Errors.Add(error);
        LogError(error, inThrown);
      }
      else {
        DataErrors.Add(error);
        LogError(error, inThrown);
      }
    }
    private static void LogError(ErrorException e, bool inThrown) {
      List<string> tfc = new List<string>();
      string filePath = Environment.CurrentDirectory + "\\Logs";
      string fileName = "APLogException";
      if (!inThrown) {
        fileName = "APLogDataError";
      }
      string fileExt = "LOG";
      // create lines to print and log
      tfc.Add("------------------------------------------------------");
      tfc.Add("- Time       : " + e.DateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));
      tfc.Add("- Key        : " + e.Key);
      tfc.Add("- Type       : " + e.ErrorType.ToString());
      tfc.Add("- Thrown     : " + e.Thrown);
      tfc.Add("- Class      : " + e.ClassWithError);
      tfc.Add("- Method     : " + e.MethodWithError);
      tfc.Add("- Source     : " + e.Source);
      tfc.Add("- HelpLink   : " + e.HelpLink);
      tfc.Add("- TargetSite : " + e.TargetSite);
      tfc.Add("- Message    : " + e.Message);
      if (e.StackTrace != null && e.StackTrace.Length > 0) {
        tfc.Add("- StackTrace : " + e.StackTrace);
      }
      if (e.InnerException != null) {
        tfc.Add("- InnerExcep.: " + e.InnerException.Message == null ? "" : e.InnerException.Message);
      }
      // seperate from next thrown exception-group
      if (e.Thrown) {
        tfc.Add("");
        tfc.Add("");
      }
      // print lines
      if ((inThrown && Debugger.DebugExceptions) || (!inThrown && Debugger.DebugErrors)) {
        foreach (string line in tfc) {
          Console.WriteLine(line);
        }
      }
      // prepare file
      PrepareLogFile(filePath, fileName, fileExt);
      // log ErrorException
      FileManager.addContentTo(filePath, fileName, fileExt, tfc);
    }
    #endregion logging

    #region file-methods
    private static void PrepareLogFile(string filePath, string fileName, string fileExt) {
      FileManager.createFile(filePath, fileName, fileExt, false);
      long fileSize = new System.IO.FileInfo(filePath + "\\" + fileName + "." + fileExt).Length / 1000;
      if (fileSize >= _maxSizeInKB) {
        RenameLogFiles(filePath, fileName, fileExt);
      }
    }
    private static void RenameLogFiles(string filePath, string fileName, string fileExt) {
      string fNO = fileName + "_" + (_maxLogFiles - 1);
      string fNN = "";
      try {
        // aelteste datei loeschen
        FileManager.deleteFile(filePath, fNO, fileExt, true);
        // dateien von hinten ausgehend um eins nach hinten verschieben
        for (int i = _maxLogFiles - 2; i >= 0; i--) {
          fNO = fileName + "_" + (i);
          if (i == 0) {
            fNO = fileName;
          }
          fNN = fileName + "_" + (i + 1);
          FileManager.renameFile(filePath, fNO, fileExt, fNN, true);
        }
        // neue Datei erstellen
        FileManager.createFile(filePath, fileName, fileExt);
      }
      catch (Exception e) {
        string errMsg = "ERROR@ErrorHandler.RenameLogFiles: Could not rename log files (" + filePath + "\\" + fNO + "." + fileExt + ")";
        Console.WriteLine(errMsg);
        Console.WriteLine(e.ToString());
        throw new Exception(errMsg + "\r\n  -> Exception: " + e.ToString());
      }
    }
    #endregion file-methods

  }

}
