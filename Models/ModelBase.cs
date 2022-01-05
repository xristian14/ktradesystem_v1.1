using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace ktradesystem.Models
{
    class ModelBase : INotifyPropertyChanged
    {
        public ModelBase()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            DispatcherInvoke((Action)(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
            }));
        }

        public Dispatcher _dispatcher;

        public void DispatcherInvoke(Action action)
        {
            _dispatcher.Invoke(action);
        }
    }
}
