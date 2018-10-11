using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhoneExtractVerify.Api.Service.Interface
{
    public interface ITwilioHelperService
    {
        Task<List<string>> ProcessListCandidateNumbersAsync(List<string> listCandidatePhoneNumbers);
    }
}