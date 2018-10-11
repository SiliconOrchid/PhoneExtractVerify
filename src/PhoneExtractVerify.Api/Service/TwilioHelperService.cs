using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Twilio;
using Twilio.Rest.Lookups.V1;

using PhoneExtractVerify.Api.Models;
using PhoneExtractVerify.Api.Service.Interface;

namespace PhoneExtractVerify.Api.Service
{
    public class TwilioHelperService : ITwilioHelperService
    {
        private TwilioCredentials _twilioCredentials;

        public TwilioHelperService(IOptions<TwilioCredentials> account)
        {
            _twilioCredentials = account.Value ?? throw new ArgumentNullException(nameof(account));

            TwilioClient.Init(_twilioCredentials.AccountSid, _twilioCredentials.AuthToken );
        }

        /// <summary>
        /// Iterate a collection of candidate phone numbers, calling private method that makes
        /// </summary>
        /// <param name="listCandidatePhoneNumbers"></param>
        /// <returns></returns>
        public async Task<List<string>> ProcessListCandidateNumbersAsync(List<string> listCandidatePhoneNumbers)
        {
            List<string> listVerifiedNumbers = new List<string>();

            var listTasks = new List<Task<string>>();

            foreach (var candidatePhoneNumber in listCandidatePhoneNumbers)
            {
                var task = VerifyWithTwilioAsync(candidatePhoneNumber);
                listTasks.Add(task);
            }

            foreach (var taskResult in await Task.WhenAll(listTasks))
            {
                listVerifiedNumbers.Add(taskResult);
            }


                //await Task.WhenAll();


                //foreach (var candidatePhoneNumber in listCandidatePhoneNumbers)
                //{
                //    string resultFromTwilio = await VerifyWithTwilioAsync(candidatePhoneNumber);
                //    if (!string.IsNullOrEmpty(resultFromTwilio))
                //    {
                //        listVerifiedNumbers.Add(resultFromTwilio);
                //    }
                //}
                return listVerifiedNumbers;
        }

        private async Task<string> VerifyWithTwilioAsync(string numberToTest)
        {
            try
            { 
                var twilioPhoneNumberResource = await PhoneNumberResource.FetchAsync(
                        pathPhoneNumber: new Twilio.Types.PhoneNumber(numberToTest)
                    );

                if (twilioPhoneNumberResource == null || twilioPhoneNumberResource.PhoneNumber == null)
                { 
                    Console.WriteLine($"TwilioHelperService : Using number '{numberToTest}', Twilio Api response was null");
                    return String.Empty;
                }

                Console.WriteLine($"TwilioHelperService : Using number '{numberToTest}', Twilio Api returned valid object:  National Format : '{twilioPhoneNumberResource.NationalFormat}' . Carrier : '{twilioPhoneNumberResource.Carrier}' . Phone Number :  {twilioPhoneNumberResource.PhoneNumber}");
                return twilioPhoneNumberResource.PhoneNumber.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TwilioHelperService : Exception : {ex.Message}");
                return String.Empty;
            }
        }



    }
}
