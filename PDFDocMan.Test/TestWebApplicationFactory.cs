using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PDFDocMan.Api.Db;

namespace PDFDocMan.Test
{
    public class TestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        private readonly string _dbSuffix;
        public TestWebApplicationFactory(string dbSuffix)
        {
            _dbSuffix = dbSuffix;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var sqlDbCtx = services.Single(d => d.ServiceType == typeof(DbContextOptions<DocDbContext>));

                services.Remove(sqlDbCtx);

                services.AddDbContext<DocDbContext>(options =>
                {
                    options.UseInMemoryDatabase($"InMemoryDocDb-{_dbSuffix}");
                });

                var sp = services.BuildServiceProvider();

                using var scope = sp.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var context = scopedServices.GetRequiredService<DocDbContext>();
                TestSetup.InitTestDb(context);
            });
        }
    }
}
