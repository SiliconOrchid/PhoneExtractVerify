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
        private readonly IMediatorService _mediatorService;

        public PhoneReaderController(IMediatorService mediatorService)
        {
            _mediatorService = mediatorService ?? throw new ArgumentNullException();
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

            try
            { 
                List<string> listVerifiedPhoneNumbers = await _mediatorService.ProcessPhoneNumber(imageBytes);
                return Ok(string.Join(" - ", listVerifiedPhoneNumbers));
            }
            catch
            {
                return StatusCode(500);
            }
        }
    }
}