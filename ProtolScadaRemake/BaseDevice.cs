// BaseDevice.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

public enum DeviceState
{
    Closed,
    Moving,
    Opened,
    Fault,
    Unknown
}

public abstract class BaseDevice : INotifyPropertyChanged
{
    private DeviceState _state;
    private Point _position;
    private string _tagName;

    public DeviceState State
    {
        get => _state;
        set
        {
            _state = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(StateImage));
        }
    }

    public Point Position
    {
        get => _position;
        set
        {
            _position = value;
            OnPropertyChanged();
        }
    }

    public string TagName
    {
        get => _tagName;
        set
        {
            _tagName = value;
            OnPropertyChanged();
        }
    }

    public abstract string StateImage { get; }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}