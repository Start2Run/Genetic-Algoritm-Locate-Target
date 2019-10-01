using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GeneticAlgorithm
{
    public class Individual : IComparable<Individual>
    {
        public List<Genome> Chromosome; // represents a solution
        //private double error; // smaller values are better for minimization
        public int GenomeLength => Chromosome.Count;
        public int GenesNumber => _numGenes;
        public double Error => GeneticAlgorithm.Error(this, _target);

        private readonly int _numGenes; // problem dimension
        private readonly double _minGene; // smallest value for a chromosome cell
        private readonly double _maxGene;
        private double _mutateRate; // used during reproduction by Mutate
        private double _mutateChange; // used during reproduction
        private readonly Genome _target;
        private readonly int _digits;
        private readonly bool _isDouble;
        private static readonly Random Rnd = new Random(0); // used by ctor
        public Individual(Genome target, double minGene, double maxGene, double mutateRate, double mutateChange, bool isDouble = true)
        {
            _numGenes = target.Genes.Count();
            _minGene = minGene;
            _maxGene = maxGene;
            _mutateRate = mutateRate;
            _mutateChange = mutateChange;
            _target = target;
            _isDouble = isDouble;
            _digits = mutateChange.ToString(CultureInfo.InvariantCulture).Substring(mutateChange.ToString(CultureInfo.InvariantCulture).IndexOf(".", StringComparison.Ordinal) + 1).Length;

            Chromosome = new List<Genome>();

            IncreaseGenomeMemory();
        }

        public double GetRnd(double min, double max)
        {
            if (_isDouble) return Math.Round((max - min) * Rnd.NextDouble() + min, _digits);
            return Rnd.Next((int)min, (int)max + 1);
        }

        public Genome Position(int geneIndex)
        {
            var position = new Genome(GenesNumber);
            for (var i = 0; i <= geneIndex; i++)
            {
                for (var j = 0; j < GenesNumber; j++)
                {
                    position.Genes[j] += Chromosome[i].Genes[j];
                }
            }
            return position;
        }

        public void IncreaseGenomeMemory(bool empty = false)
        {
            Chromosome.Add(new Genome(_numGenes));
            if (empty) return;
            for (var i = 0; i < Chromosome.Last().Genes.Count(); i++)
            {
                Chromosome.Last().Genes[i] = GetRnd(_minGene, _maxGene);
            }
        }

        public int CompareTo(Individual other) // from smallest error (better) to largest
        {
            if (Error < other.Error) return -1;
            return Error > other.Error ? 1 : 0;
        }

        public static Individual Solve(Genome target, int popSize, double minX, double maxX, double mutateRate, double mutateChange, double tau, int maxGeneration, double exitError)
        {
            // assumes existence of an accessible Error function and a Individual class and a Random object rnd
            var population = new Individual[popSize];
            Individual bestSolution = null; // best solution found by any individual
            var bestError = double.MaxValue; // smaller values better

            // population initialization
            for (var i = 0; i < population.Length; ++i)
            {
                population[i] = new Individual(target, minX, maxX, mutateRate, mutateChange);
                if (!(population[i].Error < bestError)) continue;
                bestError = population[i].Error;
                bestSolution = population[i];
            }

            // process
            var gen = 0; bool done = false;
            while (gen < maxGeneration && done == false)
            {
                if (gen % 200 == 0)
                {
                    Console.WriteLine("\nGeneration = " + gen);
                    Console.WriteLine("Best error = " + bestError.ToString("F6"));
                }

                var parents = Select(2, population, tau); // pick 2 good (not necessarily best) Individuals
                var children = Reproduce(parents[0], parents[1], minX, maxX, mutateRate, mutateChange, target); // create 2 children
                Place(children[0], children[1], population); // sort pop, replace two worst with new children
                Immigrate(population, minX, maxX, mutateRate, mutateChange, target); // bring in a random Individual

                for (var i = popSize - 3; i < popSize; ++i) // check the 3 new Individuals
                {
                    for (var j = population[i].GenomeLength - 1; j >= 0; j--)
                        if (population[i].Chromosome[j].Genes.SequenceEqual((new Genome(population[i].GenesNumber)).Genes))
                            population[i].Chromosome.RemoveAt(j);
                    if (!(population[i].Error < bestError)) continue;
                    bestError = population[i].Error;
                    bestSolution = population[i];
                    if (!(bestError <= exitError)) continue;
                    done = true;
                    Console.WriteLine("\nEarly exit at generation " + gen);
                }
                ++gen;
            }
            foreach (var v in population)
            {
                Console.WriteLine();
                foreach (var g in v.Chromosome)
                {
                    Console.Write("[" + g.Genes[0] + "," + g.Genes[1] + "," + g.Genes[2] + "]");
                }
                Console.Write("-->" + v.Error + "         ");
            }
            Console.WriteLine(); Console.WriteLine();

            return bestSolution;
        } // Solve

        private static Individual[] Select(int n, Individual[] population, double tau) // select n 'good' Individuals
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

        private static Individual[] Reproduce(Individual parent1, Individual parent2, double minGene, double maxGene, double mutateRate, double mutateChange, Genome target) // crossover and mutation
        {
            var numGenes = parent1.GenesNumber;
            var genomeLength = Math.Min(parent1.GenomeLength, parent2.GenomeLength);
            var c = Rnd.Next(0, genomeLength - 1); // crossover point. 0 means 'between 0 and 1'.
            var cross = Rnd.Next(0, numGenes - 1); // crossover point. 0 means 'between 0 and 1'.

            var child1 = new Individual(target, minGene, maxGene, mutateRate, mutateChange); // random chromosome
            for (var i = 1; i < parent2.GenomeLength; i++)
                child1.IncreaseGenomeMemory();

            var child2 = new Individual(target, minGene, maxGene, mutateRate, mutateChange);
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
                if (child1.Chromosome[i].Genes.SequenceEqual((new Genome(parent1.GenesNumber)).Genes))
                    child1.Chromosome.RemoveAt(i);
            for (var i = child2.GenomeLength - 1; i >= 0; i--)
                if (child2.Chromosome[i].Genes.SequenceEqual((new Genome(parent2.GenesNumber)).Genes))
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
            for (var i = 0; i < child.GenesNumber; ++i)
            {
                if (!(Rnd.NextDouble() < mutateRate)) continue;
                var delta = child.GetRnd(lo, hi);//(hi - lo) * rnd.NextDouble() + lo;
                child.Chromosome[c].Genes[i] = delta;
            }
        }

        private static void Place(Individual child1, Individual child2, Individual[] population)
        {
            // place child1 and child2 into the population, replacing two worst individuals
            var popSize = population.Length;
            Array.Sort(population);
            population[popSize - 1] = child1;
            population[popSize - 2] = child2;
        }

        private static void Immigrate(Individual[] population, double minGene, double maxGene, double mutateRate, double mutateChange, Genome target)
        {
            // kill off third-worst Individual and replace with new Individual
            // assumes population is sorted
            var immigrant = new Individual(target, minGene, maxGene, mutateRate, mutateChange);
            var popSize = population.Length;
            population[popSize - 3] = immigrant; // replace third worst individual
        }
    }
}