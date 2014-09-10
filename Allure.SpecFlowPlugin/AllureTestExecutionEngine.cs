namespace Allure.SpecFlowPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    using NUnit.Framework;

    using TechTalk.SpecFlow;
    using TechTalk.SpecFlow.Bindings;
    using TechTalk.SpecFlow.Bindings.Discovery;
    using TechTalk.SpecFlow.BindingSkeletons;
    using TechTalk.SpecFlow.Configuration;
    using TechTalk.SpecFlow.ErrorHandling;
    using TechTalk.SpecFlow.Infrastructure;
    using TechTalk.SpecFlow.Tracing;
    using TechTalk.SpecFlow.UnitTestProvider;

    using AllureCSharpCommons;
    using AllureCSharpCommons.Events;

    public class AllureTestExecutionEngine : ITestExecutionEngine
    {
        private TestExecutionEngine engine;
        private const string SuiteUidKey = "Allure.SuitUid";
        private static readonly Allure lifecycle = Allure.Lifecycle;

        public AllureTestExecutionEngine(
            IStepFormatter stepFormatter,
            ITestTracer testTracer,
            IErrorProvider errorProvider,
            IStepArgumentTypeConverter stepArgumentTypeConverter,
            RuntimeConfiguration runtimeConfiguration,
            IBindingRegistry bindingRegistry,
            IUnitTestRuntimeProvider unitTestRuntimeProvider,
            IStepDefinitionSkeletonProvider stepDefinitionSkeletonProvider,
            IContextManager contextManager,
            IStepDefinitionMatchService stepDefinitionMatchService,
            IDictionary<string,
            IStepErrorHandler> stepErrorHandlers,
            IBindingInvoker bindingInvoker,
            IRuntimeBindingRegistryBuilder bindingRegistryBuilder)
        {
            this.engine = new TestExecutionEngine(
                stepFormatter,
                testTracer,
                errorProvider,
                stepArgumentTypeConverter,
                runtimeConfiguration,
                bindingRegistry,
                unitTestRuntimeProvider,
                stepDefinitionSkeletonProvider,
                contextManager,
                stepDefinitionMatchService,
                stepErrorHandlers,
                bindingInvoker,
                bindingRegistryBuilder);
        }

        public void Initialize(Assembly[] bindingAssemblies)
        {
            Allure.ResultsPath = "AllureResults/";
            this.engine.Initialize(bindingAssemblies);
        }

        public void OnFeatureStart(FeatureInfo featureInfo)
        {
            this.engine.OnFeatureStart(featureInfo);

            if (string.IsNullOrEmpty(FeatureUid))
            {
                FeatureUid = Guid.NewGuid().ToString();
            }

            var title = FeatureContext.Current.FeatureInfo.Title;

            var evt = new TestSuiteStartedEvent(FeatureUid, title)
                {
                    Title = title
                };

            lifecycle.Fire(evt);
        }

        public void OnFeatureEnd()
        {
            lifecycle.Fire(new TestSuiteFinishedEvent(FeatureUid));

            this.engine.OnFeatureEnd();
        }

        public void OnScenarioStart(ScenarioInfo scenarioInfo)
        {
            this.engine.OnScenarioStart(scenarioInfo);

            var evt = new TestCaseStartedEvent(FeatureUid, ScenarioContext.Current.ScenarioInfo.Title);
            lifecycle.Fire(evt);
        }

        public void OnAfterLastStep()
        {
            this.engine.OnAfterLastStep();
        }

        public void OnScenarioEnd()
        {
//            var testError = ScenarioContext.Current.TestError;
//            if (testError == null)
//            {
//                lifecycle.Fire(new TestCaseFinishedEvent());
//            }
//            else if (testError is AssertionException)
//            {
//                lifecycle.Fire(new TestCaseFailureEvent
//                {
//                    Throwable = new AssertionException(testError.Message),
//                    StackTrace = testError.StackTrace
//                });
//            }
//            else
//            {
//                lifecycle.Fire(new TestCaseFailureEvent
//                {
//                    Throwable = new Exception(testError.Message),
//                    StackTrace = testError.StackTrace
//                });
//            }

            lifecycle.Fire(new TestCaseFinishedEvent());
            this.engine.OnScenarioEnd();
        }

        public void OnTestRunEnd()
        {
            this.engine.OnTestRunEnd();

            var evt = new StepFinishedEvent();
            lifecycle.Fire(evt);
        }

        public void Step(StepDefinitionKeyword stepDefinitionKeyword, string keyword, string text, string multilineTextArg, Table tableArg)
        {
            lifecycle.Fire(new StepStartedEvent(keyword + text));

            this.engine.Step(stepDefinitionKeyword, keyword, text, multilineTextArg, tableArg);

            var testError = ScenarioContext.Current.TestError;
            if (testError == null)
            {
                lifecycle.Fire(new StepFinishedEvent());
            }
            else if (testError is AssertionException)
            {
                lifecycle.Fire(new StepFailureEvent
                {
                    Throwable = new AssertionException(testError.Message)
                });
            }
            else
            {
                lifecycle.Fire(new StepFailureEvent
                {
                    Throwable = new Exception(testError.Message)
                });
            }
        }

        public void Pending()
        {
            this.engine.Pending();

            lifecycle.Fire(new TestCasePendingEvent());
        }

        public FeatureContext FeatureContext
        {
            get
            {
                return this.engine.FeatureContext;
            }
        }

        public ScenarioContext ScenarioContext
        {
            get
            {
                return this.engine.ScenarioContext;
            }
        }

        private string FeatureUid
        {
            get
            {
                if (FeatureContext.Current.ContainsKey(SuiteUidKey))
                {
                    return FeatureContext.Current.Get<string>(SuiteUidKey);
                }

                return null;
            }
            set
            {
                FeatureContext.Current.Set(value, SuiteUidKey);
            }
        }
    }
}