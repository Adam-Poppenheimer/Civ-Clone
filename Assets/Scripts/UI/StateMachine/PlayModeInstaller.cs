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

        [SerializeField] private RectTransform PlayModeContainer   = null;
        [SerializeField] private RectTransform EscapeMenuContainer = null;

        [SerializeField] private List<CityDisplayBase>         AllCityDisplays         = null;
        [SerializeField] private List<CivilizationDisplayBase> AllCivilizationDisplays = null;
        [SerializeField] private List<UnitDisplayBase>         AllUnitDisplays         = null;

        [SerializeField] private List<RectTransform> PlayModeDefaultPanels = null;

        [SerializeField] private SpecialtyResourceSummaryDisplay SpecialtyResourceSummaryDisplay = null;

        [SerializeField] private List<UnitDisplayBase> RangedAttackStateDisplays = null;

        [SerializeField] private GameObject EscapeMenuOptionsDisplay = null;

        [SerializeField] private GameObject SaveGameDisplay = null;

        [SerializeField] private RectTransform ProposalDisplay             = null;
        [SerializeField] private RectTransform DealsReceivedDisplay        = null;
        [SerializeField] private RectTransform DeclareWarDisplay           = null;
        [SerializeField] private RectTransform FreeTechsDisplay            = null;
        [SerializeField] private RectTransform FreeGreatPeopleNotification = null;

        [SerializeField] private FreeGreatPeopleDisplay FreeGreatPeopleDisplay = null;

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

            Container.Bind<List<RectTransform>>  ().WithId("Play Mode Default Panels")      .FromInstance(PlayModeDefaultPanels);
            Container.Bind<List<UnitDisplayBase>>().WithId("Ranged Attack State Displays")  .FromInstance(RangedAttackStateDisplays);
            Container.Bind<GameObject>           ().WithId("Escape Menu Options Display")   .FromInstance(EscapeMenuOptionsDisplay);
            Container.Bind<GameObject>           ().WithId("Save Game Display")             .FromInstance(SaveGameDisplay);
            Container.Bind<RectTransform>        ().WithId("Proposal Display")              .FromInstance(ProposalDisplay);
            Container.Bind<RectTransform>        ().WithId("Deals Received Display")        .FromInstance(DealsReceivedDisplay);
            Container.Bind<RectTransform>        ().WithId("Declare War Display")           .FromInstance(DeclareWarDisplay);
            Container.Bind<RectTransform>        ().WithId("Free Techs Display")            .FromInstance(FreeTechsDisplay);
            Container.Bind<RectTransform>        ().WithId("Free Great People Notification").FromInstance(FreeGreatPeopleNotification);

            Container.Bind<FreeGreatPeopleDisplay>().FromInstance(FreeGreatPeopleDisplay);
        }

        #endregion

        #endregion

    }

}
