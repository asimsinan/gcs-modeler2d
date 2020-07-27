namespace FuzzyMsc.Entity.Model
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Variable")]
    public partial class Variable
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Variable()
        {
            variableItems = new HashSet<VariableItem>();
        }

        public long variableID { get; set; }

        public long ruleID { get; set; }

        public byte variableTypeID { get; set; }

        [StringLength(250)]
        public string variableName { get; set; }

        [StringLength(250)]
        public string visibleVariableName { get; set; }

        public virtual VariableType variableType { get; set; }

        public virtual Rule rule { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VariableItem> variableItems { get; set; }
    }
}
