using BaseCreator_Core.Helper;
using System;
using System.Collections.Generic;
using System.Security;

namespace AP_Extension.AP {

  public class Creator_CS {

    #region fields
    private AutoCreator _ac;
    private List<string> _classes;
    #endregion fields

    #region properties
    public AutoCreator AC { get { return _ac; } set { _ac = value; } }
    public List<string> Classes { get { return _classes; } set { _classes = value; } }
    #endregion properties

    #region konstruktor
    public Creator_CS(AutoCreator inAC) {
      AC = inAC;
      Classes = new List<string>();
    }
    #endregion konstruktor

    #region class_methods
    public void run() {
      Console.WriteLine("--- Creator_CS ---");
      CreateAPClass();
      CreateSysClass();
      CreateDataCore();
      CreateDBObjectRetriever();
      CreateDBObjectCreator();
      CreateDBObjectDestroyer();
      CreateDBObjectUpdater();
      CreateDBListRetriever();
      CreateAllCSClasses();
      CreateNeededFiles();
    }
    #endregion class_methods

    #region APClass
    private void CreateAPClass() {
      Console.WriteLine("  -  CreateAPClass()");
      string filePath = AC.APPath + "\\90_C#\\Aktivenplaner\\AP.Classes";
      string fileName = "APClass";
      string fileExt = "cs";
      FileWriter fw = new FileWriter(filePath, fileName, fileExt);

      fw.Altf("using System;");
      fw.Altf("using System.ComponentModel;");
      fw.Altf("using System.Runtime.CompilerServices;");
      fw.Altf("using AP.Utils;");
      fw.Altf("using AP.Utils.Database;");
      fw.Altf("");
      fw.Altf("namespace AP.Classes {");
      fw.Altf("");
      fw.Altf("  public interface APClass {");
      fw.Altf("");
      fw.Altf("    int Id { get; set; }");
      fw.Altf("    string ToShortenedString();");
      fw.Altf("    string GetInsertString();");
      fw.Altf("    DBSPContent GetInsertDBSPContent(SysUser inUser);");
      fw.Altf("    DBSPContent GetUpdateDBSPContent(SysUser inUser);");
      fw.Altf("    DBSPContent GetDeleteDBSPContent(SysUser inUser, int deleteMode);");
      fw.Altf("    DBResult SaveToDB(DBConnection connection, SysUser inUser);");
      fw.Altf("");
      fw.Altf("  }");
      fw.Altf("");
      fw.Altf("}");
      fw.Altf("");

      fw.CreateFile();
      Classes.Add(fileName);
    }
    private void CreateSysClass() {
      Console.WriteLine("  -  CreateSysClass()");
      string filePath = AC.APPath + "\\90_C#\\Aktivenplaner\\AP.Classes";
      string fileName = "SysClass";
      string fileExt = "cs";
      FileWriter fw = new FileWriter(filePath, fileName, fileExt);

      fw.Altf("using System;");
      fw.Altf("using System.ComponentModel;");
      fw.Altf("using System.Runtime.CompilerServices;");
      fw.Altf("using AP.Utils;");
      fw.Altf("");
      fw.Altf("namespace AP.Classes {");
      fw.Altf("");
      fw.Altf("  public interface SysClass {");
      fw.Altf("");
      fw.Altf("    string ToShortenedString();");
      fw.Altf("    string GetInsertString();");
      fw.Altf("");
      fw.Altf("  }");
      fw.Altf("");
      fw.Altf("}");
      fw.Altf("");

      fw.CreateFile();
      Classes.Add(fileName);
    }
    #endregion APClass

