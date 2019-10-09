using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EvolutionOptimization.Interfaces;
using EvolutionOptimization.Models;

namespace EvolutionOptimization.Managers
{
    public class SolverManager : ISolverManager
    {
        private static readonly Random Rnd = new Random(0); // used by ctor
        private readonly double _refError;
        private Genome _targetGenome;
        public Action<IEnumerable<IIndividual>, double, int> UpdateAction { get; set; }
        private readonly Configuration _config;
        public SolverManager(double[] target, Configuration config)
        {
            _config = config;
            _config.NumberOfGenes = target.Length;
            _config.Digits = _config.MutateChange.ToString(CultureInfo.InvariantCulture).Substring(_config.MutateChange.ToString(CultureInfo.InvariantCulture).IndexOf(".", StringComparison.Ordinal) + 1).Length;
            _targetGenome = new Genome(_config.NumberOfGenes) { Genes = target };
            var perfectIndividual = OptimalIndividual(_targetGenome);
            _refError = perfectIndividual.Error;
        }

        public async Task<IIndividual> Solver(CancellationToken token)
        {
            return await Task.Run(() =>
            {
                token.ThrowIfCancellationRequested();
                // assumes existence of an accessible Error function and a Individual class and a Random object rnd
                var population = new IIndividual[_config.PopSize];
                IIndividual bestSolution = null; // best solution found by any individual
                var bestError = double.MaxValue; // smaller values better

                // population initialization
                for (var i = 0; i < population.Length; ++i)
                {
                    population[i] = new Individual(_targetGenome, _refError, _config);
                    if (!(population[i].Error < bestError)) continue;
                    bestError = population[i].Error;
                    bestSolution = population[i];
                }

                // process
                var gen = 0;
                bool done = false;
                while (gen < _config.MaxGeneration && done == false)
                {
                    if (token.IsCancellationRequested)
                    {
                        // Clean up here, then...
                        token.ThrowIfCancellationRequested();
                    }

                    if (gen % 200 == 0)
                    {
                        Console.WriteLine("\nGeneration = " + gen);
                        Console.WriteLine("Best error = " + bestError.ToString("F6"));
                        UpdateAction?.Invoke(population.ToArray(), bestError, gen);
                    }

                    var parents = Select(2, population, _config.Tau); // pick 2 good (not necessarily best) Individuals
                    var children = Reproduce(parents[0], parents[1], _config.MinX, _config.MaxX, _config.MutateRate,
                        _config.MutateChange, _targetGenome); // create 2 children
                    Place(children[0], children[1], population); // sort pop, replace two worst with new children
                    Immigrate(population, _config.MinX, _config.MaxX, _config.MutateRate, _config.MutateChange,
                        _targetGenome); // bring in a random Individual

                    for (var i = _config.PopSize - 3; i < _config.PopSize; ++i) // check the 3 new Individuals
                    {
                        for (var j = population[i].GenomeLength - 1; j >= 0; j--)
                            if (population[i].Chromosome[j].Genes.SequenceEqual((new Genome(_config.NumberOfGenes)).Genes))
                                population[i].Chromosome.RemoveAt(j);
                        if (!(population[i].Error < bestError)) continue;
                        bestError = population[i].Error;
                        bestSolution = population[i];
                        if (!(bestError <= _config.ExitError)) continue;
                        done = true;
                        Console.WriteLine("\n Early exit at generation " + gen);
                    }

                    ++gen;
                }
                return bestSolution;
            }, token);
        } // Solve

        private IIndividual[] Select(int n, IIndividual[] population, double tau) // select n 'good' Individuals
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
            var candidates = new IIndividual[tournSize];

            for (var i = 0; i < tournSize; ++i)
                candidates[i] = population[indexes[i]];
            Array.Sort(candidates);

            var results = new IIndividual[n];
            for (var i = 0; i < n; ++i)
                results[i] = candidates[i];

            return results;
        }

        private IIndividual[] Reproduce(IIndividual parent1, IIndividual parent2, double minGene, double maxGene, double mutateRate, double mutateChange, Genome target) // crossover and mutation
        {
            var numGenes = _config.NumberOfGenes;
            var genomeLength = Math.Min(parent1.GenomeLength, parent2.GenomeLength);
            var c = Rnd.Next(0, genomeLength - 1); // crossover point. 0 means 'between 0 and 1'.
            var cross = Rnd.Next(0, numGenes - 1); // crossover point. 0 means 'between 0 and 1'.

            var child1 = new Individual(target, _refError, _config); // random chromosome
            for (var i = 1; i < parent2.GenomeLength; i++)
                child1.IncreaseGenomeMemory();

            var child2 = new Individual(target, _refError, _config);
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
                if (child1.Chromosome[i].Genes.SequenceEqual((new Genome(_config.NumberOfGenes)).Genes))
                    child1.Chromosome.RemoveAt(i);
            for (var i = child2.GenomeLength - 1; i >= 0; i--)
                if (child2.Chromosome[i].Genes.SequenceEqual((new Genome(_config.NumberOfGenes)).Genes))
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

        private void Mutate(IIndividual child, double maxGene, double mutateRate, double mutateChange)
        {
            var hi = mutateChange * maxGene;
            var lo = -hi;
            if (child.GenomeLength < 1) child.IncreaseGenomeMemory();
            var c = Rnd.Next(0, child.GenomeLength);
            for (var i = 0; i < _config.NumberOfGenes; ++i)
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

        private void Immigrate(IIndividual[] population, double minGene, double maxGene, double mutateRate, double mutateChange, Genome target)
        {
            // kill off third-worst Individual and replace with new Individual
            // assumes population is sorted
            var immigrant = new Individual(target, _refError, _config);
            var popSize = population.Length;
            population[popSize - 3] = immigrant; // replace third worst individual
        }

        private IIndividual OptimalIndividual(Genome target)
        {
            var perfectIndividual = new Individual(target, _refError, _config);
            perfectIndividual.Chromosome.Clear();
            var n = (int)(target.Genes.Max(gene => Math.Abs(gene) / _config.MutateChange));
            for (var i = 0; i < n; i++)
            {
                perfectIndividual.IncreaseGenomeMemory(true);
                for (var j = 0; j < _config.NumberOfGenes; j++)
                {
                    var ratio = Math.Sign(target.Genes[j]) < 0 ? Math.Abs(_config.MinX) : Math.Abs(_config.MaxX);
                    if (Math.Abs(target.Genes[j]) > Math.Abs(ratio * (i + 1)))
                        perfectIndividual.Chromosome[i].Genes[j] = Math.Sign(target.Genes[j]) * ratio;
                    else if (Math.Abs(target.Genes[j]) - ratio * i > 0)
                        perfectIndividual.Chromosome[i].Genes[j] = Math.Sign(target.Genes[j]) * (Math.Abs(target.Genes[j]) - ratio * i);
                }
            }

            for (var i = perfectIndividual.GenomeLength - 1; i >= 0; i--)
                if (perfectIndividual.Chromosome[i].Genes.SequenceEqual((new Genome(_config.NumberOfGenes)).Genes))
                    perfectIndividual.Chromosome.RemoveAt(i);

            return perfectIndividual;
        }
    }
}
