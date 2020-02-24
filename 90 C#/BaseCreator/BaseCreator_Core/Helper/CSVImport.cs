using System;
using System.Collections.Generic;

namespace BaseCreator_Core.Helper {

  public class CSVImport {

    private static string Seperator { get { return ";"; } }

    private static List<string> readFromCSV(string folderPath, string fileName) {
      //folderPath like "D:/Dropbox/Workspaces/Project-Workspace/AktivenPlanerNeu/res/Classes"
      //fileName like "Classes.csv"
      List<string> ret = FileManager.getContentAsList(folderPath, fileName, "csv");
      if (ret == null) {
        throw new Exception("ERROR@readFromCSV(...) -> Eingelesene Liste war null.");
      }
      return ret;
    }
    public static List<string> getList(string folderPath, string fileName) {
      List<string> l = readFromCSV(folderPath, fileName);
      if (l == null) {
        Console.WriteLine("ERR@CSVExport: getList: return from FileMan is null");
        return null;
      }
      return l;
    }
    public static List<List<string>> getListList(string folderPath, string fileName, string sep, bool specSep) {
      List<string> l = readFromCSV(folderPath, fileName);
      if (l == null) {
        Console.WriteLine("ERR@CSVExport: getListList: return from FileMan is null");
        return null;
      }
      List<List<string>> rDList = new List<List<string>>();
      for (int i = 0; i < l.Count; i++) {
        rDList.Add(new List<string>());
        string[] parts;
        if (specSep) {
          parts = seperate(l[i], sep);
        }
        else {
          parts = l[i].Split(sep.ToCharArray());
        }
        for (int j = 0; j < parts.Length; j++) {
          rDList[i].Add(parts[j]);
        }
      }
      return rDList;
    }
    public static List<List<string>> getListList(string folderPath, string fileName, string sep) {
      return getListList(folderPath, fileName, sep, false);
    }
    public static List<List<string>> getListList(string folderPath, string fileName) {
      return getListList(folderPath, fileName, Seperator);
    }
    public static List<List<string>> getListList(string filePath) {
      string[] info = FileManager.splitFilePath(filePath);
      return getListList(info[0], info[1]);
    }
    public static string[,] getDoubleArray(string folderPath, string fileName, string sep, bool specSep) {
      List<List<string>> ll = getListList(folderPath, fileName, sep, specSep);
      int col = 0;
      int row = 0;
      for (row = 0; row < ll.Count; row++) {
        if (ll[row].Count > col) {
          col = ll[row].Count;
        }
      }
      string[,] ret = new string[col, row];
      for (int r = 0; r < row; r++) {
        //Console.WriteLine("-A -> r:"+r);
        for (int c = 0; c < ll[r].Count; c++) {
          //Console.WriteLine("-B -> r:"+r+" , c:"+c);
          if (ll[r][c] == null) {
            ret[c, r] = "";
          }
          else {
            ret[c, r] = ll[r][c];
          }
        }
      }
      for (int i = 0; i < col; i++) {
        for (int j = 0; j < row; j++) {
          if (ret[i, j] == null) {
            ret[i, j] = "";
          }
        }
      }
      return ret;
    }
    public static string[,] getDoubleArray(string folderPath, string fileName, string sep) {
      return getDoubleArray(folderPath, fileName, sep, false);
    }
    public static string[] seperate(string in_String, string sep) {
      //Console.WriteLine("## sep: Start");
      string[] ret;
      bool inString = false;
      int parts = 0;
      for (int i = 0; i < in_String.Length; i++) {
        if (in_String[i] == '"') {
          inString = !inString;
        }
        if (in_String[i] == sep[0] && !inString) {
          parts++;
        }
      }
      //Console.WriteLine("## sep: parts: "+parts);
      ret = new string[parts + 1];
      if (inString)
        Console.WriteLine("ERR@CSVExport-Seperate: inString should be false. (" + in_String + ")");
      parts = 0;
      for (int i = 0; i < ret.Length; i++) {
        ret[i] = "";
      }
      for (int i = 0; i < in_String.Length; i++) {
        if (in_String[i] == '"') {
          inString = !inString;
        }
        if (in_String[i] != sep[0] || inString) {
          ret[parts] += in_String[i];
        }
        if (in_String[i] == sep[0] && !inString) {
          parts++;
        }
      }
      for (int i = 0; i < ret.Length; i++) {
        if (ret[i].Length == 0) {
          ret[i] = "";
        }
      }
      //Console.WriteLine("## sep: Ende");
      return ret;
    }
    public static int testIfCorrectFormat(string fall, string[,] in_String) {
      switch (fall) {
        case "SQL":
          return tICF_SQL(in_String);
        case "Java":
          return tICF_Java(in_String);
        default:
          Console.WriteLine("ERR@CSVExport: testIfCorrectFormat: Wrong string-Input");
          return -1;
      }
    }
    private static int tICF_SQL(string[,] in_String) {
      //Console.WriteLine("-Start TestIfCorrectFormat");
      if (in_String == null) {
        return 1;
      }
      if (in_String.Length != 13) {
        return 2;
      }
      if (in_String[0, 0].Contains("Klasse")) {
        if (!in_String[0, 0].Equals("Klasse")) {
          in_String[0, 0] = "Klasse";
        }
      }
      else {
        return 3;
      }
      if (!in_String[1, 0].Equals("Attribut")) {
        return 4;
      }
      if (!in_String[2, 0].Equals("Art")) {
        return 5;
      }
      if (!in_String[3, 0].Equals("Groesse")) {
        return 6;
      }
      if (!in_String[4, 0].Equals("PrimKey")) {
        return 7;
      }
      if (!in_String[5, 0].Equals("NotNull")) {
        return 8;
      }
      if (!in_String[6, 0].Equals("AutoIncre")) {
        return 9;
      }
      if (!in_String[7, 0].Equals("default")) {
        return 11;
      }
      if (!in_String[8, 0].Equals("constraint")) {
        return 12;
      }
      if (!in_String[9, 0].Equals("Foreign Key to:")) {
        return 13;
      }
      if (!in_String[10, 0].Equals("on delete")) {
        return 14;
      }
      if (!in_String[11, 0].Equals("on update")) {
        return 15;
      }
      if (!in_String[12, 0].Equals("Kommentar")) {
        return 16;
      }

      if (in_String.GetLength(0) <= 1) {
        return 20;
      }
      //Console.WriteLine("-End TestIfCorrectFormat (true)");
      return 0;
    }
    private static int tICF_Java(string[,] in_StringArr) {
      //Console.WriteLine("-Start TestIfCorrectFormat");
      if (in_StringArr == null) {
        return 1;
      }
      if (in_StringArr.Length != 8) {
        return 2;
      }
      if (in_StringArr[0, 0].Contains("Klasse")) {
        if (!in_StringArr[0, 0].Equals("Klasse")) {
          in_StringArr[0, 0] = "Klasse";
        }
      }
      else {
        return 3;
      }
      if (!in_StringArr[1, 0].Equals("Attribut")) {
        return 4;
      }
      if (!in_StringArr[2, 0].Equals("Art")) {
        return 5;
      }
      if (!in_StringArr[3, 0].Equals("Groesse")) {
        return 6;
      }
      if (!in_StringArr[4, 0].Equals("PrimKey")) {
        return 7;
      }
      if (!in_StringArr[5, 0].Equals("NotNull")) {
        return 8;
      }
      if (!in_StringArr[6, 0].Equals("AutoIncre")) {
        return 9;
      }
      if (!in_StringArr[7, 0].Equals("default")) {
        return 10;
      }
      if (in_StringArr.GetLength(0) <= 1) {
        return 20;
      }
      //Console.WriteLine("-End TestIfCorrectFormat (true)");
      return 0;
    }
    
