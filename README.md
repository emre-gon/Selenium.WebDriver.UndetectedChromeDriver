<h3 align="center">Sl.Selenium.Extensions.Drivers</h3>

<div align="center">

[![Status](https://img.shields.io/badge/status-active-success.svg)]()
[![License](https://img.shields.io/github/license/emre-gon/Sl.Selenium.Extensions.Drivers)](/LICENSE)
[![NuGet](https://img.shields.io/nuget/v/Selenium.WebDriver.UndetectedChromeDriver.svg)](https://www.nuget.org/packages/Selenium.WebDriver.UndetectedChromeDriver)


</div>

---

C# Port of https://github.com/ultrafunkamsterdam/undetected-chromedriver

Automatically downloads latest chrome driver.

Usage:


```cs
using (var driver = UndetectedChromeDriver.Instance("profile_name"))
{
    driver.GoTo("https://google.com")
}


```

## Issues

Looking for an equivalent of Python b'string' regex replace for driver binary patching. It was still able to pass some systems though.