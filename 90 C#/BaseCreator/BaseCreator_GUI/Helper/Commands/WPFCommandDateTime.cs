using System;
using System.Reflection;

namespace BaseCreator_GUI.Helper.Commands {

  public class WPFCommandDateTime : WPFCommand {

    #region fields and properties
    // Gibt an welche Action ausgefuehrt werden soll
    private WPFCommandType _commandType;
    // Action ist ein spezieller Delegate! (Funktionszeiger)
    protected Action<DateTime> _executeDateTime;
    public override MethodInfo MethodInfo { get { return _executeDateTime.Method; } }
    // Func ist ein spezieller Delegate mit einem Rueckgabewert vom Typ Boolean! (Funktionszeiger)
    protected Func<bool> _canExecute;
    // EventHandler
    public override event EventHandler CanExecuteChanged;
    #endregion fields and properties

    #region constructors
    // WPFCommandType.DateTime
    public WPFCommandDateTime(Action<DateTime> execute, Func<bool> canExecute) {
      _commandType = WPFCommandType.DateTime;
      this._executeDateTime = execute;
      this._canExecute = canExecute;
    }
    public WPFCommandDateTime(Action<DateTime> execute) : this(execute, null) { }
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
        case WPFCommandType.DateTime:
          _executeDateTime((DateTime)parameter);
          break;
        default:
          throw new InvalidOperationException("Exception at WPFCommand.Execute() -> Invalid WPFCommandType!");
      }
    }
    #endregion Class-Methods

  }

}
