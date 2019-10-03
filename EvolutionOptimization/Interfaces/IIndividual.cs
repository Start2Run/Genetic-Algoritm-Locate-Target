using System;
using System.Collections.Generic;

namespace EvolutionOptimization.Interfaces
{
    public interface IIndividual : IComparable<IIndividual>

    {
        Guid Id { get; set; }
        List<IGenome> Chromosome { get; } // represents a solution
        double Error { get; }
        int GenomeLength { get; }
        double GetRnd(double min, double max);
        void IncreaseGenomeMemory(bool empty = false);
    }
}
