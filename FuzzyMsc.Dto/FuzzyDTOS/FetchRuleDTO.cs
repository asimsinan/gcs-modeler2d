using FuzzyMsc.Entity.Model;
using System.Collections.Generic;


namespace FuzzyMsc.Dto.FuzzyDTOS
{
    public class FetchRuleDTO
    {
        public FetchRuleDTO()
        {
            RuleListText = new List<RuleListText>();
            VariableList = new List<Variable>();
            VariableItemList = new List<VariableItem>();
        }
        public Rule FuzzyRule { get; set; }
        public List<RuleListText> RuleListText { get; set; }
        public List<Variable> VariableList { get; set; }
        public List<VariableItem> VariableItemList { get; set; }
    }
}
