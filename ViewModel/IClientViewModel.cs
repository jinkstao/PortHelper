namespace PortHelper.ViewModel
{
    public interface IClientViewModel
    {
        #region Properties

        public string Name { get; }

        #endregion Properties
    }

    public interface IClientViewModel<out T> : IClientViewModel
    {
        #region Properties

        public T Entity { get; }

        #endregion Properties
    }
}