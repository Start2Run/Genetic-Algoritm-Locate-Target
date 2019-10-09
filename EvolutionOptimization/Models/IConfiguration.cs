namespace EvolutionOptimization.Models
{
    public interface IConfiguration
    {
        bool HasDoubleValues { get; set; }
        int Digits { get; set; }
        double MutateChange { get; set; }
        int MaxNumberOfSteps { get; set; }
        int NumberOfGenes { get; set; }
        int PopSize { get; set; }
        double MinX { get; set; } // aka minGene, maxGene
        double MaxX { get; set; }
        double MutateRate { get; set; } // likelihood that a gene is changed(0.1-.0.5)
        double Tau { get; set; } // selection pressure (percent pop selected for tournament selection) (0.3-0.7)
        int MaxGeneration { get; set; } // loop counter
        double ExitError { get; set; }
    }
}