    #region create_CS_classes
    private void CreateAllCSClasses() {
      Console.WriteLine("  -  CreateAllCSClasses()");
      string filePath = AC.APPath + "\\90_C#\\Aktivenplaner\\AP.Classes";
      string fileName = "Single_APClass";
      string fileExt = "cs";
      FileWriter fw = null;
      foreach (CSV_Tabelle c in AC.DbTables) {
        fileName = c.DataName;
        Console.WriteLine("    -> " + fileName);
        fw = new FileWriter(filePath + (c.DataName.StartsWith("Sys") ? "\\SysClasses" : "\\APClasses"), fileName, fileExt);

        fw.Altf("using System;");
        fw.Altf("using AP.Utils;");
        fw.Altf("using AP.Utils.Database;");
        fw.Altf("using System.ComponentModel;");
        fw.Altf("using System.Runtime.CompilerServices;");
        fw.Altf("using System.Collections.Generic;");
        fw.Altf("");
        fw.Altf("namespace AP.Classes {");
        fw.Altf("");
        fw.Altf("  public class " + c.DataName + " : INotifyPropertyChanged, APClass {");
        fw.Altf("");
        fw.Altf("    #region fields and properties");
        CreateAllCSClasses_FieldsProps(fw, c);
        fw.Altf("    #endregion fields and properties");
        fw.Altf("");
        fw.Altf("    #region Constructors");
        CreateAllCSClasses_Constructors(fw, c);
        fw.Altf("    #endregion Constructors");
        fw.Altf("");
        fw.Altf("    #region ClassMethods");
        CreateAllCSClasses_ClassMethods(fw, c);
        fw.Altf("    #endregion ClassMethods");
        fw.Altf("");
        fw.Altf("    #region DB_Methods");
        CreateAllCSClasses_GetInsertDBSPContent(fw, c);
        CreateAllCSClasses_GetUpdateDBSPContent(fw, c);
        CreateAllCSClasses_GetDeleteDBSPContent(fw, c);
        CreateAllCSClasses_GetInsertString(fw, c);
        CreateAllCSClasses_SaveToDB(fw, c);
        fw.Altf("    #endregion DB_Methods");
        fw.Altf("");
        fw.Altf("    #region PropertyChanged");
        CreateAllCSClasses_PropertyChanged(fw, c);
        fw.Altf("    #endregion PropertyChanged");
        fw.Altf("");
        fw.Altf("  }");
        fw.Altf("");
        fw.Altf("}");
        fw.Altf("");

        fw.CreateFile();
        Classes.Add(fileName);
      }
    }
    private void CreateAllCSClasses_FieldsProps(FileWriter fw, CSV_Tabelle c) {
      string swb = "TODO";
      fw.Altf("    public  static readonly string table  = \"" + c.TableName + "\";");
      fw.Altf("    public  static readonly string view  = \"" + c.ViewName + "\";");
      fw.Altf("    private static          int    maxID  = 0;");
      fw.Altf("    private                 bool   _dirty = false;");
      for (int i = 0; i < c.Columns.Count; i++) {
        fw.Altf("    private " + GetAPType(c, i) + " _" + c.Columns[i].Attribut + ";");
      }
      if (c.HasSysTextReference) {
        fw.Altf("    private List<SysLanguageText> _iDescriptions;");
      }
      fw.Altf("");
      fw.Altf("    public bool Dirty { get { return _dirty; } set { _dirty = value; OnPropertyChanged(); } }");
      for (int i = 0; i < c.Columns.Count; i++) {
        swb = AC.GetStringWithoutBeginning(c.Columns[i].Attribut);
        fw.Altf("    public " + GetAPType(c, i) + " " + swb + " { get { return _" + c.Columns[i].Attribut + "; } set { if(_" + c.Columns[i].Attribut + "!=value){_" + c.Columns[i].Attribut + " = value; OnPropertyChanged(); Dirty = true;} } }");
      }
      if (c.HasSysTextReference) {
        fw.Altf("    public List<SysLanguageText> Descriptions { get { return _iDescriptions; } set { _iDescriptions = value; OnPropertyChanged(); } }");
      }
    }
    private void CreateAllCSClasses_Constructors(FileWriter fw, CSV_Tabelle c) {
      string sysTextAttribut = "TODO";
      string swb = "TODO";
      string temp = "TODO";
      // alle attribute
      fw.Altf("    // all attributes");
      temp = "    public " + c.DataName + "(";
      for (int i = 0; i < c.Columns.Count; i++) {
        temp += GetAPType(c, i) + " in" + c.Columns[i].Attribut;
        if (i < c.Columns.Count - 1) {
          temp += ", ";
        }
      }
      temp += ") {";
      fw.Altf(temp);
      temp = "";
      swb = "";
      for (int i = 0; i < c.Columns.Count; i++) {
        if (c.Columns[i].Art == "SysText") {
          sysTextAttribut = "in" + c.Columns[i].Attribut;
        }
        swb = AC.GetStringWithoutBeginning(c.Columns[i].Attribut);
        fw.Altf("      " + swb + " = in" + c.Columns[i].Attribut + ";");
      }
      if (c.HasSysTextReference) {
        fw.Altf("      Descriptions = new List<SysLanguageText>();");
      }
      fw.Altf("      " + "if(in" + c.Columns[0].Attribut + " > maxID) {");
      fw.Altf("        " + "maxID = in" + c.Columns[0].Attribut + ";");
      fw.Altf("      " + "}");
      fw.Altf("      Dirty = false;");
      fw.Altf("    }");
      // alle attribute, auto ID
      fw.Altf("    // all attributes with auto Id");
      temp = "    public " + c.DataName + "(";
      for (int i = 1; i < c.Columns.Count; i++) {
        temp += GetAPType(c, i) + " in" + c.Columns[i].Attribut;
        if (i < c.Columns.Count - 1) {
          temp += ", ";
        }
      }
      temp += ") {";
      fw.Altf(temp);
      fw.Altf("      // maxID++;");
      fw.Altf("      // " + AC.GetStringWithoutBeginning(c.Columns[0].Attribut) + " = maxID;");
      fw.Altf("      " + AC.GetStringWithoutBeginning(c.Columns[0].Attribut) + " = -1;");
      temp = "";
      swb = "";
      for (int i = 1; i < c.Columns.Count; i++) {
        if (c.Columns[i].Art == "SysText") {
          sysTextAttribut = "in" + c.Columns[i].Attribut;
        }
        swb = AC.GetStringWithoutBeginning(c.Columns[i].Attribut);
        fw.Altf("      " + swb + " = in" + c.Columns[i].Attribut + ";");
      }
      if (c.HasSysTextReference) {
        fw.Altf("      Descriptions = new List<SysLanguageText>();");
      }
      fw.Altf("      Dirty = true;");
      fw.Altf("    }");
      // nur not-null-Uebergabeparameter, auto-ID
      fw.Altf("    // only not null attributes with auto Id");
      bool allNN = true;
      for (int i = 1; i < c.Columns.Count; i++) {
        if (!c.Columns[i].NotNull) {
          allNN = false;
        }
      }
      if (!allNN) {
        temp = "    public " + c.DataName + "(";
        for (int i = 1; i < c.Columns.Count; i++) {
          if (c.Columns[i].NotNull) {
            temp += GetAPType(c, i) + " in" + c.Columns[i].Attribut;
            if (i < c.Columns.Count - 1) {
              temp += ", ";
            }
          }
        }
        if (temp.EndsWith(", ")) {
          temp = temp.Substring(0, temp.Length - 2);
        }
        temp += ") {";
        fw.Altf(temp);
        fw.Altf("      // maxID++;");
        fw.Altf("      // " + AC.GetStringWithoutBeginning(c.Columns[0].Attribut) + " = maxID;");
        fw.Altf("      " + AC.GetStringWithoutBeginning(c.Columns[0].Attribut) + " = -1;");
        temp = "";
        swb = "";
        for (int i = 1; i < c.Columns.Count; i++) {
          if (c.Columns[i].Art == "SysText") {
            sysTextAttribut = "in" + c.Columns[i].Attribut;
          }
          if (c.Columns[i].NotNull) {
            swb = AC.GetStringWithoutBeginning(c.Columns[i].Attribut);
            fw.Altf("      " + swb + " = in" + c.Columns[i].Attribut + ";");
          }
        }
        if (c.HasSysTextReference) {
          fw.Altf("      Descriptions = new List<SysLanguageText>();");
        }
        fw.Altf("      Dirty = true;");
        fw.Altf("    }");
      }
      // keine Uebergabeparameter, default ausgefuellt und auto ID
      fw.Altf("    // no attributes, default values, auto Id");
      fw.Altf("    private " + c.DataName + "() {");
      fw.Altf("      // maxID++;");
      fw.Altf("      // " + AC.GetStringWithoutBeginning(c.Columns[0].Attribut) + " = maxID;");
      fw.Altf("      " + AC.GetStringWithoutBeginning(c.Columns[0].Attribut) + " = -1;");
      swb = "";
      for (int i = 0; i < c.Columns.Count; i++) {
        swb = "      " + AC.GetStringWithoutBeginning(c.Columns[i].Attribut) + " = ";
        if (c.Columns[i].DefaultVal.Length > 0) {
          if (c.Columns[i].Art != "int") {
            if (c.Columns[i].Art == "nvarchar") {
              swb += "\"";
            }
            if (c.Columns[i].Art == "datetime" || c.Columns[i].Art == "date") {
              if (c.Columns[i].DefaultVal == "NOW") {
                swb += "DateTime.Now";
              }
              else {
                swb += "new DateTime(" + AC.GetDateTimeString(c.Columns[i].DefaultVal) + ")";
              }
            }
            else {
              if (c.Columns[i].Art == "bool") {
                if (c.Columns[i].DefaultVal == "0") {
                  swb += "false";
                }
                else {
                  swb += "true";
                }
              }
              else {
                swb += c.Columns[i].DefaultVal;
              }
            }
            if (c.Columns[i].Art == "nvarchar") {
              swb += "\"";
            }
            swb += ";";
          }
          else {



            if (c.Columns[i].ForeignKeyTo == "Enum") {
              //swb += GetDataTypeForInt(e, i) + ".0;";
              swb += "new Enum();";
            }
            else if (c.Columns[i].ForeignKeyTo == "") {
              swb += c.Columns[i].DefaultVal + ";";
            }
            else {
              //string dtfi = "";
              swb += "new " + GetDataTypeForInt(c, i) + "();";
            }



          }
        }
        else {
          swb = "      // " + c.Columns[i].Attribut;
        }
        if (c.Columns[i].Attribut.StartsWith("e") || c.Columns[i].Attribut.StartsWith("i")) {
          swb = "      // " + c.Columns[i].Attribut;
        }
        fw.Altf(swb);
      }
      if (c.HasSysTextReference) {
        fw.Altf("      Descriptions = new List<SysLanguageText>();");
      }
      fw.Altf("      Dirty = true;");
      fw.Altf("    }");
    }
    private void CreateAllCSClasses_ClassMethods(FileWriter fw, CSV_Tabelle c) {
      string swb = "TODO";
      string temp = "TODO";
      fw.Altf("    public override string ToString() {");
      fw.Altf("      string s = \"[\";");
      for (int i = 0; i < c.Columns.Count; i++) {
        swb = AC.GetStringWithoutBeginning(c.Columns[i].Attribut);
        if (c.Columns[i].Attribut.StartsWith("i")) {
          temp = "      s += (" + swb + "!=null)?(" + swb + ".ToShortenedString()):(\"NULL\")";
        }
        else if (c.Columns[i].Attribut.StartsWith("e")) {
          temp = "      s += (" + swb + "!=null)?(" + swb + ".Description.Description):(\"NULL\")";
        }
        else {
          temp = "      s += " + swb;
        }
        if (i < c.Columns.Count - 1) {
          temp += " + \",\";";
        }
        else {
          temp += " + \"]\";";
        }
        fw.Altf(temp);
      }
      fw.Altf("      return s;");
      fw.Altf("    }");
      fw.Altf("    public string ToShortenedString() {");
      fw.Altf("      string s = \"[\";");
      temp = "";
      bool anotherOne = false;
      for (int i = 0; i < c.Columns.Count; i++) {
        bool forShortString = c.Columns[i].MakeUnique.Length > 0;
        if (anotherOne && forShortString) {
          temp += " + \",\";";
          fw.Altf(temp);
        }
        swb = AC.GetStringWithoutBeginning(c.Columns[i].Attribut);
        if (forShortString) {
          if (c.Columns[i].Attribut.StartsWith("i")) {
            temp = "      s += (" + swb + "!=null)?(" + swb + ".ToShortenedString()):(\"NULL\")";
          }
          else if (c.Columns[i].Attribut.StartsWith("e")) {
            temp = "      s += (" + swb + "!=null)?(" + swb + ".Description.Description):(\"NULL\")";
          }
          else {
            temp = "      s += " + swb;
          }
          anotherOne = true;
        }
      }
      fw.Altf(temp + ";");
      fw.Altf("      s += \"]\";");
      fw.Altf("      return s;");
      fw.Altf("    }");
      if (c.HasSysTextReference) {
        fw.Altf("    public SysLanguageText GetDescriptionText(SysLanguage inLanguage) {");
        fw.Altf("      foreach(SysLanguageText slt in Descriptions) {");
        fw.Altf("        if(slt.Language == inLanguage) {");
        fw.Altf("          return slt;");
        fw.Altf("        }");
        fw.Altf("      }");
        fw.Altf("      return null;");
        fw.Altf("    }");
      }
    }
    private void CreateAllCSClasses_GetInsertDBSPContent(FileWriter fw, CSV_Tabelle c) {
      string temp = "TODO";
      string spName = (c.IsSys ? "spSys" : "spAP") + "Insert" + c.DataName;
      fw.Altf("    public DBSPContent GetInsertDBSPContent(SysUser inUser) {");
      fw.Altf("      DBSPContent cont = new DBSPContent(\"" + spName + "\");");
      fw.Altf("      cont.AddParameter(\"@in_nCallerId\", inUser.Id, DBSPDirection.In, DBSPType.Int32);");
      if (!c.DataName.StartsWith("Sys")) {
        fw.Altf("      cont.AddParameter(\"@in_eDatenstatus\", Datenstatus==null?1:Datenstatus.Id, DBSPDirection.In, DBSPType.Int32);");
      }
      for (int i = c.DataName.StartsWith("Sys") ? 1 : 3; i < c.Columns.Count; i++) {
        string id = "";
        if (c.Columns[i].Attribut.StartsWith("i") || c.Columns[i].Attribut.StartsWith("e")) {
          fw.Altf("      " + "if(" + AC.GetStringWithoutBeginning(c.Columns[i].Attribut) + " == null) {");
          if (c.Columns[i].Attribut.StartsWith("i")) {
            id = "." + ((c.Columns[i].Art.StartsWith("Sys")) ? "" : c.Columns[i].Art) + "Id";
          }
          else if (c.Columns[i].Attribut.StartsWith("e")) {
            id = "." + "Id";
          }
          temp = "cont.AddParameter(\"@in_" + c.Columns[i].Attribut + "\", null, DBSPDirection.In, DBSPType.";
          temp += GetDBSPType(c.Columns[i].Art);
          temp += ");";
          fw.Altf("        " + temp);
          fw.Altf("      " + "} else {");
          if (c.Columns[i].Attribut.StartsWith("i")) {
            id = "." + ((c.Columns[i].Art.StartsWith("Sys")) ? "" : "") + "Id";
          }
          else if (c.Columns[i].Attribut.StartsWith("e")) {
            id = "." + "Id";
          }
          temp = "cont.AddParameter(\"@in_" + c.Columns[i].Attribut + "\", " + AC.GetStringWithoutBeginning(c.Columns[i].Attribut) + id + ", DBSPDirection.In, DBSPType.";
          temp += GetDBSPType(c.Columns[i].Art);
          temp += ");";
          fw.Altf("        " + temp);
          fw.Altf("      " + "}");
        }
        else if (c.Columns[i].Art == "datetime" || c.Columns[i].Art == "date" || c.Columns[i].Art == "time") {
          fw.Altf("      " + "if(" + AC.GetStringWithoutBeginning(c.Columns[i].Attribut) + " == new DateTime()) {");
          fw.Altf("        cont.AddParameter(\"@in_" + c.Columns[i].Attribut + "\", null, DBSPDirection.In, DBSPType." + GetDBSPType(c.Columns[i].Art) + ");");
          fw.Altf("      " + "} else {");
          fw.Altf("        cont.AddParameter(\"@in_" + c.Columns[i].Attribut + "\", " + AC.GetStringWithoutBeginning(c.Columns[i].Attribut) + id + ", DBSPDirection.In, DBSPType." + GetDBSPType(c.Columns[i].Art) + ");");
          fw.Altf("      " + "}");
        }
        else {
          temp = "cont.AddParameter(\"@in_" + c.Columns[i].Attribut + "\", " + AC.GetStringWithoutBeginning(c.Columns[i].Attribut) + ", DBSPDirection.In, DBSPType.";
          temp += GetDBSPType(c.Columns[i].Art);
          temp += ");";
          fw.Altf("      " + temp);
        }
      }
      fw.Altf("      cont.AddParameter(\"@out_" + c.Columns[0].Attribut + "\", 0, DBSPDirection.Out, DBSPType.Int32);");
      fw.Altf("      cont.AppendHandlingParameters();");
      fw.Altf("      return cont;");
      fw.Altf("    }");
    }
    private void CreateAllCSClasses_GetUpdateDBSPContent(FileWriter fw, CSV_Tabelle c) {
      string temp = "TODO";
      string spName = (c.IsSys ? "spSys" : "spAP") + "Update" + c.DataName;
      fw.Altf("    public DBSPContent GetUpdateDBSPContent(SysUser inUser) {");
      fw.Altf("      DBSPContent cont = new DBSPContent(\"" + spName + "\");");
      fw.Altf("      cont.AddParameter(\"@in_nCallerId\", inUser.Id, DBSPDirection.In, DBSPType.Int32);");
      fw.Altf("      cont.AddParameter(\"@in_" + c.Columns[0].Attribut + "\", this." + c.Columns[0].AttributWOB + ", DBSPDirection.In, DBSPType.Int32);");
      if (!c.DataName.StartsWith("Sys")) {
        fw.Altf("      cont.AddParameter(\"@in_eDatenstatus\", Datenstatus==null?1:Datenstatus.Id, DBSPDirection.In, DBSPType.Int32);");
      }
      for (int i = c.DataName.StartsWith("Sys") ? 1 : 3; i < c.Columns.Count; i++) {
        string id = "";
        if (c.Columns[i].Attribut.StartsWith("i") || c.Columns[i].Attribut.StartsWith("e")) {
          fw.Altf("      " + "if(" + AC.GetStringWithoutBeginning(c.Columns[i].Attribut) + " == null) {");
          if (c.Columns[i].Attribut.StartsWith("i")) {
            id = "." + ((c.Columns[i].Art.StartsWith("Sys")) ? "" : c.Columns[i].Art) + "Id";
          }
          else if (c.Columns[i].Attribut.StartsWith("e")) {
            id = "." + "Id";
          }
          temp = "cont.AddParameter(\"@in_" + c.Columns[i].Attribut + "\", null, DBSPDirection.In, DBSPType.";
          temp += GetDBSPType(c.Columns[i].Art);
          temp += ");";
          fw.Altf("        " + temp);
          fw.Altf("      " + "} else {");
          if (c.Columns[i].Attribut.StartsWith("i")) {
            id = "." + ((c.Columns[i].Art.StartsWith("Sys")) ? "" : "") + "Id";
          }
          else if (c.Columns[i].Attribut.StartsWith("e")) {
            id = "." + "Id";
          }
          temp = "cont.AddParameter(\"@in_" + c.Columns[i].Attribut + "\", " + AC.GetStringWithoutBeginning(c.Columns[i].Attribut) + id + ", DBSPDirection.In, DBSPType.";
          temp += GetDBSPType(c.Columns[i].Art);
          temp += ");";
          fw.Altf("        " + temp);
          fw.Altf("      " + "}");
        }
        else if (c.Columns[i].Art == "datetime" || c.Columns[i].Art == "date" || c.Columns[i].Art == "time") {
          fw.Altf("      " + "if(" + AC.GetStringWithoutBeginning(c.Columns[i].Attribut) + " == new DateTime()) {");
          fw.Altf("        cont.AddParameter(\"@in_" + c.Columns[i].Attribut + "\", null, DBSPDirection.In, DBSPType." + GetDBSPType(c.Columns[i].Art) + ");");
          fw.Altf("      " + "} else {");
          fw.Altf("        cont.AddParameter(\"@in_" + c.Columns[i].Attribut + "\", " + AC.GetStringWithoutBeginning(c.Columns[i].Attribut) + id + ", DBSPDirection.In, DBSPType." + GetDBSPType(c.Columns[i].Art) + ");");
          fw.Altf("      " + "}");
        }
        else {
          temp = "cont.AddParameter(\"@in_" + c.Columns[i].Attribut + "\", " + AC.GetStringWithoutBeginning(c.Columns[i].Attribut) + ", DBSPDirection.In, DBSPType.";
          temp += GetDBSPType(c.Columns[i].Art);
          temp += ");";
          fw.Altf("      " + temp);
        }
      }
      fw.Altf("      cont.AddParameter(\"@out_" + c.Columns[0].Attribut + "\", 0, DBSPDirection.Out, DBSPType.Int32);");
      fw.Altf("      cont.AppendHandlingParameters();");
      fw.Altf("      return cont;");
      fw.Altf("    }");
    }
    private void CreateAllCSClasses_GetDeleteDBSPContent(FileWriter fw, CSV_Tabelle c) {
      string spName = (c.IsSys ? "spSys" : "spAP") + "Delete" + c.DataName;
      fw.Altf("    public DBSPContent GetDeleteDBSPContent(SysUser inUser, int deleteMode) {");
      fw.Altf("      DBSPContent cont = new DBSPContent(\"" + spName + "\");");
      fw.Altf("      cont.AddParameter(\"@in_nCallerId\", inUser.Id, DBSPDirection.In, DBSPType.Int32);");
      fw.Altf("      cont.AddParameter(\"@in_" + c.Columns[0].Attribut + "\", this." + c.Columns[0].AttributWOB + ", DBSPDirection.In, DBSPType.Int32);");
      fw.Altf("      cont.AddParameter(\"@in_nDeleteMode\", deleteMode, DBSPDirection.In, DBSPType.Int32);");
      fw.Altf("      cont.AddParameter(\"@out_" + c.Columns[0].Attribut + "\", 0, DBSPDirection.Out, DBSPType.Int32);");
      fw.Altf("      cont.AppendHandlingParameters();");
      fw.Altf("      return cont;");
      fw.Altf("    }");
    }
    private void CreateAllCSClasses_GetInsertString(FileWriter fw, CSV_Tabelle c) {
      fw.Altf("    public string GetInsertString() {");
      fw.Altf("      string ret = \"INSERT INTO \" + table + \" VALUES (\";");
      bool first = true;
      // tmp -> mit oder ohne komma vor der aufzaehlung
      string tmp = "";
      string s = "";
      foreach (CSV_Spalte col in c.Columns) {
        if (first) {
          tmp = "";
          first = false;
        }
        else {
          tmp = ", ";
        }
        // nvcad = wenn noetig -> '
        string nvcad = "";
        if (col.Art == "nvarchar" || col.Art == "datetime" || col.Art == "date" || col.Art == "time") {
          nvcad = "'";
        }
        string dat = AC.GetStringWithoutBeginning(col.Attribut);
        string dada = AC.GetStringWithoutBeginning(col.Attribut);
        if (col.Attribut.StartsWith("i") || col.Attribut.StartsWith("e")) {
          dat += ".Id";
        }
        string s123 = AC.GetStringWithoutBeginning(col.Attribut);
        if (!col.NotNull) {
          s123 = "((DateTime)" + s123 + ")";
        }
        if (col.Art == "datetime") {
          dat = s123 + ".ToString(\"yyyy-MM-dd HH:mm:ss.fff\")";
        }
        if (col.Art == "date") {
          dat = s123 + ".ToString(\"yyyy-MM-dd\")";
        }
        if (col.Art == "time") {
          dat = s123 + ".ToString(\"HH:mm:ss.fff\")";
        }
        // entsch -> kompakter string auf (wenn xy!=null dann xy sonst NULL)
        string entsch = "((" + dada + " != null) ? (" + dat + " +\"\"):(\"NULL\"))";
        if (col.Art == "decimal") {
          entsch += ".Replace(\",\",\".\")";
        }
        s = "      ret += \"" + tmp + nvcad + "\" + " + entsch + " + \"" + nvcad + "\";";
        fw.Altf(s);
      }
      fw.Altf("      ret += \");\";");
      fw.Altf("      return ret;");
      fw.Altf("    }");
    }
    private void CreateAllCSClasses_SaveToDB(FileWriter fw, CSV_Tabelle c) {
      string bedingung = "";
      string sqlQuery = "";
      int colHelper = 0;
      try {
        if (c.TableName == "tblAPTransaktion") {
          Console.WriteLine("transaktion");
        }
        List<List<CSV_Spalte>> GMUC = c.GetMakeUniqueCollections();
        if (GMUC == null || GMUC.Count <= 1) {
          bedingung = "lastId";
        }
        else {
          foreach (CSV_Spalte unis in GMUC[1]) {
            if (colHelper > 0) {
              bedingung += " AND ";
            }
            string posId = "";
            string posApos = "";
            string convertedWOB = unis.AttributWOB;
            if (unis.NeedsQuote()) {
              posApos = "'";
            }
            if (unis.IsClassReference()) {
              posId = ".Id";
            }
            string datetimeformat = "";
            if (unis.Art == "datetime" || unis.Art == "time") {
              datetimeformat = "yyyy-MM-dd HH:mm:ss";
            }
            if (unis.Art == "date") {
              datetimeformat = "yyyy-MM-dd";
            }
            if (unis.Art == "date" || unis.Art == "time" || unis.Art == "datetime") {
              if (unis.NotNull) {
                posId = ".ToString(\"" + datetimeformat + "\")";
              }
              else {
                posId = ".ToString(\"" + datetimeformat + "\")";
                convertedWOB = "((DateTime)" + unis.AttributWOB + ")";
              }
            }
            if (unis.Art == "decimal") {
              posId = ".ToString().Replace(',', '.')";
            }
            bedingung += unis.Attribut + " \"+((" + unis.AttributWOB + "!=null)?(\"=" + posApos + "\"+" + convertedWOB + posId + "+\"" + posApos + "\"):(\"IS NULL\"))+\"";
            colHelper++;
          }
        }
        if (bedingung == "lastId") {
          sqlQuery = "SELECT nId FROM " + c.TableName + " ORDER BY nId DESC LIMIT 1;";
        }
        else {
          sqlQuery = "SELECT nId FROM " + c.TableName + " WHERE " + bedingung + ";";
        }
      }
      catch (Exception e) {
        throw e;
      }
      string lastChangeUpdate = "";
      foreach (CSV_Spalte spalte in c.Columns) {
        if (spalte.Attribut == "dtLastChange") {
          lastChangeUpdate = "LastChange = DateTime.Now;";
        }
      }
      //Console.WriteLine("Bedingung: where " + bedingung);
      fw.Altf("    public DBResult SaveToDB(DBConnection connection, SysUser inUser){");
      fw.Altf("      DBResult result = DBResult.GetDummyDBResult();");
      fw.Altf("      DBResult resultId = DBResult.GetDummyDBResult();");
      fw.Altf("      string query = \"\";");
      fw.Altf("      if(Dirty || Id < 0){");
      fw.Altf("        try {");
      if (lastChangeUpdate != null && lastChangeUpdate.Length > 0) {
        fw.Altf("          " + lastChangeUpdate);
      }
      fw.Altf("          if (Id < 0) {");
      fw.Altf("            result = DBCalls.CallSP(connection, GetInsertDBSPContent(inUser));");
      fw.Altf("            if(result.IsEmpty || !result.Successfull){");
      fw.Altf("              return result;");
      fw.Altf("            }");
      fw.Altf("            query = \"" + sqlQuery + "\";");
      fw.Altf("            resultId = DBCalls.ExecQuery(connection, query);");
      fw.Altf("            int newId = -1;");
      fw.Altf("            if(!resultId.Successfull || result.IsEmpty) {");
      fw.Altf("              return resultId;");
      fw.Altf("            }");
      fw.Altf("            if(!Int32.TryParse(resultId.GetData(0,0), out newId)) {");
      fw.Altf("              return resultId;");
      fw.Altf("            }");
      fw.Altf("            Id = newId;");
      fw.Altf("            Dirty = false;");
      fw.Altf("          }");
      fw.Altf("          else {");
      fw.Altf("            result = DBCalls.CallSP(connection, GetUpdateDBSPContent(inUser));");
      fw.Altf("            if(result.IsEmpty || !result.Successfull){");
      fw.Altf("              return result;");
      fw.Altf("            }");
      fw.Altf("            Dirty = false;");
      fw.Altf("          }");
      fw.Altf("          return result;");
      fw.Altf("        } catch (Exception e) {");
      fw.Altf("          throw ErrorHandler.GetErrorException(e, ErrorType.DatabaseError, \"" + c.DataName + "\", \"SaveToDB\"");
      fw.Altf("            , \"Konnte den Datensatz " + c.DataName + " <\"+ToShortenedString()+\"> nicht in die Datenbank speichern.\", \"" + c.DataName.Substring(0, 3) + "-01\", true);");
      fw.Altf("        }");
      fw.Altf("      }");
      fw.Altf("      return result;");
      fw.Altf("    }");
    }
    private void CreateAllCSClasses_PropertyChanged(FileWriter fw, CSV_Tabelle c) {
      fw.Altf("    public event PropertyChangedEventHandler PropertyChanged;");
      fw.Altf("    private void OnPropertyChanged([CallerMemberName] string property = null) {");
      fw.Altf("      if (PropertyChanged != null)");
      fw.Altf("        PropertyChanged(this, new PropertyChangedEventArgs(property));");
      fw.Altf("    }");
    }
    #endregion create_CS_classes

