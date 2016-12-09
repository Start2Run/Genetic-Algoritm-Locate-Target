using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgoritm
{
    class GeneticAlgoritm
    {
  static Random rnd = new Random(0); // useed in many places

    static void Main(string[] args)
    {
      Console.WriteLine("\nBegin Evolutionary Optimization demo\n");
      Console.WriteLine("Goal is to minimize f(x0,x1,x2,x3,x4,x5) = x0^2 + x1^2 + x2^2 + x3^2 + x4^2 + x5^2");
      Console.WriteLine("Known solution is 0.0 at x0 = x1 = x2 = x3 = x4 = x5 = 0.000000");

      int dim = 6; // problem dimension = number genes
      int popSize = 1000;
      double minX = -10.0; // aka minGene, maxGene
      double maxX = 10.0;
      double mutateRate = 0.1; // likelihood that a gene is changed(0.1-.0.5)
      double mutateChange = 0.1; // controls magnitude of mutation change(0.1-0.001)
      double tau = 0.40; // selection pressure (percent pop selected for tournament selection) (0.3-0.7)
      int maxGeneration = 1000000; // loop counter
      double exitError = 0.00001; // early exit if meet this error or less

      Console.WriteLine("\nSetting problem dimension = numGenes = " + dim);
      Console.WriteLine("Setting population size = " + popSize);
      Console.WriteLine("Setting minX = minGene = " + minX.ToString("F1"));
      Console.WriteLine("Setting maxX = maxGene = " + maxX.ToString("F1"));
      Console.WriteLine("Setting mutation rate = " + mutateRate.ToString("F2"));
      Console.WriteLine("Setting mutation change factor = " + mutateChange.ToString("F2"));
      Console.WriteLine("Setting selection pressure (tau) = " + tau.ToString("F2"));
      Console.WriteLine("Setting max generations = " + maxGeneration);
      Console.WriteLine("Setting early-exit error value = " + exitError.ToString("F6"));

      Console.WriteLine("\nBeginning evolutionary optimization loop");
      double[] bestSolution = Solve(dim, popSize, minX, maxX, mutateRate, mutateChange, tau, maxGeneration, exitError);

      Console.WriteLine("\nBest solution found:\n");
      for (int i = 0; i < bestSolution.Length; ++i)
        Console.Write(bestSolution[i].ToString("F6") + " ");

      double error = GeneticAlgoritm.Error(bestSolution);
      Console.WriteLine("\n\n(Absolute) error value at best solution = " + error.ToString("F6"));

      
      double z = 0;
            for (int i = 0; i < bestSolution.Length; i++)
            {
                z += (bestSolution[i] * bestSolution[i]);
            }
      Console.WriteLine("\n\n Value = " + z);
      Console.WriteLine("\nEnd Evolutionary Optimization demo\n");
      Console.ReadLine();
    } // Main

    static double[] Solve(int dim, int popSize, double minX, double maxX, double mutateRate, double mutateChange, double tau, int maxGeneration, double exitError)
    {
      // assumes existence of an accessible Error function and a Individual class and a Random object rnd
      Individual[] population = new Individual[popSize];
      double[] bestSolution = new double[dim]; // best solution found by any individual
      double bestError = double.MaxValue; // smaller values better

      // population initialization
      for (int i = 0; i < population.Length; ++i)
      {
        population[i] = new Individual(dim, minX, maxX, mutateRate, mutateChange);
        if (population[i].error < bestError)
        {
          bestError = population[i].error;
          Array.Copy(population[i].chromosome, bestSolution, dim);
        }
      }

      // process
      int gen = 0; bool done = false;
      while (gen < maxGeneration && done == false)
      {
        if (gen % 200 == 0)
        {
          Console.WriteLine("\nGeneration = " + gen);
          Console.WriteLine("Best error = " + bestError.ToString("F6"));
        }

        Individual[] parents = Select(2, population, tau); // pick 2 good (not necessarily best) Individuals
        Individual[] children = Reproduce(parents[0], parents[1], minX, maxX, mutateRate, mutateChange); // create 2 children
        Place(children[0], children[1], population); // sort pop, replace two worst with new children
        Immigrate(population, dim, minX, maxX, mutateRate, mutateChange); // bring in a random Individual

        for (int i = popSize - 3; i < popSize; ++i) // check the 3 new Individuals
        {
          if (population[i].error < bestError)
          {
            bestError = population[i].error;
            population[i].chromosome.CopyTo(bestSolution, 0);
            if (bestError < exitError)
            {
              done = true;
              Console.WriteLine("\nEarly exit at generation " + gen);
            }
          }
        }
        ++gen;
      }
      return bestSolution;
    } // Solve

    private static Individual[] Select(int n, Individual[] population, double tau) // select n 'good' Individuals
    {
      // tau is selection pressure = % of population to grab
      int popSize = population.Length;
      int[] indexes = new int[popSize];
      for (int i = 0; i < indexes.Length; ++i)
        indexes[i] = i;

      for (int i = 0; i < indexes.Length; ++i) // shuffle
      {
        int r = rnd.Next(i, indexes.Length);
        int tmp = indexes[r]; indexes[r] = indexes[i]; indexes[i] = tmp;
      }

      int tournSize = (int)(tau * popSize);
      if (tournSize < n) tournSize = n;
      Individual[] candidates = new Individual[tournSize];

      for (int i = 0; i < tournSize; ++i)
        candidates[i] = population[indexes[i]];
      Array.Sort(candidates);

      Individual[] results = new Individual[n];
      for (int i = 0; i < n; ++i)
        results[i] = candidates[i];

      return results;
    }

    private static Individual[] Reproduce(Individual parent1, Individual parent2, double minGene, double maxGene, double mutateRate, double mutateChange) // crossover and mutation
    {
      int numGenes = parent1.chromosome.Length;

      int cross = rnd.Next(0, numGenes - 1); // crossover point. 0 means 'between 0 and 1'.

      Individual child1 = new Individual(numGenes, minGene, maxGene, mutateRate, mutateChange); // random chromosome
      Individual child2 = new Individual(numGenes, minGene, maxGene, mutateRate, mutateChange);

      for (int i = 0; i <= cross; ++i)
        child1.chromosome[i] = parent1.chromosome[i];
      for (int i = cross + 1; i < numGenes; ++i)
        child2.chromosome[i] = parent1.chromosome[i];
      for (int i = 0; i <= cross; ++i)
        child2.chromosome[i] = parent2.chromosome[i];
      for (int i = cross + 1; i < numGenes; ++i)
        child1.chromosome[i] = parent2.chromosome[i];

      Mutate(child1, maxGene, mutateRate, mutateChange);
      Mutate(child2, maxGene, mutateRate, mutateChange);

      child1.error = GeneticAlgoritm.Error(child1.chromosome);
      child2.error = GeneticAlgoritm.Error(child2.chromosome);

      Individual[] result = new Individual[2];
      result[0] = child1;
      result[1] = child2;

      return result;
    } // Reproduce

    private static void Mutate(Individual child, double maxGene, double mutateRate, double mutateChange)
    {
      double hi = mutateChange * maxGene;
      double lo = -hi;
      for (int i = 0; i < child.chromosome.Length; ++i)
      {
        if (rnd.NextDouble() < mutateRate)
        {
          double delta = (hi - lo) * rnd.NextDouble() + lo;
          child.chromosome[i] += delta;
        }
      }
    }

    private static void Place(Individual child1, Individual child2, Individual[] population)
    {
      // place child1 and child2 into the population, replacing two worst individuals
      int popSize = population.Length;
      Array.Sort(population);
      population[popSize - 1] = child1;
      population[popSize - 2] = child2;
      return;
    }

    private static void Immigrate(Individual[] population, int numGenes, double minGene, double maxGene, double mutateRate, double mutateChange)
    {
      // kill off third-worst Individual and replace with new Individual
      // assumes population is sorted
      Individual immigrant = new Individual(numGenes, minGene, maxGene, mutateRate, mutateChange);
      int popSize = population.Length;
      population[popSize - 3] = immigrant; // replace third worst individual
    }

    // ===========================================

    public static double Error(double[] x)
    {
      // absolute error for hyper-sphere function
      double trueMin = 100.0;
      double z = 0.0;
      for (int i = 0; i < x.Length; ++i)
        z += (x[i] * x[i]);
      return Math.Abs(trueMin - z);
    }

    // ===========================================

  } // Program class



  // ------------------------------------------------------------------------------------------------

  public class Individual : IComparable<Individual>
  {
    public double[] chromosome; // represents a solution
    public double error; // smaller values are better for minimization

    private int numGenes; // problem dimension
    private double minGene; // smallest value for a chromosome cell
    private double maxGene;
    private double mutateRate; // used during reproduction by Mutate
    private double mutateChange; // used during reproduction

    static Random rnd = new Random(0); // used by ctor

    public Individual(int numGenes, double minGene, double maxGene, double mutateRate, double mutateChange)
    {
      this.numGenes = numGenes;
      this.minGene = minGene;
      this.maxGene = maxGene;
      this.mutateRate = mutateRate;
      this.mutateChange = mutateChange;
      this.chromosome = new double[numGenes];
      for (int i = 0; i < this.chromosome.Length; ++i)
        this.chromosome[i] = (maxGene - minGene) * rnd.NextDouble() + minGene;
      this.error = GeneticAlgoritm.Error(this.chromosome);
    }

    public int CompareTo(Individual other) // from smallest error (better) to largest
    {
      if (this.error < other.error) return -1;
      else if (this.error > other.error) return 1;
      else return 0;
    }
    }
}
