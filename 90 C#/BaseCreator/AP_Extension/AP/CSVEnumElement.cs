using System;
using System.Security;

namespace AP_Extension.AP {

  public class CSVEnumElement {
    public int ID { get; set; }
    public CSVEnum Enum { get; set; }
    public string Description { get; set; }
    public string DtText { get; set; }
    public string DtKuerzel { get; set; }
    public CSVEnumElement(int inID, CSVEnum inEnum, string inDescription, string inDtText, string inDtKuerzel) {
      ID = inID;
      Enum = inEnum;
      Description = inDescription;
      DtText = inDtText;
      DtKuerzel = inDtKuerzel;
    }
  }

}
