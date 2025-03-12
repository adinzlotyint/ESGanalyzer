public class UploadService {
    public async Task FileHandling(string filePath) {
        Console.WriteLine($"Handling the file: {File.Exists(filePath)}");
    }
}
