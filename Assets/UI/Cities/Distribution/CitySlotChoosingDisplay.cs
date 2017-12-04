using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation;

using Assets.Simulation.Cities.Distribution;
using Assets.Simulation.Cities.Territory;
using Assets.Simulation.Cities.Buildings;

namespace Assets.UI.Cities.Distribution {

    public class CitySlotChoosingDisplay : CityDisplayBase {

        #region instance fields and properties

        private IWorkerDistributionLogic DistributionLogic;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(SlotDisplayClickedSignal displayClickedSignal, IWorkerDistributionLogic distributionLogic){
            displayClickedSignal.Listen(OnDisplayClicked);
            DistributionLogic = distributionLogic;
        }

        private void OnDisplayClicked(IWorkerSlotDisplay display) {
            if(ObjectToDisplay == null) {
                return;
            }

            var slot = display.SlotToDisplay;

            if(slot.IsOccupied && !slot.IsLocked) {
                LockOccupiedSlot(slot);
            }else if(slot.IsOccupied && slot.IsLocked) {
                UnassignLockedSlot(slot);
            }else {
                AssignAndLockUnoccupiedSlot(slot);
            }

            display.Refresh();
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
            if(DistributionLogic.GetUnemployedPeopleInCity(ObjectToDisplay) <= 0) {
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
