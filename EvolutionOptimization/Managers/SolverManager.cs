using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using EvolutionOptimization.Helpers;
using EvolutionOptimization.Interfaces;
using EvolutionOptimization.Models;

namespace EvolutionOptimization.Managers
{
    public class SolverManager
    {
        private static readonly Random Rnd = new Random(0); // used by ctor
        private readonly double _refError;
        private Genome _targetGenome;

        public SolverManager(double[] target)
        {
            Configuration.NumberOfGenes = target.Length;
            Configuration.Digits = Configuration.MutateChange.ToString(CultureInfo.InvariantCulture).Substring(Configuration.MutateChange.ToString(CultureInfo.InvariantCulture).IndexOf(".", StringComparison.Ordinal) + 1).Length;
            _targetGenome = new Genome(Configuration.NumberOfGenes) { Genes = target };
            var perfectIndividual = OptimalIndividual(_targetGenome);
            _refError = perfectIndividual.Error;
        }

        public async Task<IIndividual> Solver()
        {
            return await Task.Run(() =>
            {
                // assumes existence of an accessible Error function and a Individual class and a Random object rnd
                var population = new Individual[Configuration.PopSize];
                Individual bestSolution = null; // best solution found by any individual
                var bestError = double.MaxValue; // smaller values better

                // population initialization
                for (var i = 0; i < population.Length; ++i)
                {
                    population[i] = new Individual(_targetGenome, _refError);
                    if (!(population[i].Error < bestError)) continue;
                    bestError = population[i].Error;
                    bestSolution = population[i];
                }

                // process
                var gen = 0;
                bool done = false;
                while (gen < Configuration.MaxGeneration && done == false)
                {
                    if (gen % 200 == 0)
                    {
                        Console.WriteLine("\nGeneration = " + gen);
                        Console.WriteLine("Best error = " + bestError.ToString("F6"));
                    }

                    var parents = Select(2, population, Configuration.Tau); // pick 2 good (not necessarily best) Individuals
                    var children = Reproduce(parents[0], parents[1], Configuration.MinX, Configuration.MaxX, Configuration.MutateRate,
                        Configuration.MutateChange, _targetGenome); // create 2 children
                    Place(children[0], children[1], population); // sort pop, replace two worst with new children
                    Immigrate(population, Configuration.MinX, Configuration.MaxX, Configuration.MutateRate, Configuration.MutateChange,
                        _targetGenome); // bring in a random Individual

                    for (var i = Configuration.PopSize - 3; i < Configuration.PopSize; ++i) // check the 3 new Individuals
                    {
                        for (var j = population[i].GenomeLength - 1; j >= 0; j--)
                            if (population[i].Chromosome[j].Genes.SequenceEqual((new Genome(Configuration.NumberOfGenes)).Genes))
                                population[i].Chromosome.RemoveAt(j);
                        if (!(population[i].Error < bestError)) continue;
                        bestError = population[i].Error;
                        bestSolution = population[i];
                        if (!(bestError <= Configuration.ExitError)) continue;
                        done = true;
                        Console.WriteLine("\n Early exit at generation " + gen);
                    }

                    ++gen;
                }
                return bestSolution;
            });
        } // Solve

        private IIndividual[] Select(int n, Individual[] population, double tau) // select n 'good' Individuals
        {
            // tau is selection pressure = % of population to grab
            var popSize = population.Length;
            var indexes = new int[popSize];
            for (var i = 0; i < indexes.Length; ++i)
                indexes[i] = i;

            for (var i = 0; i < indexes.Length; ++i) // shuffle
            {
                var r = Rnd.Next(i, indexes.Length);
                var tmp = indexes[r]; indexes[r] = indexes[i]; indexes[i] = tmp;
            }

            var tournSize = (int)(tau * popSize);
            if (tournSize < n) tournSize = n;
            var candidates = new Individual[tournSize];

            for (var i = 0; i < tournSize; ++i)
                candidates[i] = population[indexes[i]];
            Array.Sort(candidates);

            var results = new Individual[n];
            for (var i = 0; i < n; ++i)
                results[i] = candidates[i];

            return results;
        }

