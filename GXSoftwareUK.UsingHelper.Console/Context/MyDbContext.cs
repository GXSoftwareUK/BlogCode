namespace GXSoftwareUK.UsingHelper.Console.Context
{
    using System.Data.Entity;

    public  class MyDbContext : DbContext
    {
        public MyDbContext(): base("name=DbContext"){}

        public MyDbContext(string connectionString) : base(connectionString) { }

        public virtual DbSet<MyTeam> MyTeams { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

        }
    }
}
