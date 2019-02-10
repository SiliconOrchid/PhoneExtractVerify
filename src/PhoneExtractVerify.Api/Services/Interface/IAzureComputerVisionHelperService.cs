using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhoneExtractVerify.Api.Services.Interface
{
    public interface IAzureComputerVisionHelperService
    {
        Task<string> RecognisePrintedText(byte[] imageBytes);
        Task<string> RecogniseHandwrittenText(byte[] imageBytes);

        List<string> ExtractWordsFromPrintedResult(string jsonResponse);
        List<string> ExtractWordsFromHandwrittenResult(string jsonResponse);
    }
}