using Task = System.Threading.Tasks.Task;

namespace Microsoft.Maui.ApplicationModel
{
    partial class AppActionsImplementation : IAppActions
    {
        public bool IsSupported =>
            false;

        public Task<IEnumerable<AppAction>> GetAsync()
        {
            throw new NotImplementedException();
        }


        public Task SetAsync(IEnumerable<AppAction> actions)
        {
            throw new NotImplementedException();
        }


        public event EventHandler<AppActionEventArgs> AppActionActivated;
    }
}
