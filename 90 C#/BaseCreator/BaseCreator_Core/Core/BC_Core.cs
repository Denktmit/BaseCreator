using System;
using System.IO;
using System.Linq;
using System.Data;
using System.Data.OleDb;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BaseCreator_Core.Model;
using BaseCreator_Core.Helper;
using AP_Extension;
using VDUtils.Helper;
using BaseCreator_Model.Model;

namespace BaseCreator_Core.Core {

  public static class BC_Core {

    #region fields_and_properties
    private static ObservableCollection<BCFile> _files;
    public static ObservableCollection<BCFile> Files {
      get {
        if (_files == null)
          LoadFiles();
        return _files;
      }
    }

    private static ObservableCollection<BCTemplate> _templates;
    public static ObservableCollection<BCTemplate> Templates {
      get {
        if (_templates == null)
          LoadTemplates();
        return _templates;
      }
    }

    private static ObservableCollection<DataType> _dataTypes;
    public static ObservableCollection<DataType> DataTypes {
      get {
        if (_dataTypes == null)
          LoadDataTypes();
        return _dataTypes;
      }
    }

    private static ObservableCollection<TemplateTarget> _templateTargets;
    public static ObservableCollection<TemplateTarget> TemplateTargets {
      get {
        if (_templateTargets == null)
          LoadTemplateTargets();
        return _templateTargets;
      }
    }
    #endregion  fields_and_properties

    #region loading
    public static void Refresh() {
      LoadFiles();
      LoadTemplates();
    }
    private static void LoadFiles() {
      _files = new ObservableCollection<BCFile>();
      string filesPath = Configurations.GetValue("speicherpfad") + "FILES\\";
      List<string> loaded = FileManager.getFilesInFolder(filesPath);
      foreach (string f in loaded) {
        try {
          BCFile loadedFile = ReadFile(filesPath + f);
          if (loadedFile != null)
            Files.Add(loadedFile);
        }
        catch (Exception ex) {
          ErrorHandler.CreateErrorException(ex, ErrorType.LoadError, "BC_Core", "LoadFiles"
          , "Die Datei konnte nicht geladen werden.", "49");
        }
      }
    }
    private static void LoadTemplates() {
      _templates = new ObservableCollection<BCTemplate>();
      string filesPath = Configurations.GetValue("speicherpfad") + "TEMPLATES\\";
      List<string> loaded = FileManager.getFilesInFolder(filesPath);
      foreach (string f in loaded) {
        try {
          BCTemplate loadedFile = ReadTemplate(filesPath + f);
          if (loadedFile != null)
            Templates.Add(loadedFile);
        }
        catch (Exception ex) {
          ErrorHandler.CreateErrorException(ex, ErrorType.LoadError, "BC_Core", "LoadTemplates"
          , "Das Template konnte nicht geladen werden.", "77");
        }
      }
      BCTemplate tsql = new BCTemplate("AP-SQL");
      tsql.IsSelected = true;
      tsql.Hardcoded = true;
      tsql.Content = "* Code des AutoCreators aus Aktivenplaner *";
      Templates.Add(tsql);
      BCTemplate tcs = new BCTemplate("AP-CS");
      tcs.IsSelected = true;
      tcs.Hardcoded = true;
      tcs.Content = "* Code des AutoCreators aus Aktivenplaner *";
      Templates.Add(tcs);
      BCTemplate tphp = new BCTemplate("AP-PHP");
      tphp.IsSelected = true;
      tphp.Hardcoded = true;
      tphp.Content = "* Code des AutoCreators aus Aktivenplaner *";
      Templates.Add(tphp);
      BCTemplate thtml = new BCTemplate("AP-HTML");
      thtml.IsSelected = true;
      thtml.Hardcoded = true;
      thtml.Content = "* Code des AutoCreators aus Aktivenplaner *";
      Templates.Add(thtml);
      //BCTemplate trds = new BCTemplate("AP-RandDS");
      //trds.IsSelected = true;
      //trds.Hardcoded = true;
      //trds.Content = "* Code des AutoCreators aus Aktivenplaner *";
      //Templates.Add(trds);
    }
    private static void LoadDataTypes() {
      _dataTypes = new ObservableCollection<DataType>();
      foreach (DataType dt in DataType.DataTypes)
        _dataTypes.Add(dt);
    }
    private static void LoadTemplateTargets() {
      _templateTargets = new ObservableCollection<TemplateTarget>();
      foreach (TemplateTarget tt in TemplateTarget.TemplateTargets)
        _templateTargets.Add(tt);
    }
    public static bool ImportFile(string filePath) {
      try {
        if (!ConvertExcelToCsv(filePath)) {
          return false;
        }
        string filesPath = Configurations.GetValue("speicherpfad") + "FILES\\";
        string csvOutputFile = filesPath + "" + FileManager.splitFilePath(filePath)[1] + ".csv";
        BCFile loadedFile = ReadFile(csvOutputFile);
        if (loadedFile != null)
          Files.Add(loadedFile);
        return true;
      } catch(Exception ex) {
        ErrorHandler.CreateErrorException(ex, ErrorType.LoadError, "BC_Core", "ImportFile"
          , "Die Datei konnte nicht importiert werden.", "101");
        return false;
      }
    }
    #endregion loading

