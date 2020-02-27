using System;
using System.Collections.Generic;
using AP_Extension.AP;
using BaseCreator_Model.Model;
using VDUtils.Helper;

namespace AP_Extension {

  public static class APAC {

    private static AutoCreator _ac;
    public static AutoCreator AC {
      get {
        if (_ac == null)
          _ac = new AutoCreator();
        return _ac;
      }
    }

    public static bool CreateResult(List<List<string>> data, string filePath, string function) {
      try {
        if (AC.FilePath != filePath)
          AC.ImportData(data, filePath);
        switch (function) {
          case "AP-SQL":
            return AC.CreateSQL();
          case "AP-CS":
            return AC.CreateCS();
          case "AP-PHP":
            return AC.CreatePHP();
          case "AP-HTML":
            return AC.CreateHTML();
          case "AP-RandDS":
            return AC.CreateRandDS();
          default:
            return false;
        }
      } catch (Exception ex) {
        ErrorHandler.CreateErrorException(ex, ErrorType.AutoCreateError, "APAC", "CreateResult"
          , "Fehler beim Erstellen der AP-Dateien.", "39");
        return false;
      }
    }
    public static bool CreateResult(BCFile file, string function) {
      try {
        if (AC.FilePath != file.FilePath)
          AC.ImportData(file);
        switch (function) {
          case "AP-SQL":
            return AC.CreateSQL();
          case "AP-CS":
            return AC.CreateCS();
          case "AP-PHP":
            return AC.CreatePHP();
          case "AP-HTML":
            return AC.CreateHTML();
          case "AP-RandDS":
            return AC.CreateRandDS();
          default:
            return false;
        }
      } catch (Exception ex) {
        ErrorHandler.CreateErrorException(ex, ErrorType.AutoCreateError, "APAC", "CreateResult"
          , "Fehler beim Erstellen der AP-Dateien.", "39");
        return false;
      }
    }

  }

}
