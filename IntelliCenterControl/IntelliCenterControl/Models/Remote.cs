namespace IntelliCenterControl.Models
{
    public class Remote : Circuit<IntelliCenterConnection>
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
                if (_type == value) return;
                _type = value;
                OnPropertyChanged();
            }
        }

        public Remote(string name) : base(name, CircuitType.REMOTE)
        {

        }
    }
}