    #region getter
    public static BCFile GetFile(string darstellung) {
      foreach (BCFile f in Files) {
        if (f.Darstellung == darstellung)
          return f;
      }
      return null;
    }
    public static BCDatabase GetDatabase(BCFile file, string darstellung) {
      foreach (BCDatabase db in file.Databases) {
        if (db.Darstellung == darstellung)
          return db;
      }
      return null;
    }
    public static BCTable GetTable(BCDatabase db, string darstellung) {
      foreach (BCTable tab in db.Tables) {
        if (tab.Darstellung == darstellung)
          return tab;
      }
      return null;
    }
    public static BCColumn GetColumn(BCTable tab, string darstellung) {
      foreach (BCColumn col in tab.Columns) {
        if (col.Darstellung == darstellung)
          return col;
      }
      return null;
    }
    public static DataType GetDataType(string darstellung) {
      foreach (DataType dt in DataTypes) {
        if (dt.Darstellung == darstellung)
          return dt;
      }
      return null;
    }
    public static TemplateTarget GetTemplateTarget(string darstellung) {
      foreach (TemplateTarget dt in TemplateTargets) {
        if (dt.Darstellung == darstellung)
          return dt;
      }
      return null;
    }
    public static BCTemplate GetTemplate(string bezeichnung) {
      foreach (BCTemplate f in Templates) {
        if (f.Bezeichnung == bezeichnung)
          return f;
      }
      return null;
    }
    #endregion getter

