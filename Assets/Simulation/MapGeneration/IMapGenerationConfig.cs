using System;
using System.Collections.Generic;

using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public interface IMapGenerationConfig {

        #region properties

        bool UseFixedSeed { get; }
        int  FixedSeed    { get; }

        float JitterProbability { get; }

        int SectionSizeMin { get; }
        int SectionSizeMax { get; }

        int LandPercentage { get; }

        float MountainThreshold  { get; }
        float HillsThreshold     { get; }
        float FlatlandsThreshold { get; }

        float SectionRaisePressureMin { get; }
        float SectionRaisePressureMax { get; }

        int LakeMaxSize              { get; }
        int InlandSeaMaxSize         { get; }
        int ContinentalShelfDistance { get; }



        int HardMapBorderX { get; }
        int HardMapBorderZ { get; }

        int SoftMapBorderX { get; }
        int SoftMapBorderZ { get; }

        int SoftBorderAvoidance { get; }

        int RegionBorder { get; }

        int RegionCount { get; }


        float StartingMoisture { get; }

        float EvaporationCoefficient   { get; }
        float PrecipitationCoefficient { get; }
        float RunoffCoefficient        { get; }
        float SeepageCoefficient       { get; }

        HexDirection WindDirection { get; }
        float        WindStrength  { get; }

        float FlatlandsCloudMaximum { get; }
        float HillsCloudMaximum     { get; }
        float MountainsCloudMaximum { get; }


        int RiverSegmentPercentage { get; }
        int RiverMaxLengthInHexes  { get; }



        float LowTemperature  { get; }
        float HighTemperature { get; }

        HemisphereMode Hemispheres { get; }

        float TemperatureJitter { get; }

        float CoastChancePerAdjacentOcean { get; }
        float CoastChancePerNearbyOcean   { get; }


        IMapGenerationTemplate MapTemplate { get; }

        int MinStrategicCopies { get; }
        int MaxStrategicCopies { get; }

        YieldSummary YieldScoringWeights { get; }

        #endregion

    }

}