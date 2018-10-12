using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using PhoneExtractVerify.Api.Services.Interface;


namespace PhoneExtractVerify.Api.Services
{
    public class MediatorService : IMediatorService
    {
        private readonly IWordProcessingService _wordProcessingService;
        private readonly ITwilioHelperService _twilioHelperService;
        private readonly IAzureComputerVisionHelperService _azureComputerVisionHelperService;

        public MediatorService(IWordProcessingService wordProcessingService, ITwilioHelperService twilioHelperService, IAzureComputerVisionHelperService azureComputerVisionHelperService)
        {
            _wordProcessingService = wordProcessingService ?? throw new ArgumentNullException();
            _twilioHelperService = twilioHelperService ?? throw new ArgumentNullException();
            _azureComputerVisionHelperService = azureComputerVisionHelperService ?? throw new ArgumentNullException();
        }

        public async Task<List<string>> ProcessPhoneNumber(byte[] imageBytes)
        {
            // Call the Azure Computer Vision service (for OCR), returning a complex json object.
            string jsonResponse = await _azureComputerVisionHelperService.ExtractPrintedText(imageBytes);
            //string jsonResponse = await _azureCognitionHelperService.ReadHandwrittenText(imageBytes);

            // Extract words from OCR response, discarding other information.
            List<string> listAllWords = _azureComputerVisionHelperService.ExtractWords(jsonResponse);

            // Filter raw list of words into a new list of candidate phone numbers.
            List<string> listCandidatePhoneNumbers = _wordProcessingService
                .AddWords(listAllWords)
                .GetCandidatePhoneNumbers()
                .ExtractWordsWithNumbers()
                .ReformatAsUKInternational()
                .GetMinLengthNumbers()
                .ListProcessedWords;

            // Call Twilio Lookup Api to verify list of candidate phone numbers.
            List<string> listVerifiedPhoneNumbers = await _twilioHelperService.ProcessListCandidateNumbersAsync(listCandidatePhoneNumbers);

            return listVerifiedPhoneNumbers;
        }
    }
}
