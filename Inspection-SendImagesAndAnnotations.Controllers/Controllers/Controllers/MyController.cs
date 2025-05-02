using Microsoft.AspNetCore.Mvc;
using InspectionSendImagesAndAnnotations.Messages.Dtos;
using InspectionSendImagesAndAnnotations.Controllers.DtoFactory;

namespace InspectionSendImagesAndAnnotations.Controllers
{
    [ApiController]
    [Route("Api/SendAnnotationsAndImages")]
    public class MyController : BaseController
    {
        public MyController(IMessageSession messageSession, IDtoFactory dtoFactory)
            : base(messageSession, dtoFactory) { }

        [HttpPost("SendAnnotations")]
        public async Task<IActionResult> SendAnnotations([FromBody] InspectionRequest dto)
        {
            try
            {
                var response = await _messageSession.Request<InspectionResponse>(dto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error while processing the request: {ex.Message}");
            }
        }
    }

}
