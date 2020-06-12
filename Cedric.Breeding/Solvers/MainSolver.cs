using System;
using System.Linq;
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

        public void Solve(SetOfPlants targets)
        {
            BitSolver bitSolver = new BitSolver(PoolOfPlants);
            bitSolver.Solve(targets);
        }
        
    }
}
