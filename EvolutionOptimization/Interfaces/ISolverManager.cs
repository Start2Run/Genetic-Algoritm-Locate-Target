using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EvolutionOptimization.Interfaces
{
    public interface ISolverManager
    {
        Task<IIndividual> Solver(CancellationToken cts);
        Action<IEnumerable<IIndividual>> UpdateAction { get; set; }
    }
}
