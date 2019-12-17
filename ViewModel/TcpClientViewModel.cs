using System.Net;
using System.Net.Sockets;

namespace PortHelper.ViewModel
{
    public class TcpClientViewModel : IClientViewModel<Socket>
    {
        #region Constructors

        public TcpClientViewModel(Socket socket)
        {
            Entity = socket;
        }

        #endregion Constructors

        #region Properties

        public Socket Entity { get; }

        public string Name
        {
            get
            {
                var ipEndPoint = Entity.RemoteEndPoint as IPEndPoint;
                return $"{ipEndPoint?.Address}:{ipEndPoint?.Port}";
            }
        }

        #endregion Properties
    }
}