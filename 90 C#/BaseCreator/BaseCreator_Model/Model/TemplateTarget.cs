using System;
using System.Collections.Generic;

namespace BaseCreator_Model.Model {

  public class TemplateTarget {

    private string _darstellung;
    private string _targetClass;

    public string Darstellung { get { return _darstellung; } }
    public string TargetClass { get { return _targetClass; } }

    public TemplateTarget(string darstellung, string targetClass) {
      _darstellung = darstellung;
      _targetClass = targetClass;
    }

    private static List<TemplateTarget> _templateTargets;
    public static List<TemplateTarget> TemplateTargets {
      get {
        if (_templateTargets == null)
          LoadTemplateTargets();
        return _templateTargets;
      }
    }
    private static void LoadTemplateTargets() {
      _templateTargets = new List<TemplateTarget>();
      _templateTargets.Add(new TemplateTarget("Keins", "-"));
      _templateTargets.Add(new TemplateTarget("File", "BCFile"));
      _templateTargets.Add(new TemplateTarget("Database", "BCDatabase"));
      _templateTargets.Add(new TemplateTarget("Table", "BCTable"));
      _templateTargets.Add(new TemplateTarget("Column", "BCColumn"));
    }
    public static TemplateTarget GetTemplateTarget(string darstellung) {
      foreach (TemplateTarget dt in TemplateTargets) {
        if (dt.Darstellung == darstellung)
          return dt;
      }
      return null;
    }

  }

}
