using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

using Assets.UI.Cities;
using Assets.UI.Civilizations;
using Assets.UI.HexMap;
using Assets.UI.Units;
using Assets.UI.Technology;
using Assets.UI.SpecialtyResources;

using Assets.UI.StateMachine.States;

namespace Assets.UI.StateMachine {

    public class PlayModeInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private RectTransform PlayModeContainer;
        [SerializeField] private RectTransform EscapeMenuContainer;

        [SerializeField] private List<CityDisplayBase>         AllCityDisplays;
        [SerializeField] private List<CivilizationDisplayBase> AllCivilizationDisplays;
        [SerializeField] private List<UnitDisplayBase>         AllUnitDisplays;

        [SerializeField] private List<RectTransform> PlayModeDefaultPanels;

        [SerializeField] private SpecialtyResourceSummaryDisplay SpecialtyResourceSummaryDisplay;

        [SerializeField] private List<UnitDisplayBase> RangedAttackStateDisplays;

        [SerializeField] private GameObject EscapeMenuOptionsDisplay;

        [SerializeField] private GameObject SaveGameDisplay;

        [SerializeField] private RectTransform ProposalDisplay;
        [SerializeField] private RectTransform DealsReceivedDisplay;
        [SerializeField] private RectTransform DeclareWarDisplay;
        [SerializeField] private RectTransform FreeTechsDisplay;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<RectTransform>().WithId("Play Mode Container"  ).FromInstance(PlayModeContainer);
            Container.Bind<RectTransform>().WithId("Escape Menu Container").FromInstance(EscapeMenuContainer);

            Container.Bind<List<CityDisplayBase>>        ().FromInstance(AllCityDisplays);
            Container.Bind<List<CivilizationDisplayBase>>().FromInstance(AllCivilizationDisplays);
            Container.Bind<List<UnitDisplayBase>>        ().FromInstance(AllUnitDisplays);

            Container.Bind<SpecialtyResourceSummaryDisplay>().FromInstance(SpecialtyResourceSummaryDisplay);

            Container.Bind<List<RectTransform>>  ().WithId("Play Mode Default Panels")    .FromInstance(PlayModeDefaultPanels);
            Container.Bind<List<UnitDisplayBase>>().WithId("Ranged Attack State Displays").FromInstance(RangedAttackStateDisplays);
            Container.Bind<GameObject>           ().WithId("Escape Menu Options Display") .FromInstance(EscapeMenuOptionsDisplay);
            Container.Bind<GameObject>           ().WithId("Save Game Display")           .FromInstance(SaveGameDisplay);
            Container.Bind<RectTransform>        ().WithId("Proposal Display")            .FromInstance(ProposalDisplay);
            Container.Bind<RectTransform>        ().WithId("Deals Received Display")      .FromInstance(DealsReceivedDisplay);
            Container.Bind<RectTransform>        ().WithId("Declare War Display")         .FromInstance(DeclareWarDisplay);
            Container.Bind<RectTransform>        ().WithId("Free Techs Display")          .FromInstance(FreeTechsDisplay);
        }

        #endregion

        #endregion

    }

}
