using System.ComponentModel;
using System.Threading.Tasks;

namespace PortHelper.ViewModel
{
    public interface IBaseProtocolViewModel : INotifyPropertyChanged
    {
        #region Properties

        bool Connected { get; set; }

        #endregion Properties

        #region Methods

        Task OpenAsync();

        Task Send();

        #endregion Methods
    }
}