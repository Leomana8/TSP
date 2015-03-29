using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP.ModelTSP
{
    // алгоритм иммитации и отжига
    // http://www.codeproject.com/Articles/26758/Simulated-Annealing-Solving-the-Travelling-Salesma
    class SimulatedAnnealing
    {
        private string filePath;
        private List<int> currentOrder = new List<int>();
        private List<int> nextOrder = new List<int>();
        private double[,] distances;
        private Random random = new Random();
        private double shortestDistance = 0;
        private double temperature;
        private double coolingRate;
        private double absoluteTemperature;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="temperature">температура</param>
        /// <param name="absoluteTemperature">абсолютная температура</param>
        /// <param name="coolingRate">скорость охлаждения</param>
        public SimulatedAnnealing(double temperature, double absoluteTemperature, double coolingRate)
        {
            this.temperature = temperature;
            this.absoluteTemperature = absoluteTemperature;
            this.coolingRate = coolingRate;
        }

        public double TotalDistance
        {
            get
            {
                return shortestDistance;
            }
        }

        public string FilePath
        {
            get
            {
                return filePath;
            }
            set
            {
                filePath = value;
            }
        }

        public List<int> CitiesOrder
        {
            get
            {
                return currentOrder;
            }
            set
            {
                currentOrder = value;
            }
        }

        /// <summary>
        /// Load cities from the text file representing the adjacency matrix
        /// </summary>
        private void LoadCities(Cities cities)
        {
            double[][] arrayDistances = cities.GetArrayDistances();
            int numCities = cities.NumCities;
            distances = new double[numCities, numCities];
            for (int i = 0; i < numCities; i++)
            {
                for (int j = 0; j < numCities; j++)
                {
                    distances[i, j] = (double)arrayDistances[i][j];
                }

                //the number of rows in this matrix represent the number of cities
                //we are representing each city by an index from 0 to N - 1
                //where N is the total number of cities
                currentOrder.Add(i);
            }

            if (currentOrder.Count < 1)
                throw new Exception("No cities to order.");
        }

        /// <summary>
        /// Calculate the total distance which is the objective function
        /// </summary>
        /// <param name="order">A list containing the order of cities</param>
        /// <returns></returns>
        private double GetTotalDistance(List<int> order)
        {
            double distance = 0;

            for (int i = 0; i < order.Count - 1; i++)
            {
                distance += distances[order[i], order[i + 1]];
            }

            if (order.Count > 0)
            {
                distance += distances[order[order.Count - 1], 0];
            }

            return distance;
        }

        /// <summary>
        /// Get the next random arrangements of cities
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        private List<int> GetNextArrangement(List<int> order)
        {
            List<int> newOrder = new List<int>();

            for (int i = 0; i < order.Count; i++)
                newOrder.Add(order[i]);

            //we will only rearrange two cities by random
            //starting point should be always zero - so zero should not be included

            int firstRandomCityIndex = random.Next(1, newOrder.Count);
            int secondRandomCityIndex = random.Next(1, newOrder.Count);

            int dummy = newOrder[firstRandomCityIndex];
            newOrder[firstRandomCityIndex] = newOrder[secondRandomCityIndex];
            newOrder[secondRandomCityIndex] = dummy;

            return newOrder;
        }

        /// <summary>
        /// Annealing Process
        /// </summary>
        public int[] Solution(Cities cities)
        {
            int iteration = -1;
            double deltaDistance = 0;
            LoadCities(cities);

            double distance = GetTotalDistance(currentOrder);

            while (temperature > absoluteTemperature)
            {
                nextOrder = GetNextArrangement(currentOrder);

                deltaDistance = GetTotalDistance(nextOrder) - distance;

                //if the new order has a smaller distance
                //or if the new order has a larger distance but satisfies Boltzman condition then accept the arrangement
                if ((deltaDistance < 0) || (distance > 0 && Math.Exp(-deltaDistance / temperature) > random.NextDouble()))
                {
                    for (int i = 0; i < nextOrder.Count; i++)
                        currentOrder[i] = nextOrder[i];

                    distance = deltaDistance + distance;
                }

                //cool down the temperature
                temperature *= coolingRate;

                iteration++;
            }
            shortestDistance = distance;
            return CitiesOrder.ToArray();
        }
    }
}
