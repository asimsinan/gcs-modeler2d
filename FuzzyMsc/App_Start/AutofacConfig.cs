using Autofac;
using Autofac.Integration.Mvc;
using FuzzyMsc.Bll.Interface;
using FuzzyMsc.Entity.Model;
using FuzzyMsc.Pattern.DataContext;
using FuzzyMsc.Pattern.EF6;
using FuzzyMsc.Pattern.Repositories;
using FuzzyMsc.Pattern.UnitOfWork;
using FuzzyMsc.Service.Interface;
using FuzzyMsc.ServicePattern;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace FuzzyMsc
{
    public class AutofacConfig
    {
        public static void Register()
        {

            #region AutofacConfig

            var builder = new ContainerBuilder();

            // Register your MVC controllers. (MvcApplication is the name of
            // the class in Global.asax.)
            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            // OPTIONAL: Register model binders that require DI.
            builder.RegisterModelBinders(typeof(MvcApplication).Assembly);
            builder.RegisterModelBinderProvider();

            // OPTIONAL: Register web abstractions like HttpContextBase.
            builder.RegisterModule<AutofacWebTypesModule>();

            // OPTIONAL: Enable property injection in view pages.
            builder.RegisterSource(new ViewRegistrationSource());

            // OPTIONAL: Enable property injection into action filters.
            builder.RegisterFilterProvider();

            #region Başvuru Sistemi Sınıfları
            builder.RegisterType<GCSModelerContext>().As<IDataContextAsync>().InstancePerLifetimeScope();
            builder.RegisterType<UnitOfWork>().As<IUnitOfWorkAsync>().InstancePerLifetimeScope();
            var managers = Assembly.Load("FuzzyMsc.Bll").GetTypes()
                .Where(p => p.IsClass && p.GetInterfaces().Any(k => k.GetInterfaces().Contains(typeof(IBaseManager))))
                .Select(p => new
                {
                    Manager = p,
                    Interface = p.GetInterfaces().FirstOrDefault(k => k.GetInterfaces().Contains(typeof(IBaseManager)))
                })
                .ToList();

            foreach (var item in managers)
            {
                builder.RegisterType(item.Manager).As(item.Interface).InstancePerLifetimeScope();
            }

            var services = Assembly.Load("FuzzyMsc.Service").GetTypes()
                .Where(p => p.IsClass && p.GetInterfaces().Any(k => k.GetInterfaces().Contains(typeof(IBaseService))))
                .Select(p => new
                {
                    Service = p,
                    Model = p.BaseType != null && p.BaseType.IsGenericType && (p.BaseType.GetGenericTypeDefinition() == typeof(Service<>)) ?
                    p.BaseType.GetGenericArguments().SingleOrDefault(l => l.BaseType == typeof(Pattern.EF6.Entity)) : null,
                    Interface = p.GetInterfaces().FirstOrDefault(k => k.GetInterfaces().Contains(typeof(IBaseService)))
                })
                .ToList();

            foreach (var item in services)
            {
                if (item.Model != null)
                {
                    var genericrepo = typeof(Repository<>).MakeGenericType(item.Model);
                    var genericrepoInterface = typeof(IRepositoryAsync<>).MakeGenericType(item.Model);
                    builder.RegisterType(genericrepo).As(genericrepoInterface).InstancePerLifetimeScope();

                }
                builder.RegisterType(item.Service).As(item.Interface).InstancePerLifetimeScope();
            }

            #endregion
            

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            #endregion
        }

    }
}