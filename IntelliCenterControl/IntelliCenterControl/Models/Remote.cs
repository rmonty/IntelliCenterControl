using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text;
using IntelliCenterControl.Annotations;

namespace IntelliCenterControl.Models
{
    public class Remote : Circuit
    {
        public enum RemoteType
        {
            IS4,
            IS10,
            SPACMD,
            QT
        }

        private RemoteType _type;

        public RemoteType Type
        {
            get => _type;
            set
            {
                _type = value;
                OnPropertyChanged();
            }
        }

        public Remote(string name) : base(name, CircuitType.REMOTE)
        {

        }
    }
}
