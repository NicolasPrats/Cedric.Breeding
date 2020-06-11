using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cedric.Breeding.Data;
using Cedric.Breeding.Solvers;

namespace Cedric.Breeding
{
    class Program
    {
        static string[] Sample = new string[] {
"YHGWGX",
"WHWXYH",
"XYWWGX",
"WGYGXX",
"YYYWYH",
"XXYWYW",
"WHXWHY",
"WGHWHH",
"YYYWYW",
"WGYWHW",
"XGGWGH",
"WYYXYX",
"XGHHGW",
"XYYWGH",
"WHYWHW",
"WYYWGG",
"YYWYHX",
"WXYXGH",
"XHHWWX",
"XWYXGX",
"XGHXGW",
"HHXWHX"
};
        static void Main()
        {
            //var targetPlant = PlantFactory.Instance.GetRandomPlant();
            var targetPlants = new Plant[] {
                PlantFactory.Instance.ParsePlant("YYYYGG", double.MaxValue),
                PlantFactory.Instance.ParsePlant("YYYGGH", double.MaxValue),
                PlantFactory.Instance.ParsePlant("YYYYYY", double.MaxValue),
                PlantFactory.Instance.ParsePlant("GGGGGG", double.MaxValue),
                PlantFactory.Instance.ParsePlant("YGHYGH", double.MaxValue)
             };


            var poolOfPlants = new SetOfPlants();

            foreach (var genome in Sample)
            {
                Plant item = PlantFactory.Instance.ParsePlant(genome, 0);
                poolOfPlants.Add(item);
            }

            MainSolver solver = new MainSolver(poolOfPlants);
            solver.Solve(targetPlants);
        }

    }

}
