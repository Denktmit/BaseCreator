using System;
using System.Collections.Generic;

namespace BaseCreator_Model.Model {

  public class DataType {

    private string _darstellung;
    private string _sQLtyp;
    private string _cStyp;
    private string _prefix;
    private string _groesse;

    public string Darstellung { get { return _darstellung; } }
    public string SQLtyp { get { return _sQLtyp; } }
    public string CStyp { get { return _cStyp; } }
    public string Prefix { get { return _prefix; } }
    public string Groesse { get { return _groesse; } }

    public DataType(string darstellung, string sqltyp, string cstyp, string prefix, string groesse) {
      _darstellung = darstellung;
      _sQLtyp = sqltyp;
      _cStyp = cstyp;
      _prefix = prefix;
      _groesse = groesse;
    }

    private static List<DataType> _dataTypes;
    public static List<DataType> DataTypes {
      get {
        if (_dataTypes == null)
          LoadDataTypes();
        return _dataTypes;
      }
    }
    private static void LoadDataTypes() {
      _dataTypes = new List<DataType>();
      _dataTypes.Add(new DataType("Zahl", "INT", "int", "n", "-"));
      _dataTypes.Add(new DataType("Kommazahl", "DECIMAL(10,2)", "double", "f", "10,2"));
      _dataTypes.Add(new DataType("Text(1)", "NVARCHAR(1)", "string", "sz", "1"));
      _dataTypes.Add(new DataType("Text(5)", "NVARCHAR(5)", "string", "sz", "5"));
      _dataTypes.Add(new DataType("Text(25)", "NVARCHAR(25)", "string", "sz", "25"));
      _dataTypes.Add(new DataType("Text(50)", "NVARCHAR(50)", "string", "sz", "50"));
      _dataTypes.Add(new DataType("Text(100)", "NVARCHAR(100)", "string", "sz", "100"));
      _dataTypes.Add(new DataType("Text(500)", "NVARCHAR(500)", "string", "sz", "500"));
      _dataTypes.Add(new DataType("Text(1000)", "NVARCHAR(1000)", "string", "sz", "1000"));
      _dataTypes.Add(new DataType("Text(5000)", "NVARCHAR(5000)", "string", "sz", "5000"));
      _dataTypes.Add(new DataType("Text(10000)", "NVARCHAR(10000)", "string", "sz", "10000"));
      _dataTypes.Add(new DataType("Zeitpunkt", "DATETIME", "DateTime", "dt", "-"));
      _dataTypes.Add(new DataType("Datum", "DATE", "DateTime", "d", "-"));
      _dataTypes.Add(new DataType("Uhrzeit", "TIME", "DateTime", "t", "-"));
      _dataTypes.Add(new DataType("Wahrheitswert", "BOOL", "bool", "b", "-"));
      _dataTypes.Add(new DataType("Verweis", "x", "x", "i", "-"));
    }
    public static DataType GetDataType(string darstellung) {
      foreach (DataType dt in DataTypes) {
        if (dt.Darstellung == darstellung)
          return dt;
      }
      return null;
    }
  }

}
