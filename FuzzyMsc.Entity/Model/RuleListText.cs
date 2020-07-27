namespace FuzzyMsc.Entity.Model
{
    using System.ComponentModel.DataAnnotations.Schema;


    [Table("RuleListText")]
    public partial class RuleListText
    {
        public long ruleListTextID { get; set; }

        public long ruleID { get; set; }

        public string ruleText { get; set; }

        public virtual Rule rule { get; set; }
    }
}