    #region DataCore
    private void CreateDataCore() {
      Console.WriteLine("  -  CreateDataCore()");
      string filePath = AC.APPath + "\\90_C#\\Aktivenplaner\\AP.Core";
      string fileName = "DataCore";
      string fileExt = "cs";
      FileWriter fw = new FileWriter(filePath, fileName, fileExt);

      fw.Altf("using System;");
      fw.Altf("using System.Configuration;");
      fw.Altf("using System.Collections.Generic;");
      fw.Altf("using AP.Utils;");
      fw.Altf("using AP.Utils.Database;");
      fw.Altf("using AP.Classes;");
      fw.Altf("");
      fw.Altf("namespace AP.Core {");
      fw.Altf("");
      fw.Altf("  public class " + fileName + " {");
      fw.Altf("");
      fw.Altf("    #region fields and properties");
      CreateDataCore_FieldsProps(fw);
      fw.Altf("    #endregion fields and properties");
      fw.Altf("");
      fw.Altf("    #region constructors");
      CreateDataCore_Constructors(fw);
      fw.Altf("    #endregion constructors");
      fw.Altf("");
      fw.Altf("    #region loadData");
      CreateDataCore_LoadListOfPossibleCoreElements(fw);
      CreateDataCore_LoadCoreElement(fw);
      CreateDataCore_LoadAllCoreElements(fw);
      CreateDataCore_LoadAllSysElements(fw);
      fw.Altf("    #endregion loadData");
      fw.Altf("");
      fw.Altf("    #region helper_methods");
      CreateDataCore_PrintCoreElement(fw);
      CreateDataCore_GetCoreElement(fw);
      CreateDataCore_GetClassList(fw);
      CreateDataCore_GetObject(fw);
      CreateDataCore_AddObject(fw);
      CreateDataCore_RemoveObject(fw);
      CreateDataCore_LoadAllDescriptions(fw);
      fw.Altf("    #endregion helper_methods");
      fw.Altf("");
      fw.Altf("    #region Refresh_classes");
      CreateDataCore_RefreshCoreElement(fw);
      CreateDataCore_RefreshClasses(fw);
      fw.Altf("    #endregion Refresh_classes");
      fw.Altf("");
      fw.Altf("  }");
      fw.Altf("");
      fw.Altf("}");
      fw.Altf("");

      fw.CreateFile();
      Classes.Add(fileName);
    }
    private void CreateDataCore_FieldsProps(FileWriter fw) {
      fw.Altf("    private static DataCore _inst;");
      fw.Altf("    private List<DataCoreElement> _dces = new List<DataCoreElement>();");
      fw.Altf("    private List<string> _pdce = new List<string>();");
      fw.Altf("    private int _debug = 3; // 0->no debug, 1->all consoleWrites, 2->all methods, 3->top methods, 4->to be decided");
      fw.Altf("");
      fw.Altf("    public static DataCore Instance { get { if (_inst == null) { _inst = new DataCore(false); } return _inst; } private set { _inst = value; } }");
      fw.Altf("    public List<DataCoreElement> CoreElements { get { return _dces; } set { _dces = value; } }");
      fw.Altf("    public List<string> PossibleDataCoreElements { get { return _pdce; } set { _pdce = value; } }");
      fw.Altf("    public int Debug { get { return _debug; } set { _debug = value; } }");
    }
    private void CreateDataCore_Constructors(FileWriter fw) {
      fw.Altf("    private DataCore(bool loadAllData) {");
      fw.Altf("      if(0<Debug&&Debug<=3) { Console.WriteLine(\" + Create DataCore(\"+loadAllData+\")\"); }");
      fw.Altf("      LoadListOfPossibleCoreElements();");
      fw.Altf("      if (loadAllData) {");
      fw.Altf("        LoadAllCoreElements();");
      fw.Altf("      }");
      fw.Altf("      else {");
      fw.Altf("        LoadAllSysElements();");
      fw.Altf("      }");
      fw.Altf("    }");
    }
    private void CreateDataCore_LoadListOfPossibleCoreElements(FileWriter fw) {
      fw.Altf("    public void LoadListOfPossibleCoreElements() {");
      fw.Altf("      if(0<Debug&&Debug<=3) { Console.WriteLine(\" + DataCore.LoadListOfPossibleCoreElements()\"); }");
      fw.Altf("      List<string> pces = FileManager.getContentAsList(Environment.CurrentDirectory + \"\\\\bin\", \"pce\", \"txt\");");
      fw.Altf("      PossibleDataCoreElements = new List<string>();");
      fw.Altf("      for(int i=0; i<pces.Count; i++) {");
      fw.Altf("        if (i >= 6) {");
      fw.Altf("          PossibleDataCoreElements.Add(pces[i]);");
      fw.Altf("        }");
      fw.Altf("      }");
      fw.Altf("      try {");
      fw.Altf("        //PossibleDataCoreElements.Sort((x, y) => Int32.Parse(x.Substring(0, 3)).CompareTo(Int32.Parse(y.Substring(0, 3))));");
      fw.Altf("        List<string> tmp = new List<string>();");
      fw.Altf("        for(int i=0; i<1000; i++) {");
      fw.Altf("          foreach(string s in PossibleDataCoreElements) {");
      fw.Altf("            if (!String.IsNullOrWhiteSpace(s) && Int32.Parse(s.Substring(0, 3)) == i) {");
      fw.Altf("              tmp.Add(s);");
      fw.Altf("            }");
      fw.Altf("          }");
      fw.Altf("        }");
      fw.Altf("        PossibleDataCoreElements = tmp;");
      fw.Altf("      } catch(Exception e) {");
      fw.Altf("        throw ErrorHandler.GetErrorException(e, ErrorType.SystemException, \"DataCore\", \"LoadListOfPossibleCoreElements\"");
      fw.Altf("            , \"Die geladenen Daten konnten nicht sortiert werden.\", \"180\", true);");
      fw.Altf("      }");
      fw.Altf("      for(int i=0; i<PossibleDataCoreElements.Count; i++) {");
      fw.Altf("        PossibleDataCoreElements[i] = PossibleDataCoreElements[i].Substring(3);");
      fw.Altf("      }");
      fw.Altf("      foreach (string s in PossibleDataCoreElements) {");
      fw.Altf("        if (Debug==1){");
      fw.Altf("          Console.WriteLine(s);");
      fw.Altf("        }");
      fw.Altf("        CoreElements.Add(new DataCoreElement(s));");
      fw.Altf("      }");
      fw.Altf("    }");
    }
    private void CreateDataCore_LoadCoreElement(FileWriter fw) {
      string sysTextReference = "TODO";
      fw.Altf("    public void LoadCoreElement(DataCoreElement inDce, bool loadDescs) {");
      fw.Altf("      if(0<Debug&&Debug<=3) { Console.WriteLine(\" + DataCore.LoadCoreElement(\"+inDce.ElementName+\")\"); }");
      fw.Altf("      DBResult loaded = null;");
      fw.Altf("      List<APClass> list = new List<APClass>();");
      fw.Altf("      string errMsg = \"\";");
      fw.Altf("      switch(inDce.ElementName){");
      fw.Altf("        // start: TODO: implement new classes here");
      fw.Altf("        ");
      fw.Altf("        /*case \"NewClass\":");
      fw.Altf("          if(Debug==1){ Console.WriteLine(\"  - Load NewClass from database.\"); }");
      fw.Altf("            TODO");
      fw.Altf("            break; */");
      fw.Altf("        ");
      fw.Altf("        // end: TODO: implement new classes here");
      int helplinkext = 111;
      foreach (var c in AC.DbTables) {
        fw.Altf("        case \"" + c.DataName + "\":");
        fw.Altf("          if(Debug==1){ Console.WriteLine(\"  - Load " + c.DataName + " from database.\"); }");
        // fw.Altf("//V2          loaded = DBCalls.ExecQuery(ServerManager.Connection, \"select i"+c.DataName+" from " + c.ViewName + ";\");");
        fw.Altf("          loaded = DBCalls.ExecQuery(ServerManager.Connection, \"select * from " + c.ViewName + "_full;\");");
        fw.Altf("          list = inDce.Elements;");
        fw.Altf("          foreach (var r in loaded.Result) {");
        // fw.Altf("//V2            int tmpId = 0;");
        // fw.Altf("//V2            if(Int32.TryParse(r[0], out tmpId)) {");
        // fw.Altf("//V2              ObjectRetriever.Load" + c.DataName + "_ById(tmpId);");
        // fw.Altf("//V2            }");
        fw.Altf("            try {");
        fw.Altf("              errMsg = \"\";");
        int idx = 0;
        string st = "";   // temp-string to print
        string t = "";    // temp-type-string
        foreach (var col in c.Columns) {
          fw.Altf("              errMsg += \"" + col.Attribut + " = \" + ((r[" + idx + "]==null||r[" + idx + "]==\"null\"||r[" + idx + "]==\"NULL\"||r[" + idx + "]==\"\")?\"null\":r[" + idx + "]) + \", \";");
          idx++;
        }
        idx = 0;
        foreach (var col in c.Columns) {
          t = GetCSType(col.Art, col.NotNull);
          st = "" + t + " i_" + col.Attribut + " = ";
          switch (t) {
            // not-null values
            case "int":
              st += "Int32.Parse(r[" + idx + "]);";
              break;
            case "string":
              st += "r[" + idx + "];";
              break;
            case "DateTime":
              st += "DateTime.Parse(r[" + idx + "]);";
              break;
            case "double":
              st += "Double.Parse(r[" + idx + "]);";
              break;
            case "bool":
              st += "Convert.ToBoolean(r[" + idx + "]);";
              break;
            // possible null values
            case "int?":
              st += "(r[" + idx + "]==null||r[" + idx + "]==\"null\"||r[" + idx + "]==\"NULL\"||r[" + idx + "]==\"\")?(int?)null:(int?)Int32.Parse(r[" + idx + "]);";
              break;
            case "DateTime?":
              st += "(r[" + idx + "]==null||r[" + idx + "]==\"null\"||r[" + idx + "]==\"NULL\"||r[" + idx + "]==\"\")?(DateTime?)null:(DateTime?)DateTime.Parse(r[" + idx + "]);";
              break;
            case "double?":
              st += "(r[" + idx + "]==null||r[" + idx + "]==\"null\"||r[" + idx + "]==\"NULL\"||r[" + idx + "]==\"\")?(double?)null:(double?)Double.Parse(r[" + idx + "]);";
              break;
            case "bool?":
              st += "(r[" + idx + "]==null||r[" + idx + "]==\"null\"||r[" + idx + "]==\"NULL\"||r[" + idx + "]==\"\")?(bool?)null:(bool?)Convert.ToBoolean(r[" + idx + "]);";
              break;
            default:
              throw ErrorHandler.GetErrorException(ErrorType.AutoCreateError, "Creator_CS", "CreateDataCore", "Error at c-sharp-type", "190", true);
          }
          fw.Altf("              " + st);
          idx++;
        }
        st = c.DataName + " tmpObject = new " + c.DataName + "(";
        idx = 0;
        foreach (var col in c.Columns) {
          if (col.Art == "SysText") {
            sysTextReference = col.AttributWOB;
          }
          if (idx == 0) {
            st += "i_" + col.Attribut;
          }
          else {
            if (col.Attribut.StartsWith("i")) {
              st += ", (" + col.Art + ")GetObject(\"" + col.Art + "\", i_" + col.Attribut + ")";
            }
            else if (col.Attribut.StartsWith("e")) {
              st += ", (SysEnumElement)GetObject(\"SysEnumElement\", i_" + col.Attribut + ")";
            }
            else {
              st += ", i_" + col.Attribut;
            }
          }
          idx++;
        }
        st += ");";
        fw.Altf("              " + st);
        fw.Altf("              list.Add(tmpObject);");
        if (c.HasSysTextReference) {
          fw.Altf("              if(loadDescs) tmpObject.Descriptions = LoadAllDescriptions(tmpObject." + sysTextReference + ");");
        }
        fw.Altf("            } catch (Exception e) {");
        fw.Altf("              ErrorHandler.CreateErrorException(e, ErrorType.SystemException, \"DataCore\", \"LoadCoreElement\", \"Exception at loading " + c.DataName + "(\" + errMsg + \")\", \"" + helplinkext + "\");");
        fw.Altf("            }");
        fw.Altf("          }");
        if (c.ReferencesItself) {
          fw.Altf("          Refresh" + c.DataName + "();");
        }
        fw.Altf("          break;");
        helplinkext++;
      }
      fw.Altf("        default:");
      fw.Altf("          throw ErrorHandler.GetErrorException(ErrorType.InputError, \"DataCore\", \"LoadCoreElement\", \"Exception at loading \" + inDce.ElementName + \"(\" + errMsg + \")\", \"210\", true);");
      fw.Altf("      }");
      fw.Altf("    }");
    }
    private void CreateDataCore_LoadAllCoreElements(FileWriter fw) {
      fw.Altf("    private void LoadAllCoreElements() {");
      fw.Altf("      if(0<Debug&&Debug<=3) { Console.WriteLine(\" + DataCore.LoadAllCoreElements()\"); }");
      fw.Altf("      CoreElements = new List<DataCoreElement>();");
      fw.Altf("      DataCoreElement tmp = null;");
      fw.Altf("      foreach(string s in PossibleDataCoreElements) {");
      fw.Altf("        tmp = new DataCoreElement(s);");
      fw.Altf("        CoreElements.Add(tmp);");
      fw.Altf("      }");
      fw.Altf("      foreach(DataCoreElement dce in CoreElements) {");
      fw.Altf("        LoadCoreElement(dce, false);");
      fw.Altf("      }");
      fw.Altf("    }");
    }
    private void CreateDataCore_LoadAllSysElements(FileWriter fw) {
      fw.Altf("    private void LoadAllSysElements() {");
      fw.Altf("      if(0<Debug&&Debug<=3) { Console.WriteLine(\" + DataCore.LoadAllSysElements()\"); }");
      fw.Altf("      CoreElements = new List<DataCoreElement>();");
      fw.Altf("      DataCoreElement tmp = null;");
      fw.Altf("      foreach(string s in PossibleDataCoreElements) {");
      fw.Altf("        tmp = new DataCoreElement(s);");
      fw.Altf("        CoreElements.Add(tmp);");
      fw.Altf("      }");
      fw.Altf("      foreach (DataCoreElement dce in CoreElements) {");
      fw.Altf("        if (dce.ElementName.StartsWith(\"Sys\") && dce.ElementName!=\"SysHistory\") {");
      fw.Altf("          LoadCoreElement(dce, false);");
      fw.Altf("        }");
      fw.Altf("      }");
      fw.Altf("    }");
    }
    private void CreateDataCore_PrintCoreElement(FileWriter fw) {
      fw.Altf("    public void PrintCoreElement(DataCoreElement inDce) {");
      fw.Altf("      if(0<Debug&&Debug<=2) { Console.WriteLine(\" + DataCore.PrintCoreElement(\"+inDce.ElementName+\")\"); }");
      fw.Altf("      foreach(APClass apc in inDce.Elements) {");
      fw.Altf("        Console.WriteLine(\"  - \" + apc.ToString());");
      fw.Altf("      }");
      fw.Altf("    }");
    }
    private void CreateDataCore_GetCoreElement(FileWriter fw) {
      fw.Altf("    public DataCoreElement GetCoreElement(string inClass) {");
      fw.Altf("      if(0<Debug&&Debug<=2) { Console.WriteLine(\" + DataCore.GetCoreElement(\"+inClass+\")\"); }");
      fw.Altf("      foreach (var c in CoreElements) {");
      fw.Altf("        if (c.ElementName == inClass) {");
      fw.Altf("          return c;");
      fw.Altf("        }");
      fw.Altf("      }");
      fw.Altf("      throw ErrorHandler.GetErrorException(ErrorType.InputError, \"DataCore\", \"GetCoreElement\" , \"Das CoreElement der Klasse <\" + inClass + \"> konnte nicht gefunden werden.\", \"211\", true);");
      fw.Altf("    }");
    }
    private void CreateDataCore_GetClassList(FileWriter fw) {
      fw.Altf("    public List<APClass> GetClassList(string inClass) {");
      fw.Altf("      if(0<Debug&&Debug<=2) { Console.WriteLine(\" + DataCore.GetClassList(\"+inClass+\")\"); }");
      fw.Altf("      return GetCoreElement(inClass).Elements;");
      fw.Altf("    }");
    }
    private void CreateDataCore_GetObject(FileWriter fw) {
      fw.Altf("    public APClass GetObject(string inClass, int? inId){");
      fw.Altf("      if(0<Debug&&Debug<=2) { Console.WriteLine(\" + DataCore.GetObject(\"+inClass+\", \"+inId+\")\"); }");
      fw.Altf("      int tmp = -1;");
      fw.Altf("      if(inId!=null){");
      fw.Altf("        tmp = (int)inId;");
      fw.Altf("      }");
      fw.Altf("      return GetObject(inClass, tmp);");
      fw.Altf("    }");
      fw.Altf("    public APClass GetObject(string inClass, int inId){");
      fw.Altf("      try {");
      fw.Altf("        List<APClass> tmp = GetClassList(inClass);");
      fw.Altf("        switch (inClass) {");
      fw.Altf("        // start: TODO: implement new classes here");
      fw.Altf("        ");
      fw.Altf("        /*case \"NewClass\":");
      fw.Altf("            foreach(NewClass nc in tmp) {");
      fw.Altf("              if(nc.Id == inId) {");
      fw.Altf("                return nc;");
      fw.Altf("              }");
      fw.Altf("            }");
      fw.Altf("            break; */");
      fw.Altf("        ");
      fw.Altf("        // end: TODO: implement new classes here");
      foreach (var c in AC.DbTables) {
        fw.Altf("          case \"" + c.DataName + "\":");
        fw.Altf("            foreach (" + c.DataName + " o in tmp) {");
        fw.Altf("              if(o." + AC.GetStringWithoutBeginning(c.Columns[0].Attribut) + " == inId) {");
        fw.Altf("                return o;");
        fw.Altf("              }");
        fw.Altf("            }");
        fw.Altf("            break;");
      }
      fw.Altf("          default:");
      fw.Altf("            throw ErrorHandler.GetErrorException(ErrorType.InputError, \"DataCore\", \"GetObject\", \"Die uebergebene Klasse < \" + inClass + \" > existiert nicht.\", \"212\", true);");
      fw.Altf("        }");
      fw.Altf("      } catch (Exception e) {");
      fw.Altf("        ErrorHandler.CreateErrorException(e, ErrorType.SystemException, \"DataCore\", \"GetObject\", \"Objekt mit der ID \" + inId + \" der Klasse \" + inClass + \" konnte nicht geladen werden.\", \"213\");");
      fw.Altf("        //throw ErrorHandler.GetException(\"DataCore\", \"GetObject(...)\", \"Objekt mit der ID \" + inId + \" der Klasse \" + inClass + \" konnte nicht geladen werden.\", e);");
      fw.Altf("        return null;");
      fw.Altf("      }");
      fw.Altf("      return null;");
      fw.Altf("    }");
    }
    private void CreateDataCore_AddObject(FileWriter fw) {
      fw.Altf("    public bool AddObject(APClass inObject) {");
      fw.Altf("      List<APClass> loadedList = GetClassList(inObject.GetType().Name);");
      fw.Altf("      foreach(APClass apc in loadedList) {");
      fw.Altf("        if(apc.Id == inObject.Id) {");
      fw.Altf("          return false;");
      fw.Altf("        }");
      fw.Altf("      }");
      fw.Altf("      loadedList.Add(inObject);");
      fw.Altf("      return true;");
      fw.Altf("    }");
    }
    private void CreateDataCore_LoadAllDescriptions(FileWriter fw) {
      fw.Altf("    public List<SysLanguageText> LoadAllDescriptions(SysText inSysText) {");
      fw.Altf("      throw new NotImplementedException();");
      fw.Altf("      //List<SysLanguageText> ret = new List<SysLanguageText>();");
      fw.Altf("      //foreach(SysLanguageText slt in GetClassList(\"SysLanguageText\")) {");
      fw.Altf("      //  if (slt.Description == inSysText) {");
      fw.Altf("      //    ret.Add(slt);");
      fw.Altf("      //  }");
      fw.Altf("      //}");
      fw.Altf("      //return ret;");
      fw.Altf("    }");
    }
    private void CreateDataCore_RefreshCoreElement(FileWriter fw) {
      fw.Altf("    public bool RefreshCoreElement(string inClass){");
      fw.Altf("      return RefreshCoreElement(GetCoreElement(inClass));");
      fw.Altf("    }");
      fw.Altf("    public bool RefreshCoreElement(DataCoreElement inDce){");
      fw.Altf("      try {");
      fw.Altf("        inDce.Elements.Clear();");
      fw.Altf("        LoadCoreElement(inDce, true);");
      fw.Altf("      } catch (Exception ex) {");
      fw.Altf("        throw ErrorHandler.GetErrorException(ex, ErrorType.LoadError, \"DataCore\", \"RefreshCoreElement\"");
      fw.Altf("          , \"Das CoreElement konnte nicht neu geladen werden.\", \"2113\", true);");
      fw.Altf("      }");
      fw.Altf("      return true;");
      fw.Altf("    }");
    }
    private void CreateDataCore_RefreshClasses(FileWriter fw) {
      foreach (CSV_Tabelle table in AC.DbTables) {
        if (table.ReferencesItself) {
          int helper = 0;
          string query = "SELECT i" + table.DataName;
          foreach (CSV_Spalte column in table.Columns) {
            if (column.Art == table.DataName) {
              query += ", " + column.Attribut;
              helper++;
            }
          }
          query += " FROM " + table.ViewName + ";";
          fw.Altf("    private void Refresh" + table.DataName + "() {");
          fw.Altf("      if (0 < Debug && Debug <= 2) { Console.WriteLine(\" + DataCore.Refresh" + table.DataName + "()\"); }");
          fw.Altf("      DBResult res = DBCalls.ExecQuery(ServerManager.Connection, \"" + query + "\");");
          fw.Altf("      foreach (List<string> row in res.Result) {");
          fw.Altf("        int id = -1;");
          fw.Altf("        id = Int32.Parse(row[0]);");
          fw.Altf("        " + table.DataName + " loaded = (" + table.DataName + ")GetObject(\"" + table.DataName + "\", id);");
          fw.Altf("        if (loaded == null) {");
          fw.Altf("          continue;");
          fw.Altf("        }");
          helper = 1;
          foreach (CSV_Spalte column in table.Columns) {
            if (column.Art == table.DataName) {
              fw.Altf("        // " + column.AttributWOB);
              fw.Altf("        if (loaded." + column.AttributWOB + " == null && row[" + helper + "] != \"null\" && row[" + helper + "] != \"NULL\" && row[" + helper + "] != \"\") {");
              fw.Altf("          int refId = Int32.Parse(row[" + helper + "]);");
              fw.Altf("          loaded." + column.AttributWOB + " = (" + table.DataName + ")GetObject(\"" + table.DataName + "\", refId);");
              fw.Altf("        }");
              helper++;
            }
          }
          fw.Altf("      }");
          fw.Altf("    }");
        }
      }
    }
    private void CreateDataCore_RemoveObject(FileWriter fw) {
      fw.Altf("    public bool RemoveObject(APClass inObject) {");
      fw.Altf("      List<APClass> loadedList = GetClassList(inObject.GetType().Name);");
      fw.Altf("      foreach (APClass apc in loadedList) {");
      fw.Altf("        if (apc == inObject) {");
      fw.Altf("          loadedList.Remove(apc);");
      fw.Altf("          return true;");
      fw.Altf("        }");
      fw.Altf("        if (apc.Id == inObject.Id) {");
      fw.Altf("          loadedList.Remove(apc);");
      fw.Altf("          return true;");
      fw.Altf("        }");
      fw.Altf("      }");
      fw.Altf("      return false;");
      fw.Altf("    }");
    }
    #endregion DataCore

