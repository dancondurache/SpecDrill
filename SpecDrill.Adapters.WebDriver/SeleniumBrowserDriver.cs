﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using SpecDrill.Adapters.WebDriver.ElementLocatorExtensions;
using SpecDrill.Adapters.WebDriver.SeleniumExtensions;
using SpecDrill.Infrastructure.Logging;
using SpecDrill.SecondaryPorts.AutomationFramework;
using SpecDrill.Infrastructure.Logging.Interfaces;

namespace SpecDrill.Adapters.WebDriver
{
    internal class SeleniumBrowserDriver : IBrowserDriver
    {
        private IWebDriver seleniumDriver = null;

        private ILogger Log = Infrastructure.Logging.Log.Get<SeleniumBrowserDriver>();

        public SeleniumBrowserDriver(IWebDriver seleniumDriver)
        {
            this.seleniumDriver = seleniumDriver;
        }

        public static IBrowserDriver Create(IWebDriver seleniumDriver)
        {
            return new SeleniumBrowserDriver(seleniumDriver);
        }

        public void GoToUrl(string url)
        {
            seleniumDriver.Navigate().GoToUrl(url);
        }

        public void Exit()
        {
            seleniumDriver.Quit();
        }

        public string Title
        {
            get { return seleniumDriver.Title; }
            set { }

        }

        public void ChangeBrowserDriverTimeout(TimeSpan timeout)
        {
            this.seleniumDriver.Manage().Timeouts().ImplicitlyWait(timeout);
        }

        public ReadOnlyCollection<object> FindElements(IElementLocator locator)
        {
            var result = new List<object>();
            var elements = seleniumDriver.FindElements(locator.ToSelenium());
            result.AddRange(elements);
            return new ReadOnlyCollection<object>(result);
        }

        public object FindElement(IElementLocator locator)
        {
            var elements = seleniumDriver.FindElements(locator.ToSelenium());
            if (elements == null || elements.Count == 0)
                return null;
            return elements[0];
        }

        public object ExecuteJavaScript(string js, params object[] arguments)
        {
            var javaScriptExecutor = (seleniumDriver as IJavaScriptExecutor);

            if (javaScriptExecutor == null)
            {
                //TODO: Log reason
                return false;
            }

            try
            {
                return javaScriptExecutor.ExecuteScript(js, arguments);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error when executing JavaScript");
                return false;
            }
        }

        public void MoveToElement(IElement element)
        {
            //ExecuteJavaScript(string.Format(@"{0} sdDispatch(arguments[0],'mouseover');", sdDispatch), (element as SeleniumElement).Element);
            var actions = new Actions(this.seleniumDriver);
            actions.MoveToElement((element as SeleniumElement).Element);
            actions.Perform();
        }

        public void DragAndDropElement(IElement startFromElement, IElement stopToElement)
        {
            //ExecuteJavaScript(string.Format(@"{0} sdDispatch(arguments[0],'mouseover');", sdDispatch), (element as SeleniumElement).Element);
            var actions = new Actions(this.seleniumDriver);
            actions.DragAndDrop((startFromElement as SeleniumElement).Element,
                (stopToElement as SeleniumElement).Element);
            actions.Build().Perform();
        }

        public void RefreshPage()
        {
            seleniumDriver.Navigate().Refresh();
        }

        public void Maximize()
        {
            seleniumDriver.Manage().Window.Maximize();
        }
    }
}
