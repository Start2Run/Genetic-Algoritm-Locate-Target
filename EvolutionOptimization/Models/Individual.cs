using System;
using System.Collections.Generic;
using System.Linq;
using EvolutionOptimization.Helpers;
using EvolutionOptimization.Interfaces;

namespace EvolutionOptimization.Models
{
    public class Individual : IIndividual
    {
        private static readonly Random Rnd = new Random(0); // used by ctor
        private readonly Genome _target;
        private readonly double _refError;
        private IConfiguration _config;
        public Guid Id { get; set; } = Guid.NewGuid();
        public List<IGenome> Chromosome { get; }
        public double Error => Helper.Error(this, _target, _refError, _config);
        public int GenomeLength => Chromosome.Count;

        public Individual(Genome target, double refError, IConfiguration config)
        {
            _target = target;
            _refError = refError;
            _config = config;

            Chromosome = new List<IGenome>();

            IncreaseGenomeMemory();
        }

        public int CompareTo(IIndividual other) // from smallest error (better) to largest
        {
            if (Error < other.Error) return -1;
            return Error > other.Error ? 1 : 0;
        }

        public IGenome Position(int geneIndex)
        {
            var position = new Genome(_target.Genes.Length);
            for (var i = 0; i <= geneIndex; i++)
            {
                for (var j = 0; j < _target.Genes.Length; j++)
                {
                    position.Genes[j] += Chromosome[i].Genes[j];
                }
            }
            return position;
        }

        public double GetRnd(double min, double max)
        {
            if (_config.HasDoubleValues) return Math.Round((max - min) * Rnd.NextDouble() + min, _config.Digits);
            return Rnd.Next((int)min, (int)max + 1);
        }

        public void IncreaseGenomeMemory(bool empty = false)
        {
            Chromosome.Add(new Genome(_config.NumberOfGenes));
            if (empty) return;
            for (var i = 0; i < Chromosome.Last().Genes.Count(); i++)
            {
                Chromosome.Last().Genes[i] = GetRnd(_config.MinX, _config.MaxX);
            }
        }
    }
}
