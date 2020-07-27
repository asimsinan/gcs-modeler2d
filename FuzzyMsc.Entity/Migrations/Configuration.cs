namespace FuzzyMsc.Entity.Migrations
{
    using System.Data.Entity.Migrations;
    

    internal sealed class Configuration : DbMigrationsConfiguration<FuzzyMsc.Entity.Model.GCSModelerContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(FuzzyMsc.Entity.Model.GCSModelerContext context)
        {
            
        }
    }
}
