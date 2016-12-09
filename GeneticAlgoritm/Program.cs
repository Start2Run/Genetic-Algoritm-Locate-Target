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
    static double _mutateChange = 0.1;
    static int maxNumberOfSteps = 100;
    static double reference=0;

    static void Main(string[] args)
    {
      int cromosomeMem = 3; // problem dimension = number genes
      Genom target =new Genom(cromosomeMem) { Genes = new double[]{4.2,-2.5,8}};
      
      int popSize = 31;
      double minX = -1.0; // aka minGene, maxGene
      double maxX = 1.0;
      double mutateRate = 0.1; // likelihood that a gene is changed(0.1-.0.5)
      double mutateChange =_mutateChange; // controls magnitude of mutation change(0.1-0.001)
      double tau = 0.40; // selection pressure (percent pop selected for tournament selection) (0.3-0.7)
      
      int maxGeneration = 100000; // loop counter

      Individual perfectIndivid = OptimalIndividual(minX, maxX, mutateRate, mutateChange, target);
      reference = perfectIndivid.Error; // early exit if meet this error or less

      double exitError = 0; // 3% exit error
     
      Console.WriteLine("\nBeginning evolutionary optimization loop");
      Individual bestSolution = Solve(target, popSize, minX, maxX, mutateRate, mutateChange, tau, maxGeneration, exitError);

      Console.WriteLine("\n\n Accuracy value at best solution = " + bestSolution.Error.ToString("F6")+ "%");

      Console.WriteLine("\nEnd Evolutionary Optimization demo\n");
      Console.ReadLine();
    } // Main

        static Individual OptimalIndividual(double minGene, double maxGene, double mutateRate, double mutateChange, Genom target)
        {
            Individual perfectIndividual = new Individual(target,minGene,maxGene,mutateRate,mutateChange);
            perfectIndividual.chromosome.Clear();
            int n= (int)(target.Genes.Max(gene => Math.Abs(gene) / mutateChange));
            for (int i = 0; i < n;  i++)
            {
                perfectIndividual.IncreaseGenomMemory(true);
                for (int j = 0; j < perfectIndividual.GenesNumber; j++)
                {
                    double ratio = Math.Sign(target.Genes[j]) < 0?Math.Abs(minGene): Math.Abs(maxGene);
                    if (Math.Abs(target.Genes[j]) > Math.Abs(ratio * (i+1)))
                        perfectIndividual.chromosome[i].Genes[j] = Math.Sign(target.Genes[j]) * ratio;
                    else if (Math.Abs(target.Genes[j]) - ratio * i > 0)
                        perfectIndividual.chromosome[i].Genes[j] = Math.Sign(target.Genes[j])* (Math.Abs(target.Genes[j]) - ratio * i);
                }
            }

            for (int i = perfectIndividual.GenomLength - 1; i >= 0; i--)
                if (perfectIndividual.chromosome[i].Genes.SequenceEqual((new Genom(perfectIndividual.GenesNumber)).Genes))
                    perfectIndividual.chromosome.RemoveAt(i);

            return perfectIndividual;
        }

        static Individual Solve(Genom target,int popSize, double minX, double maxX, double mutateRate, double mutateChange, double tau, int maxGeneration, double exitError)
        {
        // assumes existence of an accessible Error function and a Individual class and a Random object rnd
        Individual[] population = new Individual[popSize];
        Individual bestSolution=null; // best solution found by any individual
        double bestError = double.MaxValue; // smaller values better

        // population initialization
        for (int i = 0; i < population.Length; ++i)
        {
        population[i] = new Individual(target, minX, maxX, mutateRate, mutateChange);
        if (population[i].Error < bestError)
        {
            bestError = population[i].Error;
            bestSolution = population[i];
        }
      }

      // process
      int gen = 0; bool done = false;
      int age = 0;
      while (gen < maxGeneration && done == false)
      {
        if (gen % 200 == 0)
        {
          Console.WriteLine("\nGeneration = " + gen);
          Console.WriteLine("Best error = " + bestError.ToString("F6"));
        }

        Individual[] parents = Select(2, population, tau); // pick 2 good (not necessarily best) Individuals
        Individual[] children = Reproduce(parents[0], parents[1], minX, maxX, mutateRate, mutateChange,target); // create 2 children
        Place(children[0], children[1], population); // sort pop, replace two worst with new children
        Immigrate(population, minX, maxX, mutateRate, mutateChange,target); // bring in a random Individual

        for (int i = popSize - 3; i < popSize; ++i) // check the 3 new Individuals
        {
                    for (int j = population[i].GenomLength - 1; j >= 0; j--)
                        if (population[i].chromosome[j].Genes.SequenceEqual((new Genom(population[i].GenesNumber)).Genes))
                            population[i].chromosome.RemoveAt(j);
          if (population[i].Error < bestError)
          {
            bestError = population[i].Error;
            bestSolution = population[i];
            if (bestError <= exitError)
            {
              done = true;
              Console.WriteLine("\nEarly exit at generation " + gen);
            }
                    }
                }
                ++gen;
      }
            foreach (Individual v in population)
            {
                Console.WriteLine();
                foreach (Genom g in v.chromosome)
                {
                    Console.Write("[" + g.Genes[0] + "," + g.Genes[1] + "," + g.Genes[2]+ "]");
                }
                Console.Write("-->" + v.Error + "         ");
            }
            Console.WriteLine(); Console.WriteLine();

            return bestSolution;
    } // Solve

    private static double Distance(Genom genom, Genom target)
        {
            double[] Deltas = new double[target.Genes.Count()];
            for (int i = 0; i < target.Genes.Count(); i++)
            {
                Deltas[i] = target.Genes[i] - genom.Genes[i];
            }
            double distance = 0;
            foreach (double delta in Deltas)
            { distance += Math.Pow(delta, 2); }
            distance = (double)Math.Sqrt(distance);
            return distance;
        }

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

    private static Individual[] Reproduce(Individual parent1, Individual parent2, double minGene, double maxGene, double mutateRate, double mutateChange,Genom target) // crossover and mutation
        {
        int numGenes = parent1.GenesNumber;
        int genomLength = Math.Min(parent1.GenomLength,parent2.GenomLength);
        int c = rnd.Next(0, genomLength - 1); // crossover point. 0 means 'between 0 and 1'.
        int cross = rnd.Next(0, numGenes - 1); // crossover point. 0 means 'between 0 and 1'.

        Individual child1 = new Individual(target, minGene, maxGene, mutateRate, mutateChange); // random chromosome
            for (int i = 1; i < parent2.GenomLength; i++)
                child1.IncreaseGenomMemory();
            
        Individual child2 = new Individual(target, minGene, maxGene, mutateRate, mutateChange);
            for (int i = 1; i < parent1.GenomLength; i++)
                child2.IncreaseGenomMemory();

        for (int i = 0; i <= c; ++i)
            for (int j = 0; j <= cross; ++j)
                child1.chromosome[i].Genes[j] = parent1.chromosome[i].Genes[j];
        for (int i = c+1; i < parent1.GenomLength; ++i)
            for (int j = cross + 1; j < numGenes; ++j)
                child2.chromosome[i].Genes[j] = parent1.chromosome[i].Genes[j];
        for (int i = 0; i <= c; ++i)
            for (int j = 0; j <= cross; ++j)
                child2.chromosome[i].Genes[j] = parent2.chromosome[i].Genes[j];
         for (int i = c+1; i < parent2.GenomLength; ++i)
            for (int j = cross + 1; j < numGenes; ++j)
                child1.chromosome[i].Genes[j] = parent2.chromosome[i].Genes[j];

            for (int i = child1.GenomLength - 1; i >= 0; i--)
                if (child1.chromosome[i].Genes.SequenceEqual((new Genom(parent1.GenesNumber)).Genes))
                    child1.chromosome.RemoveAt(i);
            for (int i = child2.GenomLength - 1; i >= 0; i--)
                if (child2.chromosome[i].Genes.SequenceEqual((new Genom(parent2.GenesNumber)).Genes))
                    child2.chromosome.RemoveAt(i);

        if (child1.Error >= Math.Min(parent1.Error,parent2.Error)) child1.IncreaseGenomMemory();
        else Mutate(child1, maxGene, mutateRate, mutateChange);
        if (child2.Error >= Math.Min(parent1.Error, parent2.Error)) child2.IncreaseGenomMemory();
        else Mutate(child2, maxGene, mutateRate, mutateChange);
        //child1.error = GeneticAlgoritm.Error(child1,target);
        //child2.error = GeneticAlgoritm.Error(child2,target);

        Individual[] result = new Individual[2];
        result[0] = child1;
        result[1] = child2;

        return result;
        } // Reproduce

    private static void Mutate(Individual child, double maxGene, double mutateRate, double mutateChange)
        {
        double hi = mutateChange * maxGene;
        double lo = -hi;
            if (child.GenomLength < 1) child.IncreaseGenomMemory();
        int c = rnd.Next(0, child.GenomLength);
        for (int i = 0; i < child.GenesNumber; ++i)
        {
            if (rnd.NextDouble() < mutateRate)
            {
                double delta = child.Rnd(lo, hi);//(hi - lo) * rnd.NextDouble() + lo;
                child.chromosome[c].Genes[i] = delta;
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

        private static void Immigrate(Individual[] population, double minGene, double maxGene, double mutateRate, double mutateChange,Genom target)
        {
            // kill off third-worst Individual and replace with new Individual
            // assumes population is sorted
            Individual immigrant = new Individual(target ,minGene, maxGene, mutateRate, mutateChange);
            int popSize = population.Length;
            population[popSize - 3] = immigrant; // replace third worst individual
        }

    // ===========================================

        public static double Error(Individual individ,Genom target)
        {
            if (individ.GenomLength < 1) return double.MaxValue;
            Genom x = new Genom(target.Genes.Count());
            double fitness = Distance(x, individ.chromosome[0]);
            for (int i = 0; i < individ.chromosome.Count - 1; ++i)
            {
                fitness += Distance(individ.Position(i), individ.Position(i + 1));
            }
            double lastSegment = Distance(individ.Position(individ.GenomLength - 1), target);

            //if (lastSegment == 0)
                //fitness+=fitness*( 1 - _mutateChange*target.Genes.Max(element=> Math.Abs(element))/individ.GenomLength);
            //else
          fitness += lastSegment * (maxNumberOfSteps - individ.GenomLength);
          return (reference==0)? fitness : (fitness-reference)/reference;
        }

    // ===========================================

  } // Program class

    // ------------------------------------------------------------------------------------------------

        public class Genom
    {
        public double[] Genes;
        public Genom(int numGenes)
        {
            Genes = new double[numGenes];
            for (int i = 0; i < numGenes; i++)
                Genes[i] = 0;
        }
    }

        public class Individual : IComparable<Individual>
        {
        public List<Genom> chromosome; // represents a solution
        //private double error; // smaller values are better for minimization
        public int GenomLength => chromosome.Count;
        public int GenesNumber => numGenes;

        private int numGenes; // problem dimenson
        private double minGene; // smallest value for a chromosome cell
        private double maxGene;
        private double mutateRate; // used during reproduction by Mutate
        private double mutateChange; // used during reproduction
        private Genom target;
        private int digits=0;
        private bool isDouble = false;

        public double Error=> GeneticAlgoritm.Error(this, this.target);

        public double Rnd(double min,double max)
        {
            if (isDouble) return Math.Round((max - min) * rnd.NextDouble() + min,digits);
            return rnd.Next((int)min, (int)max + 1);
        }

        public Genom Position(int geneIndex)
        {
            Genom position = new Genom(this.GenesNumber);
            for(int i=0;i<=geneIndex;i++)
            {
                for (int j = 0; j < this.GenesNumber; j++)
                {
                    position.Genes[j] += this.chromosome[i].Genes[j];
                }
            }
            return position;
        }

        static Random rnd = new Random(0); // used by ctor

        public Individual(Genom target, double minGene, double maxGene, double mutateRate, double mutateChange, bool isDouble=true )
        {
            this.numGenes = target.Genes.Count();
            this.minGene = minGene;
            this.maxGene = maxGene;
            this.mutateRate = mutateRate;
            this.mutateChange = mutateChange;
            this.target = target;
            this.isDouble = isDouble;
            this.digits = mutateChange.ToString().Substring(mutateChange.ToString().IndexOf(".")+1).Length;


            this.chromosome = new List<Genom>();

            IncreaseGenomMemory();
        }

        public void IncreaseGenomMemory(bool empty=false)
        {
            this.chromosome.Add(new Genom(this.numGenes));
            if (empty) return;
            for (int i = 0; i < this.chromosome.Last().Genes.Count(); i++)
            {
                this.chromosome.Last().Genes[i] = Rnd(minGene,maxGene);
            }
        }

    public int CompareTo(Individual other) // from smallest error (better) to largest
    {
      if (this.Error < other.Error) return -1;
      else if (this.Error > other.Error) return 1;
      else return 0;
    }
    }
}
