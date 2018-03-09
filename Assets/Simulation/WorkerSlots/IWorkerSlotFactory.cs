namespace Assets.Simulation.WorkerSlots {

    public interface IWorkerSlotFactory {

        #region methods

        IWorkerSlot BuildSlot(ResourceSummary baseYield);

        #endregion

    }

}