using System;
using System.Collections.Generic;
using System.Text;

namespace Cedric.Breeding
{
    public class Path
    {
        private State LastState { get; }
        private Path? PreviousPath { get; }

        private int Distance { get; set; }
        private int EstimatedRemainingDistance => LastState.EstimatedRemainingDistance;

        public int EstimatedTotalDistance => Distance + EstimatedRemainingDistance;
        public bool IsComplete => EstimatedRemainingDistance == 0;

        public Path(State lastState, Path? previousPath = null)
        {
            this.LastState = lastState;
            this.PreviousPath = previousPath;
            if (previousPath != null)
            {
                // TODO on compte 1 par fusion de plantes
                // mais on pourrait aussi incrémenter du nombre de plantes fusionnées
                // Est-ce qu'on préfère diminuer le nombre de fusions ou le nombre de plantes fusionnées ?
                Distance = previousPath.Distance + 1; 
            }
        }

        internal IEnumerable<State> GetNextStates()
        {
            return this.LastState.GetNextStates();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            BuildString(builder);
            return builder.ToString();
        }

        private void BuildString(StringBuilder builder)
        {
            if (PreviousPath != null)
            {
                PreviousPath.BuildString(builder);
                builder.AppendLine();
            }
            builder.AppendLine(this.LastState.Origin);
        }

    }
}
