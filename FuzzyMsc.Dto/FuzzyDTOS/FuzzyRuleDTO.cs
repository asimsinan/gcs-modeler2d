using System;
using System.Collections.Generic;

namespace FuzzyMsc.Dto.FuzzyDTOS
{
    public class FuzzyRuleDTO
    {
        public string Resistivity { get; set; }
        public int? Resistance { get; set; }
        public int? Saturation { get; set; }
        public int? Operator { get; set; }
        public int? Equality { get; set; }
    }

    public class FuzzyRuleListDTO
    {
        public string Result { get; set; }
        public List<FuzzyRuleDTO> Rules { get; set; }
    }

    public class RuleSetDTO {
        public string SetName { get; set; }
        public List<RuleListDTO> RuleList { get; set; }
        public List<VariableDTO> ResistivityList { get; set; }
        public List<VariableDTO> GroundList { get; set; }
    }

    public class RuleListDTO
    {
        public string Text { get; set; }
        public RuleDTO Rule { get; set; }
    }

    public class RuleDTO
    {
        public string Resistivity { get; set; }
        public string Ground { get; set; }
    }

    public class RuleEntityDTO
    {
        public long RuleID { get; set; }
        public string RuleName { get; set; }
        public DateTime? AddDate { get; set; }
        public bool? IsActive { get; set; }
    }

    public class RuleTextEntityDTO
    {
        public long RuleID { get; set; }
        public string RuleText { get; set; }

    }
}
