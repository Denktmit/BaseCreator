using BaseCreator_Core.Helper;
using System;
using System.Windows;
using System.Windows.Input;

namespace BaseCreator_GUI.Helper {

  public class BusyManager : ModelBase {

    private static bool _busy;
    private static string _reason;

    public static bool Busy {
      get {
        return _busy;
      }
      set {
        if (value != _busy) {
          _busy = value;
          _reason = "";
          UpdateCursor();
        }
      }
    }
    public static string Reason {
      get {
        return _reason;
      }
      set {
        _reason = value;
      }
    }

    public static void SetBusy(bool busy, string reason) {
      Busy = busy;
      Reason = Reason;
    }

    private static void UpdateCursor() {
      if (Busy) {
        SetBusyCursor();
      }
      else {
        ResetBusyCursor();
      }
    }

    public static void SetBusyCursor() {
      Application.Current.Dispatcher.Invoke(() => {
        Mouse.OverrideCursor = Cursors.Wait;
      });
    }
    public static void ResetBusyCursor() {
      Application.Current.Dispatcher.Invoke(() => {
        Mouse.OverrideCursor = null;
      });
    }

  }

}
