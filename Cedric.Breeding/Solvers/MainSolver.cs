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
            if (bitSolver.AlleleWithFullBits == null)
                throw new ApplicationException("failure");
            RecessiveSolver recessiveSolver = new RecessiveSolver(PoolOfPlants);
            var fullRecessivePlant = recessiveSolver.Solve(bitSolver.AlleleWithFullBits.Value);

            var PlantSolver = new PlantSolver(this.PoolOfPlants);
            PlantSolver.Solve(targets, fullRecessivePlant);
           
        }
        
    }
}
