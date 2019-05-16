namespace Assets.Simulation.MapGeneration {

    public interface IYieldAndResourcesTemplate {

        #region properties

        float StrategicNodesPerCell  { get; }
        float StrategicCopiesPerCell { get; }

        float MinFoodPerCell       { get; }
        float MinProductionPerCell { get; }

        float MaxScorePerCell { get; }        
        float MinScorePerCell { get; }

        float LandWeight  { get; }
        float WaterWeight { get; }

        #endregion

    }

}