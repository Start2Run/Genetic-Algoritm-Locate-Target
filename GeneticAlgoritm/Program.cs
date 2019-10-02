using System;
using System.Linq;

namespace GeneticAlgorithm
{
    class GeneticAlgorithm
    {
        static readonly double _mutateChange = 0.1;
        static readonly int maxNumberOfSteps = 100;
        private static double _reference;

        private static void Main(string[] args)
        {
            var chromosomeMem = 3; // problem dimension = number genes
            var target = new Genome(chromosomeMem) { Genes = new [] { 5.0, 5.0, 5.0 } };

            var popSize = 31;
            var minX = -1.0; // aka minGene, maxGene
            var maxX = 1.0;
            var mutateRate = 0.1; // likelihood that a gene is changed(0.1-.0.5)
            var mutateChange = _mutateChange; // controls magnitude of mutation change(0.1-0.001)
            var tau = 0.40; // selection pressure (percent pop selected for tournament selection) (0.3-0.7)

            var maxGeneration = 100000; // loop counter

            var perfectIndividual = OptimalIndividual(minX, maxX, mutateRate, mutateChange, target);
            _reference = perfectIndividual.Error; // early exit if meet this error or less

            double exitError = 0; // 3% exit error

            Console.WriteLine("\nBeginning evolutionary optimization loop");
            var bestSolution = Individual.Solve(target, popSize, minX, maxX, mutateRate, mutateChange, tau, maxGeneration, exitError);

            Console.WriteLine("\n\n Accuracy value at best solution = " + bestSolution.Error.ToString("F6") + "%");

            Console.WriteLine("\nEnd Evolutionary Optimization demo\n");
            Console.ReadLine();
        } // Main

        static Individual OptimalIndividual(double minGene, double maxGene, double mutateRate, double mutateChange, Genome target)
        {
            var perfectIndividual = new Individual(target, minGene, maxGene, mutateRate, mutateChange);
            perfectIndividual.Chromosome.Clear();
            var n = (int)(target.Genes.Max(gene => Math.Abs(gene) / mutateChange));
            for (var i = 0; i < n; i++)
            {
                perfectIndividual.IncreaseGenomeMemory(true);
                for (var j = 0; j < perfectIndividual.GenesNumber; j++)
                {
                    var ratio = Math.Sign(target.Genes[j]) < 0 ? Math.Abs(minGene) : Math.Abs(maxGene);
                    if (Math.Abs(target.Genes[j]) > Math.Abs(ratio * (i + 1)))
                        perfectIndividual.Chromosome[i].Genes[j] = Math.Sign(target.Genes[j]) * ratio;
                    else if (Math.Abs(target.Genes[j]) - ratio * i > 0)
                        perfectIndividual.Chromosome[i].Genes[j] = Math.Sign(target.Genes[j]) * (Math.Abs(target.Genes[j]) - ratio * i);
                }
            }

            for (var i = perfectIndividual.GenomeLength - 1; i >= 0; i--)
                if (perfectIndividual.Chromosome[i].Genes.SequenceEqual((new Genome(perfectIndividual.GenesNumber)).Genes))
                    perfectIndividual.Chromosome.RemoveAt(i);

            return perfectIndividual;
        }

        private static double Distance(Genome genome, Genome target)
        {
            var deltas = new double[target.Genes.Count()];
            for (var i = 0; i < target.Genes.Count(); i++)
            {
                deltas[i] = target.Genes[i] - genome.Genes[i];
            }
            var distance = deltas.Sum(delta => Math.Pow(delta, 2));
            distance = Math.Sqrt(distance);
            return distance;
        }
    
        public static double Error(Individual individual, Genome target)
        {
            if (individual.GenomeLength < 1) return double.MaxValue;
            var x = new Genome(target.Genes.Count());
            var fitness = Distance(x, individual.Chromosome[0]);
            for (var i = 0; i < individual.Chromosome.Count - 1; ++i)
            {
                fitness += Distance(individual.Position(i), individual.Position(i + 1));
            }
            var lastSegment = Distance(individual.Position(individual.GenomeLength - 1), target);

            fitness += lastSegment * (maxNumberOfSteps - individual.GenomeLength);
            return (Math.Abs(_reference) < 0.01) ? fitness : (fitness - _reference) / _reference;
        }
    }
}
