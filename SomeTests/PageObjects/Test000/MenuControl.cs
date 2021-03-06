﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SpecDrill;
using SpecDrill.SecondaryPorts.AutomationFramework;

namespace SomeTests.PageObjects.Test000
{
    public class MenuControl : WebControl
    {
        public IElement TxtUserName { get; private set; }
        public IElement TxtPassword { get; private set; }
        public INavigationElement<Test000HomePage> BtnLogin { get; private set; }

        public MenuControl(Browser browser, IElement parent, IElementLocator locator)
            : base(browser, parent, locator)
        {
            this.TxtUserName = WebElement.Create(this.browser, this, ElementLocator.Create(By.Id, "userName"));
            this.TxtPassword = WebElement.Create(this.browser, this, ElementLocator.Create(By.Id, "password"));
            this.BtnLogin = WebElement.CreateNavigation<Test000HomePage>(this.browser, this, ElementLocator.Create(By.Id, "login"));
        }
    }
}
