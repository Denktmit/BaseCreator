using System;
using System.Reflection;

namespace BaseCreator_GUI.Helper.Commands {

  public class WPFCommandBool : WPFCommand {

    #region fields and properties
    // Gibt an welche Action ausgefuehrt werden soll
    private WPFCommandType _commandType;
    // Action ist ein spezieller Delegate! (Funktionszeiger)
    protected Action<bool> _executeBool;
    public override MethodInfo MethodInfo { get { return _executeBool.Method; } }
    // Func ist ein spezieller Delegate mit einem Rueckgabewert vom Typ Boolean! (Funktionszeiger)
    protected Func<bool> _canExecute;
    // EventHandler
    public override event EventHandler CanExecuteChanged;
    #endregion fields and properties

    #region constructors
    //WPFCommandType.Bool
    public WPFCommandBool(Action<bool> execute, Func<bool> canExecute) {
      _commandType = WPFCommandType.Bool;
      this._executeBool = execute;
      this._canExecute = canExecute;
    }
    public WPFCommandBool(Action<bool> execute) : this(execute, null) { }
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
        case WPFCommandType.Bool:
          _executeBool((bool)parameter);
          break;
        default:
          throw new InvalidOperationException("Exception at WPFCommand.Execute() -> Invalid WPFCommandType!");
      }
    }
    #endregion Class-Methods

  }

}