    #region DBObjectRetriever
    private void CreateDBObjectRetriever() {
      Console.WriteLine("  -  CreateDBObjectRetriever()");
      string filePath = AC.APPath + "\\90_C#\\Aktivenplaner\\AP.Core";
      string fileName = "ObjectRetriever";
      string fileExt = "cs";
      FileWriter fw = new FileWriter(filePath, fileName, fileExt);

      fw.Altf("using System;");
      fw.Altf("using System.Collections.Generic;");
      fw.Altf("using AP.Classes;");
      fw.Altf("using AP.Utils;");
      fw.Altf("using AP.Utils.Database;");
      fw.Altf("");
      fw.Altf("namespace AP.Core {");
      fw.Altf("");
      fw.Altf("  public static class ObjectRetriever {");
      fw.Altf("");
      fw.Altf("    /* Start: TODO: Implement new classes <XYZ> here");
      fw.Altf("    ");
      fw.Altf("    public static XYZ DB_LoadXYZ_ById(int inId);");
      fw.Altf("    public static XYZ DC_LoadXYZ_ById(int inId);");
      fw.Altf("    public static XYZ LoadXYZ_ById(int inId);");
      fw.Altf("    public static XYZ LoadXYZ_ById(int? inId);");
      fw.Altf("    -- and this for each column that makes the class unique");
      fw.Altf("    ");
      fw.Altf("    End: TODO: Implement new classes <XYZ> here */");
      fw.Altf("    ");
      fw.Altf("    ");
      int helplinkext = 0;
      foreach (var table in AC.DbTables) {
        List<List<CSV_Spalte>> mucols = table.GetMakeUniqueCollections();   // MakeUnique-Collections
        fw.Altf("    #region Load" + table.DataName);
        foreach (List<CSV_Spalte> mus in mucols) {
          string byIdent = "";
          foreach (CSV_Spalte s in mus) {
            byIdent += s.AttributWOB;
          }
          fw.Altf("    #region Load" + table.DataName + "By_" + byIdent);
          CDBOR_DBMethod(fw, table, byIdent, mus, helplinkext);         // Write the DB-Method that loads the dataset from the database
          helplinkext++;
          CDBOR_DCMethod(fw, table, byIdent, mus, helplinkext);         // Write the DC-Method that loads the dataset from the dataCore
          CDBOR_MainMethod(fw, table, byIdent, mus, helplinkext);       // Write the Main-Method that loads the dataset
          helplinkext++;
          CDBOR_MainMethodNull(fw, table, byIdent, mus, helplinkext);       // Write the Main-Method that loads the dataset with possible null-value
          helplinkext++;
          fw.Altf("    #endregion Load" + table.DataName + "By_" + byIdent);
        }
        fw.Altf("    #endregion Load" + table.DataName);
        fw.Altf("    ");
      }
      fw.Altf("    #region LoadAllDescriptions");
      CDBOR_LoadAllDescriptions(fw);
      fw.Altf("    #endregion LoadAllDescriptions");
      fw.Altf("    ");
      fw.Altf("  }");
      fw.Altf("}");

      fw.CreateFile();
      Classes.Add(fileName);
      Console.WriteLine(" -> Done");
    }
    private void CDBOR_DBMethod(FileWriter fw, CSV_Tabelle table, string byIdent, List<CSV_Spalte> mus, int helplinkext) {
      int colonHelper = 0;
      string sysTextReference = "TODO";
      // method header
      string tmp = "    public static " + table.DataName + " DB_Load" + table.DataName + "_By" + byIdent + "(";
      // zB: ...int inId, string inVorname...
      string errorHelper = "";
      foreach (CSV_Spalte col in mus) {
        if (colonHelper > 0) {
          tmp += ", ";
          errorHelper += "+\",\"+";
        }
        tmp += col.CSArt + " in" + col.AttributWOB;
        errorHelper += "in" + col.AttributWOB;
        colonHelper++;
      }
      tmp += ") {";
      fw.Altf(tmp);
      fw.Altf("      string db = ServerManager.Connection.Database;");
      colonHelper = 0;
      foreach (CSV_Spalte col in mus) {
        if (col.IsClassReference()) {
          fw.Altf("      string q" + col.AttributWOB + " = \"" + col.Attribut + " \" + ((in" + col.AttributWOB + "==null)?(\" IS NULL\"):(\"= \"+in" + col.AttributWOB + ".Id.ToString()));");
        }
        else {
          if (col.Art == "decimal") {
            fw.Altf("      string q" + col.AttributWOB + " = \"" + col.Attribut + " = \"+ in" + col.AttributWOB + ".ToString().Replace(',', '.');");
          }
          else if (col.NeedsQuote()) {
            bool datetimePosNull = false;
            string datatmp = "";
            if (col.IsDateDateTimeTime()) {
              if (col.NotNull) {
                datatmp = col.AttributWOB + ".ToString(\"yyyy-MM-dd HH:mm:ss\")";
              }
              else {
                datetimePosNull = true;
                datatmp = col.AttributWOB + ").ToString(\"yyyy-MM-dd HH:mm:ss\")";
              }
            }
            else {
              datatmp = col.AttributWOB + ".ToString()";
            }
            fw.Altf("      string q" + col.AttributWOB + " = " + (datetimePosNull ? ("in" + col.AttributWOB + "!=null?(") : "") + "\"" + col.Attribut + " = '\"+ " + (datetimePosNull ? "((DateTime)" : "") + "in" + datatmp + " + \"'\"" + (datetimePosNull ? "):\"1=1\"" : "") + ";");
          }
          else {
            if (col.Attribut == "nId") {
              fw.Altf("      string q" + col.AttributWOB + " = \"i" + table.DataName + " = \"+ in" + col.AttributWOB + ".ToString();");
            }
            else {
              fw.Altf("      string q" + col.AttributWOB + " = \"" + col.Attribut + " = \"+ in" + col.AttributWOB + ".ToString();");
            }
          }
        }
      }
      // query
      tmp = "      string query = \"SELECT * FROM \" + db + \"." + table.ViewName + "_full WHERE ";
      // zB: ...nId="+inId+" AND szVorname='"+inVorname+"'...
      foreach (CSV_Spalte col in mus) {
        if (colonHelper > 0) {
          tmp += " AND ";
        }
        //nullinputstringhelper = "in" + col.AttributWOB + (col.IsClassReference() ? (".Id") : "");
        //nullinputstringhelper = (col.IsClassReference()?"((":"") + "in" + (col.IsClassReference() ? (col.AttributWOB + "==null)?(\"NULL\"):(in"+col.AttributWOB+".Id.ToString()))") : (col.AttributWOB));
        //tmp += col.Attribut + "=" + (col.NeedsQuote()?"'":"") + "\"+"+nullinputstringhelper+"+\""+ (col.NeedsQuote() ? "'" : "");
        tmp += "\"+q" + col.AttributWOB + "+\"";
        colonHelper++;
      }
      tmp += ";\";";
      fw.Altf(tmp);
      fw.Altf("      string errMsg = \"\";");
      fw.Altf("      try {");
      fw.Altf("        DBResult result = DBCalls.ExecQuery(ServerManager.Connection, query);");
      fw.Altf("        if (result.IsEmpty) {");
      fw.Altf("          //throw ErrorHandler.GetErrorException(result.Exception, ErrorType.DataSetNotFound, \"ObjectRetriever\", \"DB_Load" + table.DataName + "_By" + byIdent + "\"");
      fw.Altf("          //  , \"Es konnte kein " + table.DataName + " zur Query geladen werden. Query: < \"+query+\" >.\", \"DB_OR_" + helplinkext + "\", true);");
      fw.Altf("          return null;");
      helplinkext++;
      fw.Altf("        }");
      fw.Altf("        if (result.Count > 1) {");
      fw.Altf("          throw ErrorHandler.GetErrorException(result.Exception, ErrorType.DataSetNotFound, \"ObjectRetriever\", \"DB_Load" + table.DataName + "_By" + byIdent + "\"");
      fw.Altf("            , \"Zu diesen Bedingungen gibt es mehrere Datensätze. Query: < \"+query+\" >.\", \"DB_OR_" + helplinkext + "\", true);");
      helplinkext++;
      fw.Altf("        }");
      fw.Altf("        List<string> r = result.Result[0];");
      int idx = 0;
      string st = "";   // temp-string to print
      string t = "";    // temp-type-string
      foreach (var col in table.Columns) {
        fw.Altf("        errMsg += \"" + col.Attribut + " = \" + ((r[" + idx + "]==null||r[" + idx + "]==\"null\"||r[" + idx + "]==\"NULL\"||r[" + idx + "]==\"\")?\"null\":r[" + idx + "]) + \", \";");
        idx++;
      }
      idx = 0;
      foreach (var col in table.Columns) {
        if (col.Art == "SysText") {
          sysTextReference = col.AttributWOB;
        }
        t = GetCSType(col.Art, col.NotNull);
        st = "" + t + " i_" + col.Attribut + " = ";
        switch (t) {
          // not-null values
          case "int":
            st += "Int32.Parse(r[" + idx + "]);";
            break;
          case "string":
            st += "r[" + idx + "];";
            break;
          case "DateTime":
            st += "DateTime.Parse(r[" + idx + "]);";
            break;
          case "double":
            st += "Double.Parse(r[" + idx + "]);";
            break;
          case "bool":
            st += "Convert.ToBoolean(r[" + idx + "]);";
            break;
          // possible null values
          case "int?":
            st += "(r[" + idx + "]==null||r[" + idx + "]==\"null\"||r[" + idx + "]==\"NULL\"||r[" + idx + "]==\"\")?(int?)null:(int?)Int32.Parse(r[" + idx + "]);";
            break;
          case "DateTime?":
            st += "(r[" + idx + "]==null||r[" + idx + "]==\"null\"||r[" + idx + "]==\"NULL\"||r[" + idx + "]==\"\")?(DateTime?)null:(DateTime?)DateTime.Parse(r[" + idx + "]);";
            break;
          case "double?":
            st += "(r[" + idx + "]==null||r[" + idx + "]==\"null\"||r[" + idx + "]==\"NULL\"||r[" + idx + "]==\"\")?(double?)null:(double?)Double.Parse(r[" + idx + "]);";
            break;
          case "bool?":
            st += "(r[" + idx + "]==null||r[" + idx + "]==\"null\"||r[" + idx + "]==\"NULL\"||r[" + idx + "]==\"\")?(bool?)null:(bool?)Convert.ToBoolean(r[" + idx + "]);";
            break;
          default:
            throw ErrorHandler.GetErrorException(ErrorType.AutoCreateError, "Creator_CS", "CreateDataCore", "Error at c-sharp-type", "190", true);
        }
        fw.Altf("        " + st);
        idx++;
      }
      // zB: Person ret = new Person(...
      st = "        " + table.DataName + " ret = new " + table.DataName + "(";
      idx = 0;
      foreach (var col in table.Columns) {
        if (idx == 0) {
          st += "i_" + col.Attribut;
        }
        else {
          if (col.Attribut.StartsWith("i")) {
            if (col.Art == table.DataName) {
              st += ", DC_Load" + col.Art + "_ById(i_" + col.Attribut + ")";
              //st += ", null";
            }
            else {
              st += ", Load" + col.Art + "_ById(i_" + col.Attribut + ")";
            }
            // old: st += ", (" + col.Art + ")GetObject(\"" + col.Art + "\", i_" + col.Attribut + ")";
          }
          else if (col.Attribut.StartsWith("e")) {
            // old: st += ", (SysEnumElement)GetObject(\"SysEnumElement\", i_" + col.Attribut + ")";
            st += ", LoadSysEnumElement_ById(i_" + col.Attribut + ")";
          }
          else {
            st += ", i_" + col.Attribut;
          }
        }
        idx++;
      }
      st += ");";
      fw.Altf(st);
      if (table.HasSysTextReference && table.TableName != "tblSysLanguageText") {
        fw.Altf("        ret.Descriptions = LoadAllDescriptions(ret." + sysTextReference + ");");
      }
      fw.Altf("        return ret;");
      fw.Altf("      } catch (Exception e) {");
      fw.Altf("        throw ErrorHandler.GetErrorException(e, ErrorType.DatabaseError, \"ObjectRetriever\", \"DB_Load" + table.DataName + "_By" + byIdent + "\"");
      fw.Altf("          , \"Konnte " + table.DataName + " mit Beschreibung <" + byIdent + "=<\"+" + errorHelper + "+\">> nicht aus der Datenbank laden. Gelesenes Ergebnis: <\"+errMsg+\">. Query: <\"+query+\">.\", \"DB_OR_" + helplinkext + "\", true);");
      fw.Altf("      }");
      fw.Altf("    }");
    }
    private void CDBOR_DCMethod(FileWriter fw, CSV_Tabelle table, string byIdent, List<CSV_Spalte> mus, int helplinkext) {
      int colonHelper = 0;
      string errorHelper = "";
      // serve nullable Ids
      if (byIdent == "Id") {
        fw.Altf("    public static " + table.DataName + " DC_Load" + table.DataName + "_ById(int? inId) {");
        fw.Altf("      if(inId!=null) {");
        fw.Altf("        return DC_Load" + table.DataName + "_ById((int)inId);");
        fw.Altf("      }");
        fw.Altf("      return null;");
        fw.Altf("    }");
      }
      colonHelper = 0;
      // method header
      string tmp = "    public static " + table.DataName + " DC_Load" + table.DataName + "_By" + byIdent + "(";
      // zB: ...int inId, string inVorname...
      foreach (CSV_Spalte col in mus) {
        if (colonHelper > 0) {
          tmp += ", ";
          errorHelper += "+\",\"+";
        }
        tmp += col.CSArt + " in" + col.AttributWOB;
        errorHelper += "in" + col.AttributWOB;
        colonHelper++;
      }
      tmp += ") {";
      fw.Altf(tmp);
      colonHelper = 0;
      fw.Altf("      " + table.DataName + " loaded = null;");
      fw.Altf("      " + table.DataName + " ret = null;");
      fw.Altf("      int loadedElements = 0;");
      fw.Altf("      foreach(APClass apc in DataCore.Instance.GetClassList(\"" + table.DataName + "\")) {");
      fw.Altf("        loaded = (" + table.DataName + ")apc;");
      tmp = "        if(";
      foreach (CSV_Spalte col in mus) {
        if (colonHelper > 0) {
          tmp += " && ";
        }
        tmp += "loaded." + col.AttributWOB + "==in" + col.AttributWOB;
        colonHelper++;
      }
      tmp += ") {";
      fw.Altf(tmp);
      colonHelper = 0;
      fw.Altf("          loadedElements++;");
      fw.Altf("          ret = loaded;");
      fw.Altf("        }");
      fw.Altf("      }");
      fw.Altf("      if(loadedElements > 1) {");
      fw.Altf("        throw ErrorHandler.GetErrorException(ErrorType.DBConstraintViolation, \"ObjectRetriever\", \"DC_Load" + table.DataName + "_By" + byIdent + "\"");
      fw.Altf("          , \"Im DataCore sind zu viele Elemente des Typs <" + table.DataName + "> mit der Beschreibung <" + byIdent + "=<\"+" + errorHelper + "+\">>. Dies ist eine Verletzung der Datenbank-Constraints.\", \"DB_OR_" + helplinkext + "\", true);");
      fw.Altf("      }");
      fw.Altf("      return ret;");
      fw.Altf("    }");
    }
    private void CDBOR_MainMethod(FileWriter fw, CSV_Tabelle table, string byIdent, List<CSV_Spalte> mus, int helplinkext) {
      int colonHelper = 0;
      string errorHelper = "";
      // method header
      string tmp = "    public static " + table.DataName + " Load" + table.DataName + "_By" + byIdent + "(";
      // zB: ...int inId, string inVorname...
      foreach (CSV_Spalte col in mus) {
        if (colonHelper > 0) {
          tmp += ", ";
          errorHelper += "+\",\"+";
        }
        tmp += col.CSArt + " in" + col.AttributWOB;
        errorHelper += "in" + col.AttributWOB;
        colonHelper++;
      }
      tmp += ") {";
      fw.Altf(tmp);
      colonHelper = 0;
      fw.Altf("      try {");
      tmp = "        " + table.DataName + " loaded = DC_Load" + table.DataName + "_By" + byIdent + "(";
      foreach (CSV_Spalte col in mus) {
        if (colonHelper > 0) {
          tmp += ", ";
        }
        tmp += "in" + col.AttributWOB;
        colonHelper++;
      }
      tmp += ");";
      fw.Altf(tmp);
      colonHelper = 0;
      fw.Altf("        if(loaded != null) {");
      fw.Altf("          return loaded;");
      fw.Altf("        }");
      tmp = "        loaded = DB_Load" + table.DataName + "_By" + byIdent + "(";
      foreach (CSV_Spalte col in mus) {
        if (colonHelper > 0) {
          tmp += ", ";
        }
        tmp += "in" + col.AttributWOB;
        colonHelper++;
      }
      tmp += ");";
      fw.Altf(tmp);
      fw.Altf("        if(loaded != null) {");
      fw.Altf("          DataCore.Instance.GetClassList(\"" + table.DataName + "\").Add(loaded);");
      fw.Altf("          return loaded;");
      fw.Altf("        }");
      fw.Altf("        return null;");
      fw.Altf("      } catch (Exception e) {");
      fw.Altf("        throw ErrorHandler.GetErrorException(e, ErrorType.DatabaseError, \"ObjectRetriever\", \"Load" + table.DataName + "_By" + byIdent + "\"");
      fw.Altf("          , \"Konnte " + table.DataName + " mit Beschreibung <" + byIdent + "=<\"+" + errorHelper + "+\">> nicht laden.\", \"DB_OR_" + helplinkext + "\", true);");
      fw.Altf("      }");
      fw.Altf("    }");
    }
    private void CDBOR_MainMethodNull(FileWriter fw, CSV_Tabelle table, string byIdent, List<CSV_Spalte> mus, int helplinkext) {
      // check if null is even possible
      bool withnull = false;
      foreach (CSV_Spalte col in mus) {
        if (col.IsSomewhereElseNullable(AC.DbTables)) {
          withnull = true;
        }
      }
      if (!withnull && mus[0].Attribut != "nId") {
        return;
      }
      int colonHelper = 0;
      // method header
      string tmp = "    public static " + table.DataName + " Load" + table.DataName + "_By" + byIdent + "(";
      // zB: ...int inId, string inVorname...
      foreach (CSV_Spalte col in mus) {
        if (colonHelper > 0) {
          tmp += ", ";
        }
        tmp += col.CSArt + ((col.NotNull && col.Attribut != "nId") ? "" : "?") + " in" + col.AttributWOB;
        colonHelper++;
      }
      tmp += ") {";
      fw.Altf(tmp);
      colonHelper = 0;
      tmp = "      if(";
      foreach (CSV_Spalte col in mus) {
        if (colonHelper > 0) {
          tmp += " && ";
        }
        tmp += "in" + col.AttributWOB + "!=null";
        colonHelper++;
      }
      tmp += ") {";
      colonHelper = 0;
      fw.Altf(tmp);
      tmp = "        return Load" + table.DataName + "_By" + byIdent + "(";
      foreach (CSV_Spalte col in mus) {
        if (colonHelper > 0) {
          tmp += ", ";
        }
        tmp += "(" + col.CSArt + ")in" + col.AttributWOB + "";
        colonHelper++;
      }
      tmp += ");";
      colonHelper = 0;
      fw.Altf(tmp);
      fw.Altf("      }");
      fw.Altf("      return null;");
      fw.Altf("    }");
    }
    private void CDBOR_LoadAllDescriptions(FileWriter fw) {
      fw.Altf("    public static List<SysLanguageText> LoadAllDescriptions(SysText inSysText) {");
      fw.Altf("      List<SysLanguageText> ret = new List<SysLanguageText>();");
      fw.Altf("      string errMsg = \"\";");
      fw.Altf("      string query = \"SELECT iSysLanguageText FROM vwSysLanguageText WHERE iDescription = \"+inSysText.Id+\";\";");
      fw.Altf("      try {");
      fw.Altf("        List<string> idstrings = DBCalls.ExecQuery(ServerManager.Connection, query).Result[0];");
      fw.Altf("        int colonHelper = 0;");
      fw.Altf("        foreach(string idstring in idstrings) {");
      fw.Altf("          if (colonHelper > 0) {");
      fw.Altf("            errMsg += \", \";");
      fw.Altf("          }");
      fw.Altf("          errMsg += idstring;");
      fw.Altf("          int id = Int32.Parse(idstring);");
      fw.Altf("          ret.Add(LoadSysLanguageText_ById(id));");
      fw.Altf("        }");
      fw.Altf("        return ret;");
      fw.Altf("      } catch (Exception e) {");
      fw.Altf("        throw ErrorHandler.GetErrorException(e, ErrorType.DatabaseError, \"ObjectRetriever\", \"LoadAllDescriptions\"");
      fw.Altf("          , \"Konnte die Liste der SysLanguageTexts zum SysText <\"+inSysText.ToShortenedString()+\"> nicht aus der Datenbank laden. Geladene Ids: <\" + errMsg + \">\", \"DB_OR_LSLT\", true);");
      fw.Altf("      }");
      fw.Altf("    }");
    }
    #endregion DBObjectRetriever

