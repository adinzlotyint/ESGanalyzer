using ESGanalyzer.Backend.DTOs;

namespace ESGanalyzer.Backend.Services {
    public interface IAuthService {
        Task<string> RegisterAsync(RegisterRequest request);
        Task<string> LoginAsync(LoginRequest request);
    }
}
