namespace FuzzyMsc.Entity.Model
{
    using FuzzyMsc.Pattern.EF6;
    using System.Data.Entity;

    public partial class GCSModelerContext : DataContext
    {
        public GCSModelerContext()
            : base("name=GCSModelerContext")
        {
        }

        public virtual DbSet<Variable> variable { get; set; }
        public virtual DbSet<VariableItem> variableItem { get; set; }
        public virtual DbSet<VariableType> variableType { get; set; }
        public virtual DbSet<User> user { get; set; }
        public virtual DbSet<Rule> rule { get; set; }
        public virtual DbSet<RuleList> ruleList { get; set; }
        public virtual DbSet<RuleListItem> ruleListItem { get; set; }
        public virtual DbSet<RuleListText> ruleListText { get; set; }
        public virtual DbSet<Role> role { get; set; }
        public virtual DbSet<UserRole> userRole { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<GCSModelerContext>(null);
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Variable>()
                .HasMany(e => e.variableItems)
                .WithRequired(e => e.variable)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<VariableType>()
                .HasMany(e => e.variables)
                .WithRequired(e => e.variableType)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.userRoles)
                .WithRequired(e => e.user)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Rule>()
                .HasMany(e => e.variables)
                .WithRequired(e => e.rule)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Rule>()
                .HasMany(e => e.ruleLists)
                .WithRequired(e => e.rule)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Rule>()
                .HasMany(e => e.ruleListTexts)
                .WithRequired(e => e.rule)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<RuleList>()
                .HasMany(e => e.ruleListItems)
                .WithRequired(e => e.ruleList)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Role>()
                .Property(e => e.roleName)
                .IsFixedLength();

            modelBuilder.Entity<Role>()
                .HasMany(e => e.userRoles)
                .WithRequired(e => e.role)
                .WillCascadeOnDelete(false);
        }
    }
}
