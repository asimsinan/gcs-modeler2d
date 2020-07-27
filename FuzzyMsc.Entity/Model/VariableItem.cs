namespace FuzzyMsc.Entity.Model
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("VariableItem")]
    public partial class VariableItem
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public VariableItem()
        {
            ruleListItems = new HashSet<RuleListItem>();
        }

        public long variableItemID { get; set; }

        public long variableID { get; set; }

        [Required]
        [StringLength(250)]
        public string variableItemName { get; set; }

        [Required]
        [StringLength(250)]
        public string variableItemVisibleName { get; set; }

        public double minValue { get; set; }

        public double maxValue { get; set; }

        public virtual Variable variable { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RuleListItem> ruleListItems { get; set; }
    }
}
