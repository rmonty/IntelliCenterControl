using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using IntelliCenterControl.Annotations;

namespace IntelliCenterControl.Models
{
    public class IntelliCenterConnection : INotifyPropertyChanged
    {
        //
        // Summary:
        //     Describes the current state of the Connection
        public enum ConnectionState
        {
            //
            // Summary:
            //     The connection is disconnected.
            Disconnected = 0,
            //
            // Summary:
            //     The connection is connected.
            Connected = 1,
            //
            // Summary:
            //     The connection is connecting.
            Connecting = 2,
            //
            // Summary:
            //     The connection is reconnecting.
            Reconnecting = 3
        }

        private ConnectionState _state;

        public ConnectionState State
        {
            get => _state;
            set
            {
                if (value == _state) return;
                _state = value;
                OnPropertyChanged();
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
