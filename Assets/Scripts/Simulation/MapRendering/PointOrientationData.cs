﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public class PointOrientationData {

        #region instance fields and properties

        public bool IsOnGrid;

        public IHexCell Center;
        public IHexCell Left;
        public IHexCell Right;
        public IHexCell NextRight;

        public float CenterWeight;
        public float LeftWeight;
        public float RightWeight;
        public float NextRightWeight;

        public float RiverWeight;

        public float ElevationDuck;

        #endregion

        #region instance methods

        #region from Object

        public override bool Equals(object obj) {
            var otherData = obj as PointOrientationData;

            return (otherData != null)
                && IsOnGrid          == otherData.IsOnGrid
                && Center            == otherData.Center
                && Left              == otherData.Left
                && Right             == otherData.Right
                && NextRight         == otherData.NextRight
                && CenterWeight      == otherData.CenterWeight
                && LeftWeight        == otherData.LeftWeight
                && RightWeight       == otherData.RightWeight
                && NextRightWeight   == otherData.NextRightWeight
                && RiverWeight       == otherData.RiverWeight
                && ElevationDuck         == otherData.ElevationDuck;
        }

        public override int GetHashCode() {
            unchecked {
                const int HashingBase      = (int)2166136261;
                const int HashingMultipler = 16777619;

                int hash = HashingBase;

                hash = (hash * HashingMultipler) ^ IsOnGrid.GetHashCode();

                hash = (hash * HashingMultipler) ^ (!object.ReferenceEquals(null, Center)    ? Center   .GetHashCode() : 0);
                hash = (hash * HashingMultipler) ^ (!object.ReferenceEquals(null, Left)      ? Left     .GetHashCode() : 0);
                hash = (hash * HashingMultipler) ^ (!object.ReferenceEquals(null, Right)     ? Right    .GetHashCode() : 0);
                hash = (hash * HashingMultipler) ^ (!object.ReferenceEquals(null, NextRight) ? NextRight.GetHashCode() : 0);

                hash = (hash * HashingMultipler) ^ CenterWeight   .GetHashCode();
                hash = (hash * HashingMultipler) ^ LeftWeight     .GetHashCode();
                hash = (hash * HashingMultipler) ^ RightWeight    .GetHashCode();
                hash = (hash * HashingMultipler) ^ NextRightWeight.GetHashCode();
                hash = (hash * HashingMultipler) ^ RiverWeight    .GetHashCode();
                hash = (hash * HashingMultipler) ^ ElevationDuck     .GetHashCode();

                return hash;
            }
        }

        #endregion

        public void Clear() {
            IsOnGrid = false;
            
            Center    = null;
            Left      = null;
            Right     = null;
            NextRight = null;

            CenterWeight    = 0f;
            LeftWeight      = 0f;
            RightWeight     = 0f;
            NextRightWeight = 0f;
            RiverWeight     = 0f;

            ElevationDuck = 0f;
        }

        #endregion

    }

}
