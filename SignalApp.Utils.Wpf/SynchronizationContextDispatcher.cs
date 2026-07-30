namespace SignalApp.Utils.Wpf
{
    public sealed class SynchronizationContextDispatcher : IDispatcher
    {
        private readonly SynchronizationContext context;

        public SynchronizationContextDispatcher()
        {
            context = SynchronizationContext.Current
                ?? throw new InvalidOperationException("No SynchronizationContext available");
        }

        public void Invoke(Action action) => context.Send(_ => action(), null);
    }
}