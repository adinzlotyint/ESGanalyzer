namespace ESGanalyzer.Backend.Services {
    public interface IParseService {
        Task<string> ExtractTextAsync(IFormFile file);
    }
}
