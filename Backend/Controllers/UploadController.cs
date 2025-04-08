using ESGanalyzer.Backend.Services;
using ESGanalyzer.Backend.Services.Analysis;
using Microsoft.AspNetCore.Mvc;

namespace ESGanalyzer.Backend.Controllers {
    [ApiController]
    [Route("/[controller]")]
    public class UploadController : ControllerBase {
        private readonly IParseService _parseService;
        private readonly IRuleBasedAnalyzer _ruleBasedAnalyzer;

        public UploadController(IParseService parseService, IRuleBasedAnalyzer analyzer) {
            _parseService = parseService;
            _ruleBasedAnalyzer = analyzer;
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeDocx([FromForm] IFormFile file) {
            if (file == null || Path.GetExtension(file.FileName)?.ToLower() != ".docx") {
                return BadRequest("Only .docx files are supported.");
            }

            string text = await _parseService.ExtractTextAsync(file);
            return Ok(_ruleBasedAnalyzer.Analyze(text));
        } 
    }
}
