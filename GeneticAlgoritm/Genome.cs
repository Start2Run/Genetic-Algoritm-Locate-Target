namespace GeneticAlgorithm
{
    public class Genome
    {
        public double[] Genes;
        public Genome(int numGenes)
        {
            Genes = new double[numGenes];
            for (var i = 0; i < numGenes; i++)
                Genes[i] = 0;
        }
    }
}