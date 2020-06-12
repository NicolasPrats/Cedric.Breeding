using System;
using System.Collections.Generic;
using System.Text;
using Cedric.Breeding.Data;

namespace Cedric.Breeding.Solvers
{
    public class MainSolver
    {
        public SetOfPlants PoolOfPlants { get; }

        public MainSolver(SetOfPlants poolOfPlants)
        {
            this.PoolOfPlants = poolOfPlants;
        }

        public void Solve(Plant[] targets)
        {
            DominantsSolver almostFullDominantsSolver = new DominantsSolver(PoolOfPlants);
            almostFullDominantsSolver.Solve();
            RecessiveSolver recessiveSolver = new RecessiveSolver(PoolOfPlants);
            recessiveSolver.Solve();
        }
        
    }
}
