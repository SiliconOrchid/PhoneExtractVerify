using System.Collections.Generic;

namespace PhoneExtractVerify.Api.Service.Interface
{
    public interface IWordProcessingService
    {
        WordProcessingService AddWords(List<string> listWords);
        WordProcessingService ExtractWordsWithNumbers();
        WordProcessingService GetCandidatePhoneNumbers();
        WordProcessingService GetMinLengthNumbers();
    }
}