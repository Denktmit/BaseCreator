using System;
using System.Reflection;

namespace BaseCreator_GUI.Helper.Commands {

  public class WPFCommandObject : WPFCommand {

    #region fields and properties
    // Gibt an welche Action ausgefuehrt werden soll
    private WPFCommandType _commandType;
    // Action ist ein spezieller Delegate! (Funktionszeiger)
    protected Action<object> _executeObject;
    public override MethodInfo MethodInfo { get { return _executeObject.Method; } }
    // Func ist ein spezieller Delegate mit einem Rueckgabewert vom Typ Boolean! (Funktionszeiger)
    protected Func<bool> _canExecute;
    // EventHandler
    public override event EventHandler CanExecuteChanged;
    #endregion fields and properties

    #region constructors
    // WPFCommandType.Object
    public WPFCommandObject(Action<object> execute, Func<bool> canExecute) {
      _commandType = WPFCommandType.Object;
      this._executeObject = execute;
      this._canExecute = canExecute;
    }
    public WPFCommandObject(Action<object> execute) : this(execute, null) { }
    #endregion constructors

    #region static_methods
    #endregion static_methods

    #region Class-Methods
    public override void RaiseCanExecuteChanged() {
      if (CanExecuteChanged != null) {
        CanExecuteChanged(this, new EventArgs());
      }
      // oder CanExecuteChanged.Invoke(this, new EventArgs());
    }
    public override bool CanExecute(object parameter) {
      return (_canExecute != null) ? _canExecute() : true;
    }
    protected override void SpecialisedExecute(object parameter) {
      switch (_commandType) {
        case WPFCommandType.Object:
          _executeObject(parameter);
          break;
        default:
          throw new InvalidOperationException("Exception at WPFCommand.Execute() -> Invalid WPFCommandType!");
      }
    }
    #endregion Class-Methods

  }

}
