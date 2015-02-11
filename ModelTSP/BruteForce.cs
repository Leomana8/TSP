using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP.ModelTSP
{
    class BruteForce
    {
        private double _totalDistance;
        private decimal _maxTour;
        public double TotalDistance
        {
            get { return _totalDistance; }
        }

        public BruteForce(decimal maxTour)
        {
            _maxTour = maxTour;
        }
        public int[] Solution(Cities cities)
        {
            List<int> tour = new List<int>();
            for(int i = 0; i < cities.NumCities; i++ )
            {
                tour.Add(i);
            }
            double best_score = cities.GetTotalDistance(tour.ToArray());
            List<int> best_tour = new List<int>(tour);
            decimal maxItr = _maxTour;
            foreach (var perm in Permutate(tour, tour.Count))
            {
                double s = cities.GetTotalDistance(tour.ToArray());
                if (s < best_score)
                {
                    best_score = s;
                    best_tour = new List<int>(perm as List<int>);
                }
                if (--maxItr < 0)
                    break;
            }
            _totalDistance = best_score;
            return best_tour.ToArray();
        }

        public static void RotateRight(IList sequence, int count)
        {
            object tmp = sequence[count - 1];
            sequence.RemoveAt(count - 1);
            sequence.Insert(0, tmp);
        }

        public static IEnumerable<IList> Permutate(IList sequence, int count)
        {
            if (count == 1) yield return sequence;
            else
            {
                for (int i = 0; i < count; i++)
                {
                    foreach (var perm in Permutate(sequence, count - 1))
                        yield return perm;
                    RotateRight(sequence, count);
                }
            }
        }
    }
}
