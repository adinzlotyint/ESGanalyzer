using ESGanalyzer.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace ESGanalyzer.Backend.Controllers {
    [ApiController]
    [Route("/[controller]")]
    public class UploadController : ControllerBase {
        private readonly IParseService _parseService;

        public UploadController(IParseService parseService) {
            _parseService = parseService;
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeDocx([FromForm] IFormFile file) {
            if (file == null || Path.GetExtension(file.FileName)?.ToLower() != ".docx") {
                return BadRequest("Only .docx files are supported.");
            }

            string text = await _parseService.ExtractTextAsync(file);

        } 
    }
}
