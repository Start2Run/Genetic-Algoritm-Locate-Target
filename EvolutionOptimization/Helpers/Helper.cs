﻿using System;
using System.Linq;
using EvolutionOptimization.Interfaces;
using EvolutionOptimization.Models;

namespace EvolutionOptimization.Helpers
{
    internal static class Helper
    {
        public static double Distance(IGenome genome, IGenome target)
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

        public static double GetDouble(this Random rnd, double min, double max, Configuration config)
        {
            if (config.HasDoubleValues) return Math.Round((max - min) * rnd.NextDouble() + min, config.Digits);
            return rnd.Next((int) min, (int) max + 1);
        }

        public static double Error(Individual individual, Genome target, double refError, IConfiguration config)
        {
            if (individual.GenomeLength < 1) return double.MaxValue;
            var x = new Genome(target.Genes.Count());
            var fitness = Distance(x, individual.Chromosome[0]);
            for (var i = 0; i < individual.Chromosome.Count - 1; ++i)
            {
                fitness += Distance(individual.Position(i), individual.Position(i + 1));
            }
            var lastSegment = Distance(individual.Position(individual.GenomeLength - 1), target);

            fitness += lastSegment * (config.MaxNumberOfSteps - individual.GenomeLength);
            return (Math.Abs(refError) < 0.01) ? fitness : (fitness - refError) / refError;
        }
    }
}