    #region DBObjectCreator
    private void CreateDBObjectCreator() {
      Console.WriteLine("  -  CreateDBObjectCreator()");
      string filePath = AC.APPath + "\\90_C#\\Aktivenplaner\\AP.Core";
      string fileName = "ObjectCreator";
      string fileExt = "cs";
      FileWriter fw = new FileWriter(filePath, fileName, fileExt);

      fw.Altf("using System;");
      fw.Altf("using AP.Classes;");
      fw.Altf("using AP.Utils;");
      fw.Altf("using AP.Utils.Database;");
      fw.Altf("using AP.Core.Instances;");
      fw.Altf("");
      fw.Altf("namespace AP.Core {");
      fw.Altf("");
      fw.Altf("  public static class ObjectCreator {");
      fw.Altf("");
      fw.Altf("    /* Start: TODO: Implement new classes <XYZ> here");
      fw.Altf("    ");
      fw.Altf("    public static XYZ CreateXYZ(*only NOT NULL columns*);");
      fw.Altf("    public static XYZ CreateXYZ(*all columns*);");
      fw.Altf("    ");
      fw.Altf("    End: TODO: Implement new classes <XYZ> here */");
      fw.Altf("    ");
      fw.Altf("    ");
      int helplinkext = 0;
      foreach (var table in AC.DbTables) {
        fw.Altf("    #region Create" + table.DataName);
        // create copy
        CDBOC_CreateCopy(fw, table, helplinkext);
        helplinkext += 10;
        // create with only NOT-NULLS
        string notnullparams = CDBOC_CreateNN(fw, table, helplinkext);
        helplinkext += 10;
        // create with all variables
        CDBOC_CreateAV(fw, table, helplinkext, notnullparams);
        helplinkext += 10;
        fw.Altf("    #endregion Create" + table.DataName);
        fw.Altf("    ");
      }
      fw.Altf("  }");
      fw.Altf("}");

      fw.CreateFile();
      Classes.Add(fileName);
      Console.WriteLine(" -> Done");
    }
    private void CDBOC_CreateCopy(FileWriter fw, CSV_Tabelle table, int helplinktext) {
      fw.Altf("    // create copy in database");
      fw.Altf("    public static " + table.DataName + " Create" + table.DataName + "(" + table.DataName + " in" + table.DataName + "){");
      fw.Altf("      return Create" + table.DataName + "(");
      int colHelper = 0;
      foreach (CSV_Spalte col in table.Columns) {
        if (col.Attribut != "nId") {
          string r = "        ";
          if (colHelper != 0) {
            r += ", ";
          }
          else {
            r += "  ";
          }
          r += "in" + table.DataName + "." + col.AttributWOB;
          fw.Altf(r);
          colHelper++;
        }
      }
      fw.Altf("      );");
      fw.Altf("    }");
    }
    private string CDBOC_CreateNN(FileWriter fw, CSV_Tabelle table, int helplinkext) {
      if (table.OnlyNotNull()) {
        return "";
      }
      string inVals = "";
      string newVals = "";
      string vals = "";
      string ident = "";
      string makeunis = "";
      string unisIdent = "";
      int colHelper = 0;
      foreach (CSV_Spalte col in table.Columns) {
        if ((col.Attribut != "nId" && col.NotNull) || col.MakeUnique.Length > 0) {
          if (colHelper != 0) {
            inVals += ", ";
            newVals += ", ";
          }
          ident += col.AttributWOB;
          inVals += col.CSArt + " in" + col.AttributWOB;
          newVals += "in" + col.AttributWOB;
          colHelper++;
        }
      }
      List<List<CSV_Spalte>> mucols = table.GetMakeUniqueCollections();   // MakeUnique-Collections
      string byIdent = "";
      colHelper = 0;
      vals = "";
      foreach (List<CSV_Spalte> mus in mucols) {
        foreach (CSV_Spalte s in mus) {
          if (s.Attribut != "nId") {
            byIdent += s.AttributWOB;
            if (colHelper != 0) {
              vals += ", ";
            }
            vals += "in" + s.AttributWOB;
            colHelper++;
          }
        }
      }
      fw.Altf("    // not null columns provided");
      if (mucols == null || mucols.Count <= 1) {
        return "no make uniques";
      }
      fw.Altf("    public static " + table.DataName + " Create" + table.DataName + "(" + inVals + "){");
      fw.Altf("      " + table.DataName + " newObject = null;");
      fw.Altf("      // check if " + table.DataName + " already exists");
      fw.Altf("      try{");
      fw.Altf("        newObject = ObjectRetriever.Load" + table.DataName + "_By" + byIdent + "(" + vals + ");");
      fw.Altf("      }");
      fw.Altf("      catch {");
      fw.Altf("        newObject = null;");
      fw.Altf("      }");
      fw.Altf("      // return if it already exists");
      fw.Altf("      if (newObject != null) {");
      fw.Altf("        return newObject;");
      fw.Altf("      }");
      fw.Altf("      // or try to create new " + table.DataName);
      fw.Altf("      else {");
      fw.Altf("        bool ownException = false;");
      fw.Altf("        try {");
      fw.Altf("          newObject = new " + table.DataName + "(" + newVals + ");");
      fw.Altf("          DBResult result = newObject.SaveToDB(ServerManager.Connection, CurrentUser.Instance.User);");
      fw.Altf("          string sqlRes = \"Error: \" + result.Error + \", ErrorMsg.: \" + result.ErrorMsg;");
      fw.Altf("          if (result.Error == 0) {");
      fw.Altf("            if (DataCore.Instance.AddObject(newObject)) {");
      fw.Altf("              return newObject;");
      fw.Altf("            }");
      fw.Altf("            else {");
      fw.Altf("              ownException = true;");
      fw.Altf("              throw ErrorHandler.GetErrorException(result.Exception, ErrorType.CreationError, \"ObjectCreator\", \"Create" + table.DataName + "\"");
      fw.Altf("            , \"Es konnte kein neues Objekt der Klasse <" + table.DataName + "> registriert werden. (DataCore-Error)\", \"OC-" + helplinkext + "\", true);");
      helplinkext++;
      fw.Altf("            }");
      fw.Altf("          }");
      fw.Altf("          else {");
      fw.Altf("            ownException = true;");
      fw.Altf("            throw ErrorHandler.GetErrorException(result.Exception, ErrorType.CreationError, \"ObjectCreator\", \"Create" + table.DataName + "\"");
      fw.Altf("          , \"Es konnte kein neues Objekt der Klasse <" + table.DataName + "> angelegt werden. DB-Rückmeldung: <\" + sqlRes + \">.\", \"OC-" + helplinkext + "\", true);");
      helplinkext++;
      fw.Altf("          }");
      fw.Altf("        }");
      fw.Altf("        catch (Exception e) {");
      fw.Altf("          string errMsg = \"Es konnte kein neues Objekt der Klasse <" + table.DataName + "> angelegt und registriert werden. (Exception: \"+e.Message+\")\";");
      fw.Altf("          if (ownException) {");
      fw.Altf("            errMsg = e.Message;");
      fw.Altf("          }");
      fw.Altf("          throw ErrorHandler.GetErrorException(e, ErrorType.CreationError, \"ObjectCreator\", \"Create" + table.DataName + "\"");
      fw.Altf("          , errMsg, \"OC-" + helplinkext + "\", true);");
      fw.Altf("        }");
      fw.Altf("      }");
      fw.Altf("    }");
      return inVals;
    }
    private void CDBOC_CreateAV(FileWriter fw, CSV_Tabelle table, int helplinkext, string exisitngParams) {
      string inVals = "";
      string vals = "";
      string ident = "";
      int colHelper = 0;
      foreach (CSV_Spalte col in table.Columns) {
        if (col.Attribut != "nId") {
          if (colHelper != 0) {
            inVals += ", ";
            vals += ", ";
          }
          ident += col.AttributWOB;
          inVals += col.CSArt + " in" + col.AttributWOB;
          vals += "in" + col.AttributWOB;
          colHelper++;
        }
      }
      colHelper = 0;
      string makeunis = "";
      string unisIdent = "";
      string singleMakeUni = "";
      string singleUnisIdent = "";
      string lastMakeUniqueValue = "";
      bool multipleMakeUniques = false;
      foreach (CSV_Spalte col in table.Columns) {
        if (col.Attribut != "nId" && col.MakeUnique.Length > 0) {
          if (colHelper != 0) {
            makeunis += ", ";
          }
          else {
            lastMakeUniqueValue = col.MakeUnique;
          }
          if (col.MakeUnique != lastMakeUniqueValue) {
            multipleMakeUniques = true;
          }
          makeunis += "in" + col.AttributWOB;
          unisIdent += col.AttributWOB;
          singleMakeUni = "in" + col.AttributWOB;
          singleUnisIdent = col.AttributWOB;
          colHelper++;
        }
      }
      if (inVals == exisitngParams) {
        return;
      }
      fw.Altf("    // all columns provided");
      fw.Altf("    public static " + table.DataName + " Create" + table.DataName + "(" + inVals + "){");
      fw.Altf("      " + table.DataName + " newObject = null;");
      fw.Altf("      // check if " + table.DataName + " already exists");
      fw.Altf("      try{");
      fw.Altf("        newObject = ObjectRetriever.Load" + table.DataName + "_By" + (multipleMakeUniques ? singleUnisIdent : unisIdent) + "(" + (multipleMakeUniques ? singleMakeUni : makeunis) + ");");
      fw.Altf("      }");
      fw.Altf("      catch {");
      fw.Altf("        newObject = null;");
      fw.Altf("      }");
      fw.Altf("      // return if it already exists");
      fw.Altf("      if (newObject != null) {");
      fw.Altf("        return newObject;");
      fw.Altf("      }");
      fw.Altf("      // or try to create new " + table.DataName);
      fw.Altf("      else {");
      fw.Altf("        bool ownException = false;");
      fw.Altf("        try {");
      fw.Altf("          newObject = new " + table.DataName + "(" + vals + ");");
      fw.Altf("          DBResult result = newObject.SaveToDB(ServerManager.Connection, CurrentUser.Instance.User);");
      fw.Altf("          string sqlRes = \"Error: \" + result.Error + \", ErrorMsg.: \" + result.ErrorMsg;");
      fw.Altf("          if (result.Error == 0) {");
      fw.Altf("            if (DataCore.Instance.AddObject(newObject)) {");
      fw.Altf("              return newObject;");
      fw.Altf("            }");
      fw.Altf("            else {");
      fw.Altf("              ownException = true;");
      fw.Altf("              throw ErrorHandler.GetErrorException(result.Exception, ErrorType.CreationError, \"ObjectCreator\", \"Create" + table.DataName + "\"");
      fw.Altf("            , \"Es konnte kein neues Objekt der Klasse <" + table.DataName + "> registriert werden. (DataCore-Error)\", \"OC-" + helplinkext + "\", true);");
      helplinkext++;
      fw.Altf("            }");
      fw.Altf("          }");
      fw.Altf("          else {");
      fw.Altf("            ownException = true;");
      fw.Altf("            throw ErrorHandler.GetErrorException(result.Exception, ErrorType.CreationError, \"ObjectCreator\", \"Create" + table.DataName + "\"");
      fw.Altf("          , \"Es konnte kein neues Objekt der Klasse <" + table.DataName + "> angelegt werden. DB-Rückmeldung: <\" + sqlRes + \">.\", \"OC-" + helplinkext + "\", true);");
      helplinkext++;
      fw.Altf("          }");
      fw.Altf("        }");
      fw.Altf("        catch (Exception e) {");
      fw.Altf("          string errMsg = \"Es konnte kein neues Objekt der Klasse <" + table.DataName + "> angelegt und registriert werden. (Exception: \"+e.Message+\")\";");
      fw.Altf("          if (ownException) {");
      fw.Altf("            errMsg = e.Message;");
      fw.Altf("          }");
      fw.Altf("          throw ErrorHandler.GetErrorException(e, ErrorType.CreationError, \"ObjectCreator\", \"Create" + table.DataName + "\"");
      fw.Altf("          , errMsg, \"OC-" + helplinkext + "\", true);");
      fw.Altf("        }");
      fw.Altf("      }");
      fw.Altf("    }");
    }
    #endregion DBObjectCreator

