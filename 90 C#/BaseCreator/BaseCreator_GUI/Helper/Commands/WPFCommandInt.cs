using System;
using System.Reflection;

namespace BaseCreator_GUI.Helper.Commands {

  public class WPFCommandInt : WPFCommand {

    #region fields and properties
    // Gibt an welche Action ausgefuehrt werden soll
    private WPFCommandType _commandType;
    // Action ist ein spezieller Delegate! (Funktionszeiger)
    protected Action<int> _executeInt;
    public override MethodInfo MethodInfo { get { return _executeInt.Method; } }
    // Func ist ein spezieller Delegate mit einem Rueckgabewert vom Typ Boolean! (Funktionszeiger)
    protected Func<bool> _canExecute;
    // EventHandler
    public override event EventHandler CanExecuteChanged;
    #endregion fields and properties

    #region constructor
    // WPFCommandType.Int
    public WPFCommandInt(Action<int> execute, Func<bool> canExecute) {
      _commandType = WPFCommandType.Int;
      this._executeInt = execute;
      this._canExecute = canExecute;
    }
    public WPFCommandInt(Action<int> execute) : this(execute, null) { }
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
        case WPFCommandType.Int:
          int i = 0;
          if(parameter is int) {
            _executeInt((int)parameter);
            break;
          }
          if(parameter is string) {
            _executeInt(Int32.Parse(parameter.ToString()));
            break;
          }
          _executeInt((int)parameter);
          break;
        default:
          throw new InvalidOperationException("Exception at WPFCommand.Execute() -> Invalid WPFCommandType!");
      }
    }
    #endregion Class-Methods

  }

}
