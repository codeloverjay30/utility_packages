using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ILoggerBuilderFactoryServices.Test
{
    [TestFixture]
    public class FactoryIntegrationTests
    {
        public class SimpleConfig : ILogConfiguration
        {
            public string? LogPrefix { get; set; } = "Default";
        }

        [Test]
        public void AddConfiguration_ShouldRegisterProviderAsSingleton()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging(builder =>
            {
                var factory = new ILoggerBuilderFactory<SimpleConfig> { Builder = builder };
                factory.AddConfiguration<SimpleConfig>(opt => opt.LogPrefix = "NewPrefix");
            });

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var loggerProvider = serviceProvider.GetService<ILoggerProvider>();
            var options = serviceProvider.GetService<IOptions<SimpleConfig>>();

            // Assert
            Assert.That(loggerProvider , Is.TypeOf<MyUniversalLoggerProvider<SimpleConfig>>());
            Assert.That(options.Value.LogPrefix , Is.EqualTo("NewPrefix"));
        }
    }
}
