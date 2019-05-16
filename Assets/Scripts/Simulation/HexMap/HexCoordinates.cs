using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.HexMap {

    [Serializable]
    public struct HexCoordinates : IEquatable<HexCoordinates> {

        #region static fields and properties

        private static HexCoordinates[] ClockwiseNeighborDirections = new HexCoordinates[6] {
            new HexCoordinates(0, 1),
            new HexCoordinates(1, 0),
            new HexCoordinates(1, -1),
            new HexCoordinates(0, -1),
            new HexCoordinates(-1, 0),
            new HexCoordinates(-1, 1),
        };

        #endregion

        #region instance fields and properties

        public int X {
            get { return _x; }
            private set { _x = value; }
        }
        [SerializeField] private int _x;

        public int Z {
            get { return _z; }
            private set { _z = value; }
        }
        [SerializeField] private int _z;

        public int Y {
            get { return -X - Z; }
        }

        #endregion

        #region constructors

        public HexCoordinates(int x, int z) {
            _x = x;
            _z = z;
        }

        #endregion

        #region static methods

        public static HexCoordinates FromOffsetCoordinates(int x, int z) {
            return new HexCoordinates(x - z / 2, z);
        }

        public static int ToOffsetCoordinateX(HexCoordinates cubeCoords) {
            return cubeCoords.X + (cubeCoords.Z - (cubeCoords.Z & 1)) / 2;
        }

        public static int ToOffsetCoordinateZ(HexCoordinates cubeCoords) {
            return cubeCoords.Z;
        }

        public static HexCoordinates Add(HexCoordinates a, HexCoordinates b) {
            return new HexCoordinates(a.X + b.X, a.Z + b.Z);
        }

        public static HexCoordinates Subtract(HexCoordinates a, HexCoordinates b) {
            return new HexCoordinates(a.X - b.X, a.Z - b.Z);
        }

        public static HexCoordinates Scale(HexCoordinates a, int k) {
            return new HexCoordinates(a.X * k, a.Z * k);
        }

        public static HexCoordinates GetCoordinateInDirection(HexDirection direction) {
            return ClockwiseNeighborDirections[(int)direction];
        }

        public static HexCoordinates GetNeighborInDirection(HexCoordinates coordinates, HexDirection direction) {
            var clockwiseDirection = ClockwiseNeighborDirections[(int)direction];

            return new HexCoordinates(coordinates.X + clockwiseDirection.X, coordinates.Z + clockwiseDirection.Z);
        }

        public static int GetDistanceBetween(HexCoordinates a, HexCoordinates b) {
            return GetLength(Subtract(a, b));
        }

        public static int GetLength(HexCoordinates coordinates) {
            return (Math.Abs(coordinates.X) + Math.Abs(coordinates.Y) + Math.Abs(coordinates.Z)) / 2;
        }

        public static List<HexCoordinates> GetCoordinatesInRadius(HexCoordinates center, int radius){
			List<HexCoordinates> results = new List<HexCoordinates>();
			for(int dx = -radius; dx <= radius; dx++){
				for(int dy = Math.Max(-radius, -dx - radius); dy <= Math.Min( radius, -dx + radius); dy++){
					int dz = -dx - dy;
					results.Add(Add(center, new HexCoordinates(dx, dz)));
				}
			}
			return results;
		}

        public static List<HexCoordinates> GetCoordinatesInRing(HexCoordinates center, int radius) {
            List<HexCoordinates> results = new List<HexCoordinates>();

			HexCoordinates hexToAdd = Add(center, Scale(ClockwiseNeighborDirections[4], radius));
			for(int i = 0; i < 6; i++){
				for(int j = 0; j < radius; j++){
					results.Add(hexToAdd);
					hexToAdd = GetNeighborInDirection(hexToAdd, (HexDirection)i);
				}
			}

			return results;
        }

        #region operators

        public static bool operator ==(HexCoordinates a, HexCoordinates b) {
            return a.X == b.X && a.Z == b.Z;
        }

        public static bool operator !=(HexCoordinates a, HexCoordinates b) {
            return a.X != b.X || a.Z != b.Z;
        }

        #endregion

        #endregion

        #region instance methods

        #region from Object

        public override string ToString() {
            return string.Format("({0}, {1}, {2})", X, Y, Z);
        }

        public override bool Equals(object obj){
			if (obj == null){
				return false;
			}
			
            if(!(obj is HexCoordinates)) {
                return false;
            }
			HexCoordinates objAsHexCoords = (HexCoordinates)obj;
			
			return (X == objAsHexCoords.X) && (Z == objAsHexCoords.Z);
		}

        public override int GetHashCode() {
            return X ^ Z;
        }

        #endregion

        #region from IEquatable<HexCoordinates>

        public bool Equals(HexCoordinates coordinates) {
            return X == coordinates.X && Z == coordinates.Z;
        }

        #endregion

        public string ToStringOnSeparateLines() {
            return string.Format("{0}\n{1}\n{2}", X, Y, Z);
        }

        #endregion

    }

}
