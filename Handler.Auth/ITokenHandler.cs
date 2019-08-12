using System.Threading.Tasks;

namespace Handler.Auth
{
    public interface ITokenHandler<in T>
    {
        Task StoreAccessToken(T allTokenNeededData);
        Task<string> GetAccessTokenSilently(T allTokenNeededData);

        Task<string> GetAccessTokenOnBehalfOf(T allTokenNeededData);
    }
}
