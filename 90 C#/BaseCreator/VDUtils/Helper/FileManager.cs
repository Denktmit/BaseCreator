using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace VDUtils.Helper {

  public static class FileManager {

    public static bool exists(string fullFilePath, bool errText) {
      if (System.IO.File.Exists(fullFilePath)) {
        return true;
      }
      if (errText) {
        Console.WriteLine("ERROR@FileManager.exists: File couldnt be found.(" + fullFilePath + ")");
      }
      return false;
    }
    public static bool exists(string folderPath, string fileName, string fileType, bool errText) {
      if (System.IO.File.Exists(folderPath + "\\" + fileName + "." + fileType)) {
        return true;
      }
      if (errText) {
        Console.WriteLine("ERROR@FileManager.exists: File couldnt be found.(" + fileName + "." + fileType + " in " + folderPath + ")");
      }
      return false;
    }
    public static bool exists(string folderPath, string fileName, string fileType) {
      return exists(folderPath, fileName, fileType, true);
    }
    public static bool open(string folderPath, string fileName, string fileType) {
      if (exists(folderPath, fileName, fileType, false)) {
        try {
          //Java: Desktop.getDesktop().open(new File(folderPath + "/" + fileName + "." + fileType));
          System.Diagnostics.Process.Start(folderPath + "\\" + fileName + "." + fileType);
          return true;
        }
        catch (Exception e) {
          string errMsg = "ERROR@FileManager.open: Could not open the file (" + fileName + "." + fileType + " in " + folderPath + ")";
          Console.WriteLine(errMsg);
          Console.WriteLine(e.ToString());
          throw new Exception(errMsg + "\r\n  -> Exception: " + e.ToString());
        }
      }
      throw new Exception("ERROR@FileManager.open: file does not exist (" + fileName + "." + fileType + " in " + folderPath + ")");
    }
    public static bool createFile(string fullFilePath, bool errText) {
      string[] fi = splitFilePath(fullFilePath);
      return createFile(fi[0], fi[1], fi[2], errText);
    }
    public static bool createFile(string folderPath, string fileName, string fileType, bool errText) {
      if (!exists(folderPath, fileName, fileType, false)) {
        try {
          if (System.IO.Directory.CreateDirectory(folderPath).Exists) {
            using (System.IO.File.Create(folderPath + "\\" + fileName + "." + fileType)) {
              return true;
            }
          }
          else {
            using (System.IO.File.Create(folderPath + "\\" + fileName + "." + fileType)) {
              return true;
            }
          }
        }
        catch (Exception e) {
          string errMsg = "ERROR@FileManager.createTxt: Could not create file (Exception) (" + folderPath + "\\" + fileName + "." + fileType + ")";
          Console.WriteLine(errMsg);
          Console.WriteLine(e.ToString());
          throw new Exception(errMsg + "\r\n  -> Exception: " + e.ToString());
        }
      }
      if (errText)
        Console.WriteLine("ERROR@FileManager.createTxt: File already exists (" + fileName + "." + fileType + " in " + folderPath + ")");
      return false;
    }
    public static bool createFile(string folderPath, string fileName, string fileType) {
      return createFile(folderPath, fileName, fileType, true);
    }
    public static List<string> getContentAsList(string fullFilePath) {
      string[] infos = splitFilePath(fullFilePath);
      return getContentAsList(infos[0], infos[1], infos[2]);
    }
    public static List<string> getContentAsList(string folderPath, string fileName, string fileType) {
      if (exists(folderPath, fileName, fileType, false)) {
        List<string> ret = new List<string>();
        try {
          var utf8WithoutBOM = new System.Text.UTF8Encoding(false);
          var content = System.IO.File.ReadAllLines(folderPath + "\\" + fileName + "." + fileType, utf8WithoutBOM);
          foreach (string item in content) {
            ret.Add(item);
          }
          return ret;
        }
        catch (IOException e) {
          string errMsg = "ERROR@FileManager.getContentAsList: Couldnt read file (" + fileName + "." + fileType + " in " + folderPath + ")";
          Console.WriteLine(errMsg);
          Console.WriteLine(e.ToString());
          throw new Exception(errMsg + "\r\n  -> Exception: " + e.ToString());
        }
      }
      else {
        throw new Exception("ERROR@getContentAsList(" + folderPath + "\\" + fileName + "." + fileType + ") -> File does not exist.");
      }
    }
    public static string getContentAsString(string fullFilePath) {
      string[] infos = splitFilePath(fullFilePath);
      return getContentAsString(infos[0], infos[1], infos[2]);
    }
    public static string getContentAsString(string folderPath, string fileName, string fileType) {
      List<string> temp = getContentAsList(folderPath, fileName, fileType);
      string ret = "";
      for (int i = 0; i < temp.Count; i++) {
        ret += temp[i] + "\r\n";
      }
      return ret;
    }
    public static string getLine(string folderPath, string fileName, string fileType, int lineIndex) {
      List<string> temp = getContentAsList(folderPath, fileName, fileType);
      if (temp.Count <= lineIndex) {
        string errMsg = "ERROR@FileManager.getLine: LineIndex is to great for the content. (getLine(" + folderPath + "/" + fileName + "." + fileType + ")";
        Console.WriteLine(errMsg);
        throw new Exception(errMsg);
      }
      return temp[lineIndex];
    }
    public static bool deleteFile(string filePath) {
      return deleteFile(filePath, false);
    }
    public static bool deleteFile(string folderPath, string fileName, string fileType) {
      return deleteFile(folderPath, fileName, fileType, false);
    }
    public static bool deleteFile(string filePath, bool ignoreException) {
      if (exists(filePath, false)) {
        try {
          System.IO.File.Delete(filePath);
          return true;
        }
        catch (Exception e) {
          string errMsg = "ERROR@FileManager.deleteFile: Couldnt delete file (" + filePath + ")";
          Console.WriteLine(errMsg);
          Console.WriteLine(e.ToString());
          throw new Exception(errMsg + "\r\n  -> Exception: " + e.ToString());
        }
      }
      if (!ignoreException) {
        throw new Exception("ERROR@FileManager.deleteFile: File does not exist.");
      }
      return false;
    }
    public static bool deleteFile(string folderPath, string fileName, string fileType, bool ignoreException) {
      return deleteFile(folderPath + "\\" + fileName + "." + fileType, ignoreException);
    }
    public static bool setContent(string folderPath, string fileName, string fileType, List<string> newContent) {
      if (exists(folderPath, fileName, fileType, false)) {
        try {
          StringBuilder newFile = new StringBuilder();
          for(int i=0; i<newContent.Count; i++){
            string line = newContent[i];
            newFile.Append(line);
            if (i < newContent.Count - 1)
              newFile.Append("\r\n");
          }
          string nc = newFile.ToString();
          var utf8WithoutBOM = new System.Text.UTF8Encoding(false);
          System.IO.File.WriteAllText(folderPath + "\\" + fileName + "." + fileType, nc, utf8WithoutBOM);
          return true;
        }
        catch (Exception e) {
          string errMsg = ("ERROR@FileManager.setContent: Couldnt set new content to file (" + fileName + "." + fileType + " in " + folderPath + ")");
          Console.WriteLine(errMsg);
          Console.WriteLine(e.ToString());
          throw new Exception(errMsg + "; " + e.ToString());
        }
      }
      throw new Exception("ERROR@FileManager.setContent: File does not exist. (" + fileName + "." + fileType + " in " + folderPath + ")");
    }
    public static bool writeNewFileWithContent(string folderPath, string fileName, string fileType, List<string> newContent, bool forceOverride) {
      try {
        bool existing = exists(folderPath, fileName, fileType, false);
        if (existing && forceOverride) {
          setContent(folderPath, fileName, fileType, newContent);
          return true;
        }
        if (!existing) {
          createFile(folderPath, fileName, fileType, false);
          setContent(folderPath, fileName, fileType, newContent);
          return true;
        }
        throw new Exception("ERROR@FileManager.writeNewFileWithContent: File already exists and no forceOverride was given. (" + fileName + "." + fileType + " in " + folderPath + ")");
      }
      catch (Exception e) {
        string errMsg = ("ERROR@FileManager.writeNewFileWithContent: Could not write new File with content. (" + fileName + "." + fileType + " in " + folderPath + ")");
        Console.WriteLine(errMsg);
        Console.WriteLine(e.ToString());
        throw new Exception(errMsg + "; " + e.ToString());
      }
    }
    public static bool writeNewTxt(string folderPath, string fileName, string fileType, List<string> newContent) {
      if (!exists(folderPath, fileName, fileType, false)) {
        if (createFile(folderPath, fileName, fileType)) {
          return setContent(folderPath, fileName, fileType, newContent);
        }
        throw new Exception("ERROR@FileManager.writeNewTxt: File could not be created. (" + fileName + "." + fileType + " in " + folderPath + ")");
      }
      throw new Exception("ERROR@FileManager.writeNewTxt: File already exists. (" + fileName + "." + fileType + " in " + folderPath + ")");
    }
    public static bool changeLineTo(string folderPath, string fileName, string fileType, int lineIndex, string newContent) {
      List<string> list = getContentAsList(folderPath, fileName, fileType);
      string errMsg = "";
      if (list != null) {
        try {
          if (list.Count <= lineIndex) {
            errMsg = "ERROR@FileManager.changeLineTo: LineIndex is to great for the content. (" + folderPath + "/" + fileName + "." + fileType + "; LineIndex: " + lineIndex + ")";
            Console.WriteLine(errMsg);
            throw new Exception(errMsg);
          }
          list[lineIndex] = newContent;
          return setContent(folderPath, fileName, fileType, list);
        }
        catch (Exception e) {
          errMsg = ("ERROR@FileManager.changeLineTo: Could not change line in file (" + fileName + "." + fileType + " in " + folderPath + ")");
          Console.WriteLine(errMsg);
          Console.WriteLine(e.ToString());
          throw new Exception(errMsg + "\r\n  -> Exception: " + e.ToString());
        }
      }
      errMsg = ("ERROR@FileManager.changeLineTo: Content is null. (getLine(" + folderPath + "/" + fileName + "." + fileType + ")");
      Console.WriteLine(errMsg);
      throw new Exception(errMsg);
    }
    public static bool addLineTo(string folderPath, string fileName, string fileType, string newContent) {
      if (exists(folderPath, fileName, fileType, false)) {
        try {
          List<string> temp = getContentAsList(folderPath, fileName, fileType);
          temp.Add(newContent);
          return setContent(folderPath, fileName, fileType, temp);
        }
        catch (Exception e) {
          string errMsg = ("ERROR@FileManager.addLineTo: Could not add line to file (" + fileName + "." + fileType + " in " + folderPath + ")");
          Console.WriteLine(errMsg);
          Console.WriteLine(e.ToString());
          throw new Exception(errMsg + "\r\n  -> Exception: " + e.ToString());
        }
      }
      throw new Exception("ERROR@FileManager.addLineTo: file does not exist (" + fileName + "." + fileType + " in " + folderPath + ")");
    }
    public static bool addLineToWithCreate(string folderPath, string fileName, string fileType, string newContent) {
      if (!exists(folderPath, fileName, fileType, false)) {
        try {
          createFile(folderPath, fileName, fileType);
        }
        catch (Exception e) {
          string errMsg = "ERROR@FileManager.addLineToWithCreate: Could not create the file (" + fileName + "." + fileType + " in " + folderPath + ")";
          Console.WriteLine(errMsg);
          Console.WriteLine(e.ToString());
          throw new Exception(errMsg + "\r\n  -> Exception: " + e.ToString());
        }
      }
      try {
        return addLineTo(folderPath, fileName, fileType, newContent);
      }
      catch (Exception e) {
        string errMsg = ("ERROR@TxtMan-addLineTo: Could not add line to file (" + fileName + " in " + folderPath + ")");
        Console.WriteLine(errMsg);
        Console.WriteLine(e.ToString());
        throw new Exception(errMsg + "\r\n  -> Exception: " + e.ToString());
      }
    }
    public static bool addContentTo(string folderPath, string fileName, string fileType, List<string> newContent) {
      try {
        createFile(folderPath, fileName, fileType, false);
        //List<string> oldContent = getContentAsList(folderPath, fileName, fileType);
        //oldContent.AddRange(newContent);
        //setContent(folderPath, fileName, fileType, oldContent);
        var utf8WithoutBOM = new System.Text.UTF8Encoding(false);
        System.IO.File.AppendAllLines(folderPath + "\\" + fileName + "." + fileType, newContent, utf8WithoutBOM);
        return true;
      }
      catch (Exception e) {
        string errMsg = ("ERROR@FileManager.addContentTo: Couldnt add new content to file (" + fileName + "." + fileType + " in " + folderPath + ")");
        Console.WriteLine(errMsg);
        Console.WriteLine(e.ToString());
        Console.WriteLine("");
        throw new Exception(errMsg + "; " + e.ToString());
      }
    }
    public static bool removeLine(string folderPath, string fileName, string fileType, int lineIndex) {
      List<string> list = getContentAsList(folderPath, fileName, fileType);
      string errMsg = "";
      if (list != null) {
        try {
          list.Remove(list[lineIndex]);
          return setContent(folderPath, fileName, fileType, list);
        }
        catch (Exception e) {
          errMsg = ("ERROR@FileManager.removeLine: Could not remove line in file (" + fileName + "." + fileType + " in " + folderPath + ")");
          Console.WriteLine(errMsg);
          Console.WriteLine(e.ToString());
          throw new Exception(errMsg + "\r\n  -> Exception: " + e.ToString());
        }
      }
      errMsg = ("ERROR@FileManager.removeLine: Content is null. (getLine(" + folderPath + "/" + fileName + "." + fileType + ")");
      Console.WriteLine(errMsg);
      throw new Exception(errMsg);
    }
    public static bool renameFile(string folderPath, string fileName, string fileType, string newfileName) {
      return renameFile(folderPath, fileName, fileType, newfileName, false);
    }
    public static bool renameFile(string folderPath, string fileName, string fileType, string newfileName, bool ignoreException) {
      if (exists(folderPath, fileName, fileType, false)) {
        try {
          List<string> list = getContentAsList(folderPath, fileName, fileType);
          deleteFile(folderPath, fileName, fileType);
          return writeNewTxt(folderPath, newfileName, fileType, list);
        }
        catch (Exception e) {
          string errMsg = ("ERROR@FileManager.renameFile: Could not rename file (" + fileName + "." + fileType + " in " + folderPath + ")");
          Console.WriteLine(errMsg);
          Console.WriteLine(e.ToString());
          throw new Exception(errMsg + "\r\n  -> Exception: " + e.ToString());
        }
      }
      if (!ignoreException) {
        throw new Exception("ERROR@FileManager.renameFile: file does not exist (" + fileName + "." + fileType + " in " + folderPath + ")");
      }
      return false;
    }
    public static string[] splitFilePath(string filePath) {
      string[] ret = new string[3];
      try {
        string[] tmp = filePath.Split('.');
        ret[2] = tmp[tmp.Length - 1];
        string tf = filePath.Substring(0, filePath.Length - ret[2].Length);
        tmp = tf.Split('\\');
        ret[1] = tmp[tmp.Length - 1];
        ret[1] = ret[1].Substring(0, ret[1].Length - 1);
        ret[0] = tf.Substring(0, tf.Length - (ret[1].Length + 1));
        while (ret[0][ret[0].Length - 1] == '\\' || ret[0][ret[0].Length - 1] == '/') {
          ret[0] = ret[0].Substring(0, ret[0].Length - 1);
        }
        return ret;
      }
      catch (Exception e) {
        throw new Exception("Der FilePath konnte nicht aufgeteilt werden. Filepath:<" + filePath + ">.", e);
      }
    }
    public static string getFolderPath(string filePath) {
      return splitFilePath(filePath)[0];
    }
    public static string getFileName(string filePath) {
      return splitFilePath(filePath)[1];
    }
    public static string getFileType(string filePath) {
      return splitFilePath(filePath)[2];
    }
    public static List<string> getFilesInFolder(string folderPath) {
      List<string> ret = new List<string>();
      DirectoryInfo di = new DirectoryInfo(folderPath);
      foreach (FileInfo file in di.EnumerateFiles()) {
        ret.Add(file.Name);
      }
      return ret;
    }

  }

}