    #region methods
    public static bool SaveChanges() {
      List<string> errorFiles = new List<string>();
      foreach (BCFile file in Files) {
        bool dirty = file.Dirty;
        if (!dirty) {
          foreach (BCDatabase db in file.Databases) {
            if (db.Dirty || dirty) {
              dirty = true;
              break;
            }
            foreach (BCTable tab in db.Tables) {
              if (tab.Dirty || dirty) {
                dirty = true;
                break;
              }
              foreach (BCColumn col in tab.Columns) {
                if (col.Dirty || dirty) {
                  dirty = true;
                  break;
                }
              }
            }
          }
        }
        if (dirty) {
          if (!Save(file))
            errorFiles.Add(file.FilePath);
        }
      }
      foreach(BCTemplate tem in Templates) {
        if(tem.Dirty)
          if (!Save(tem))
            errorFiles.Add(tem.FilePath);
      }
      if (errorFiles.Count > 0)
        return false;
      return true;
    }
    public static bool Save(BCTemplate template) {
      if (template.Darstellung.Contains("AP-"))
        return true;
      try {
        string[] fileInfo = FileManager.splitFilePath(template.CurrentFilePath);
        List<string> cont = new List<string>();
        // Header
        cont.Add("-------------------------------------------");
        cont.Add("- Bezeichnung: " + template.Bezeichnung);
        cont.Add("- Target: " + template.Target.Darstellung);
        cont.Add("- Dateiname: " + template.Dateiname);
        cont.Add("");
        cont.Add("");
        cont.Add("");
        cont.Add("");
        cont.Add("");
        cont.Add("");
        cont.Add("LastChange: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
        cont.Add("-------------------------------------------");
        cont.Add("");
        // Content
        cont.Add(template.Content);
        if (!FileManager.writeNewFileWithContent(fileInfo[0], fileInfo[1], fileInfo[2], cont, true)) {
          return false;
        }
        string oldfp = "";
        if (template.FilePath != template.CurrentFilePath) {
          oldfp = template.FilePath;
        }
        template.FilePath = template.CurrentFilePath;
        template.Darstellung = template.Bezeichnung;
        template.Dirty = false;
        if(oldfp!="")
          Templates.Add(ReadTemplate(oldfp));
        return true;
      } catch(Exception ex) {
        ErrorHandler.CreateErrorException(ex, ErrorType.CreationError, "BC_Core", "Save"
          , "Das Template konnte nicht gespeichert werden.", "206");
      }
      return false;
    }
    public static bool Save(BCFile file) {
      try {
        // Datei speichern
        bool success = CSVImport.Save(file.FilePath, GetDList(file));
        // Dirty-Flag ruecksetzen
        if (success) {
          file.Dirty = false;
          foreach (BCDatabase db in file.Databases) {
            db.Dirty = false;
            foreach (BCTable tab in db.Tables) {
              tab.Dirty = false;
              foreach (BCColumn col in tab.Columns) {
                col.Dirty = false;
              }
            }
          }
        }
        return success;
      }
      catch (Exception ex) {
        ErrorHandler.CreateErrorException(ex, ErrorType.CreationError, "BC_Core", "Save"
          , "Die Datei konnte nicht gespeichert werden.", "316");
        return false;
      }
    }
    public static bool Save(BCDatabase db) {
      return Save(db.File);
    }
    public static bool Save(BCTable table) {
      return Save(table.Database);
    }
    public static bool Save(BCColumn column) {
      return Save(column.Table);
    }
    public static BCFile ReadFile(string filepath) {
      try {
        List<List<string>> loaded = CSVImport.getListList(filepath);
        int headerCheck = CheckHeader(loaded[0]);
        if (headerCheck != 0)
          throw new Exception("Header invalid (Column " + headerCheck + ")");
        BCFile file = new BCFile(filepath, loaded);
        // Zeilen auswerten
        for (int i = 1; i < loaded.Count; i++) {
          List<string> row = loaded[i];
          // Inhalte lesen
          string dbname = row[0];
          string tabname = row[1];
          if (tabname.StartsWith("AP"))
            tabname = tabname.Substring(2);
          string tabkuerzel = row[2];
          string colname = row[3];
          string datatype = row[4];
          // Objekte erstellen
          BCDatabase db = CreateNewDatabase(file, dbname);
          BCTable tab = GetTable(db, tabname);
          if (tab == null)
            tab = CreateNewTable(db, tabname, false);
          tab.Kuerzel = tabkuerzel;
          BCColumn col = CreateNewColumn(tab, colname);
          col.DataType = GetDataType(datatype);
          col.Verweis = row[5];
          col.NotNull = row[6] == "x";
          col.AutoIncrement = row[7] == "x";
          col.PrimaryKey = row[8] == "x";
          col.Default = row[9];
          col.MakeUnique = row[10];
          col.Kommentar = row[11];
          if (colname.StartsWith(col.DataType.Prefix)) {
            col.Darstellung = colname.Substring(col.DataType.Prefix.Length);
          }
        }
        // Verweise auswerten
        foreach (BCDatabase db in file.Databases) {
          foreach (BCTable tab in db.Tables) {
            foreach (BCColumn col in tab.Columns) {
              if (col.DataType.Darstellung == "Verweis") {
                string tempdb = col.Verweis.Split('.')[0];
                string temptable = col.Verweis.Split('.')[1];
                BCDatabase tdb = GetDatabase(file, tempdb);
                if (tdb == null)
                  throw new Exception("Der Verweis auf die Datenbank <" + tempdb + "> konnte nicht aufgeloest werden.");
                BCTable ttab = GetTable(tdb, temptable);
                if (ttab == null)
                  throw new Exception("Der Verweis auf die Tabelle <" + temptable + "> konnte nicht aufgeloest werden.");
                col.Reference = ttab;
              }
            }
          }
        }
        int h = 0;
        while (h < 1000) {
          foreach (BCDatabase db in file.Databases) {
            foreach (BCTable tab in db.Tables) {
              tab.RefreshDependencyLevel();
            }
          }
          h++;
        }
        // Dirty ruecksetzen
        foreach (BCDatabase db in file.Databases) {
          db.Dirty = false;
          foreach (BCTable tab in db.Tables) {
            tab.Dirty = false;
            foreach (BCColumn col in tab.Columns) {
              col.Dirty = false;
            }
          }
        }
        file.Dirty = false;
        return file;
      }
      catch (Exception ex) {
        ErrorHandler.CreateErrorException(ex, ErrorType.LoadError, "BC_Core", "ReadFile"
          , "Die Datei konnte nicht geladen werden.", "227");
      }
      return null;
    }
    public static BCTemplate ReadTemplate(string filepath) {
      try {
        List<string> lines = FileManager.getContentAsList(filepath);
        // -------------------------------------------
        string bez = lines[1].Replace("- Bezeichnung: ", "");
        TemplateTarget tt = GetTemplateTarget(lines[2].Replace("- Target: ", ""));
        string dateiname = lines[3].Replace("- Dateiname: ", "");

        // -------------------------------------------
        //
        string cont = "";
        for (int i = 13; i < lines.Count; i++) {
          cont += lines[i];
          if (i < lines.Count - 1)
            cont += "\r\n";
        }
        BCTemplate tem = new BCTemplate(FileManager.splitFilePath(filepath)[1], filepath);
        tem.Bezeichnung = bez;
        tem.Target = tt;
        tem.Dateiname = dateiname;
        tem.Content = cont;
        tem.Dirty = false;
        return tem;
      } catch(Exception ex) {
        ErrorHandler.CreateErrorException(ex, ErrorType.LoadError, "BC_Core", "ReadTemplate"
         , "Das Template konnte nicht geladen werden.", "364");
      }
      return null;
    }
    public static BCFile CreateNewFile(string filename) {
      string filepath = Configurations.GetValue("speicherpfad") + "FILES\\" + filename + ".csv";
      try {
        BCFile newFile = GetFile(filename);
        if (newFile != null)
          return newFile;
        newFile = new BCFile(filepath);
        bool succes = Save(newFile);
        if (succes) {
          newFile.Dirty = true;
          Files.Add(newFile);
          return newFile;
        }
      }
      catch (Exception ex) {
        ErrorHandler.CreateErrorException(ex, ErrorType.CreationError, "BC_Core", "CreateNewFile"
          , "Die neue Datei konnte nicht erstellt werden.", "127");
      }
      return null;
    }
    public static BCDatabase CreateNewDatabase(BCFile file, string dbname) {
      try {
        BCDatabase newDB = GetDatabase(file, dbname);
        if (newDB != null)
          return newDB;
        newDB = new BCDatabase(file, dbname);
        newDB.Dirty = true;
        file.Databases.Add(newDB);
        return newDB;
      }
      catch (Exception ex) {
        ErrorHandler.CreateErrorException(ex, ErrorType.CreationError, "BC_Core", "CreateNewDatabase"
          , "Die neue Datenbank konnte nicht erstellt werden.", "143");
      }
      return null;
    }
    public static BCTable CreateNewTable(BCDatabase db, string tablename) {
      return CreateNewTable(db, tablename, true);
    }
    public static BCTable CreateNewTable(BCDatabase db, string tablename, bool createStandardColumns) {
      try {
        BCTable newTable = GetTable(db, tablename);
        if (newTable != null)
          return newTable;
        newTable = new BCTable(db, tablename);
        newTable.Kuerzel = tablename.Substring(0, 3);
        // Standardspalten erstellen
        if (createStandardColumns) {
          foreach (BCColumn col in GetStandardColumns(newTable)) {
            newTable.Columns.Add(col);
          }
        }
        // newTable zurueck geben
        newTable.Dirty = true;
        db.Tables.Add(newTable);
        return newTable;
      }
      catch (Exception ex) {
        ErrorHandler.CreateErrorException(ex, ErrorType.CreationError, "BC_Core", "CreateNewTable"
          , "Die neue Tabelle konnte nicht erstellt werden.", "159");
      }
      return null;
    }
    public static BCColumn CreateNewColumn(BCTable table, string columnname) {
      try {
        BCColumn newColumn = GetColumn(table, columnname);
        if (newColumn != null)
          return newColumn;
        newColumn = new BCColumn(table, columnname);
        newColumn.Dirty = true;
        table.Columns.Add(newColumn);
        return newColumn;
      }
      catch (Exception ex) {
        ErrorHandler.CreateErrorException(ex, ErrorType.CreationError, "BC_Core", "CreateNewColumn"
          , "Die neue Datenbank konnte nicht erstellt werden.", "438");
      }
      return null;
    }
    public static BCTemplate CreateNewTemplate(string templateName) {
      string filepath = Configurations.GetValue("speicherpfad") + "TEMPLATES\\" + templateName + ".txt";
      try {
        BCTemplate newt = GetTemplate(templateName);
        if (newt != null)
          return newt;
        newt = new BCTemplate(templateName, filepath);
        bool succes = Save(newt);
        if (succes) {
          newt.Dirty = true;
          Templates.Add(newt);
          return newt;
        }
      }
      catch (Exception ex) {
        ErrorHandler.CreateErrorException(ex, ErrorType.CreationError, "BC_Core", "CreateNewTemplate"
          , "Das neue Template konnte nicht erstellt werden.", "487");
      }
      return null;
    }
    public static bool DeleteFile(BCFile file) {
      try {
        Files.Remove(file);
        return FileManager.deleteFile(file.FilePath);
      }
      catch (Exception ex) {
        ErrorHandler.CreateErrorException(ex, ErrorType.CreationError, "BC_Core", "DeleteFile"
          , "Die Datei konnte nicht gelöscht werden.", "186");
        BCFile loaded = GetFile(file.Darstellung);
        if (loaded == null)
          Files.Add(file);
      }
      return false;
    }
    public static bool DeleteDatabase(BCDatabase db) {
      try {
        db.File.Databases.Remove(db);
        db.File.Dirty = true;
        return true;
      }
      catch (Exception ex) {
        ErrorHandler.CreateErrorException(ex, ErrorType.CreationError, "BC_Core", "DeleteDatabase"
          , "Die Datenbank konnte nicht gelöscht werden.", "202");
        BCDatabase loaded = GetDatabase(db.File, db.Darstellung);
        if (loaded == null)
          db.File.Databases.Add(db);
      }
      return false;
    }
    public static bool DeleteTable(BCTable table) {
      try {
        table.Database.Tables.Remove(table);
        table.Database.Dirty = true;
        return true;
      }
      catch (Exception ex) {
        ErrorHandler.CreateErrorException(ex, ErrorType.CreationError, "BC_Core", "DeleteTable"
          , "Die Tabelle konnte nicht gelöscht werden.", "217");
        BCTable loaded = GetTable(table.Database, table.Darstellung);
        if (loaded == null)
          table.Database.Tables.Add(table);
      }
      return false;
    }
    public static bool DeleteColumn(BCColumn column) {
      try {
        column.Table.Columns.Remove(column);
        column.Table.Dirty = true;
        return true;
      }
      catch (Exception ex) {
        ErrorHandler.CreateErrorException(ex, ErrorType.CreationError, "BC_Core", "DeleteColumn"
          , "Die Spalte konnte nicht gelöscht werden.", "232");
        BCColumn loaded = GetColumn(column.Table, column.Darstellung);
        if (loaded == null)
          column.Table.Columns.Add(column);
      }
      return false;
    }
    public static bool DeleteTemplate(BCTemplate template) {
      try {
        Templates.Remove(template);
        return FileManager.deleteFile(template.FilePath);
      }
      catch (Exception ex) {
        ErrorHandler.CreateErrorException(ex, ErrorType.CreationError, "BC_Core", "DeleteTemplate"
          , "Das Template konnte nicht gelöscht werden.", "565");
        BCTemplate loaded = GetTemplate(template.Bezeichnung);
        if (loaded == null)
          Templates.Add(template);
      }
      return false;
    }
    public static bool UnsavedChanges() {
      foreach (BCFile file in Files) {
        if (file.Dirty)
          return true;
        foreach (BCDatabase db in file.Databases) {
          if (db.Dirty)
            return true;
          foreach (BCTable tab in db.Tables) {
            if (tab.Dirty)
              return true;
            foreach (BCColumn col in tab.Columns) {
              if (col.Dirty)
                return true;
            }
          }
        }
      }
      foreach(BCTemplate tem in Templates) {
        if (tem.Dirty)
          return true;
      }
      return false;
    }
    #endregion methods

    #region converter
    public static bool ConvertExcelToCsv(string excelFilePath, int worksheetNumber = 1) {
      if (!File.Exists(excelFilePath)) {
        ErrorHandler.CreateErrorException(ErrorType.FileNotFound, "BC_Core", "ConvertExcelToCsv"
          , "Die Quelldatei konnte nicht gefunden werden.", "493");
        return false;
      }
      string filesPath = Configurations.GetValue("speicherpfad") + "FILES\\";
      string csvOutputFile = filesPath + "" + FileManager.splitFilePath(excelFilePath)[1]+".csv";
      if (File.Exists(csvOutputFile)) {
        ErrorHandler.CreateErrorException(ErrorType.CreationError, "BC_Core", "ConvertExcelToCsv"
          , "Die Zieldatei existiert bereits.", "500");
        return false;
      }
      // connection string
      var cnnStr = String.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties=\"Excel 8.0;IMEX=1;HDR=NO\"", excelFilePath);
      cnnStr = String.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0;IMEX=1;HDR=NO\"", excelFilePath);
      var cnn = new OleDbConnection(cnnStr);
      // get schema, then data
      var dt = new DataTable();
      try {
        cnn.Open();
        var schemaTable = cnn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
        if (schemaTable.Rows.Count < worksheetNumber) {
          ErrorHandler.CreateErrorException(ErrorType.InputError, "BC_Core", "ConvertExcelToCsv"
            , "Die angegebene Arbeitsblattnummer konnte nicht gefunden werden.", "513");
          return false;
        }
        string worksheet = schemaTable.Rows[worksheetNumber - 1]["table_name"].ToString().Replace("'", "");
        string sql = String.Format("select * from [{0}]", worksheet);
        var da = new OleDbDataAdapter(sql, cnn);
        da.Fill(dt);
      }
      catch (Exception ex) {
        ErrorHandler.CreateErrorException(ex, ErrorType.OtherError, "BC_Core", "ConvertExcelToCsv"
          , "Es ist ein Fehler beim Umwandeln der Datei aufgetreten.", "523");
        return false;
      }
      finally {
        // free resources
        cnn.Close();
      }
      // write out CSV data
      using (var wtr = new StreamWriter(csvOutputFile)) {
        foreach (DataRow row in dt.Rows) {
          bool firstLine = true;
          foreach (DataColumn col in dt.Columns) {
            if (!firstLine) { wtr.Write(","); }
            else { firstLine = false; }
            var data = row[col.ColumnName].ToString().Replace("\"", "\"\"");
            wtr.Write(String.Format("\"{0}\"", data));
          }
          wtr.WriteLine();
        }
      }
      return true;
    }
    public static bool ConvertExcelToCSV_2(string sourceFile, string worksheetName = "{0}") {
      string filesPath = Configurations.GetValue("speicherpfad") + "FILES\\";
      string targetFile = filesPath + "" + FileManager.splitFilePath(sourceFile)[1] + ".csv";
      string strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + sourceFile + ";Extended Properties=\" Excel.0;HDR=Yes;IMEX=1\"";
      //strConn = String.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0;IMEX=1;HDR=NO\"", targetFile);
      OleDbConnection conn = null;
      StreamWriter wrtr = null;
      OleDbCommand cmd = null;
      OleDbDataAdapter da = null;
      try {
        conn = new OleDbConnection(strConn);
        conn.Open();
        cmd = new OleDbCommand("SELECT * FROM [" + worksheetName + "$]", conn);
        cmd.CommandType = CommandType.Text;
        wrtr = new StreamWriter(targetFile);
        da = new OleDbDataAdapter(cmd);
        DataTable dt = new DataTable();
        da.Fill(dt);
        for (int x = 0; x < dt.Rows.Count; x++) {
          string rowString = "";
          for (int y = 0; y < dt.Columns.Count; y++) {
            rowString += "\"" + dt.Rows[x][y].ToString() + "\",";
          }
          wrtr.WriteLine(rowString);
        }
        Console.WriteLine();
        Console.WriteLine("Done! Your " + sourceFile + " has been converted into " + targetFile + ".");
        Console.WriteLine();
      }
      catch (Exception exc) {
        Console.WriteLine(exc.ToString());
        //Console.ReadLine();
        return false;
      }
      finally {
        if (conn.State == ConnectionState.Open)
          conn.Close();
        conn.Dispose();
        cmd.Dispose();
        da.Dispose();
        wrtr.Close();
        wrtr.Dispose();
      }
      return true;
    }
    #endregion converter

