using ESGanalyzer.Backend.Models;

namespace ESGanalyzer.Backend.Services.Analysis {
    public class RuleBasedAnalyzer : IRuleBasedAnalyzer {
        public ESGAnalysisResult _result = new();
        private readonly IEnumerable<ICriterionAnalyzer> _analyzers;

        public RuleBasedAnalyzer(IEnumerable<ICriterionAnalyzer> analyzers) {
            _analyzers = analyzers;
        }

        public ESGAnalysisResult Analyze(string text) {
            foreach (ICriterionAnalyzer analyzer in _analyzers) {
                analyzer.Evaluate(text,_result);
            }

            return _result;
        }

    }
}
