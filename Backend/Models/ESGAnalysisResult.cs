namespace ESGanalyzer.Backend.Models {
    public class ESGAnalysisResult {
        public bool HasScopeEmissions { get; set; }
        public bool HasStandardReferences { get; set; }
        public bool HasNumericConsistency { get; set; }
        public bool HasReductionTargets { get; set; }
        public bool HasClimateRiskDiscussion { get; set; }
        public bool HasEfficiencyIndicators { get; set; }

        public int GetTotalScore() {
            int score = 0;
            if (HasScopeEmissions) score++;
            if (HasStandardReferences) score++;
            if (HasNumericConsistency) score++;
            if (HasReductionTargets) score++;
            if (HasClimateRiskDiscussion) score++;
            if (HasEfficiencyIndicators) score++;
            return score;
        }
    }
}
