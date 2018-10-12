using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhoneExtractVerify.Api.Services.Interface
{
    public interface IAzureComputerVisionHelperService
    {
        Task<string> ExtractPrintedText(byte[] imageBytes);
        Task<string> ReadHandwrittenText(byte[] imageBytes);

        List<string> ExtractWords(string jsonResponse);
    }
}