    #region creation
    public static bool CreateFileTemplate(BCFile file, BCTemplate template) {
      if (template.Hardcoded)
        return APAC.CreateResult(file, template.Darstellung);
      return Creator.Create(file, template);
    }
    public static bool CreateFilesTemplates(List<BCFile> files, List<BCTemplate> templates) {
      return CreateFilesTemplates(files, templates, false);
    }
    public static bool CreateFilesTemplates(List<BCFile> files, List<BCTemplate> templates, bool abortAtError) {
      List<string> errors = new List<string>();
      foreach(BCFile file in files) {
        foreach(BCTemplate tem in templates) {
          try {
            if(!CreateFileTemplate(file, tem)) {
              throw new Exception("Fehler in Bearbeitung");
            }
          } catch(Exception ex) {
            ErrorException ee = ErrorHandler.GetErrorException(ex, ErrorType.CreationError, "BC_Core", "CreateFilesTemplates"
              , "Die Datei <"+file.FilePath+"> konnte nicht mit der Vorlage <"+tem.FilePath+"> bearbeitet werden.", "782", abortAtError);
            errors.Add(file.FilePath + " x " + tem.FilePath);
            if (abortAtError)
              throw ee;
          }
        }
      }
      if (errors.Count > 0)
        return false;
      return true;
    }
    #endregion creation

