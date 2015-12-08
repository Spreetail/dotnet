using System.Web.Routing;
using System.ServiceModel.Activation;
using System.ServiceModel;
using System;
using System.Collections.ObjectModel;
using System.ServiceModel.Description;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Description;
using System.Web;
using System.Web.Routing;

namespace Sample.Wcf
{
    internal class Routes
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.Add(new ServiceRoute("test/route",
                new WebScriptServiceHostFactory(), typeof(SampleService)));

            //routes.Add(new ServiceRoute("WebsiteUtilities/BulkSaleProfitService",
            //    new WebServiceHostFactory(), typeof(m3.WebsiteUtilities.BulkSaleProfitService)));


        }
    }

    public class M3ProfiledServiceHostFactory : WebServiceHostFactory
    {
        public M3ProfiledServiceHostFactory() : base()
        {

        }

        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            var svc = base.CreateServiceHost(serviceType, baseAddresses);
            return svc;
        }
    }

    public class M3ProfiledServiceHost : ServiceHost
    {
        public M3ProfiledServiceHost() : base() { }
        public M3ProfiledServiceHost(Type serviceType, params Uri[] baseAddresses) : base(serviceType, baseAddresses)
        {
        }
        protected override void InitializeRuntime()
        {
            base.InitializeRuntime();
        }
        public override void AddServiceEndpoint(ServiceEndpoint endpoint)
        {
            base.AddServiceEndpoint(endpoint);
        }
        public override ReadOnlyCollection<ServiceEndpoint> AddDefaultEndpoints()
        {
            //why is this not using the webHttp binding? and instead using basicHttp????
            //more importantly...why is it picking up the webHttp binding by default anyways??? need to enable dotpeek debugging and step into this shiznit. BOOYAH.98ik
            var endpoints = base.AddDefaultEndpoints();
            foreach (var ep in endpoints)
                ep.Behaviors.Add(new StackExchange.Profiling.Wcf.WcfMiniProfilerBehavior());
            return endpoints;
        }
        protected override void ApplyConfiguration()
        {
            base.ApplyConfiguration();
        }
        protected override void OnOpening()
        {
            base.OnOpening();
        }
    }
}