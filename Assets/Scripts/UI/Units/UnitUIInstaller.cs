using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.UI.Units {

    public class UnitUIInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private GameObject AbilityDisplayPrefab;
        [SerializeField] private GameObject UnitMapIconPrefab;
        [SerializeField] private GameObject UnitIconSlotPrefab;

        [SerializeField] private UnitMapIconManager UnitMapIconManager;

        [SerializeField] private CombatSummaryDisplay CombatSummaryDisplay;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.BindMemoryPool<IAbilityDisplay, AbilityDisplayMemoryPool>()
                .To<AbilityDisplay>()
                .FromComponentInNewPrefab(AbilityDisplayPrefab);

            Container.BindMemoryPool<UnitMapIcon, UnitMapIcon.Pool>()
                     .WithInitialSize(20)
                     .To<UnitMapIcon>()
                     .FromComponentInNewPrefab(UnitMapIconPrefab)
                     .UnderTransformGroup("Unit Map Icons");

            Container.BindMemoryPool<UnitIconSlot, UnitIconSlot.Pool>()
                     .WithInitialSize(20)
                     .To<UnitIconSlot>()
                     .FromComponentInNewPrefab(UnitIconSlotPrefab)
                     .UnderTransformGroup("Unit Icon Slots");

            Container.Bind<IUnitMapIconManager>().To<UnitMapIconManager>().FromInstance(UnitMapIconManager);

            Container.Bind<CombatSummaryDisplay>().WithId("Combat Summary Display").FromInstance(CombatSummaryDisplay);
        }

        #endregion

        #endregion

    }

}