    #region ObjectDestroyer
    private void CreateDBObjectDestroyer() {
      Console.WriteLine("  -  CreateDBObjectDestroyer()");
      string filePath = AC.APPath + "\\90_C#\\Aktivenplaner\\AP.Core";
      string fileName = "ObjectDestroyer";
      string fileExt = "cs";
      FileWriter fw = new FileWriter(filePath, fileName, fileExt);

      fw.Altf("using System;");
      fw.Altf("using AP.Classes;");
      fw.Altf("using AP.Utils;");
      fw.Altf("using AP.Utils.Database;");
      fw.Altf("using AP.Core.Instances;");
      fw.Altf("");
      fw.Altf("namespace AP.Core {");
      fw.Altf("");
      fw.Altf("  public static class ObjectDestroyer {");
      fw.Altf("");
      fw.Altf("    /* Start: TODO: Implement new classes <XYZ> here");
      fw.Altf("    ");
      fw.Altf("    public static XYZ DestroyXYZ(int id);");
      fw.Altf("    ");
      fw.Altf("    End: TODO: Implement new classes <XYZ> here */");
      fw.Altf("    ");
      fw.Altf("    ");
      int helplinkext = 0;
      foreach (var table in AC.DbTables) {
        fw.Altf("    #region Destroy" + table.DataName);
        CDBOD_Destroy(fw, table, helplinkext);
        CDBOD_DestroySave(fw, table, helplinkext);
        CDBOD_DestroyDefault(fw, table, helplinkext);
        CDBOD_DestroyCascading(fw, table, helplinkext);
        helplinkext += 10;
        fw.Altf("    #endregion Destroy" + table.DataName);
        fw.Altf("    ");
      }
      fw.Altf("  }");
      fw.Altf("}");

      fw.CreateFile();
      Classes.Add(fileName);
      Console.WriteLine(" -> Done");
    }
    private void CDBOD_Destroy(FileWriter fw, CSV_Tabelle table, int helplinkext) {
      fw.Altf("    private static bool Destroy" + table.DataName + "(int id, int deleteMode) {");
      fw.Altf("      " + table.DataName + " newObject = null;");
      fw.Altf("      // check if " + table.DataName + " exists");
      fw.Altf("      try {");
      fw.Altf("        newObject = ObjectRetriever.Load" + table.DataName + "_ById(id);");
      fw.Altf("      }");
      fw.Altf("      catch (Exception e) {");
      fw.Altf("        throw ErrorHandler.GetErrorException(e, ErrorType.CreationError, \"ObjectDestroyer\", \"Destroy" + table.DataName + "\"");
      fw.Altf("          , \"Es gab einen Fehler beim Überprüfen, ob es das Objekt der Klasse <" + table.DataName + "> überhaupt gibt.\", \"OD-" + helplinkext + "\", true);");
      helplinkext++;
      fw.Altf("      }");
      fw.Altf("      // return true if it doesnt even exist          ? Oder doch false?");
      fw.Altf("      if (newObject == null) {");
      fw.Altf("        //return true;");
      fw.Altf("        throw ErrorHandler.GetErrorException(ErrorType.CreationError, \"ObjectDestroyer\", \"Destroy" + table.DataName + "\"");
      fw.Altf("              , \"Es konnte kein Objekt der Klasse < " + table.DataName + " > mit der Id \"+id+\" gefunden werden.\", \"OD-" + helplinkext + "\", true);");
      helplinkext++;
      fw.Altf("      }");
      fw.Altf("      // or try to destroy " + table.DataName + "");
      fw.Altf("      else {");
      fw.Altf("        try {");
      fw.Altf("          DBResult result = DBCalls.CallSP(ServerManager.Connection, newObject.GetDeleteDBSPContent(CurrentUser.Instance.User, deleteMode));");
      fw.Altf("          if (result.Error == 0) {");
      fw.Altf("            if (DataCore.Instance.RemoveObject(newObject)){");
      fw.Altf("              return true;");
      fw.Altf("            }");
      fw.Altf("            else {");
      fw.Altf("              throw ErrorHandler.GetErrorException(result.Exception, ErrorType.CreationError, \"ObjectDestroyer\", \"Destroy" + table.DataName + "\"");
      fw.Altf("              , \"Das Objekt der Klasse <" + table.DataName + "> konnte nicht deregestriert werden.\", \"OD-" + helplinkext + "\", true);");
      helplinkext++;
      fw.Altf("            }");
      fw.Altf("          }");
      fw.Altf("          else {");
      fw.Altf("            throw ErrorHandler.GetErrorException(result.Exception, ErrorType.CreationError, \"ObjectDestroyer\", \"Destroy" + table.DataName + "\"");
      fw.Altf("            , \"Das Objekt der Klasse <" + table.DataName + "> konnte nicht gelöscht werden. Fehlermeldung: <\" + result.ErrorMsg + \">\", \"OD-" + helplinkext + "\", true);");
      helplinkext++;
      fw.Altf("          }");
      fw.Altf("        }");
      fw.Altf("        catch (Exception e) {");
      fw.Altf("          throw ErrorHandler.GetErrorException(e, ErrorType.CreationError, \"ObjectDestroyer\", \"Destroy" + table.DataName + "\"");
      fw.Altf("          , \"Das Objekt der Klasse <" + table.DataName + "> konnte nicht deregistriert oder gelöscht werden.\", \"OD-" + helplinkext + "\", true);");
      helplinkext++;
      fw.Altf("        }");
      fw.Altf("      }");
      fw.Altf("    }");
    }
    private void CDBOD_DestroySave(FileWriter fw, CSV_Tabelle table, int helplinkext) {
      fw.Altf("    public static bool Destroy" + table.DataName + "Save(int id, bool catchExceptions) {");
      fw.Altf("      try {");
      fw.Altf("        return Destroy" + table.DataName + "(id, 0);");
      fw.Altf("      } catch (Exception e) {");
      fw.Altf("        if (!catchExceptions) {");
      fw.Altf("          throw e;");
      fw.Altf("        }");
      fw.Altf("      }");
      fw.Altf("      return false;");
      fw.Altf("    }");
      fw.Altf("    public static bool Destroy" + table.DataName + "Save(int id) {");
      fw.Altf("      return Destroy" + table.DataName + "Save(id, true);");
      fw.Altf("    }");
    }
    private void CDBOD_DestroyDefault(FileWriter fw, CSV_Tabelle table, int helplinkext) {
      fw.Altf("    public static bool Destroy" + table.DataName + "Default(int id, bool catchExceptions) {");
      fw.Altf("      try {");
      fw.Altf("        return Destroy" + table.DataName + "(id, 1);");
      fw.Altf("      } catch (Exception e) {");
      fw.Altf("        if (!catchExceptions) {");
      fw.Altf("          throw e;");
      fw.Altf("        }");
      fw.Altf("      }");
      fw.Altf("      return false;");
      fw.Altf("    }");
      fw.Altf("    public static bool Destroy" + table.DataName + "Default(int id) {");
      fw.Altf("      return Destroy" + table.DataName + "Default(id, true);");
      fw.Altf("    }");
    }
    private void CDBOD_DestroyCascading(FileWriter fw, CSV_Tabelle table, int helplinkext) {
      fw.Altf("    public static bool Destroy" + table.DataName + "Cascading(int id, bool catchExceptions) {");
      fw.Altf("      try {");
      fw.Altf("        return Destroy" + table.DataName + "(id, 2);");
      fw.Altf("      } catch (Exception e) {");
      fw.Altf("        if (!catchExceptions) {");
      fw.Altf("          throw e;");
      fw.Altf("        }");
      fw.Altf("      }");
      fw.Altf("      return false;");
      fw.Altf("    }");
      fw.Altf("    public static bool Destroy" + table.DataName + "Cascading(int id) {");
      fw.Altf("      return Destroy" + table.DataName + "Cascading(id, true);");
      fw.Altf("    }");
    }
    #endregion ObjectDestroyer

