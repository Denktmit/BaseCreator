using BaseCreator_Core.Core;
using BaseCreator_Core.Helper;
using BaseCreator_Core.Model;
using BaseCreator_GUI.Helper.Commands;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Security;
using System.Windows;

namespace BaseCreator_GUI.ViewModel {

  public class BC_GUI_ViewModel : ModelBase {

    #region fields and properties
    private string _newDateiPfad;
    private string _newFileName;
    private string _newDBName;
    private string _newTableName;
    private string _newColumnName;
    private BCFile _selectedFile;
    private BCDatabase _selectedDatabase;
    private BCTable _selectedTable;
    private BCColumn _selectedColumn;
    private DataType _selectedDataType;
    private TemplateTarget _selectedTemplateTarget;
    private BCTemplate _selectedTemplate;
    private BCDatabase _verwDB;

    public string NewDateiPfad {
      get { return _newDateiPfad; }
      set {
        _newDateiPfad = value;
        OnPropertyChanged();
      }
    }
    public string NewFileName {
      get { return _newFileName; }
      set {
        _newFileName = value;
        OnPropertyChanged();
      }
    }
    public string NewDBName {
      get { return _newDBName; }
      set {
        _newDBName = value;
        OnPropertyChanged();
      }
    }
    public string NewTableName {
      get { return _newTableName; }
      set {
        _newTableName = value;
        OnPropertyChanged();
      }
    }
    public string NewColumnName {
      get { return _newColumnName; }
      set {
        _newColumnName = value;
        OnPropertyChanged();
      }
    }
    public ObservableCollection<BCFile> Files {
      get { return BC_Core.Files; }
    }
    public BCFile SelectedFile {
      get { return _selectedFile; }
      set {
        if (_selectedFile != null)
          _selectedFile.IsSelected = false;
        _selectedFile = value;
        if (value != null)
          value.IsSelected = true;
        OnPropertyChanged();
        OnPropertyChanged("Databases");
        if (Databases!=null&& Databases.Count > 0)
          SelectedDatabase = Databases[0];
      }
    }
    public ObservableCollection<BCDatabase> Databases {
      get { return SelectedFile?.Databases; }
    }
    public BCDatabase SelectedDatabase {
      get { return _selectedDatabase; }
      set {
        if (_selectedDatabase != null)
          _selectedDatabase.IsSelected = false;
        _selectedDatabase = value;
        if (value != null)
          value.IsSelected = true;
        OnPropertyChanged();
        OnPropertyChanged("Tables");
        if (Tables!=null&& Tables.Count > 0)
          SelectedTable = Tables[0];
      }
    }
    public ObservableCollection<BCTable> Tables {
      get { return SelectedDatabase?.Tables; }
    }
    public BCTable SelectedTable {
      get { return _selectedTable; }
      set {
        if (_selectedTable != null)
          _selectedTable.IsSelected = false;
        _selectedTable = value;
        if (value != null)
          value.IsSelected = true;
        OnPropertyChanged();
        OnPropertyChanged("Columns");
        if (Columns!=null&& Columns.Count > 0)
          SelectedColumn = Columns[0];
      }
    }
    public ObservableCollection<BCColumn> Columns {
      get { return SelectedTable?.Columns; }
    }
    public BCColumn SelectedColumn {
      get { return _selectedColumn; }
      set {
        if (_selectedColumn != null)
          _selectedColumn.IsSelected = false;
        _selectedColumn = value;
        if (value != null) {
          value.IsSelected = true;
          if (value.DataType.Darstellung == "Verweis") {
            VerwDB = value.Reference.Database;
          }
        }
        OnPropertyChanged();
      }
    }
    public ObservableCollection<DataType> DataTypes {
      get { return BC_Core.DataTypes; }
    }
    public DataType SelectedDataType {
      get { return _selectedDataType; }
      set {
        _selectedDataType = value;
        OnPropertyChanged();
      }
    }
    public ObservableCollection<TemplateTarget> TemplateTargets {
      get { return BC_Core.TemplateTargets; }
    }
    public TemplateTarget SelectedTemplateTarget {
      get { return _selectedTemplateTarget; }
      set {
        _selectedTemplateTarget = value;
        OnPropertyChanged();
      }
    }
    public ObservableCollection<BCTemplate> Templates {
      get { return BC_Core.Templates; }
    }
    public BCTemplate SelectedTemplate {
      get { return _selectedTemplate; }
      set {
        _selectedTemplate = value;
        OnPropertyChanged();
      }
    }
    public BCDatabase VerwDB {
      get { return _verwDB; }
      set {
        _verwDB = value;
        OnPropertyChanged();
      }
    }
    #endregion fields and properties

