using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhoneExtractVerify.Api.Services.Interface
{
    public interface IMediatorService
    {
        Task<List<string>> ProcessPhoneNumber(byte[] imageBytes);
    }
}
