using System;
using System.Security;
using System.Configuration;

namespace BaseCreator_Core.Helper {

  public static class Configurations {

    private static Properties _props;
    public static Properties Properties {
      get {
        if (_props == null) {
          LoadPropertiesFromHarddrive();
        }
        return _props;
      }
    }

    public static string GetValue(string inKey) {
      try {
        //string ret = ConfigurationManager.AppSettings[inKey];
        string ret = null;
        try {
          ret = Properties.Get(inKey);
        }
        catch (Exception e) {
          ErrorHandler.CreateErrorException(e, ErrorType.LoadError, "Configurations", "GetValue"
            , "Der Wert konnte nicht geladen werden.", "28");
          Environment.Exit(0);
        }
        if (ret == null || String.IsNullOrWhiteSpace(ret))
          throw new Exception("Konnte die Einstellung zu <" + inKey + "> nicht laden.");
        return ret;
      }
      catch (Exception e) {
        throw e;
      }
    }
    public static string GetValue(string inKey, string defaultValue) {
      string loaded = GetValue(inKey);
      if (loaded == null) {
        return defaultValue;
      }
      return loaded;
    }

    private static void LoadPropertiesFromHarddrive() {
      if (FileManager.exists("D:\\Playground\\BaseCreator", "BaseCreator", "properties", false)) {
        _props = new Properties("D:\\Playground\\BaseCreator", "BaseCreator");
      }
      else if (FileManager.exists("V:\\Playground\\BaseCreator", "BaseCreator", "properties", false)) {
        _props = new Properties("V:\\Playground\\BaseCreator", "BaseCreator");
      }
      else {
        ErrorHandler.CreateErrorException(ErrorType.LoadError, "Configurations", "GetValue"
          , "Die Properties konnten nicht geladen werden.", "56");
        Environment.Exit(0);
      }
    }

  }

}
