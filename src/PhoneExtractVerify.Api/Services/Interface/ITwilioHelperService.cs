using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhoneExtractVerify.Api.Services.Interface
{
    public interface ITwilioHelperService
    {
        Task<List<string>> ProcessListCandidateNumbersAsync(List<string> listCandidatePhoneNumbers);
    }
}