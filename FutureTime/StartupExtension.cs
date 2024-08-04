using User;

namespace FutureTime
{
    public static class StartupExtension
    {
        public static IServiceCollection Inject(this IServiceCollection services)
        {
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IUserRepo, UserRepo>();
            
            return services;
        }
    }
}
