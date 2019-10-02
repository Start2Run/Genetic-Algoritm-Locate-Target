using System;
using System.Collections.Generic;
using GeneticAlgorithm.Interfaces;

namespace EvolutionOptimization.Interfaces
{
    public interface IIndividual: IComparable<IIndividual>

    {
    Guid Id { get; set; }
    List<IGenome> Chromosome { get; } // represents a solution
    double Error { get; }
    int GenomeLength { get; }
    IGenome Position(int i);
    }
}
