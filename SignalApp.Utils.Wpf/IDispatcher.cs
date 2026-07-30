namespace SignalApp.Utils.Wpf
{
    public interface IDispatcher
    {
        void Invoke(Action action);
    }
}