﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using SpecDrill.Adapters.WebDriver;
using SpecDrill.Adapters.WebDriver.ElementLocatorExtensions;
using SpecDrill.AutomationScopes;
using SpecDrill.Configuration;
using SpecDrill.Infrastructure;
using SpecDrill.Infrastructure.Enums;
using SpecDrill.Infrastructure.Logging;
using SpecDrill.Infrastructure.Logging.Interfaces;
using SpecDrill.SecondaryPorts.AutomationFramework;
using SpecDrill.SecondaryPorts.AutomationFramework.Core;
using SpecDrill.SecondaryPorts.AutomationFramework.Model;

namespace SpecDrill
{
    public class Browser : IBrowser
    {
        private readonly Settings configuration;

        private ILogger Log = Infrastructure.Logging.Log.Get<Browser>();

        private readonly IBrowserDriver browserDriver;

        private static readonly Stack<TimeSpan> timeoutHistory = new Stack<TimeSpan>();

        public Browser(Settings configuration)
        {
            this.configuration = configuration;

            var driverFactory = new SeleniumBrowserFactory(configuration);

            var browserName = configuration.WebDriver.BrowserDriver.ToEnum<BrowserNames>();

            browserDriver = driverFactory.Create(browserName);

            var cfgMaxWait = TimeSpan.FromMilliseconds(configuration.MaxWait == 0 ? 60000 : configuration.MaxWait);

            // set initial browser driver timeout to configuration or 1 minute if not defined
            lock (timeoutHistory)
            {
                timeoutHistory.Push(cfgMaxWait);
                browserDriver.ChangeBrowserDriverTimeout(cfgMaxWait);
            }
        }

        public T Open<T>()
            where T : IPage
        {
            var homePage = configuration.Homepages.FirstOrDefault(homepage => homepage.PageObjectType == typeof(T).Name);
            if (homePage != null)
            {
                
                //Action navigateToUrl = () => this.GoToUrl(string.Format("file:///{0}{1}",
                //    System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location).Replace('\\', '/')
                //    , homePage.Url));
                
                Action navigateToUrl = homePage.IsFileSystemPath ?
                    (Action)(() => this.GoToUrl(string.Format("file:///{0}{1}", System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location).Replace('\\', '/'), homePage.Url)))
                    : () => this.GoToUrl(homePage.Url);

                navigateToUrl();

                var targetPage = this.CreatePage<T>();

                Wait.WithRetry().Doing(navigateToUrl).Until(() => targetPage.IsLoaded);

                return targetPage;
            }

            throw new Exception(string.Format("Page ({0}) cannot be found in Homepages section of settings file.", typeof(T).Name));
        }

        public T CreatePage<T>()
            where T : IPage
        {
            return (T)Activator.CreateInstance(typeof(T), this);
        }

        public void GoToUrl(string url)
        {
            browserDriver.GoToUrl(url);
        }

        public string PageTitle
        {
            get { return browserDriver.Title; }
        }

        public IDisposable ImplicitTimeout(TimeSpan timeout, string message = null)
        {
            return new ImplicitWaitScope(browserDriver, timeoutHistory, timeout, message);
        }

        //public IElement PeekElement(IElementLocator locator)
        //{
        //    using (ImplicitTimeout(TimeSpan.FromSeconds(1)))
        //    {
        //        var webElement = WebElement.Create(this, null, locator);
        //        var nativeElement = webElement.NativeElement;
        //        return nativeElement == null ? null : webElement;
        //    }
        //}

        public IElement PeekElement(IElement element)
        {
            using (ImplicitTimeout(TimeSpan.FromSeconds(1)))
            {
                var webElement = WebElement.Create(this, element.Parent, element.Locator);
                var searchResult = webElement.NativeElementSearchResult;
                var nativeElement = searchResult.NativeElement;
                return nativeElement == null ? null : webElement;
            }
        }

        public void Exit()
        {
            browserDriver.Exit();
        }

        public IElement FindElement(IElementLocator locator)
        {
            return WebElement.Create(this, null, locator);
        }

        public IList<IElement> FindElements(IElementLocator locator)
        {
            var elements = this.browserDriver.FindElements(locator);

            var elementCount = elements?.Count ?? 0;

            var result = new List<IElement>();
            if (elementCount > 0)
            {
                for (int i=0; i<elements.Count; i++)
                {
                    result.Add(WebElement.Create(this, null, locator));
                }
            }

            return result;
        }

        public SearchResult FindNativeElement(IElementLocator locator)
        {
            if (locator.Index.HasValue)
            {
                var elements = browserDriver.FindElements(locator);
                if (locator.Index > elements.Count)
                {
                    throw new Exception($"Browser.FindNativeElement : Not enough elements. You want element number {locator.Index} but only {elements.Count} were found.");
                }
                
                return SearchResult.Create(elements[locator.Index.Value], elements.Count);

            }
            else
            {
                //TODO: make this right by always using FindElements !
                return SearchResult.Create(browserDriver.FindElement(locator), 1);
            }
        }

        public object ExecuteJavascript(string js, params object[] arguments)
        {
            return browserDriver.ExecuteJavaScript(js, arguments);
        }

        public void HoverOver(IElement element)
        {
            browserDriver.MoveToElement(element);
        }

        public void DragAndDropElement(IElement startFromElement, IElement stopToElement)
        {
            browserDriver.DragAndDropElement(startFromElement, stopToElement);
        }

        public void RefreshPage()
        {
            browserDriver.RefreshPage();
        }

        public void MaximizePage()
        {
            browserDriver.Maximize();
        }
    }
}
