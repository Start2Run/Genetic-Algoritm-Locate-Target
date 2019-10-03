using System;
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

        public static double GetDouble(this Random rnd, double min, double max)
        {
            if (Configuration.HasDoubleValues) return Math.Round((max - min) * rnd.NextDouble() + min, Configuration.Digits);
            return rnd.Next((int)min, (int)max + 1);
        }

        //public static double Error(Individual individual, Genome target, double refError)
        //{
        //    if (individual.GenomeLength < 1) return double.MaxValue;
        //    var x = new Genome(target.Genes.Count());
        //    var distance = Distance(x, individual.Chromosome[0]);
        //    for (var i = 0; i < individual.Chromosome.Count - 1; ++i)
        //    {
        //        distance += Distance(individual.Position(i), individual.Position(i + 1));
        //    }
        //    var lastSegment = Distance(individual.Position(individual.GenomeLength - 1), target);

        //    var optimalDistance = Distance(x, target);
        //    var fitness = (1 / (distance + lastSegment * refError)) * optimalDistance; // lastSegment * (Configuration.MaxNumberOfSteps - individual.GenomeLength);
        //    //return (Math.Abs(refError) < 0.01) ? fitness : (fitness - refError) / refError;
        //    return fitness;
        //}

        public static double Error(Individual individual, Genome target, double refError)
        {
            if (individual.GenomeLength < 1) return double.MaxValue;
            var x = new Genome(target.Genes.Count());
            var fitness = Distance(x, individual.Chromosome[0]);
            for (var i = 0; i < individual.Chromosome.Count - 1; ++i)
            {
                fitness += Distance(individual.Position(i), individual.Position(i + 1));
            }
            var lastSegment = Distance(individual.Position(individual.GenomeLength - 1), target);

            fitness += lastSegment * (Configuration.MaxNumberOfSteps - individual.GenomeLength);
            return (Math.Abs(refError) < 0.01) ? fitness : (fitness - refError) / refError;
            //return lastSegment / Distance(x, target) * 100;
        }
    }
}