    #region ViewModels
    #endregion ViewModels

    #region WPFCommands
    private WPFCommand _cmdCloseProgram;
    private WPFCommand _cmdRefresh;
    private WPFCommand _cmdDebug;
    private WPFCommand _cmdCreateResults;
    private WPFCommand _cmdImport;
    private WPFCommand _cmdSelectFile;
    private WPFCommand _cmdSave;
    private WPFCommand _cmdSaveChanges;
    private WPFCommand _cmdCreateFile;
    private WPFCommand _cmdDeleteFile;
    private WPFCommand _cmdCreateDatabase;
    private WPFCommand _cmdDeleteDatabase;
    private WPFCommand _cmdCreateTable;
    private WPFCommand _cmdDeleteTable;
    private WPFCommand _cmdCreateColumn;
    private WPFCommand _cmdDeleteColumn;
    private WPFCommand _cmdSaveTemplate;
    private WPFCommand _cmdCreateTemplate;
    private WPFCommand _cmdDeleteTemplate;

    public WPFCommand CmdCloseProgram { get { return _cmdCloseProgram; } }
    public WPFCommand CmdRefresh { get { return _cmdRefresh; } }
    public WPFCommand CmdDebug { get { return _cmdDebug; } }
    public WPFCommand CmdCreateResults { get { return _cmdCreateResults; } }
    public WPFCommand CmdImport { get { return _cmdImport; } }
    public WPFCommand CmdSelectFile { get { return _cmdSelectFile; } }
    public WPFCommand CmdSave { get { return _cmdSave; } }
    public WPFCommand CmdSaveChanges { get { return _cmdSaveChanges; } }
    public WPFCommand CmdCreateFile { get { return _cmdCreateFile; } }
    public WPFCommand CmdDeleteFile { get { return _cmdDeleteFile; } }
    public WPFCommand CmdCreateDatabase { get { return _cmdCreateDatabase; } }
    public WPFCommand CmdDeleteDatabase { get { return _cmdDeleteDatabase; } }
    public WPFCommand CmdCreateTable { get { return _cmdCreateTable; } }
    public WPFCommand CmdDeleteTable { get { return _cmdDeleteTable; } }
    public WPFCommand CmdCreateColumn { get { return _cmdCreateColumn; } }
    public WPFCommand CmdDeleteColumn { get { return _cmdDeleteColumn; } }
    public WPFCommand CmdSaveTemplate { get { return _cmdSaveTemplate; } }
    public WPFCommand CmdCreateTemplate { get { return _cmdCreateTemplate; } }
    public WPFCommand CmdDeleteTemplate { get { return _cmdDeleteTemplate; } }

    private void CreateCommands() {
      _cmdCloseProgram    = new WPFCommandVoid(CloseProgram, CanCloseProgram);
      _cmdRefresh         = new WPFCommandVoid(Btn_Refresh);
      _cmdDebug           = new WPFCommandVoid(Btn_Debug);
      _cmdCreateResults   = new WPFCommandVoid(Btn_CreateResults );
      _cmdImport          = new WPFCommandVoid(Btn_Import        );
      _cmdSelectFile      = new WPFCommandVoid(Btn_SelectFile    );
      _cmdSave            = new WPFCommandVoid(Btn_Save          );
      _cmdSaveChanges     = new WPFCommandVoid(Btn_SaveChanges   );
      _cmdCreateFile      = new WPFCommandVoid(Btn_CreateFile    );
      _cmdDeleteFile      = new WPFCommandVoid(Btn_DeleteFile    );
      _cmdCreateDatabase  = new WPFCommandVoid(Btn_CreateDatabase);
      _cmdDeleteDatabase  = new WPFCommandVoid(Btn_DeleteDatabase);
      _cmdCreateTable     = new WPFCommandVoid(Btn_CreateTable   );
      _cmdDeleteTable     = new WPFCommandVoid(Btn_DeleteTable   );
      _cmdCreateColumn    = new WPFCommandVoid(Btn_CreateColumn  );
      _cmdDeleteColumn    = new WPFCommandVoid(Btn_DeleteColumn  );
      _cmdSaveTemplate    = new WPFCommandVoid(Btn_SaveTemplate  );
      _cmdCreateTemplate  = new WPFCommandVoid(Btn_CreateTemplate);
      _cmdDeleteTemplate  = new WPFCommandVoid(Btn_DeleteTemplate);
    }
    #endregion WPFCommands

