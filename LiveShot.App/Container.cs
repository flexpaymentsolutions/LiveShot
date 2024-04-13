using System.Reflection;
using LiveShot.UI.Views;
using Microsoft.Extensions.DependencyInjection;

namespace LiveShot.App
{
    public static class Container
    {
        public static IServiceCollection ConfigureUI(this IServiceCollection services)
        {
            IEnumerable<Type>? views = Assembly
                //.GetExecutingAssembly()
                .GetAssembly(typeof(CaptureScreenView))
                .GetTypes()
                //.Where(a => a.Namespace == typeof(App).Namespace + ".Views");
                .Where(a => a.Namespace == "LiveShot.UI.Views");

            foreach (var view in views) 
                services.AddTransient(view);

            return services;
        }
    }
}