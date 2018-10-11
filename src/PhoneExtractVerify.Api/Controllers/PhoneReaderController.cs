using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using PhoneExtractVerify.Api.Services.Interface;


namespace PhoneExtractVerify.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhoneReaderController : Controller
    {
        private readonly IWordProcessingService _wordProcessingService;
        private readonly ITwilioHelperService _twilioHelperService;
        private readonly IAzureCognitionHelperService _azureCognitionHelperService;


        public PhoneReaderController(IWordProcessingService wordProcessingService, ITwilioHelperService twilioHelperService, IAzureCognitionHelperService azureCognitionHelperService)
        {
            _wordProcessingService = wordProcessingService ?? throw new ArgumentNullException();
            _twilioHelperService = twilioHelperService ?? throw new ArgumentNullException();
            _azureCognitionHelperService = azureCognitionHelperService ?? throw new ArgumentNullException();
        }


        [HttpPost]
        public async Task<ActionResult> Post()
        {
            // Read the image from the POSTed data stream, into a byte array
            byte[] imageBytes;
            using (var ms = new MemoryStream(2048))
            {
                await Request.Body.CopyToAsync(ms);
                imageBytes = ms.ToArray();
            }

            // Call the Azure Computer Vision service (for OCR), returning a complex json object.
            string jsonResponse = await _azureCognitionHelperService.ExtractPrintedText(imageBytes);
            //string jsonResponse = await _azureCognitionHelperService.ReadHandwrittenText(imageBytes);

            // Extract words from OCR response, discarding other information.
            List<string> listAllWords = _azureCognitionHelperService.ExtractWords(jsonResponse);

            // Filter raw list of words into a new list of candidate phone numbers.
            List<string> listCandidatePhoneNumbers = _wordProcessingService
                .AddWords(listAllWords)
                .GetCandidatePhoneNumbers()
                .ExtractWordsWithNumbers()
                .ReformatAsUKInternational()
                .GetMinLengthNumbers()
                .ListProcessedWords;

            // Call Twilio Lookup Api to verifying list of candidate phone numbers.
            List<string> listVerifiedPhoneNumbers = await _twilioHelperService.ProcessListCandidateNumbersAsync(listCandidatePhoneNumbers);

            return Ok(string.Join(" - ", listVerifiedPhoneNumbers));
        }
    }
}