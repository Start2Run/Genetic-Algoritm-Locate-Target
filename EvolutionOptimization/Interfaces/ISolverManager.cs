using System.Threading.Tasks;

namespace EvolutionOptimization.Interfaces
{
    public interface IEvolutionaryOptimization
    {
        Task<IIndividual> Solver();
    }
}
