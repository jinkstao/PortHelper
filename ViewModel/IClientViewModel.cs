using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace PortHelper.ViewModel
{
    public interface IClientViewModel
    {
        public string Name { get; }
    }

    public interface IClientViewModel<out T> : IClientViewModel
    {
        public T Entity { get; }
    }
}
