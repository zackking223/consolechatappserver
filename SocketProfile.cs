using System.Net.Sockets;

namespace ChatAppConsoleServer
{
    class SocketProfile
    {
        public Socket theSocket;
        public string type;
        public string receiverName;
        public string roomName;
    }
}
