using System.Threading.Tasks;

namespace Handler.AuthN
{
    public interface ITokenHandler<in T>
    {
        Task StoreAccessToken(T allTokenNeededData);
        Task<string> GetAccessToken(T allTokenNeededData);
    }
}
