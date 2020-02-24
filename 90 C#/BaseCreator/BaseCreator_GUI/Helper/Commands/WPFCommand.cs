using System;
using System.Windows.Input;
using System.Reflection;
using BaseCreator_Core.Helper;
using BaseCreator_GUI.Helper;

namespace BaseCreator_GUI.Helper.Commands {

  public abstract class WPFCommand : ICommand {

    public abstract MethodInfo MethodInfo { get; }
    public abstract event EventHandler CanExecuteChanged;
    public abstract void RaiseCanExecuteChanged();
    public abstract bool CanExecute(object parameter);
    //public abstract void Execute(object parameter);
    public void Execute(object parameter) {
      string methodCall = MethodInfo.Name + "(" + ((parameter == null) ? "" : parameter.ToString()) + ")";
      if (Debugger.DebugMethods) {
        Console.WriteLine(" + " + methodCall);
      }
      BusyManager.SetBusy(true, methodCall);
      SpecialisedExecute(parameter);
      BusyManager.SetBusy(false, "");
    }
    protected abstract void SpecialisedExecute(object parameter);

  }

}
