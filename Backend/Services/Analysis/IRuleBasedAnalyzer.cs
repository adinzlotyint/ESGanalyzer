using ESGanalyzer.Backend.Models;

namespace ESGanalyzer.Backend.Services.Analysis {
    public interface IRuleBasedAnalyzer {
        public ESGAnalysisResult Analyze(string text);
    }
}
