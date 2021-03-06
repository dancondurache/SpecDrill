﻿using System;
using System.Collections.Generic;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.IE;
using SpecDrill.Configuration;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Opera;
using OpenQA.Selenium.Safari;
using SpecDrill.Infrastructure.Enums;
using SpecDrill.SecondaryPorts.AutomationFramework;
//using OpenQA.Selenium.Appium.Android;
//using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Remote;
using SpecDrill.Infrastructure;
//using OpenQA.Selenium.Appium.Enums;

namespace SpecDrill.Adapters.WebDriver
{
    public class SeleniumBrowserFactory : IBrowserDriverFactory
    {
        private readonly Dictionary<BrowserNames, Func<string, IBrowserDriver>> driverFactory;
        //private readonly Dictionary<BrowserNames>
        private readonly Settings configuration = null;
        public SeleniumBrowserFactory(Settings configuration)
        {
            this.configuration = configuration;
            driverFactory = new Dictionary<BrowserNames, Func<string, IBrowserDriver>>
        {
            { BrowserNames.chrome, bdp =>
            //TODO: extract window parameters in specDrillConfig.json
            {
                var chromeOptions = new ChromeOptions();
                chromeOptions.AddArgument("window-size=1920,1080");
                return SeleniumBrowserDriver.Create(new ChromeDriver(bdp, chromeOptions));
            }},
            { BrowserNames.ie, bdp => SeleniumBrowserDriver.Create(new InternetExplorerDriver(bdp)) },
            { BrowserNames.firefox, bdp => SeleniumBrowserDriver.Create(new FirefoxDriver()) },
            { BrowserNames.opera, bdp => SeleniumBrowserDriver.Create(new OperaDriver()) },
            { BrowserNames.safari, bdp => SeleniumBrowserDriver.Create(new SafariDriver()) }
            //,{ BrowserNames.appium, bdp => {
            //    DesiredCapabilities capabilities = new DesiredCapabilities();
            //    capabilities.SetCapability(MobileCapabilityType.PlatformVersion, "5.1");
            //    capabilities.SetCapability(MobileCapabilityType.DeviceName, "Samsung Galaxy S4");
            //    //capabilities.SetCapability(MobileCapabilityType.App, app);
            //    capabilities.SetCapability("unicodeKeyboard", true);
            //    capabilities.SetCapability("autoAcceptAlerts", true);
            //    return SeleniumBrowserDriver.Create(new AndroidDriver<AppiumWebElement>(
            //        new Uri("http://127.0.0.1:4723/wd/hub"), capabilities, TimeSpan.FromSeconds(60))); } }
        };
        }

        public IBrowserDriver Create(BrowserNames browserName)
        {
            if (configuration.WebDriver.IsRemote)
            {
                switch (browserName)
                {
                    case BrowserNames.chrome:
                        return CreateRemoteWebDriver(DesiredCapabilities.Chrome());
                    case BrowserNames.firefox:
                        return CreateRemoteWebDriver(DesiredCapabilities.Firefox());
                    case BrowserNames.opera:
                        return CreateRemoteWebDriver(DesiredCapabilities.Opera());
                    case BrowserNames.safari:
                        return CreateRemoteWebDriver(DesiredCapabilities.Safari());
                    case BrowserNames.ie:
                        return CreateRemoteWebDriver(DesiredCapabilities.InternetExplorer());
                    default:
                        throw new Exception($"Value Not Supported `{browserName}`!");
                }
            }

            return driverFactory[browserName](configuration.WebDriver.BrowserDriversPath);
        }

        public IBrowserDriver CreateRemoteWebDriver(DesiredCapabilities desiredCapabilities)
        {
            return SeleniumBrowserDriver.Create(
                            new RemoteWebDriver(
                                new Uri(configuration.WebDriver.SeleniumServerUri), desiredCapabilities));
        }
    }
}
