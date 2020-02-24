using System;
using System.Configuration;
using System.Collections.Generic;
using System.Diagnostics;

namespace BaseCreator_Core.Helper {

  /// <summary>
  /// Provides a static variable about what should be printed to the console.
  /// It is a bitmask.
  /// Default: 2
  /// 0:    Nothing
  /// 1:    Exceptions
  /// 2:    Errors
  /// 4:    Methods
  /// ...
  /// 124:  Other
  /// </summary>
  public static class Debugger {

    private static int _debugMask = -1;
    public static int DebugMask {
      get {
        if (_debugMask < 0) {
          LoadDebugMask();
        }
        return _debugMask;
      }
    }

    public static bool DebugNothing { get { return DebugMask == 0; } }
    public static bool DebugExceptions { get { return (DebugMask & 1) == 1; } }
    public static bool DebugErrors { get { return (DebugMask & 2) == 2; } }
    public static bool DebugMethods { get { return (DebugMask & 4) == 4; } }
    public static bool DebugOther { get { return (DebugMask & 124) == 124; } }

    public static string _logFolder = Environment.CurrentDirectory + "\\Logs";
    public static string LogFolder { get { return _logFolder; } }

    private static void LoadDebugMask() {
      try {
        string loadedDebugLevel = Configurations.GetValue("debugmask", "255");
        if (loadedDebugLevel == null) {
          // Use 2 as default debugmask
          loadedDebugLevel = "2";
          Console.WriteLine("\n! The value of the debugmask could not be loaded from the App.config-file. The value <debugmask> is missing.\n");
          ErrorHandler.CreateErrorException(ErrorType.PropertyMissing, "Debugger", "LoadDebugMask"
          , "The value of the debugmask could not be loaded from the App.config-file. The value <debugmask> is missing.", "Dbg-002");
        }
        _debugMask = Int32.Parse(loadedDebugLevel);
      }
      catch (Exception e) {
        throw ErrorHandler.GetErrorException(e, ErrorType.PropertyMissing, "Debugger", "LoadDebugMask"
          , "The value of the debugmask could not be loaded from the App.config-file.", "Dbg-001", true);
      }
    }


    public static void WriteAbsolut(string text) {
      LogDebugging(text, DebugType.Absolut, 2);
      Console.Write(text);
    }
    public static void WriteLineAbsolut(string text) {
      LogDebugging(text, DebugType.Absolut, 2);
      Console.WriteLine(text);
    }
    public static void Write(string text) {
      //Console.Write(text);
      WriteOther(text, true);
    }
    public static void WriteLine(string text) {
      //Console.WriteLine(text);
      WriteLineOther(text, true);
    }
    public static void WriteException(string text) {
      LogDebugging(text, DebugType.Exception, 2);
      if (DebugExceptions)
        Console.Write(text);
    }
    public static void WriteLineException(string text) {
      LogDebugging(text, DebugType.Exception, 2);
      if (DebugExceptions)
        Console.WriteLine(text);
    }
    public static void WriteError(string text) {
      LogDebugging(text, DebugType.Error, 2);
      if (DebugErrors)
        Console.Write(text);
    }
    public static void WriteLineError(string text) {
      LogDebugging(text, DebugType.Error, 2);
      if (DebugErrors)
        Console.WriteLine(text);
    }
    public static void WriteMethod(string text) {
      LogDebugging(text, DebugType.Method, 2);
      if (DebugMethods)
        Console.Write(text);
    }
    public static void WriteLineMethod(string text) {
      LogDebugging(text, DebugType.Method, 2);
      if (DebugMethods)
        Console.WriteLine(text);
    }
    public static void WriteOther(string text, bool intern = false) {
      LogDebugging(text, DebugType.Other, intern ? 3 : 2);
      if (DebugOther)
        Console.Write(text);
    }
    public static void WriteLineOther(string text, bool intern = false) {
      LogDebugging(text, DebugType.Other, intern ? 3 : 2);
      if (DebugOther)
        Console.WriteLine(text);
    }

    private static void LogDebugging(string text, DebugType type, int stacklevels) {
      string filename = "";
      List<string> content = new List<string>();
      switch (type) {
        case DebugType.Absolut:
          filename = "Debug_Absolut";
          break;
        case DebugType.Other:
          filename = "Debug_Other";
          break;
        case DebugType.Exception:
          filename = "Debug_Exception";
          break;
        case DebugType.Error:
          filename = "Debug_Error";
          break;
        case DebugType.Method:
          filename = "Debug_Method";
          break;
        default:
          filename = "Debug_Default";
          break;
      }
      var callingMethod = new StackTrace().GetFrame(stacklevels).GetMethod();
      string c = callingMethod.DeclaringType.Name;
      string m = callingMethod.Name;
      content.Add("-------------------------");
      content.Add("" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"));
      content.Add(c + " - " + m);
      content.Add(text);
      FileManager.addContentTo(LogFolder, filename, "log", content);
    }

  }

  public enum DebugType {
    Absolut = 0
      , Other = 1
      , Exception = 2
      , Error = 3
      , Method = 4
  }

}
