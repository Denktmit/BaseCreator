using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VDUtils.Helper {

  public class ModelBase : INotifyPropertyChanged {

    public event PropertyChangedEventHandler PropertyChanged;

    public void OnPropertyChanged([CallerMemberName] string property = null) {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(property));
    }

    internal void RaisePropertyChanged(string prop) {
      if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(prop)); }
    }

  }

}