    public static void printList(List<string> l) {
      Console.WriteLine("-----------------------------------------------------------");
      Console.WriteLine("V--------List:---------V------------V-----------V--------V");
      for (int i = 0; i < l.Count; i++) {
        Console.WriteLine(l[i]);
      }
      Console.WriteLine("A--------List:---------A------------A-----------A--------A");
      Console.WriteLine("-----------------------------------------------------------");
    }

    public static void printDList(List<List<string>> l) {
      Console.WriteLine("-----------------------------------------------------------");
      Console.WriteLine("V--------DList:---------V------------V-----------V--------V");
      int maxL = getMaxOfDList(l);
      if (maxL > 15) {
        maxL = 15;
      }
      for (int i = 0; i < l.Count; i++) {
        for (int j = 0; j < l[i].Count; j++) {
          string s = l[i][j];
          Console.Write(getStringWithLength(s, maxL));
        }
        Console.WriteLine("");
      }
      Console.WriteLine("A--------DList:---------A------------A-----------A--------A");
      Console.WriteLine("-----------------------------------------------------------");
    }

    public static void printDArray(string[,] dAS) {
      Console.WriteLine("-----------------------------------------------------------");
      Console.WriteLine("V--------DArray:--------V------------V-----------V--------V");
      int maxFL = 20;
      for (int r = 0; r < dAS.GetLength(0); r++) {
        for (int c = 0; c < dAS.Length && c < 40; c++) {
          string s = dAS[c, r];
          Console.Write("<" + getStringWithLength(s, maxFL) + ">");
        }
        Console.WriteLine("#!#");
      }
      Console.WriteLine("A--------DArray:--------A------------A-----------A--------A");
      Console.WriteLine("-----------------------------------------------------------");
    }
    private static int getMaxOfList(List<string> l) {
      int m = 0;
      for (int i = 0; i < l.Count; i++) {
        if (l[i].Length > m) {
          m = l[i].Length;
        }
      }
      return m;
    }
    private static int getMaxOfDList(List<List<string>> ll) {
      int m = 0;
      for (int i = 0; i < ll.Count; i++) {
        if (getMaxOfList(ll[i]) > m) {
          m = getMaxOfList(ll[i]);
        }
      }
      return m;
    }
    private static string getStringWithLength(string s, int l) {
      string r = s;
      if (s.Length < l) {
        for (int a = 0; a < l - s.Length; a++) {
          r += " ";
        }
      }
      else {
        if (s.Length > l - 1) {
          r = s.Substring(0, l - 1) + " ";
        }
      }
      return r;
    }

    public static bool Save(string filePath, List<List<string>> content) {
      try {
        string[] fileInfo = FileManager.splitFilePath(filePath);
        List<string> cont = new List<string>();
        foreach(List<string> row in content) {
          string r = "";
          foreach(string s in row) {
            r += s+""+Seperator;
          }
          r = r.Substring(0, r.Length - 1);
          cont.Add(r);
        }
        FileManager.writeNewFileWithContent(fileInfo[0], fileInfo[1], fileInfo[2], cont, true);
      } catch(Exception ex) {
        throw new Exception("CSV-Datei konnte nicht gespeichert werden.", ex);
      }
      return true;
    }

  }

}
