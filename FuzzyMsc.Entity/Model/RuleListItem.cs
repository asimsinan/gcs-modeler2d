namespace FuzzyMsc.Entity.Model
{
    using System.ComponentModel.DataAnnotations.Schema;
 
    [Table("RuleListItem")]
    public partial class RuleListItem
    {
        public long ruleListItemID { get; set; }

        public long ruleListID { get; set; }

        public long? variableItemID { get; set; }

        public long? resistance { get; set; }

        public long? saturation { get; set; }

        public long? Operator { get; set; }

        public long? equality { get; set; }

        public virtual VariableItem variableItem { get; set; }

        public virtual RuleList ruleList { get; set; }
    }
}