        private IIndividual[] Reproduce(IIndividual parent1, IIndividual parent2, double minGene, double maxGene, double mutateRate, double mutateChange, Genome target) // crossover and mutation
        {
            var numGenes = Configuration.NumberOfGenes;
            var genomeLength = Math.Min(parent1.GenomeLength, parent2.GenomeLength);
            var c = Rnd.Next(0, genomeLength - 1); // crossover point. 0 means 'between 0 and 1'.
            var cross = Rnd.Next(0, numGenes - 1); // crossover point. 0 means 'between 0 and 1'.

            var child1 = new Individual(target,_refError); // random chromosome
            for (var i = 1; i < parent2.GenomeLength; i++)
                child1.IncreaseGenomeMemory();

            var child2 = new Individual(target, _refError);
            for (var i = 1; i < parent1.GenomeLength; i++)
                child2.IncreaseGenomeMemory();

            for (var i = 0; i <= c; ++i)
                for (var j = 0; j <= cross; ++j)
                    child1.Chromosome[i].Genes[j] = parent1.Chromosome[i].Genes[j];
            for (var i = c + 1; i < parent1.GenomeLength; ++i)
                for (var j = cross + 1; j < numGenes; ++j)
                    child2.Chromosome[i].Genes[j] = parent1.Chromosome[i].Genes[j];
            for (var i = 0; i <= c; ++i)
                for (var j = 0; j <= cross; ++j)
                    child2.Chromosome[i].Genes[j] = parent2.Chromosome[i].Genes[j];
            for (var i = c + 1; i < parent2.GenomeLength; ++i)
                for (var j = cross + 1; j < numGenes; ++j)
                    child1.Chromosome[i].Genes[j] = parent2.Chromosome[i].Genes[j];

            for (var i = child1.GenomeLength - 1; i >= 0; i--)
                if (child1.Chromosome[i].Genes.SequenceEqual((new Genome(Configuration.NumberOfGenes)).Genes))
                    child1.Chromosome.RemoveAt(i);
            for (var i = child2.GenomeLength - 1; i >= 0; i--)
                if (child2.Chromosome[i].Genes.SequenceEqual((new Genome(Configuration.NumberOfGenes)).Genes))
                    child2.Chromosome.RemoveAt(i);

            if (child1.Error >= Math.Min(parent1.Error, parent2.Error)) child1.IncreaseGenomeMemory();
            else Mutate(child1, maxGene, mutateRate, mutateChange);
            if (child2.Error >= Math.Min(parent1.Error, parent2.Error)) child2.IncreaseGenomeMemory();
            else Mutate(child2, maxGene, mutateRate, mutateChange);

            var result = new Individual[2];
            result[0] = child1;
            result[1] = child2;

            return result;
        } // Reproduce

        private static void Mutate(Individual child, double maxGene, double mutateRate, double mutateChange)
        {
            var hi = mutateChange * maxGene;
            var lo = -hi;
            if (child.GenomeLength < 1) child.IncreaseGenomeMemory();
            var c = Rnd.Next(0, child.GenomeLength);
            for (var i = 0; i < Configuration.NumberOfGenes; ++i)
            {
                if (!(Rnd.NextDouble() < mutateRate)) continue;
                var delta = child.GetRnd(lo, hi);//(hi - lo) * rnd.NextDouble() + lo;
                child.Chromosome[c].Genes[i] = delta;
            }
        }

        private void Place(IIndividual child1, IIndividual child2, IIndividual[] population)
        {
            // place child1 and child2 into the population, replacing two worst individuals
            var popSize = population.Length;
            Array.Sort(population);
            population[popSize - 1] = child1;
            population[popSize - 2] = child2;
        }

        private void Immigrate(Individual[] population, double minGene, double maxGene, double mutateRate, double mutateChange, Genome target)
        {
            // kill off third-worst Individual and replace with new Individual
            // assumes population is sorted
            var immigrant = new Individual(target, _refError);
            var popSize = population.Length;
            population[popSize - 3] = immigrant; // replace third worst individual
        }

        private IIndividual OptimalIndividual(Genome target)
        {
            var perfectIndividual = new Individual(target, _refError);
            perfectIndividual.Chromosome.Clear();
            var n = (int)(target.Genes.Max(gene => Math.Abs(gene) / Configuration.MutateChange));
            for (var i = 0; i < n; i++)
            {
                perfectIndividual.IncreaseGenomeMemory(true);
                for (var j = 0; j < Configuration.NumberOfGenes; j++)
                {
                    var ratio = Math.Sign(target.Genes[j]) < 0 ? Math.Abs(Configuration.MinX) : Math.Abs(Configuration.MaxX);
                    if (Math.Abs(target.Genes[j]) > Math.Abs(ratio * (i + 1)))
                        perfectIndividual.Chromosome[i].Genes[j] = Math.Sign(target.Genes[j]) * ratio;
                    else if (Math.Abs(target.Genes[j]) - ratio * i > 0)
                        perfectIndividual.Chromosome[i].Genes[j] = Math.Sign(target.Genes[j]) * (Math.Abs(target.Genes[j]) - ratio * i);
                }
            }

            for (var i = perfectIndividual.GenomeLength - 1; i >= 0; i--)
                if (perfectIndividual.Chromosome[i].Genes.SequenceEqual((new Genome(Configuration.NumberOfGenes)).Genes))
                    perfectIndividual.Chromosome.RemoveAt(i);

            return perfectIndividual;
        }
    }
}
