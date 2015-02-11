using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP.ModelTSP
{
    class GeneticAlgorithm
    {
        private LocationGA _startLocation;
		private KeyValuePair<LocationGA[], double>[] _populationWithDistances;
        private int _populationCount;
        int _maxTime;

        public GeneticAlgorithm(int popalationCount, int maxTime)
        {
            this._populationCount = popalationCount;
            this._maxTime = maxTime;
        }

        public Location[] Soulition(Cities cities)
        {

            //// для преобразования Location к LocationGA
            //Func<Location, LocationGA> toLocationGA = delegate(Location a)
            //{
            //    return (LocationGA)a;
            //};
            //LocationGA[] _randomLocations = Array.ConvertAll(cities.GetLocations(), new Converter<Location, LocationGA>(toLocationGA));
            ////_randomLocations = (LocationGA[])cities.GetLocations();  
            LocationGA[] _randomLocations = new LocationGA[cities.NumCities];
            _randomLocations = cities.GetLocations().Select(x => new LocationGA(x)).ToArray();
            GoGeneticAlgorithm((LocationGA)_randomLocations[0], _randomLocations);

            int i = _maxTime;
            Location[] _bestSolutionSoFar = GetBestSolutionSoFar().ToArray();

		    bool _mutateFailedCrossovers = false;
		    bool _mutateDuplicates = false;
		    bool _mustDoCrossovers = true;
            while (i-- > 0)
            {
                MustMutateFailedCrossovers = _mutateFailedCrossovers;
				MustDoCrossovers = _mustDoCrossovers;
				Reproduce();

					if (_mutateDuplicates)
						MutateDuplicates();

					var newSolution = GetBestSolutionSoFar().ToArray();
                    if (!newSolution.SequenceEqual(_bestSolutionSoFar))
                    {
                        _bestSolutionSoFar = newSolution;
                    }
            }
            return (Location[])_bestSolutionSoFar;

        }
        private void GoGeneticAlgorithm(LocationGA startLocation, LocationGA[] destinations )
		{
			if (startLocation == null)
				throw new ArgumentNullException("startLocation");

			if (destinations == null)
				throw new ArgumentNullException("destinations");

			if (_populationCount < 2)
				throw new ArgumentOutOfRangeException("populationCount");

            if (_populationCount % 2 != 0)
				throw new ArgumentException("The populationCount parameter must be an even value.", "populationCount");

			_startLocation = startLocation;
			destinations = (LocationGA[])destinations.Clone();

			foreach(var destination in destinations)
				if (destination == null)
					throw new ArgumentException("The destinations array can't contain null values.", "destinations");

			// This commented method uses a search of the kind "look for the nearest non visited LocationGA".
			// This is rarely the shortest path, yet it is already a "somewhat good" path.
			//destinations = _GetFakeShortest(destinations);

            _populationWithDistances = new KeyValuePair<LocationGA[], double>[_populationCount];

			// Create initial population.
            for (int solutionIndex = 0; solutionIndex < _populationCount; solutionIndex++)
			{
				var newPossibleDestinations = (LocationGA[])destinations.Clone();

				// Try commenting the next 2 lines of code while keeping the _GetFakeShortest active.
				// If you avoid the algorithm from running and press reset, you will see that it always
				// start with a path that seems "good" but is not the best.
				for(int randomIndex=0; randomIndex<newPossibleDestinations.Length; randomIndex++)
					RandomProvider.FullyRandomizeLocations(newPossibleDestinations);

				var distance = LocationGA.GetTotalDistance(startLocation, newPossibleDestinations);
				var pair = new KeyValuePair<LocationGA[], double>(newPossibleDestinations, distance);

				_populationWithDistances[solutionIndex] = pair;
			}

			Array.Sort(_populationWithDistances, _sortDelegate);
		}

		private LocationGA[] _GetFakeShortest(LocationGA[] destinations)
		{
			LocationGA[] result = new LocationGA[destinations.Length];

			var currentLocation = _startLocation;
			for(int fillingIndex=0; fillingIndex<destinations.Length; fillingIndex++)
			{
				int bestIndex = -1;
				double bestDistance = double.MaxValue;

				for(int evaluatingIndex=0; evaluatingIndex<destinations.Length; evaluatingIndex++)
				{
					var evaluatingItem = destinations[evaluatingIndex];
					if (evaluatingItem == null)
						continue;

					double distance = currentLocation.GetDistance(evaluatingItem);
					if (distance < bestDistance)
					{
						bestDistance = distance;
						bestIndex = evaluatingIndex;
					}
				}

				result[fillingIndex] = destinations[bestIndex];
				currentLocation = destinations[bestIndex];
				destinations[bestIndex] = null;
			}

			return result;
		}

		private static readonly Comparison<KeyValuePair<LocationGA[], double>> _sortDelegate = _Sort;
		private static int _Sort(KeyValuePair<LocationGA[], double> value1, KeyValuePair<LocationGA[], double> value2)
		{
			return value1.Value.CompareTo(value2.Value);
		}

        private IEnumerable<LocationGA> GetBestSolutionSoFar()
		{
			foreach(var location in _populationWithDistances[0].Key)
				yield return location;
		}

		public bool MustMutateFailedCrossovers { get; set; }
		public bool MustDoCrossovers { get; set; }

		public void Reproduce()
		{
			var bestSoFar = _populationWithDistances[0];

			int halfCount = _populationWithDistances.Length / 2;
			for(int i=0; i<halfCount; i++)
			{
				var parent = _populationWithDistances[i].Key;
				var child1 = _Reproduce(parent);
				var child2 = _Reproduce(parent);

				var pair1 = new KeyValuePair<LocationGA[], double>(child1, LocationGA.GetTotalDistance(_startLocation, child1));
				var pair2 = new KeyValuePair<LocationGA[], double>(child2, LocationGA.GetTotalDistance(_startLocation, child2));
				_populationWithDistances[i*2] = pair1;
				_populationWithDistances[i*2 + 1] = pair2;
			}

			// We keep the best alive from one generation to the other.
			_populationWithDistances[_populationWithDistances.Length-1] = bestSoFar;

			Array.Sort(_populationWithDistances, _sortDelegate);
		}

		public void MutateDuplicates()
		{
			bool needToSortAgain = false;
			int countDuplicates = 0;

			var previous = _populationWithDistances[0];
			for(int i=1; i<_populationWithDistances.Length; i++)
			{
				var current = _populationWithDistances[i];
				if (!previous.Key.SequenceEqual(current.Key))
				{
					previous = current;
					continue;
				}

				countDuplicates++;

				needToSortAgain = true;
				RandomProvider.MutateRandomLocations(current.Key);
				_populationWithDistances[i] = new KeyValuePair<LocationGA[], double>(current.Key, LocationGA.GetTotalDistance(_startLocation, current.Key));
			}

			if (needToSortAgain)
				Array.Sort(_populationWithDistances, _sortDelegate);
		}

		private LocationGA[] _Reproduce(LocationGA[] parent)
		{
			var result = (LocationGA[])parent.Clone();

			if (!MustDoCrossovers)
			{
				// When we are not using cross-overs, we always apply mutations.
				RandomProvider.MutateRandomLocations(result);
				return result;
			}

			// if you want, you can ignore the next three lines of code and the next
			// if, keeping the call to RandomProvider.MutateRandomLocations(result); always
			// invoked and without crossovers. Doing that you will not promove evolution through
			// "sexual reproduction", yet the good result will probably be found.
			int otherIndex = RandomProvider.GetRandomValue(_populationWithDistances.Length/2);
			var other = _populationWithDistances[otherIndex].Key;
			RandomProvider._CrossOver(result, other, MustMutateFailedCrossovers);

			if (!MustMutateFailedCrossovers)
				if (RandomProvider.GetRandomValue(10) == 0)
					RandomProvider.MutateRandomLocations(result);

			return result;
        } 

        private class LocationGA : Location
        {
            public LocationGA(Location derived)
            {
                this.X = derived.X;
                this.Y = derived.Y;
            }
            public static double GetTotalDistance(LocationGA startLocation, LocationGA[] locations)
            {
                if (startLocation == null)
                    throw new ArgumentNullException("startLocation");

                if (locations == null)
                    throw new ArgumentNullException("locations");

                if (locations.Length == 0)
                    throw new ArgumentException("The locations array must have at least one element.", "locations");

                foreach (var location in locations)
                    if (location == null)
                        throw new ArgumentException("The locations array can't contain null values.");

                double result = startLocation.GetDistance(locations[0]);
                int countLess1 = locations.Length - 1;
                for (int i = 0; i < countLess1; i++)
                {
                    var actual = locations[i];
                    var next = locations[i + 1];

                    var distance = actual.GetDistance(next);
                    result += distance;
                }

                result += locations[locations.Length - 1].GetDistance(startLocation);

                return result;
            }

            public static void SwapLocations(LocationGA[] locations, int index1, int index2)
            {
                if (locations == null)
                    throw new ArgumentNullException("locations");

                if (index1 < 0 || index1 >= locations.Length)
                    throw new ArgumentOutOfRangeException("index1");

                if (index2 < 0 || index2 >= locations.Length)
                    throw new ArgumentOutOfRangeException("index2");

                var location1 = locations[index1];
                var location2 = locations[index2];
                locations[index1] = location2;
                locations[index2] = location1;
            }

            // Moves an item in the list. That is, if we go from position 1 to 5, the items
            // that were previously 2, 3, 4 and 5 become 1, 2, 3 and 4.
            public static void MoveLocations(LocationGA[] locations, int fromIndex, int toIndex)
            {
                if (locations == null)
                    throw new ArgumentNullException("locations");

                if (fromIndex < 0 || fromIndex >= locations.Length)
                    throw new ArgumentOutOfRangeException("fromIndex");

                if (toIndex < 0 || toIndex >= locations.Length)
                    throw new ArgumentOutOfRangeException("toIndex");

                var temp = locations[fromIndex];

                if (fromIndex < toIndex)
                {
                    for (int i = fromIndex + 1; i <= toIndex; i++)
                        locations[i - 1] = locations[i];
                }
                else
                {
                    for (int i = fromIndex; i > toIndex; i--)
                        locations[i] = locations[i - 1];
                }

                locations[toIndex] = temp;
            }

            public static void ReverseRange(LocationGA[] locations, int startIndex, int endIndex)
            {
                if (locations == null)
                    throw new ArgumentNullException("locations");

                if (startIndex < 0 || startIndex >= locations.Length)
                    throw new ArgumentOutOfRangeException("startIndex");

                if (endIndex < 0 || endIndex >= locations.Length)
                    throw new ArgumentOutOfRangeException("endIndex");

                if (endIndex < startIndex)
                {
                    int temp = endIndex;
                    endIndex = startIndex;
                    startIndex = temp;
                }

                while (startIndex < endIndex)
                {
                    LocationGA temp = locations[endIndex];
                    locations[endIndex] = locations[startIndex];
                    locations[startIndex] = temp;

                    startIndex++;
                    endIndex--;
                }
            }
        }// LocationGA

        static class RandomProvider
        {
            private static readonly Random _random = new Random();
            public static int GetRandomValue(int limit)
            {
                return _random.Next(limit);
            }

            public static void MutateRandomLocations(LocationGA[] locations)
            {
                if (locations == null)
                    throw new ArgumentNullException("locations");

                if (locations.Length < 2)
                    throw new ArgumentException("The locations array must have at least two items.", "locations");

                // I opted to give up to 10% of the chromosome size in number of mutations.
                // Maybe I should find a better number of make this configurable.
                int mutationCount = GetRandomValue(locations.Length / 10) + 1;
                for (int mutationIndex = 0; mutationIndex < mutationCount; mutationIndex++)
                {
                    int index1 = GetRandomValue(locations.Length);
                    int index2 = GetRandomValue(locations.Length - 1);
                    if (index2 >= index1)
                        index2++;

                    switch (GetRandomValue(3))
                    {
                        case 0: LocationGA.SwapLocations(locations, index1, index2); break;
                        case 1: LocationGA.MoveLocations(locations, index1, index2); break;
                        case 2: LocationGA.ReverseRange(locations, index1, index2); break;
                        default: throw new InvalidOperationException();
                    }
                }
            }
            public static void FullyRandomizeLocations(LocationGA[] locations)
            {
                if (locations == null)
                    throw new ArgumentNullException("locations");

                // This code does a full randomization of the destination locations without creating a new array.
                // If we have 3 items, for example, it will first determine which one of the 3 will be in the last
                // place, swapping items if needed. Then, it will chose which one of the first 2 items is put at the 
                // second place. And, as everything works by swaps, the item in the first position is obviously the
                // only one that remains, that's why the i>0 is used instead of i>=0.
                int count = locations.Length;
                for (int i = count - 1; i > 0; i--)
                {
                    int value = GetRandomValue(i + 1);
                    if (value != i)
                        LocationGA.SwapLocations(locations, i, value);
                }
            }

            internal static void _CrossOver(LocationGA[] locations1, LocationGA[] locations2, bool mutateFailedCrossovers)
            {
                // I am not validating parameters because this method is internal.
                // If you want to make it public, you should validate the parameters.

                var availableLocations = new HashSet<LocationGA>(locations1);

                int startPosition = GetRandomValue(locations1.Length);
                int crossOverCount = GetRandomValue(locations1.Length - startPosition);

                if (mutateFailedCrossovers)
                {
                    bool useMutation = true;
                    int pastEndPosition = startPosition + crossOverCount;
                    for (int i = startPosition; i < pastEndPosition; i++)
                    {
                        if (locations1[i] != locations2[i])
                        {
                            useMutation = false;
                            break;
                        }
                    }

                    // if the crossover is not going to give any change, we
                    // force a mutation.
                    if (useMutation)
                    {
                        MutateRandomLocations(locations1);
                        return;
                    }
                }

                Array.Copy(locations2, startPosition, locations1, startPosition, crossOverCount);
                List<int> toReplaceIndexes = null;

                // Now we will remove the used locations from the available locations.
                // If we can't remove one, this means it was used in duplicate. At this
                // moment we only register those indexes that have duplicate locations.
                int index = 0;
                foreach (var value in locations1)
                {
                    if (!availableLocations.Remove(value))
                    {
                        if (toReplaceIndexes == null)
                            toReplaceIndexes = new List<int>();

                        toReplaceIndexes.Add(index);
                    }

                    index++;
                }

                // Finally we will replace duplicated items by those that are still available.
                // This is how we avoid having chromosomes that contain duplicated places to go.
                if (toReplaceIndexes != null)
                {
                    // To do this, we enumerate two objects in parallel.
                    // If we could use foreach(var indexToReplace, location from toReplaceIndexex, location1) it would be great.
                    using (var enumeratorIndex = toReplaceIndexes.GetEnumerator())
                    {
                        using (var enumeratorLocation = availableLocations.GetEnumerator())
                        {
                            while (true)
                            {
                                if (!enumeratorIndex.MoveNext())
                                {
                                    //Debug.Assert(!enumeratorLocation.MoveNext());
                                    break;
                                }

                                if (!enumeratorLocation.MoveNext())
                                    throw new InvalidOperationException("Something wrong happened.");

                                locations1[enumeratorIndex.Current] = enumeratorLocation.Current;
                            }
                        }
                    }
                }
            }

        }// RandomProvider


    }// GeneticAlgorithm
}
