using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace XLink.Utility
{
    public class AppConfiguration
    {
        private static AppConfiguration instance;
        private static readonly object lockObject = new object(); // lock object for thread safety

        public IConfigurationRoot Configuration { get; }

        private AppConfiguration()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .AddUserSecrets<AppConfiguration>();

            Configuration = builder.Build();
        }

        public static AppConfiguration Instance
        {
            get
            {
                lock (lockObject)
                {
                    return instance ??= new AppConfiguration();
                }
            }
        }
    }
}