    #region constructors
    public BC_GUI_ViewModel() {
      // set properties
      _newDateiPfad = "";
      _newFileName = "File-Name";
      _newDBName = "DB-Name";
      _newTableName = "Tabelle-Name";
      _newColumnName = "Spalte-Name";
      // set viewmodels
      if (Templates.Count > 0)
        SelectedTemplate = Templates[0];
      // load commands
      CreateCommands();
      // load gui-texts
      // start init
      int init = 0;
      try {
        init = Init();
        if (init != 0) {
          throw new VerificationException("Error at creating <BC_GUI_ViewModel>. Error:" + init);
        }
      }
      catch (Exception e) {
        string className = "BC_GUI_ViewModel";
        string methodName = "Constructor";
        string errMsg = "Error at constructor of <" + className + ">. Error:" + init;
        throw ErrorHandler.GetErrorException(e, ErrorType.CreationError, className, methodName, errMsg, "XYZ", true);
      }
    }
    #endregion constructors

    #region Init-Methods
    public int Init() {
      // load (test) data
      // do stuff
      return 0;
    }
    #endregion Init-Methods

    #region Btn-Methods
    private void Btn_Refresh() {
      if (GetConfirmation("Möchtest du wirklich alles neu laden?" +
        "\nDadurch gehen ungespeicherte Änderungen verloren.")) {
        BC_Core.Refresh();
        if (Templates.Count > 0)
          SelectedTemplate = Templates[0];
        NewDateiPfad = "";
        NewFileName = "File-Name";
        NewDBName = "DB-Name";
        NewTableName = "Tabelle-Name";
        NewColumnName = "Spalte-Name";
        OnPropertyChanged("Files");
        OnPropertyChanged("SelectedFile");
        OnPropertyChanged("Databases");
        OnPropertyChanged("SelectedDatabase");
        OnPropertyChanged("Tables");
        OnPropertyChanged("SelectedTable");
        OnPropertyChanged("Columns");
        OnPropertyChanged("SelectedColumn");
        OnPropertyChanged("DataTypes");
        OnPropertyChanged("SelectedDataType");
        OnPropertyChanged("Templates");
        OnPropertyChanged("SelectedTemplate");
        OnPropertyChanged("VerwDB");
        OnPropertyChanged("VerwTable");
      }
    }
    private void Btn_Debug() {
      Console.WriteLine("DEBUG");
    }
    private void Btn_CreateResults() {
      MessageBox.Show("ToDo: Btn_CreateResults", "ToDo", MessageBoxButton.OK);
    }
    private void Btn_Import() {
      if (!String.IsNullOrWhiteSpace(NewDateiPfad)) {
        if (!BC_Core.ImportFile(NewDateiPfad)) {
          ShowError("Die Datei konnte nicht importiert werden.");
        }
        else {
          OnPropertyChanged("Files");
        }
      }
    }
    private void Btn_SelectFile() {
      OpenFileDialog openFileDialog = new OpenFileDialog();
      openFileDialog.Filter = "CSV files|*.csv;*.CSV";
      openFileDialog.Filter += "|Excel files|*.xlsx";
      openFileDialog.Filter += ";*.xls";
      openFileDialog.Filter += ";*.xlsm";
      openFileDialog.Filter += "|All files (*.*)|*.*";
      openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
      if (openFileDialog.ShowDialog() == true) {
        NewDateiPfad = openFileDialog.FileName;
      }
    }
    private void Btn_Save() {
      MessageBox.Show("ToDo: Btn_Save", "ToDo", MessageBoxButton.OK);
    }
    private void Btn_SaveChanges() {
      if (GetConfirmation("Möchtest du alle Änderungen speichern?")) {
        BC_Core.SaveChanges();
      }
    }
    private void Btn_CreateFile() {
      BCFile f = BC_Core.CreateNewFile(NewFileName);
      if (f == null) {
        ShowError("Die Datei konnte nicht erstellt werden.");
        return;
      }
      OnPropertyChanged("Files");
      SelectedFile = f;
      NewFileName = "File-Name";
    }
    private void Btn_DeleteFile() {
      if (SelectedFile != null) {
        if (GetConfirmation("Möchtest du diese Datei wirklich löschen?")) {
          if (!BC_Core.DeleteFile(SelectedFile)) {
            ShowError("Die Datei konnte nicht gelöscht werden.");
          }
        }
      }
    }
    private void Btn_CreateDatabase() {
      if (SelectedFile != null) {
        BCDatabase db = BC_Core.CreateNewDatabase(SelectedFile, NewDBName);
        if (db == null) {
          ShowError("Die Datenbank konnte nicht erstellt werden.");
          return;
        }
        OnPropertyChanged("Databases");
        SelectedDatabase = db;
        NewDBName = "DB-Name";
      }
    }
    private void Btn_DeleteDatabase() {
      if (SelectedDatabase != null) {
        if (GetConfirmation("Möchtest du diese Datenbank wirklich löschen?")) {
          if (!BC_Core.DeleteDatabase(SelectedDatabase)) {
            ShowError("Die Datenbank konnte nicht gelöscht werden.");
          }
        }
      }
    }
    private void Btn_CreateTable() {
      if (SelectedDatabase != null) {
        BCTable tab = BC_Core.CreateNewTable(SelectedDatabase, NewTableName);
        if (tab == null) {
          ShowError("Die Tabelle konnte nicht erstellt werden.");
          return;
        }
        OnPropertyChanged("Tables");
        SelectedTable = tab;
        NewTableName = "Tabelle-Name";
      }
    }
    private void Btn_DeleteTable() {
      if (SelectedTable != null) {
        if (GetConfirmation("Möchtest du diese Tabelle wirklich löschen?")) {
          if (!BC_Core.DeleteTable(SelectedTable)) {
            ShowError("Die Tabelle konnte nicht gelöscht werden.");
          }
        }
      }
    }
    private void Btn_CreateColumn() {
      if (SelectedTable != null) {
        BCColumn col = BC_Core.CreateNewColumn(SelectedTable, NewColumnName);
        if (col == null) {
          ShowError("Die Spalte konnte nicht erstellt werden.");
          return;
        }
        OnPropertyChanged("Columns");
        SelectedColumn = col;
        NewColumnName = "Spalte-Name";
      }
    }
    private void Btn_DeleteColumn() {
      if (SelectedColumn != null) {
        if (GetConfirmation("Möchtest du diese Spalte wirklich löschen?")) {
          if (!BC_Core.DeleteColumn(SelectedColumn)) {
            ShowError("Die Spalte konnte nicht gelöscht werden.");
          }
        }
      }
    }
    private void Btn_SaveTemplate() {
      if (SelectedTemplate != null) {
        if (!BC_Core.Save(SelectedTemplate)) {
          ShowError("Das Template konnte nicht gespeichert werden.");
        }
        OnPropertyChanged("Templates");
        OnPropertyChanged("SelectedTemplate");
      }
    }
    private void Btn_CreateTemplate() {
      BCTemplate tem = BC_Core.CreateNewTemplate("NewTemplate");
      if (tem == null) {
        ShowError("Das Template konnte nicht erstellt werden.");
        return;
      }
      OnPropertyChanged("Templates");
      SelectedTemplate = tem;
    }
    private void Btn_DeleteTemplate() {
      if (SelectedTemplate != null) {
        if (GetConfirmation("Möchtest du dieses Template wirklich löschen?")) {
          if (!BC_Core.DeleteTemplate(SelectedTemplate)) {
            ShowError("Das Template konnte nicht gelöscht werden.");
          }
        }
      }
      if (Templates.Count > 0)
        SelectedTemplate = Templates[0];
    }
    #endregion Btn-Methods

    #region Database-Methods
    #endregion Database-Methods

    #region Class-Methods
    private void CloseProgram() {
      // shutdown
      Debugger.WriteLineAbsolut("\n\nShutting down...\n\n");
      Environment.Exit(0);
    }
    private bool CanCloseProgram() {
      if (!BC_Core.UnsavedChanges()) {
        return true;
      }
      else {
        if (GetConfirmation("Es gibt noch ungespeicherte Änderungen." +
          "\nMöchtest du das Programm dennoch beenden?")) {
          return true;
        }
      }
      return false;
    }
    #endregion Class-Methods

    #region Helper-Methods
    private void ShowError(string message) {
      MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
    private bool GetConfirmation(string message) {
      if (MessageBoxResult.Yes == MessageBox.Show(message, "Bestätigen", MessageBoxButton.YesNo, MessageBoxImage.Question))
        return true;
      return false;
    }
    #endregion Helper-Methods

    #region GUI-Language
    #endregion GUI-Language

  }

}
