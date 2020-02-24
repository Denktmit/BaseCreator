using System;
using System.Collections.Generic;
using System.Security;

namespace AP_Extension.AP {

  public class CSVEnum {
    public string Description { get; set; }
    public int Offset { get; set; }
    public List<CSVEnumElement> Elements { get; set; }
    public CSVEnum(string inDescription, int inOffset) {
      Description = inDescription;
      Offset = inOffset;
      Elements = new List<CSVEnumElement>();
    }
    public void AppendElement(int inID, string inDesc, string inDtText, string inDtKuerzel) {
      Elements.Add(new CSVEnumElement(inID, this, inDesc, inDtText, inDtKuerzel));
    }
  }

}
