# Selenium.WebDriver.UndetectedChromeDriver

[![Status](https://img.shields.io/badge/status-active-success.svg)]()
[![License](https://img.shields.io/github/license/emre-gon/Selenium.WebDriver.UndetectedChromeDriver)](/LICENSE)
[![NuGet](https://img.shields.io/nuget/v/Selenium.WebDriver.UndetectedChromeDriver.svg)](https://www.nuget.org/packages/Selenium.WebDriver.UndetectedChromeDriver)


---

C# Port of https://github.com/ultrafunkamsterdam/undetected-chromedriver

Automatically downloads latest chrome driver.

Usage:


```cs
using (var driver = UndetectedChromeDriver.Instance("profile_name"))
{
    driver.GoTo("https://google.com");
}
```
