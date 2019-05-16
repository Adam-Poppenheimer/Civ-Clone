using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation;

using Assets.Simulation.Cities.Distribution;
using Assets.Simulation.Cities;
using Assets.Simulation.WorkerSlots;

namespace Assets.UI.Cities.Distribution {

    public class CitySlotChoosingDisplay : CityDisplayBase {

        #region instance fields and properties

        private IWorkerDistributionLogic DistributionLogic;
        private IUnemploymentLogic       UnemploymentLogic;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            WorkerSlotSignals slotSignals, IWorkerDistributionLogic distributionLogic,
            IUnemploymentLogic unemploymentLogic
        ){
            slotSignals.SlotClicked.Subscribe(OnSlotClicked);

            DistributionLogic = distributionLogic;
            UnemploymentLogic = unemploymentLogic;
        }

        private void OnSlotClicked(IWorkerSlot slot) {
            if(ObjectToDisplay == null) {
                return;
            }

            if(slot.IsOccupied && !slot.IsLocked) {
                LockOccupiedSlot(slot);
            }else if(slot.IsOccupied && slot.IsLocked) {
                UnassignLockedSlot(slot);
            }else {
                AssignAndLockUnoccupiedSlot(slot);
            }

            ObjectToDisplay.PerformDistribution();            
        }

        private void LockOccupiedSlot(IWorkerSlot slot) {
            slot.IsLocked = true;
        }

        private void UnassignLockedSlot(IWorkerSlot slot) {
            slot.IsOccupied = false;
            slot.IsLocked = false;
        }

        private void AssignAndLockUnoccupiedSlot(IWorkerSlot slot) {
            if(UnemploymentLogic.GetUnemployedPeopleInCity(ObjectToDisplay) <= 0) {
                FreeUpSomeSlot();
            }

            slot.IsOccupied = true;
            slot.IsLocked = true; 
        }

        private void FreeUpSomeSlot() {
            var availableSlots = DistributionLogic.GetSlotsAvailableToCity(ObjectToDisplay)
                .Where(slot => slot.IsOccupied)
                .ToList();

            availableSlots.Sort(delegate(IWorkerSlot slotOne, IWorkerSlot slotTwo){
                if(slotOne.IsLocked == slotTwo.IsLocked) {
                    return 0;
                }else {
                    return slotOne.IsLocked ? 1 : -1;
                }
            });

            var slotToUnassign = availableSlots.FirstOrDefault();
            if(slotToUnassign != null) {
                slotToUnassign.IsOccupied = false;
                slotToUnassign.IsLocked = false;
            }
        }

        #endregion

    }

}
