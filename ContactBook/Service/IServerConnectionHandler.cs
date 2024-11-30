using System.Threading.Tasks;

namespace ContactBook.Service;

public interface IServerConnectionHandler
{
    public Task ConnectToServer();
}