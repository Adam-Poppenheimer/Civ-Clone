using System;

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

        #endregion

    }

}