using System.Collections.Generic;

namespace FuzzyMsc.Dto.FuzzyDTOS
{
    public class RuleTextAndResistivityDTO
    {
        public RuleTextAndResistivityDTO()
        {
            ruleTextList = new List<RuleTextEntityDTO>();
            resistivityList = new List<VariableDTO>();
        }
        public List<RuleTextEntityDTO> ruleTextList { get; set; }
        public List<VariableDTO> resistivityList { get; set; }
    }
}
