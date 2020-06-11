using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


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
                PlantFactory.Instance.ParsePlant("YYYYGG"),
                PlantFactory.Instance.ParsePlant("YYYGGH"),
                PlantFactory.Instance.ParsePlant("YYYYYY"),
                PlantFactory.Instance.ParsePlant("GGGGGG"),
                PlantFactory.Instance.ParsePlant("YGHYGH")
             };


            var poolOfPlants = new SetOfPlants();

            foreach (var genome in Sample)
            {
                Plant item = PlantFactory.Instance.ParsePlant(genome);
                poolOfPlants.Add(item);
            }

          
        }

        

      
    }

}
