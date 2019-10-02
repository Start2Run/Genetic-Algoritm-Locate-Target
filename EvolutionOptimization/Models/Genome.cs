using GeneticAlgorithm.Interfaces;

namespace EvolutionOptimization.Models
{
    public class Genome: IGenome
    {
        public double[] Genes { get; set; }
        public Genome(int numGenes)
        {
            Genes = new double[numGenes];
            for (var i = 0; i < numGenes; i++)
                Genes[i] = 0;
        }
    }
}