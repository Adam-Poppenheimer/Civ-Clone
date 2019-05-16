using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Units;
using Assets.Simulation.Units.Promotions;

namespace Assets.Simulation.MapManagement {

    public class PromotionTreeComposer : IPromotionTreeComposer {

        #region instance fields and properties

        private IEnumerable<IPromotion>             AvailablePromotions;
        private IEnumerable<IPromotionTreeTemplate> AvailableTemplates;

        #endregion

        #region constructors

        [Inject]
        public PromotionTreeComposer(
            [Inject(Id = "Available Promotions")]               IEnumerable<IPromotion> availablePromotions,
            [Inject(Id = "Available Promotion Tree Templates")] IEnumerable<IPromotionTreeTemplate> availableTemplates
        ){
            AvailablePromotions = availablePromotions;
            AvailableTemplates  = availableTemplates;
        }

        #endregion

        #region instance methods

        #region from IPromotionTreeComposer

        public SerializablePromotionTreeData ComposePromotionTree(IPromotionTree promotionTree) {
            var newTreeData = new SerializablePromotionTreeData();

            newTreeData.Template = promotionTree.Template.name;

            newTreeData.ChosenPromotions = new List<string>(
                promotionTree.GetChosenPromotions().Select(promotion => promotion.name)
            );

            return newTreeData;
        }

        public IPromotionTree DecomposePromotionTree(SerializablePromotionTreeData treeData) {
            var treeTemplate     = AvailableTemplates .Where(template  => template.name.Equals(treeData.Template)).First();
            var chosenPromotions = AvailablePromotions.Where(promotion => treeData.ChosenPromotions.Contains(promotion.name));

            return new PromotionTree(treeTemplate, chosenPromotions);
        }

        #endregion

        #endregion

    }

}
