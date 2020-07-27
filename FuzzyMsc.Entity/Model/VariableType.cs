namespace FuzzyMsc.Entity.Model
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("VariableType")]
    public partial class VariableType
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public VariableType()
        {
            variables = new HashSet<Variable>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public byte variableTypeID { get; set; }

        [Required]
        [StringLength(50)]
        public string variableTypeName { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Variable> variables { get; set; }
    }
}
