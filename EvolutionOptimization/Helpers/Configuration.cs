namespace EvolutionOptimization.Helpers
{
    public static class Configuration
    {
        public static bool HasDoubleValues = true;
        public static int Digits = 2;
        public static double MutateChange = 0.1;
        public static int MaxNumberOfSteps = 100;
        public static int NumberOfGenes = 3;
        public static int PopSize = 31;
        public static double MinX = -1.0; // aka minGene, maxGene
        public static double MaxX = 1.0;
        public static double MutateRate = 0.1; // likelihood that a gene is changed(0.1-.0.5)
        public static double Tau = 0.40; // selection pressure (percent pop selected for tournament selection) (0.3-0.7)
        public static int MaxGeneration = 100000; // loop counter
        public static double ExitError = 1;
    }
}
