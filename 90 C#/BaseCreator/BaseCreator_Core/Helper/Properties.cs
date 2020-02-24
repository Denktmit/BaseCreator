using System;
using System.Linq;
using System.Collections.Generic;

namespace BaseCreator_Core.Helper {

  public class Properties {

    #region fields and properties
    private static string _standardpath = Environment.CurrentDirectory + "\\properties";
    private string _FilePath;
    private List<string> _comments;
    private Dictionary<string, string> _list;

    public static string StandardPath { get { return _standardpath; } }
    public string FilePath { get { return _FilePath; } set { _FilePath = value; } }
    public List<string> Comments { get { return _comments; } set { _comments = value; } }
    public Dictionary<string, string> List { get { return _list; } set { _list = value; } }
    #endregion fields and properties

    #region constructors
    public Properties(string propertiesName, bool useStandardPath) {
      if (useStandardPath) {
        FilePath = StandardPath + "\\" + propertiesName + ".properties";
      }
      else {
        FilePath = propertiesName;
      }
      Comments = new List<string>();
      if (!Init()) {
        string className = "Properties";
        string methodName = "Constructor";
        string errMsg = "Error at constructor of <" + className + ">. File <" + FilePath + "> does not exist.";
        throw ErrorHandler.GetErrorException(ErrorType.FileNotFound, className, methodName, errMsg, "PROP01", true);
      }
    }
    public Properties(string inFilePath) : this(inFilePath, false) { }
    public Properties(string inFolderPath, string propertiesName) : this(inFolderPath + "\\" + propertiesName + ".properties", false) { }
    #endregion constructors

    #region Init-Methods
    public bool Init() {
      Reload();
      return true;
    }
    #endregion Init-Methods

    #region Database-Methods
    #endregion Database-Methods

    #region Class-Methods
    public void Reload() {
      Reload(this.FilePath);
    }
    public void Reload(string FilePath) {
      this.FilePath = FilePath;
      List = new Dictionary<string, string>();
      if (System.IO.File.Exists(FilePath)) {
        LoadFromFile(FilePath);
      }
      else {
        System.IO.File.Create(FilePath);
      }
    }
    public void Reload(string inFolderPath, string inFileName) {
      Reload(inFolderPath + "\\" + inFileName + ".properties");
    }
    public string Get(string field, string defValue) {
      return (Get(field) == null) ? (defValue) : (Get(field));
    }
    public string Get(string field) {
      if (List.ContainsKey(field)) {
        return List[field];
      }
      else {
        List.Add(field, "0");
        Save();
        return null;
      }
    }
    public void Set(string field, object value) {
      Set(field, value, false);
    }
    public void Set(string field, object value, bool saveAfterwards) {
      if (!List.ContainsKey(field)) {
        List.Add(field, value.ToString());
      }
      else {
        List[field] = value.ToString();
      }
      if (saveAfterwards) {
        Save();
      }
    }
    public void Save() {
      Save(this.FilePath);
    }
    public void Save(string FilePath) {
      this.FilePath = FilePath;
      if (!System.IO.File.Exists(FilePath)) {
        System.IO.File.Create(FilePath);
      }
      System.IO.StreamWriter file = new System.IO.StreamWriter(FilePath);
      foreach (string com in Comments) {
        file.WriteLine(com);
      }
      foreach (string prop in List.Keys.ToArray()) {
        if (!string.IsNullOrWhiteSpace(List[prop])) {
          file.WriteLine(prop + "=" + List[prop]);
        }
      }
      file.Close();
    }
    public void Save(string inFolderPath, string inFileName) {
      Save(inFolderPath + "\\" + inFileName + ".properties");
    }
    private void LoadFromFile(string file) {
      foreach (string line in System.IO.File.ReadAllLines(file)) {
        string key = "nA";
        string value = "nA";
        // check for comments
        if ((!string.IsNullOrEmpty(line)) &&
            (!line.StartsWith(";")) &&
            (!line.StartsWith("#")) &&
            (!line.StartsWith("'")) &&
            (line.Contains('='))) {
          int index = line.IndexOf('=');
          key = line.Substring(0, index).Trim();
          value = line.Substring(index + 1).Trim();
          // remove quotations at start and end
          if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
              (value.StartsWith("'") && value.EndsWith("'"))) {
            value = value.Substring(1, value.Length - 2);
          }
          try {
            //ignore dublicates
            List.Add(key, value);
          }
          catch (Exception e) {
            string className = "Properties";
            string methodName = "LoadFromFile";
            string errMsg = "Could not load property (Key:<" + key + ">, Value:<" + value + ">) from file <" + FilePath + ">.";
            throw ErrorHandler.GetErrorException(ErrorType.GetterError, className, methodName, errMsg, "PROP03", true);
          }
        }
        else {
          Comments.Add(line);
        }
      }
    }
    public override string ToString() {
      return "Properties <" + FilePath + ">";
    }
    #endregion Class-Methods

    #region Helper-Methods
    #endregion Helper-Methods

  }

}
