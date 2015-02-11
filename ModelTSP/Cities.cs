using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP.ModelTSP
{
    public class Location
    {
        int x;
        public int X
        {
            get { return x; }
            set { x = value; }
        }
        int y;
        public int Y
        {
            get { return y; }
            set { y = value; }
        }
        public int GetDistance(Location other)
        {
            int diffX = X - other.X;
            int diffY = Y - other.Y;
            return (int)Math.Sqrt(diffX * diffX + diffY * diffY);
        }

    };
    class Cities
    {
        int _numCities;
        int[][] _dists;
        Location[] _locations;
        public Location GetLocation(int city)
        {
            return _locations[city];
        }
        public Location[] GetLocations()
        {
            return _locations;
        }
	    public int NumCities
	    {
		    get { return _numCities; }
	    }

        public Cities(int numCities)
        {
            _numCities = numCities;
            if (numCities < 3)
            {
                _numCities = 3;
            }
            _dists = new int[_numCities][];
            for (int i = 0; i < _dists.Length; ++i)
                _dists[i] = new int[_numCities];
            _locations = new Location[_numCities];
        }

        public void Generate(int maxDist)
        {
            Random random = new Random();
            // генерация координат
            for (int i = 0; i < _numCities; ++i)
            {
                _locations[i] = new Location();
                _locations[i].X = random.Next(4, maxDist -4);
                _locations[i].Y = random.Next(4, maxDist -4);
            }
            // рассчет дистанций
            for (int i = 0; i < _numCities; ++i)
            {
                for (int j = i + 1; j < _numCities; ++j)
                {
                    int d = _locations[i].GetDistance(_locations[j]);
                    _dists[i][j] = d;
                    _dists[j][i] = d;
                }
            }

        }
        public int GetDistance(int city1, int city2)
        {
            return _dists[city1][city2];
        }
        public int[][] GetArrayDistances()
        {
            return _dists;
        }
    }
}
