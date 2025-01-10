using InvoiceManagementSystem.Data.Repository.Interface;
using InvoiceManagementSystem.Data;
using InvoiceManagementSystem.Service.Service.Interface;
using InvoiceManagementSystem.Service.Service;
using Microsoft.Extensions.DependencyInjection;
using InvoiceManagementSystem.Data.Data;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManagementSystem.Service
{
    public static class ServiceExtension
    {
        public static IServiceCollection RegisterRepositories(this IServiceCollection services)
        {
            services.AddLogging();
            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<IInvoiceRepository, InvoiceRepository>();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            return services;
        }

        public static IServiceCollection AddBusinessDbContext(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<InvoiceDbContext>(options =>
                        options.UseSqlServer(connectionString,
                         b => b.MigrationsAssembly("InvoiceManagementSystem.Data")));
            return services;
        }
    }
}
