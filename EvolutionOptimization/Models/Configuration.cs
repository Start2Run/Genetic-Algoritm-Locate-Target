using EvolutionOptimization.Interfaces;

namespace EvolutionOptimization.Models
{
    public class Configuration: IConfiguration
    {
        public bool HasDoubleValues { get; set; } = true;
        public int Digits { get; set; } = 2;
        public double MutateChange { get; set; } = 0.1;
        public int MaxNumberOfSteps { get; set; } = 100;
        public int NumberOfGenes { get; set; } = 3;
        public int PopSize { get; set; } = 31;
        public double MinX { get; set; } = -1.0; // aka minGene, maxGene
        public double MaxX { get; set; } = 1.0;
        public double MutateRate { get; set; } = 0.1; // likelihood that a gene is changed(0.1-.0.5)
        public double Tau { get; set; } = 0.40; // selection pressure (percent pop selected for tournament selection) (0.3-0.7)
        public int MaxGeneration { get; set; } = 100000; // loop counter
        public double ExitError { get; set; } = 1;
        public int RefreshInterval { get; set; } = 1; // The generations interval after witch to refresh the UI
    }
}
