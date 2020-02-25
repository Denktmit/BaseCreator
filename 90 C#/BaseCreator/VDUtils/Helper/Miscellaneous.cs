using System;

namespace VDUtils.Helper {

  public static class Miscellaneous {

    public static string GetCurrentLocalDateTimeStampAsString() {
      return DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
    }

    public static string GetRandomSeed(int seedLength) {
      string ret = "";
      Random zufall = new Random();
      for (int i = 0; i < seedLength; i++) {
        int c = zufall.Next(48, 123);
        if ((c > 57 && c < 65) || (c > 90 && c < 97)) {
          c = zufall.Next(97, 123);
        }
        ret += (char)(c);
      }
      return ret;
    }

    public static string GetBlanks(string already, int length) {
      return GetFiller(already, length, ' ');
    }
    public static string GetFiller(string already, int length, char filler) {
      string ret = "";
      if (already.Length < length) {
        for (int i = already.Length; i < length; i++) {
          ret += "" + filler;
        }
      }
      return ret;
    }

    public static string FillStringRight(string inString, char filler, int length) {
      if (inString.Length == length) {
        return inString;
      }
      if (inString.Length > length) {
        return inString.Substring(0, length);
      }
      return inString + GetFiller(inString, length, filler);
    }
    public static string FillStringLeft(string inString, char filler, int length) {
      if (inString.Length == length) {
        return inString;
      }
      if (inString.Length > length) {
        return inString.Substring(0, length);
      }
      return GetFiller(inString, length, filler) + inString;
    }

    public static string ConvertStringToSaveString(string s) {
      string ret = s.ToString();
      ret = ret.Replace(" ", "");
      ret = ret.Replace("-", "_");
      ret = ret.Replace("ß", "ss");
      ret = ret.Replace("ä", "ae");
      ret = ret.Replace("ö", "oe");
      ret = ret.Replace("ü", "ue");
      ret = ret.Replace("Ä", "Ae");
      ret = ret.Replace("Ö", "Oe");
      ret = ret.Replace("Ü", "Ue");
      return ret;
    }

    public static string GetCurrentDirectory() {
      return Environment.CurrentDirectory;
    }

    public static string GetStringForDatabase(string orig) {
      string ret = orig;
      ret = ret.Replace("ä", "ae");
      ret = ret.Replace("Ä", "Ae");
      ret = ret.Replace("ö", "oe");
      ret = ret.Replace("Ö", "Oe");
      ret = ret.Replace("ü", "ue");
      ret = ret.Replace("Ü", "Ue");
      ret = ret.Replace("ß", "ss");
      return ret;
    }
    public static string GetStringForDatabaseDescription(string orig) {
      string ret = GetStringForDatabase(orig);
      ret = ret.Replace(" ", "");
      return ret;
    }

  }

}