    #region ObjectUpdater
    private void CreateDBObjectUpdater() {
      Console.WriteLine("  -  CreateDBObjectUpdater()");
      string filePath = AC.APPath + "\\90_C#\\Aktivenplaner\\AP.Core";
      string fileName = "ObjectUpdater";
      string fileExt = "cs";
      FileWriter fw = new FileWriter(filePath, fileName, fileExt);

      fw.Altf("using System;");
      fw.Altf("using AP.Classes;");
      fw.Altf("using AP.Utils;");
      fw.Altf("using AP.Utils.Database;");
      fw.Altf("using AP.Core.Instances;");
      fw.Altf("");
      fw.Altf("namespace AP.Core {");
      fw.Altf("");
      fw.Altf("  public static class ObjectUpdater {");
      fw.Altf("");
      fw.Altf("    /* Start: TODO: Implement new classes <XYZ> here");
      fw.Altf("    ");
      fw.Altf("    public static bool UpdateXYZ(int id);");
      fw.Altf("    ");
      fw.Altf("    End: TODO: Implement new classes <XYZ> here */");
      fw.Altf("    ");
      fw.Altf("    ");
      int helplinkext = 0;
      foreach (var table in AC.DbTables) {
        fw.Altf("    #region Update" + table.DataName);
        CDBOU_Update(fw, table, helplinkext);
        helplinkext += 10;
        fw.Altf("    #endregion Update" + table.DataName);
        fw.Altf("    ");
      }
      fw.Altf("  }");
      fw.Altf("}");

      fw.CreateFile();
      Classes.Add(fileName);
      Console.WriteLine(" -> Done");
    }
    private void CDBOU_Update(FileWriter fw, CSV_Tabelle table, int helplinkext) {
      fw.Altf("    public static bool Update" + table.DataName + "(int id) {");
      fw.Altf("      return Update" + table.DataName + "(ObjectRetriever.Load" + table.DataName + "_ById(id));");
      fw.Altf("    }");
      fw.Altf("    public static bool Update" + table.DataName + "(" + table.DataName + " " + table.DataName.ToLower() + ") {");
      fw.Altf("      try {");
      fw.Altf("        " + table.DataName + " tmp = ObjectRetriever.DB_Load" + table.DataName + "_ById(" + table.DataName.ToLower() + ".Id);");
      foreach (CSV_Spalte col in table.Columns) {
        if (col.AttributWOB != "Id") {
          fw.Altf("        " + table.DataName.ToLower() + "." + col.AttributWOB + " = tmp." + col.AttributWOB + ";");
        }
      }
      fw.Altf("        " + table.DataName.ToLower() + ".Dirty = false;");
      fw.Altf("        return true;");
      fw.Altf("      } catch (Exception ex) {");
      fw.Altf("        ErrorHandler.CreateErrorException(ex, ErrorType.DatabaseError, \"ObjectUpdater\", \"Update" + table.DataName + "\"");
      fw.Altf("        , \"Konnte den Datensatz " + table.DataName + " < \"+" + table.DataName.ToLower() + ".ToShortenedString()+\" > nicht aus der Datenbank aktualisieren.\", \"ObUp_" + helplinkext + "\");");
      fw.Altf("      }");
      fw.Altf("      return false;");
      fw.Altf("    }");
    }
    #endregion ObjectUpdater

