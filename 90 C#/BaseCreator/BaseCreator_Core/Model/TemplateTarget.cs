using System;

namespace BaseCreator_Core.Model {

  public class TemplateTarget {

    private string _darstellung;
    private string _targetClass;

    public string Darstellung { get { return _darstellung; } }
    public string TargetClass { get { return _targetClass; } }

    public TemplateTarget(string darstellung, string targetClass) {
      _darstellung = darstellung;
      _targetClass = targetClass;
    }

  }

}
