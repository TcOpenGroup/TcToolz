using Autofac;
using BackupNow.DataAccess;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackupNow
{
    public class Bootstrapper
    {
        public static IContainer Container;
        public Bootstrapper()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();
            builder.RegisterType<FileDataService>().As<IDataService>().SingleInstance();
            builder.RegisterType<MainViewModel>().SingleInstance();
            builder.RegisterType<ScanViewModel>().SingleInstance();

            Container = builder.Build();
        }
    }
}