    #region ListRetriever
    private void CreateDBListRetriever() {
      Console.WriteLine("  -  CreateDBListRetriever()");
      string filePath = AC.APPath + "\\90_C#\\Aktivenplaner\\AP.Core";
      string fileName = "ListRetriever";
      string fileExt = "cs";
      FileWriter fw = new FileWriter(filePath, fileName, fileExt);

      fw.Altf("using System;");
      fw.Altf("using System.Collections.Generic;");
      fw.Altf("using AP.Classes;");
      fw.Altf("using AP.Utils.Database;");
      fw.Altf("");
      fw.Altf("namespace AP.Core {");
      fw.Altf("");
      fw.Altf("  public static class ListRetriever {");
      fw.Altf("");
      int helplinkext = 0;
      foreach (var table in AC.DbTables) {
        fw.Altf("    #region Load" + table.DataName);
        CDBLR_Get(fw, table);
        fw.Altf("    #endregion Load" + table.DataName);
        fw.Altf("");
      }
      fw.Altf("");
      fw.Altf("  }");
      fw.Altf("}");

      fw.CreateFile();
      Classes.Add(fileName);
      Console.WriteLine(" -> Done");
    }
    private void CDBLR_Get(FileWriter fw, CSV_Tabelle table) {
      fw.Altf("    public static List<" + table.DataName + "> Get_" + table.DataName + "() {");
      fw.Altf("      return Get_" + table.DataName + "(true, \"\", \"\");");
      fw.Altf("    }");
      fw.Altf("    public static List<" + table.DataName + "> Get_" + table.DataName + "(string where, string orderBy) {");
      fw.Altf("      return Get_" + table.DataName + "(true, where, orderBy);");
      fw.Altf("    }");
      fw.Altf("    public static List<" + table.DataName + "> Get_" + table.DataName + "(bool onlyValid) {");
      fw.Altf("      return Get_" + table.DataName + "(onlyValid, \"\", \"\");");
      fw.Altf("    }");
      fw.Altf("    public static List<" + table.DataName + "> Get_" + table.DataName + "(bool onlyValid, string where, string orderBy) {");
      fw.Altf("      List<" + table.DataName + "> returnList = new List<" + table.DataName + ">();");
      fw.Altf("      string query = \"SELECT i" + table.DataName + " FROM " + table.ViewName + "\" + (onlyValid ? \"\" : \"_full\");");
      fw.Altf("      if (where != null && where.Length > 0) {");
      fw.Altf("        query += \" WHERE \" + where;");
      fw.Altf("      }");
      fw.Altf("      if (orderBy != null && orderBy.Length > 0) {");
      fw.Altf("        query += \" ORDER BY \" + orderBy;");
      fw.Altf("      }");
      fw.Altf("      query += \";\";");
      fw.Altf("      DBResult result = DBCalls.ExecQuery(query);");
      fw.Altf("      if (!result.IsEmpty) {");
      fw.Altf("        foreach(string dbId in result.GetColumn(0, true)) {");
      fw.Altf("          returnList.Add(ObjectRetriever.Load" + table.DataName + "_ById(Int32.Parse(dbId)));");
      fw.Altf("        }");
      fw.Altf("      }");
      fw.Altf("      return returnList;");
      fw.Altf("    }");
    }
    #endregion ListRetriever

    #region NeededFiles
    private void CreateNeededFiles() {
      FileWriter fw = null;
      // App.config
      {
        fw = new FileWriter("D:\\Git\\Aktivenplaner\\90_C#\\XZ_NeededFiles", "App", "config");
        fw.Altf("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        fw.Altf("<configuration>");
        fw.Altf("  <startup>");
        fw.Altf("    <supportedRuntime version=\"v4.0\" sku=\".NETFramework,Version=v4.6.2\" />");
        fw.Altf("  </startup>");
        fw.Altf("  <appSettings>");
        fw.Altf("    <add key=\"appath\" value=\"DISABLED\" />");
        fw.Altf("    <add key=\"appath_other\" value=\"DISABLED\" />");
        fw.Altf("    <add key=\"db_server\" value=\"DISABLED\" />");
        fw.Altf("    <add key=\"db_server_other\" value=\"DISABLED\" />");
        fw.Altf("    <add key=\"db_database\" value=\"DISABLED\" />");
        fw.Altf("    <add key=\"db_user\" value=\"DISABLED\" />");
        fw.Altf("    <add key=\"db_pwd\" value=\"DISABLED\" />");
        fw.Altf("    <add key=\"db_port\" value=\"DISABLED\" />");
        fw.Altf("    <add key=\"db_sslm\" value=\"DISABLED\" />");
        fw.Altf("    <add key=\"listenpfad\" value=\"DISABLED\"/>");
        fw.Altf("    <add key=\"listenpfad_other\" value=\"DISABLED\"/>");
        fw.Altf("    <add key=\"ClientSettingsProvider.ServiceUri\" value=\"DISABLED\" />");
        fw.Altf("    <add key=\"serverUsername\" value=\"DISABLED\" />");
        fw.Altf("    <add key=\"serverPassword\" value=\"DISABLED\" />");
        fw.Altf("    <add key=\"debugmask\" value=\"DISABLED\"/>");
        fw.Altf("    <add key=\"ap_email_user\" value=\"DISABLED\"/>");
        fw.Altf("    <add key=\"ap_email_password\" value=\"DISABLED\"/>");
        fw.Altf("    <add key=\"ap_email_server\" value=\"DISABLED\"/>");
        fw.Altf("    <add key=\"ap_email_port\" value=\"DISABLED\"/>");
        fw.Altf("    <add key=\"ap_email_sender\" value=\"DISABLED\"/>");
        fw.Altf("  </appSettings>");
        fw.Altf("  <system.web>");
        fw.Altf("    <membership defaultProvider=\"ClientAuthenticationMembershipProvider\">");
        fw.Altf("      <providers>");
        fw.Altf("        <add name=\"ClientAuthenticationMembershipProvider\" type=\"System.Web.ClientServices.Providers.ClientFormsAuthenticationMembershipProvider, System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\" serviceUri=\"\" />");
        fw.Altf("      </providers>");
        fw.Altf("    </membership>");
        fw.Altf("    <roleManager defaultProvider=\"ClientRoleProvider\" enabled=\"true\">");
        fw.Altf("      <providers>");
        fw.Altf("        <add name=\"ClientRoleProvider\" type=\"System.Web.ClientServices.Providers.ClientRoleProvider, System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\" serviceUri=\"\" cacheTimeout=\"86400\" />");
        fw.Altf("      </providers>");
        fw.Altf("    </roleManager>");
        fw.Altf("  </system.web>");
        fw.Altf("</configuration>");
        fw.Altf("");
        fw.CreateFile();
      }
      // pce.txt
      {
        fw = new FileWriter("D:\\Git\\Aktivenplaner\\90_C#\\XZ_NeededFiles\\bin_Debug\\bin", "pce", "txt");
        fw.Altf("# Do NOT change these first six lines ! ");
        fw.Altf("# An entry consists of a three digit number");
        fw.Altf("#   and the classname of the already implemented");
        fw.Altf("#   class. The number represents the hierachy");
        fw.Altf("#   level of the class. (001 -> no dependency)");
        fw.Altf("# Do NOT change these first six lines !");
        for (int i = 1; i < 1000; i++) {
          string level = "";
          foreach (CSV_Tabelle s in AC.DbTables) {
            if (s.ClassDependency == i) {
              if (s.ClassDependency / 100 > 0) {
                level = "" + s.ClassDependency;
              }
              else {
                if (s.ClassDependency / 10 > 0) {
                  level = "0" + s.ClassDependency;
                }
                else {
                  if (s.ClassDependency / 1 > 0) {
                    level = "00" + s.ClassDependency;
                  }
                  else {
                    level = "000";
                  }
                }
              }
              fw.Altf(level + "" + s.DataName);
            }
          }
        }
        fw.Altf("");
        fw.CreateFile();
        // Add to projects
        fw.FolderPath = "D:\\Git\\Aktivenplaner\\90_C#\\Aktivenplaner\\AP.GUI\\bin\\Debug\\bin";
        fw.CreateFile();
        fw.FolderPath = "D:\\Git\\Aktivenplaner\\90_C#\\Aktivenplaner\\AddinTest\\bin\\Debug\\bin";
        fw.CreateFile();
      }
      // Addins-Folder
      {
        fw = new FileWriter("D:\\Git\\Aktivenplaner\\90_C#\\XZ_NeededFiles\\bin_Debug\\addins", "addins", "txt");
        fw.Altf("Hier sollten alle Addins sein, die der letzte Benutzer benutzen durfte.");
        fw.Altf("");
        fw.CreateFile();
      }
      // Logs-Folder
      {
        fw = new FileWriter("D:\\Git\\Aktivenplaner\\90_C#\\XZ_NeededFiles\\bin_Debug\\Logs", "APLogException", "LOG");
        fw.Altf("Hier werden die Exceptions geloggt.");
        fw.Altf("(Erste Log-Datei)");
        fw.Altf("");
        fw.Altf("");
        fw.Altf("");
        fw.CreateFile();
        fw = new FileWriter("D:\\Git\\Aktivenplaner\\90_C#\\XZ_NeededFiles\\bin_Debug\\Logs", "APLogDataError", "LOG");
        fw.Altf("Hier werden die Datenfehler geloggt.");
        fw.Altf("(Erste Log-Datei)");
        fw.Altf("");
        fw.Altf("");
        fw.Altf("");
        fw.CreateFile();
        fw = new FileWriter("D:\\Git\\Aktivenplaner\\90_C#\\XZ_NeededFiles\\bin_Debug\\Logs", "Debug_Absolut", "LOG");
        fw.Altf("Hier werden die Ausgaben des Debuggers geloggt.");
        fw.Altf("(Erste Log-Datei)");
        fw.Altf("");
        fw.Altf("");
        fw.Altf("");
        fw.CreateFile();
        fw = new FileWriter("D:\\Git\\Aktivenplaner\\90_C#\\XZ_NeededFiles\\bin_Debug\\Logs", "APAddinFailure", "LOG");
        fw.Altf("Hier werden die Addinfehler geloggt.");
        fw.Altf("(Erste Log-Datei)");
        fw.Altf("");
        fw.Altf("");
        fw.Altf("");
        fw.CreateFile();
      }
      // programm.properties
      {
        fw = new FileWriter("D:\\Git\\Aktivenplaner\\90_C#\\XZ_NeededFiles\\bin_Debug\\properties", "programm", "properties");
        fw.Altf("# Programminformationen");
        fw.Altf("letzteverbindung=5");
        fw.Altf("letztenutzerkennung=VD");
        fw.Altf("letztesprache=1");
        fw.Altf("");
        fw.CreateFile();
      }
      // bierbarpreise.properties
      {
        fw = new FileWriter("D:\\Git\\Aktivenplaner\\90_C#\\XZ_NeededFiles\\bin_Debug\\properties", "bierbarpreise", "properties");
        fw.Altf("# Dies sind die Preise der Bierbar");
        fw.Altf("pricealk=0.90");
        fw.Altf("pricealkfremd=1.00");
        fw.Altf("pricealkfree=0.70");
        fw.Altf("pricealkfreefremd=0.90");
        fw.Altf("pricesweetsbig=0.55");
        fw.Altf("pricesweetsmedium=0.40");
        fw.Altf("pricesweetssmall=0.30");
        fw.Altf("lastprint=1676");
        fw.Altf("pricekraut=0.50");
        fw.Altf("pricewurst=0.75");
        fw.Altf("pricebierpst=1.0");
        fw.Altf("pricealkfreipst=0.9");
        fw.Altf("");
        fw.CreateFile();
      }
      // schnapsbarpreise.properties
      {
        fw = new FileWriter("D:\\Git\\Aktivenplaner\\90_C#\\XZ_NeededFiles\\bin_Debug\\properties", "schnapsbarpreise", "properties");
        fw.Altf("# Dies sind die Preise der Schnapsbar");
        fw.Altf("leitungswasser=0.0");
        fw.Altf("");
        fw.CreateFile();
      }
    }
    #endregion NeededFiles

    #region helper_methods
    private string GetAPType(CSV_Tabelle c, int i) {
      string ret = "";
      switch (c.Columns[i].Art) {
        case "nvarchar":
          ret = "string";
          break;
        case "int":
          ret = "int";
          break;
        case "datetime":
          ret = "DateTime";
          break;
        case "date":
          ret = "DateTime";
          break;
        case "time":
          ret = "DateTime";
          break;
        case "decimal":
          ret = "double";
          break;
        default:
          if (c.Columns[i].Art.StartsWith("enum")) {
            ret = "SysEnumElement";
          }
          else {
            ret = c.Columns[i].Art;
          }
          break;
      }
      if (!c.Columns[i].NotNull && (ret == "int" || ret == "DateTime" || ret == "double")) {
        ret += "?";
      }
      return ret;
    }
    private string GetCSType(string inArt, bool inNN) {
      string ret = " _ TODO _ ";
      switch (inArt) {
        case "nvarchar":
          ret = "string";
          break;
        case "int":
          ret = "int";
          break;
        case "datetime":
          ret = "DateTime";
          break;
        case "decimal":
          ret = "double";
          break;
        case "date":
          ret = "DateTime";
          break;
        case "tinyInt":
          ret = "int";
          break;
        case "bool":
          ret = "bool";
          break;
        case "time":
          ret = "DateTime";
          break;
        default:
          ret = " _ TODO _ ";
          break;
      }
      // Enum
      if (inArt.StartsWith("e")) {
        ret = "int";
      }
      // Verweis auf eine andere Tabelle
      bool found = false;
      foreach (var c in AC.DbTables) {
        if (c.DataName == inArt) {
          found = true;
        }
      }
      if (found) {
        ret = "int";
      }
      if (!inNN && ret != "string") {
        ret += "?";
      }
      return ret;
    }
    private string GetDBSPType(string inArt) {
      string ret = " _ TODO _ ";
      switch (inArt) {
        case "nvarchar":
          return "Text";
        case "int":
          return "Int32";
        case "datetime":
          return "DateTime";
        case "decimal":
          return "Double";
        case "date":
          return "DateTime";
        case "tinyInt":
          return "Int32";
        case "bool":
          return "Bool";
        default:
          ret = " _ TODO _ ";
          break;
      }
      // Enum
      if (inArt.StartsWith("e")) {
        return "Int32";
      }
      // Verweis auf eine andere Tabelle
      bool found = false;
      foreach (var c in AC.DbTables) {
        if (c.DataName == inArt) {
          found = true;
        }
      }
      if (found) {
        ret = "Int32";
      }
      return ret;
    }
    private string GetEnumName(string inString) {
      string s = inString;
      s = s.Replace("!", "");
      s = s.Replace(" ", "_");
      return s;
    }
    public string GetDataTypeForInt(CSV_Tabelle tabelle, int spaltenId) {
      CSV_Spalte spalte = tabelle.Columns[spaltenId];
      if (spalte.ForeignKeyTo.Length == 0) {
        return "int";
      }
      if (spalte.ForeignKeyTo == "Enum") {
        if (spalte.Attribut == "nDatenstatus") {
          // return "eAP_Datenstatus_Voll";
          return "Enum";
        }
        else {
          // spezielles enum einer tabelle
          /*for (int i = 0; i < AC.Enums.Count; i++) {
            if (AC.Enums[i].Table == tabelle.TableName && AC.Enums[i].Column == tabelle.Columns[spaltenId].Attribut) {
              // return Enums[i].GetEnumName()+"_Voll";
              return "Enum";
            }
          }*/
          Console.WriteLine("Error: table: " + tabelle.TableName + " <-> " + tabelle.Columns[spaltenId].Attribut + ";");
          return " _ TODO (Enum -> GetDataTypeForInt(...) )_ ";
        }
      }
      // referenz auf anderes objekt
      return spalte.ForeignKeyTo.Split('.')[0].Substring(3, spalte.ForeignKeyTo.Split('.')[0].Length - 3);
    }
    #endregion helper_methods

  }

}