    #region helper
    private static int CheckHeader(List<string> row) {
      if (row.Count != 12)
        return -1;
      if (row[0] != "Datenbank")
        return 1;
      if (row[1] != "Tabelle")
        return 2;
      if (row[2] != "TabelleKuerzel")
        return 3;
      if (row[3] != "Attribut")
        return 4;
      if (row[4] != "Art")
        return 5;
      if (row[5] != "Verweis")
        return 6;
      if (row[6] != "NotNull")
        return 7;
      if (row[7] != "AutoIncrement")
        return 8;
      if (row[8] != "PrimaryKey")
        return 9;
      if (row[9] != "defaultValue")
        return 10;
      if (row[10] != "MakeUnique")
        return 11;
      if (row[11] != "Kommentar")
        return 12;
      return 0;
    }
    private static List<BCColumn> GetStandardColumns(BCTable table) {
      List<BCColumn> ret = new List<BCColumn>();
      BCColumn cId = new BCColumn(table, "Id");
      cId.DataType = GetDataType("Zahl");
      cId.NotNull = true;
      cId.AutoIncrement = true;
      cId.PrimaryKey = true;
      cId.Kommentar = "Standard Id der Tabelle " + table.Darstellung;
      ret.Add(cId);
      BCColumn cAendZeit = new BCColumn(table, "AendZeit");
      cAendZeit.DataType = GetDataType("Zeitpunkt");
      cAendZeit.NotNull = true;
      cAendZeit.Default = "NOW";
      cAendZeit.Kommentar = "Zeitpunkt der letzten Aenderung";
      ret.Add(cAendZeit);
      BCColumn cDatenstatus = new BCColumn(table, "Datenstatus");
      BCTable tblsee = GetTable(table.Database, "SysEnumElement");
      if (tblsee != null) {
        cDatenstatus.DataType = GetDataType("Verweis");
        cDatenstatus.Reference = tblsee;
      }
      else {
        cDatenstatus.DataType = GetDataType("Zahl");
      }
      cDatenstatus.Default = "4";
      cDatenstatus.NotNull = true;
      cDatenstatus.Kommentar = "Datenstatus des Eintrages";
      ret.Add(cDatenstatus);
      return ret;
    }
    public static List<List<string>> GetDList(BCFile file) {
      try {
        List<List<string>> content = new List<List<string>>();
        // Header
        List<string> header = new List<string>();
        header.Add("Datenbank");
        header.Add("Tabelle");
        header.Add("TabelleKuerzel");
        header.Add("Attribut");
        header.Add("Art");
        header.Add("Verweis");
        header.Add("NotNull");
        header.Add("AutoIncrement");
        header.Add("PrimaryKey");
        header.Add("defaultValue");
        header.Add("MakeUnique");
        header.Add("Kommentar");
        content.Add(header);
        // Sort before saving
        int h = 0;
        while (h < 1000) {
          foreach (BCDatabase db in file.Databases) {
            foreach (BCTable tab in db.Tables) {
              tab.RefreshDependencyLevel();
            }
          }
          h++;
        }
        file.SortDatabases();
        foreach (BCDatabase db in file.Databases) {
          db.SortTables();
        }
        // Zeilen erstellen
        foreach (BCDatabase db in file.Databases) {
          foreach (BCTable tab in db.Tables) {
            foreach (BCColumn col in tab.Columns) {
              List<string> row = new List<string>();
              row.Add("" + db.Darstellung);
              row.Add("" + tab.Darstellung);
              row.Add("" + tab.Kuerzel);
              row.Add("" + col.Darstellung);
              row.Add("" + col.DataType?.Darstellung);
              row.Add("" + col.Verweis);
              row.Add("" + (col.NotNull ? "x" : ""));
              row.Add("" + (col.AutoIncrement ? "x" : ""));
              row.Add("" + (col.PrimaryKey ? "x" : ""));
              row.Add("" + col.Default);
              row.Add("" + col.MakeUnique);
              row.Add("" + col.Kommentar);
              content.Add(row);
            }
          }
        }
        return content;
      } catch (Exception ex) {
        ErrorHandler.CreateErrorException(ex, ErrorType.CreationError, "BC_Core", "GetContent"
          , "Die Datei konnte nicht in eine List<List<string>> umgewandelt werden.", "889");
        return null;
      }
    }
    public static List<List<List<string>>> GetTList(BCFile file) {
      try {
        List<List<List<string>>> ret = new List<List<List<string>>>();
        List<List<string>> content = new List<List<string>>();
        // Sort before saving
        int h = 0;
        while (h < 1000) {
          foreach (BCDatabase db in file.Databases) {
            foreach (BCTable tab in db.Tables) {
              tab.RefreshDependencyLevel();
            }
          }
          h++;
        }
        file.SortDatabases();
        foreach (BCDatabase db in file.Databases) {
          db.SortTables();
        }
        // Zeilen erstellen
        foreach (BCDatabase db in file.Databases) {
          foreach (BCTable tab in db.Tables) {
            foreach (BCColumn col in tab.Columns) {
              List<string> row = new List<string>();
              row.Add("" + db.Darstellung);
              row.Add("" + tab.Darstellung);
              row.Add("" + tab.Kuerzel);
              row.Add("" + col.Darstellung);
              row.Add("" + col.DataType?.Darstellung);
              row.Add("" + col.Verweis);
              row.Add("" + (col.NotNull ? "x" : ""));
              row.Add("" + (col.AutoIncrement ? "x" : ""));
              row.Add("" + (col.PrimaryKey ? "x" : ""));
              row.Add("" + col.Default);
              row.Add("" + col.MakeUnique);
              row.Add("" + col.Kommentar);
              content.Add(row);
            }
          }
        }
        return ret;
      } catch (Exception ex) {
        ErrorHandler.CreateErrorException(ex, ErrorType.CreationError, "BC_Core", "GetContent"
          , "Die Datei konnte nicht in eine List<List<string>> umgewandelt werden.", "889");
        return null;
      }
    }
    #endregion helper

  }

}
