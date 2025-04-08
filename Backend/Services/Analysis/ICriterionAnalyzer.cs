using ESGanalyzer.Backend.Models;

namespace ESGanalyzer.Backend.Services.Analysis {
    public interface ICriterionAnalyzer {
        public void Evaluate(string text, ESGAnalysisResult result);
    }
}
