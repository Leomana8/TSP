using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP.ModelTSP
{
    // метод ветвей и границ
    // http://axon.cs.byu.edu/~martinez/classes/312/Projects/TSP/TSP.html
    // http://www.java2s.com/Open-Source/CSharp_Free_Code/Algorithm/Download_TSP_with_Branch_and_Bound.htm
    class BranchAndBound
    {
        #region Constructors
        private double _totalDistance;

        public double TotalDistance
        {
            get { return _totalDistance; }
        }
        private int _time;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="time">время расчета</param>
        public BranchAndBound(int time)
        {
            _time = time;
        }

        public Location[] Solution(Cities cities)
        {
            this._seed = 0;
            this._size = cities.NumCities;
            Cities = new City[_size];
            Route = new ArrayList(_size);
            bssf = null;
            for (int i = 0; i < _size; i++)
                Cities[i] = new City(cities.GetLocation(i).X, cities.GetLocation(i).Y);
            solveProblem();
            Location[] resault = new Location[bssf.Route.Count];
            int j = 0;
            foreach (City loc in bssf.Route)
            {
                resault[j] = new Location();
                resault[j].X = (int)loc.X;
                resault[j].Y = (int)loc.Y;
                j++;
            }
            _totalDistance = bssf.costOfRoute();
            return resault;
        }
        #endregion
        private class City
        {
            public City(double x, double y)
            {
                _X = x;
                _Y = y;
            }

            private double _X;
            private double _Y;

            public double X
            {
                get { return _X; }
                set { _X = value; }
            }

            public double Y
            {
                get { return _Y; }
                set { _Y = value; }
            }

            /// <summary>
            /// how much does it cost to get from this to the destination?
            /// note that this is an asymmetric cost function
            /// </summary>
            /// <param name="destination">um, the destination</param>
            /// <returns></returns>
            public double costToGetTo(City destination)
            {
                return Math.Sqrt(Math.Pow(this.X - destination.X, 2) + Math.Pow(this.Y - destination.Y, 2));
            }
        }
        private class TSPSolution
        {

            /// <summary>
            /// we use the representation [cityB,cityA,cityC] 
            /// to mean that cityB is the first city in the solution, cityA is the second, cityC is the third 
            /// and the edge from cityC to cityB is the final edge in the path.  
            /// you are, of course, free to use a different representation if it would be more convenient or efficient 
            /// for your node data structure and search algorithm. 
            /// </summary>
            public ArrayList
                Route;

            public TSPSolution(ArrayList iroute)
            {
                Route = new ArrayList(iroute);
            }


            /// <summary>
            ///  compute the cost of the current route.  does not check that the route is complete, btw.
            /// assumes that the route passes from the last city back to the first city. 
            /// </summary>
            /// <returns></returns>
            public double costOfRoute()
            {
                // go through each edge in the route and add up the cost. 
                int x;
                City here;
                double cost = 0D;

                for (x = 0; x < Route.Count - 1; x++)
                {
                    here = Route[x] as City;
                    cost += here.costToGetTo(Route[x + 1] as City);
                }
                // go from the last city to the first. 
                here = Route[Route.Count - 1] as City;
                cost += here.costToGetTo(Route[0] as City);
                return cost;
            }
        }

        #region private members

        /// <summary>
        /// the cities in the current problem.
        /// </summary>
        private City[] Cities;
        /// <summary>
        /// a route through the current problem, useful as a temporary variable. 
        /// </summary>
        private ArrayList Route;
        /// <summary>
        /// best solution so far. 
        /// </summary>
        private TSPSolution bssf;

        /// <summary>
        /// keep track of the seed value so that the same sequence of problems can be 
        /// regenerated next time the generator is run. 
        /// </summary>
        private int _seed;
        /// <summary>
        /// number of cities to include in a problem. 
        /// </summary>
        private int _size;

        /// <summary>
        /// random number generator. 
        /// </summary>
        private Random rnd;
        #endregion

        #region private members.
        private int Size
        {
            get { return _size; }
        }

        private int Seed
        {
            get { return _seed; }
        }
        #endregion

        private const int DEFAULT_SEED = -1;

        #region Private Methods

        #endregion

        #region Public Methods

        /// <summary>
        ///  return the cost of the best solution so far. 
        /// </summary>
        /// <returns></returns>
        private double costOfBssf()
        {
            if (bssf != null)
                return (bssf.costOfRoute());
            else
                return -1D;
        }

        /// <summary>
        ///  solve the problem.  This is the entry point for the solver when the run button is clicked
        /// right now it just picks a simple solution. 
        /// </summary>
        /// 
        PriorityQueue PQ;
        double BSSF;
        double currBound;
        List<int> BSSFList;
        double[] rowMins;

        private void solveProblem()
        {
            // Start off with some var power!
            Route = new ArrayList();
            double minCost;
            int minIndex = 0;
            int currIndex = 0;

            // Set our BSSF to 0, so we can create a new better one
            BSSFList = new List<int>();
            BSSFList.Add(0);

            // Begin with our first city
            Route.Add(Cities[currIndex]);

            // Use the nearest neighbor greedy algorithm
            // to find a random (naive) solution
            while (Route.Count < Cities.Length)
            {
                minCost = double.MaxValue;
                for (int j = 0; j < Cities.Length; j++)
                {
                    if (j != currIndex)
                        if (!Route.Contains(Cities[j]))
                        {
                            double currCost = Cities[currIndex].costToGetTo(Cities[j]);
                            if (currCost < minCost)
                            {
                                minCost = currCost;
                                minIndex = j;
                            }
                        }
                }
                // Update the BSSDlist and Route (creating BSSF)
                currIndex = minIndex;
                Route.Add(Cities[currIndex]);
                BSSFList.Add(currIndex);
            }
            // Save solution
            bssf = new TSPSolution(Route);
            BSSF = bssf.costOfRoute();

            //Build matrix for initial state
            double[,] initialMatrix = buildInitialMatrix();

            //Get the minimum cost for the remaining cities; kinda like bound 
            rowMins = getRowMins();

            //Generate list of children for initial state
            List<int> initStateChildren = new List<int>();
            for (int i = 1; i < Cities.Length; i++)
                initStateChildren.Add(i);

            //Build initial state                                           
            TSPState initialState = new TSPState(0, initStateChildren, initialMatrix, new List<int>(), 0, 0, 0);
            initialState.Bound += boundingFunction(initialState);

            //Set the bound 
            currBound = initialState.Bound;

            //Start our PQ and load with init state
            PQ = new PriorityQueue();
            PQ.Enqueue(initialState, initialState.Bound);

            //Run Branch and Bound 
            branchAndBoundEngine();

        }

        DateTime endTime;

        private void branchAndBoundEngine()
        {
            // Start the stop watch
            DateTime startTime = DateTime.Now;
            endTime = startTime.AddMilliseconds(_time);

            // Run until the PQ is empty, we find an optimal solution, or time runs out
            while (!PQ.IsEmpty() && DateTime.Now < endTime && BSSF > currBound)
            {
                // Get a state from the PQ
                TSPState state = PQ.Dequeue();
                // Check to see if the state is worth evaluating
                if (state.Bound < BSSF)
                {
                    // Generate the states children and iterate
                    List<TSPState> children = generateChildren(state);
                    foreach (TSPState child in children)
                    {
                        // If the bound is worth investigating...
                        if (child.Bound < bssf.costOfRoute())
                        {
                            // Check for a solution and save
                            if (child.IsSolution && child.Cost < BSSF)
                            {
                                // Save solution
                                BSSF = child.Cost;
                                BSSFList = child.PathSoFar;
                            }
                            // Otherwise assign the state's bound and Enqueue
                            else
                            {
                                double bound = child.Bound;
                                // Our bound of min cost path to destination + state bound
                                foreach (int childIndex in child.ChildList)
                                    bound += rowMins[childIndex];
                                PQ.Enqueue(child, bound);
                            }
                        }
                    }
                }
                GC.Collect();
            }

            //
            // END BRANCH AND BOUND
            //  

            // Clear the route
            Route.Clear();
            // Save the BSSF route
            for (int i = 0; i < BSSFList.Count; i++)
                Route.Add(Cities[BSSFList[i]]);

            // Create our soltuion and assign
            bssf = new TSPSolution(Route);

        }

        private List<TSPState> generateChildren(TSPState state)
        {
            // Create new state list
            List<TSPState> children = new List<TSPState>();
            // Iterate through the current child's children
            foreach (int child in state.ChildList)
            {

                // Copy values from parent state so we can modify
                List<int> childList = new List<int>(state.ChildList);
                List<int> pathSoFar = new List<int>(state.PathSoFar);
                double cost = Cities[state.City].costToGetTo(Cities[child]);
                double[,] matrix = (double[,])state.Matrix.Clone();

                // Remove child from child list
                childList.Remove(child);
                // Add the parent state city to the path so far
                pathSoFar.Add(state.City);

                // Reduce the matrix
                for (int j = 0; j <= matrix.GetUpperBound(0); j++)
                    matrix[j, state.City] = double.MaxValue;

                // Create a new state
                TSPState newState = new TSPState(state.Bound + state.Matrix[state.City, child], childList, matrix, pathSoFar, state.Cost + cost, child, state.TreeDepth + 1);
                // Update the bound
                newState.Bound += boundingFunction(newState);

                // Check for a soltuion
                if (newState.IsSolution)
                {
                    // Mark state as a solution
                    newState.Cost += Cities[newState.City].costToGetTo(Cities[0]);
                    newState.PathSoFar.Add(newState.City);
                }

                // Add child to childrens state
                children.Add(newState);
            }

            // Returnt the list for later usage
            return children;
        }

        private double[,] buildInitialMatrix()
        {
            // Create a matrix
            double[,] matrix = new double[Cities.Length, Cities.Length];
            for (int i = 0; i < Cities.Length; i++)
            {
                for (int j = 0; j < Cities.Length; j++)
                {
                    if (i == j)
                        // Assign infinity if i == j
                        matrix[i, j] = double.MaxValue;
                    else
                        // Otherwise populate the matrix with real numbers
                        matrix[i, j] = Cities[i].costToGetTo(Cities[j]);
                }
            }
            return matrix;
        }

        private double[] getRowMins()
        {
            // Create an array getting the min cost from cities
            double[] rowMins = new double[Cities.Length];
            for (int i = 0; i < Cities.Length; i++)
            {
                double rowMin = double.MaxValue;
                for (int j = 0; j < Cities.Length; j++)
                {
                    if (i != j)
                    {
                        double currCost = Cities[i].costToGetTo(Cities[j]);
                        if (currCost < rowMin)
                            rowMin = currCost;
                    }
                }
                rowMins[i] = rowMin;
            }

            return rowMins;
        }

        private double boundingFunction(TSPState state)
        {
            // Start with 0 vals and the state's matrix
            double bound = 0;
            double numRows = 0;
            double[,] matrix = state.Matrix;

            // Reduce the matrix rows
            // Create a child city list
            List<int> childList = new List<int>(state.ChildList);
            // Add the current city to the list, creating all cities
            childList.Add(state.City);

            // Iterate through state's 1D col or row
            for (int i = 0; i < childList.Count; i++)
            {
                // Look for the min cost
                double minCost = double.MaxValue;
                // Interage through the other state's 1D col or row
                for (int j = 0; j < state.ChildList.Count; j++)
                {
                    // Check if cost is less than min...
                    if (matrix[childList[i], state.ChildList[j]] < minCost)
                    {
                        // Update the min cost
                        minCost = matrix[childList[i], state.ChildList[j]];
                    }
                }

                // Then once you find min cost and it's not infinity...
                if (minCost < double.MaxValue)
                {
                    // Reduce the matrix
                    for (int j = 0; j < state.ChildList.Count; j++)
                        matrix[childList[i], state.ChildList[j]] -= minCost;
                    // Add the cost to the bound
                    bound += minCost;
                }
                else
                    // Mark as infinity
                    numRows++;
            }

            // Reduce the matrix columns
            for (int i = 0; i < state.ChildList.Count; i++)
            {
                // Look for the min cost
                double minCost = double.MaxValue;
                // Iterate through each column
                for (int j = 0; j < childList.Count; j++)
                {
                    // Update min cost if the cell's value is less
                    if (matrix[childList[j], state.ChildList[i]] < minCost)
                        minCost = matrix[childList[j], state.ChildList[i]];
                }
                // Now that we have min cost, see if it is less than infinity
                if (minCost < double.MaxValue)
                    // If so, reduce the matrix column
                    for (int j = 0; j < childList.Count; j++)
                        matrix[childList[j], state.ChildList[i]] -= minCost;
                else
                    // Mark as infinity
                    numRows++;
            }

            // If entire matrix is infinity
            if (numRows >= matrix.GetUpperBound(0))
            {
                // Save solution
                state.IsSolution = true;
                state.Cost += Cities[1].costToGetTo(Cities[state.City]);
            }
            // Return 0 if it is a solution
            if (state.IsSolution)
                return 0;

            // Otherwise return the bound as normal
            return bound;
        }
        #endregion

        private class TSPState
        {
            private double bound;
            private List<int> childList;
            private double[,] matrix;
            private List<int> pathSoFar;
            private double cost;
            private int city;
            private int treeDepth;
            private bool isSolution;

            public TSPState(double Bound, List<int> ChildList, double[,] Matrix, List<int> PathSoFar,
                            double Cost, int City, int TreeDepth)
            {
                bound = Bound;
                childList = ChildList;
                matrix = Matrix;
                pathSoFar = PathSoFar;
                cost = Cost;
                city = City;
                treeDepth = TreeDepth;

                if (childList.Count == 0)
                    isSolution = true;
                else
                    isSolution = false;


            }

            public double Bound
            {
                get { return bound; }
                set { bound = value; }
            }

            public List<int> ChildList
            {
                get { return childList; }
                set { childList = value; }
            }

            public double[,] Matrix
            {
                get { return matrix; }
                set { matrix = value; }
            }

            public List<int> PathSoFar
            {
                get { return pathSoFar; }
                set { pathSoFar = value; }
            }

            public double Cost
            {
                get { return cost; }
                set { cost = value; }
            }

            public int City
            {
                get { return city; }
                set { city = value; }
            }

            public int TreeDepth
            {
                get { return treeDepth; }
                set { treeDepth = value; }
            }

            public Boolean IsSolution
            {
                get { return isSolution; }
                set { isSolution = value; }
            }
        }
        class PriorityQueue
        {
            int total_size;
            SortedDictionary<double, Queue> storage;

            public PriorityQueue()
            {
                this.storage = new SortedDictionary<double, Queue>();
                this.total_size = 0;
            }

            public int size
            {
                get { return total_size; }
            }

            public bool IsEmpty()
            {
                // Check if empty: based on total_size because keys can have multiple states
                return (total_size == 0);
            }

            public TSPState Dequeue()
            {
                if (IsEmpty())
                {
                    throw new Exception("Please check that priorityQueue is not empty before dequeing");
                }
                else
                {
                    // Get first item from the sorted dictionary
                    var kv = storage.First();
                    // Get the Queue from the val
                    Queue q = kv.Value;
                    // Then grab the first item from it
                    TSPState deq = (TSPState)q.Dequeue();

                    // Remove if now empty
                    if (q.Count == 0)
                        storage.Remove(kv.Key);

                    // Decrement the total size
                    total_size--;

                    // Return the state that's been dequeued
                    return deq;
                }
            }

            public void Enqueue(TSPState item, double prio)
            {
                // Check if key prio exists
                if (!storage.ContainsKey(prio))
                {
                    // Add a new queue for key prio
                    storage.Add(prio, new Queue());
                }
                // Enqueue state at prio in queue
                storage[prio].Enqueue(item);
                // Inc the total size of the agenda
                total_size++;
            }

            public bool Contains(TSPState state)
            {
                // Get the state's priority for lookup
                double prio = state.Bound;
                // If it doesn't exist, absolutely false
                if (!storage.ContainsKey(prio))
                {
                    return false;
                }
                else
                {
                    // Otherwise run contains on the inner Queue
                    return storage[prio].Contains(state);
                }
            }
        }


    }
}

