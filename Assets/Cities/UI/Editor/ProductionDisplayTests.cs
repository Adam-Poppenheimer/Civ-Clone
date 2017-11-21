using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Cities.Production.UI;
using Assets.Cities.Production;
using Assets.Cities.Buildings;

namespace Assets.Cities.UI.Editor {

    [TestFixture]
    public class ProductionDisplayTests : ZenjectUnitTestFixture {

        private Mock<IProductionLogic>          ProductionLogicMock;
        private Mock<IProductionProjectDisplay> ProjectDisplayMock;
        private Mock<IProductionProjectChooser> ProjectChooserMock;
        private Mock<ITemplateValidityLogic>    TemplateValidityMock;

        [SetUp]
        public void CommonInstall() {
            ProductionLogicMock  = new Mock<IProductionLogic>();
            ProjectDisplayMock   = new Mock<IProductionProjectDisplay>();
            ProjectChooserMock   = new Mock<IProductionProjectChooser>();
            TemplateValidityMock = new Mock<ITemplateValidityLogic>();

            Container.Bind<IProductionLogic>         ().FromInstance(ProductionLogicMock.Object );
            Container.Bind<IProductionProjectDisplay>().FromInstance(ProjectDisplayMock.Object  );
            Container.Bind<IProductionProjectChooser>().FromInstance(ProjectChooserMock.Object  );
            Container.Bind<ITemplateValidityLogic>   ().FromInstance(TemplateValidityMock.Object);

            Container.Bind<ProductionDisplay>().AsSingle();
        }

        [Test(Description = "When Refresh is called, ProjectDisplay should be given the ActiveProject " +
            "of CityToDisplay")]
        public void RefreshCalled_ProjectDisplayGivenCurrentProject() {
            var productionDisplay = Container.Resolve<ProductionDisplay>();

            var project = new Mock<IProductionProject>().Object;

            var cityMock = new Mock<ICity>();
            cityMock.SetupGet(city => city.ActiveProject).Returns(project);

            productionDisplay.CityToDisplay = cityMock.Object;
            productionDisplay.Refresh();

            ProjectDisplayMock.Verify(display => display.DisplayProject(project, It.IsAny<int>()),
                "ProjectDisplay did not receive the expected call to DisplayProject");
        }

        [Test(Description = "When Refresh is called and CityToDisplay returns a null ActiveProject, " +
            "ProjectDisplay should be cleared")]
        public void RefreshCalled_ProjectDisplayClearedIfNoActiveProject() {
            var productionDisplay = Container.Resolve<ProductionDisplay>();

            var cityMock = new Mock<ICity>();

            productionDisplay.CityToDisplay = cityMock.Object;
            productionDisplay.Refresh();

            ProjectDisplayMock.Verify(display => display.ClearDisplay(),
                "ProjectDisplay did not have its ClearDisplay method called when CityToDisplay had a null ActiveProject");
        }

        [Test(Description = "When Refresh is called, the turnsLeft parameter should be calculated from " +
            "the ActiveProject and information extracted from ProductionLogic")]
        public void RefreshCalled_TurnsLeftCalculatedProperly() {
            var productionDisplay = Container.Resolve<ProductionDisplay>();

            var projectMock = new Mock<IProductionProject>();
            projectMock.SetupGet(project => project.ProductionToComplete).Returns(20);

            var cityMock = new Mock<ICity>();
            cityMock.SetupGet(city => city.ActiveProject).Returns(projectMock.Object);

            ProductionLogicMock
                .Setup(logic => logic.GetProductionProgressPerTurnOnProject(It.IsAny<ICity>(), It.IsAny<IProductionProject>()))
                .Returns(5);

            productionDisplay.CityToDisplay = cityMock.Object;
            productionDisplay.Refresh();

            ProjectDisplayMock.Verify(display => display.DisplayProject(It.IsAny<IProductionProject>(), 4),
                "ProjectDisplay.DisplayProject did not receive the expected turnsLeft argument");
        }

        [Test(Description = "When Refresh is called, ProjectChooser should have its available building templates " +
            "informed by TemplateValidityLogic")]
        public void RefreshCalled_ProjectChooserGivenAppropriateTemplates() {
            var productionDisplay = Container.Resolve<ProductionDisplay>();

            var city = new Mock<ICity>().Object;

            var validTemplates = new List<IBuildingTemplate>() {
                new Mock<IBuildingTemplate>().Object,
                new Mock<IBuildingTemplate>().Object,
                new Mock<IBuildingTemplate>().Object
            };

            TemplateValidityMock.Setup(logic => logic.GetTemplatesValidForCity(city)).Returns(validTemplates);

            ProjectChooserMock.Setup(chooser => chooser.ClearAvailableProjects());

            ProjectChooserMock.Setup(chooser => chooser.SetAvailableBuildingTemplates(It.IsAny<List<IBuildingTemplate>>()))
                .Callback<List<IBuildingTemplate>>(templates => CollectionAssert.AreEquivalent(validTemplates, templates));

            productionDisplay.CityToDisplay = city;
            productionDisplay.Refresh();

            ProjectChooserMock.VerifyAll();
        }

        [Test(Description = "When Refresh is called and CityToDisplay is null, ProductionDisplay should not " + 
            "throw any exceptions")]
        public void RefreshCalled_NoExceptionsOnNullCityToDisplay() {
            var productionDisplay = Container.Resolve<ProductionDisplay>();

            Assert.DoesNotThrow(() => productionDisplay.Refresh(),
                "ProductionDisplay falsely threw on a null CityToDisplay");
        }

        [Test(Description = "When ProjectChooser raises a NewProjectChosen event, ProductionDisplay " +
            "should find the corresponding building template and change CityToDisplay's production " +
            "project with it")]
        public void NewProjectChosenRaised_CurrentProjectOfCityChanged() {
            var productionDisplay = Container.Resolve<ProductionDisplay>();

            var templateOneMock = new Mock<IBuildingTemplate>();
            templateOneMock.SetupGet(template => template.name).Returns("Template One");

            var templateTwoMock = new Mock<IBuildingTemplate>();
            templateTwoMock.SetupGet(template => template.name).Returns("Template Two");

            ProjectChooserMock.SetupGet(chooser => chooser.ChosenProjectName).Returns("Template One");

            TemplateValidityMock.Setup(logic => logic.GetTemplatesValidForCity(It.IsAny<ICity>()))
                .Returns(new List<IBuildingTemplate>() { templateOneMock.Object, templateTwoMock.Object });

            var cityMock = new Mock<ICity>();

            productionDisplay.CityToDisplay = cityMock.Object;

            ProjectChooserMock.Raise(chooser => chooser.NewProjectChosen += null, EventArgs.Empty);

            cityMock.Verify(city => city.SetActiveProductionProject(templateOneMock.Object),
                "CityToDisplay did not have its SetActiveProductionProject method called correctly");
        }        

    }

}
