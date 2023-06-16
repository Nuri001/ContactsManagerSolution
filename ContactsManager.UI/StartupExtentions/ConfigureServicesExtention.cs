using ContactsManager.Core.Domain.IdentityEntities;
using CRUDDemmo.Filters.ActionFilters;
using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Repositories;
using RepositoryContracts;
using ServiceContracts;
using Services;

namespace CRUDDemmo
{
    public static class ConfigureServicesExtention
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddTransient<ResponseHeaderActionFilter>();

            services.AddControllersWithViews(options =>
            {
                //options.Filters.Add<ResponseHeaderActionFilter>(5);
                var logger = services.BuildServiceProvider().GetRequiredService<ILogger<ResponseHeaderActionFilter>>();
                options.Filters.Add(new ResponseHeaderActionFilter(logger)
                {
                    Key = "My-key from global",
                    Value = " my value for global",
                    Order = 2
                });

                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            });

            //add services into IoC container
            services.AddScoped<ICountriesRepository, CountriesRepository>();
            services.AddScoped<IPersonsRepository, PersonsRepository>();

            services.AddScoped<ICountriesGetterService, CountriesGetterService>();
            services.AddScoped<ICountriesAdderService, CountriesAdderService>();
            services.AddScoped<ICountriesUploaderService, CountriesUploaderService>();

            services.AddScoped<IPersonsGetterService, PersonsGetterService>();
            services.AddScoped<IPersonsAdderService, PersonsAdderService>();
            services.AddScoped<IPersonsDeleterService, PersonsDeleterService>();
            services.AddScoped<IPersonsUpdaterService, PersonsUpdaterService>();
            services.AddScoped<IPersonsSorterService, PersonsSorterService>();
            services.AddTransient<PersonsListActionFilter>();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });
           
            //Enable Identity in this oroject
            services.AddIdentity<ApplicationUser, ApplicationRole>((options) => {
                options.Password.RequiredLength = 5;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit=false;
                options.Password.RequiredUniqueChars = 3;

            })

                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>>()
                .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext,Guid>>();

            services.AddAuthorization(options =>
            {
                options.FallbackPolicy=new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

                options.AddPolicy("NotAuthorized", policy =>
                {
                policy.RequireAssertion(context => {
                    return !context.User.Identity.IsAuthenticated;

                });
                });

            });

            services.ConfigureApplicationCookie(options =>{
                options.LogoutPath = "/Account/Login";
            });


            services.AddHttpLogging(options =>
            {
                options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestProperties |Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponsePropertiesAndHeaders;
            });

            return services;

        }

    }
}
