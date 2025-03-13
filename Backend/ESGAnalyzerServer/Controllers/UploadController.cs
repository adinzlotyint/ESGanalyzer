using Microsoft.AspNetCore.Mvc;

[Route("api/upload")]
[ApiController]
public class UploadController : ControllerBase {
    private readonly UploadService _uploadService;

    public UploadController(UploadService uploadService) {
        _uploadService = uploadService;
    }

    [HttpPost]
    public async Task<IActionResult> UploadFile(IFormFile file) {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var allowedExtensions = new[] { ".pdf", ".docx" };
        var fileExtension = Path.GetExtension(file.FileName).ToLower();

        if (!allowedExtensions.Contains(fileExtension))
            return BadRequest("Invalid file type. Only PDF and DOCX are allowed.");

        var filePath = Path.Combine("Uploads", file.FileName);
        Directory.CreateDirectory("Uploads");

        using var fileStream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(fileStream);
        fileStream.Close();

        await _uploadService.ScanWithWindowsDefender(filePath);
        if (System.IO.File.Exists(filePath)) { System.IO.File.Delete(filePath); }

        return Ok($"File {file.FileName} uploaded successfully.");
    }
}
