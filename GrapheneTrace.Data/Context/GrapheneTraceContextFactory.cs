using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GrapheneTrace.Data.Context
{
    public class GrapheneTraceContextFactory : IDesignTimeDbContextFactory<GrapheneTraceContext>
    {
        public GrapheneTraceContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<GrapheneTraceContext>();

            // Connection string for LocalDB
            optionsBuilder.UseSqlServer(
                @"Server=(localdb)\mssqllocaldb;Database=GrapheneTraceDB;Trusted_Connection=True;MultipleActiveResultSets=true"
            );

            return new GrapheneTraceContext(optionsBuilder.Options);
        }
    }
}
