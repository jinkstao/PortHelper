using System.ComponentModel;
using System.Threading.Tasks;

namespace PortHelper.ViewModel
{
    public interface IBaseProtocolViewModel : INotifyPropertyChanged
    {
        bool Connected { get; set; }

        Task Send();

        Task StartListeningAsync();
    }
}