using System;

namespace BaseCreator_Core.Model {

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

  }

}
