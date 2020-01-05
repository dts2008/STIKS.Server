using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace STIKS.Common
{
    public class AppSettings
    {
        private ConfigurationBuilder _builder = new ConfigurationBuilder();

        private IConfigurationRoot _configuration = null;

        private static AppSettings _instance = new AppSettings();

        public static IConfigurationRoot Instance { get => _instance._configuration; }

        private AppSettings()
        {
            _builder.AddJsonFile("appsettings.json", optional: true);
            _configuration = _builder.Build();

            //var connectionString = _configuration.GetConnectionString("MySQLConnection");
        }
    }
}
