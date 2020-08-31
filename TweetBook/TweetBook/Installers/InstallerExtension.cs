using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TweetBook.Installers
{
    public static class InstallerExtension
    {
        public static void InstallServiceAssembly(this IServiceCollection services, IConfiguration configuration)
        {
            //ÍInstaller.IsAssignableFrom(x): x có thừa kế ÍInstaller không?
            //Cast<>: Casts the elements of an IEnumerable to the specified type: cast x tìm được thành IInstaller

            //ExprotedTypes: return collections of public types define inside Startup but visible outside the assembly
            var installers = typeof(Startup).Assembly.ExportedTypes.Where(x =>
                typeof(IInstaller).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract).Select(Activator.CreateInstance).Cast<IInstaller>().ToList();

            //Nếu bỏ Select(Activator.CreateInstance) thì: - Error: Unable to cast object of type 'System.RuntimeType' to type 'TweetBook.IInstaller'.

            installers.ForEach(installer => installer.InstallService(configuration, services));
        }
    }
}
