namespace Allure.SpecFlowPlugin
{
    using BoDi;

    using TechTalk.SpecFlow;
    using TechTalk.SpecFlow.Configuration;
    using TechTalk.SpecFlow.Infrastructure;

    public class AllurePlugin : IRuntimePlugin
    {
        public void RegisterDependencies(ObjectContainer container)
        {
            container.RegisterTypeAs<AllureTestExecutionEngine, ITestExecutionEngine>();
        }

        public void RegisterCustomizations(ObjectContainer container, RuntimeConfiguration runtimeConfiguration)
        {
        }

        public void RegisterConfigurationDefaults(RuntimeConfiguration runtimeConfiguration)
        {
        }
    }
}