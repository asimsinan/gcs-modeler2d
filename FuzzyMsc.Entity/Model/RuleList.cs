namespace FuzzyMsc.Entity.Model
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("RuleList")]
    public partial class RuleList
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public RuleList()
        {
            ruleListItems = new HashSet<RuleListItem>();
        }

        public long ruleListID { get; set; }

        public long ruleID { get; set; }

        public byte orderNumber { get; set; }

        public virtual Rule rule { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RuleListItem> ruleListItems { get; set; }
    }
}
