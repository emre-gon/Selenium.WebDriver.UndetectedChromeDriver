using OpenQA.Selenium.Remote;
using Selenium.Extensions;
using Sl.Selenium.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Selenium.WebDriver.UndetectedChromeDriver
{
    public class UndetectedChromeDriver : ChromeDriver
    {
        protected UndetectedChromeDriver(ISet<string> DriverArguments, string ProfileName, bool Headless)
            : base(DriverArguments, ProfileName, Headless)
        {

        }

        public static new SlDriver Instance(bool Headless = false)
        {
            return Instance("sl_selenium_chrome", Headless);
        }

        public static new SlDriver Instance(String ProfileName, bool Headless = false)
        {
            return Instance(new HashSet<string>(), ProfileName, Headless);
        }

        public static new SlDriver Instance(ISet<string> DriverArguments, String ProfileName, bool Headless = false)
        {
            if (!_openDrivers.IsOpen(SlDriverBrowserType.Chrome, ProfileName))
            {
                UndetectedChromeDriver cDriver = new UndetectedChromeDriver(DriverArguments, ProfileName, Headless);

                _openDrivers.OpenDriver(cDriver);
            }

            return _openDrivers.GetDriver(SlDriverBrowserType.Chrome, ProfileName);
        }

        public override void GoTo(string URL)
        {
            var webDriverResult = this.ExecuteScript("return navigator.webdriver");

            if (webDriverResult != null)
            {
                BaseDriver.ExecuteChromeCommand("Page.addScriptToEvaluateOnNewDocument",
                    new Dictionary<string, object>()
                    {
                        {"source", @"
                                Object.defineProperty(window, 'navigator', {
                                       value: new Proxy(navigator, {
                                       has: (target, key) => (key === 'webdriver' ? false : key in target),
                                       get: (target, key) =>
                                           key === 'webdriver'
                                           ? undefined
                                           : typeof target[key] === 'function'
                                           ? target[key].bind(target)
                                           : target[key]
                                       })
                                   });

                        " }
                    });

            }

            var userAgentString = (string)this.ExecuteScript("return navigator.userAgent");


            BaseDriver.ExecuteChromeCommand("Network.setUserAgentOverride",
                new Dictionary<string, object>()
                {
                        {"userAgent", userAgentString.Replace("Headless","")}
                }
            );






            var scriptResult = this.ExecuteScript(@"
               let objectToInspect = window,
                        result = [];
                    while(objectToInspect !== null)
                    { result = result.concat(Object.getOwnPropertyNames(objectToInspect));
                      objectToInspect = Object.getPrototypeOf(objectToInspect); }
                    return result.filter(i => i.match(/.+_.+_(Array|Promise|Symbol)/ig))
            ");

            if (scriptResult != null && ((ReadOnlyCollection<object>)scriptResult).Count > 0)
            {
                BaseDriver.ExecuteChromeCommand("Page.addScriptToEvaluateOnNewDocument",

                    new Dictionary<string, object>()
                    {
                        {"source", @" 
                        let objectToInspect = window,
                        result = [];
                            while(objectToInspect !== null) 
                            { result = result.concat(Object.getOwnPropertyNames(objectToInspect));
                              objectToInspect = Object.getPrototypeOf(objectToInspect); }
                            result.forEach(p => p.match(/.+_.+_(Array|Promise|Symbol)/ig)
                                                &&delete window[p]&&console.log('removed',p))
                    " }
                    }

                    );

            }

            base.GoTo(URL);
        }

        public override string DriverName()
        {
            return "undetected_" + base.DriverName();
        }

        protected override OpenQA.Selenium.Chrome.ChromeDriver CreateBaseDriver()
        {
            var service = OpenQA.Selenium.Chrome.ChromeDriverService.CreateDefaultService(DriversFolderPath(), DriverName());


            service.HostName = "127.0.0.1";

            service.SuppressInitialDiagnosticInformation = true;

            DriverArguments.Add("start-maximized");
            DriverArguments.Add("--disable-blink-features");
            //options.AddArgument("--incognito");
            DriverArguments.Add("--disable-blink-features=AutomationControlled");
            DriverArguments.Add("disable-infobars");

            if (this.Headless)
            {
                DriverArguments.Add("headless");
            }
            else
            {
                DriverArguments.Remove("headless");
            }

            DriverArguments.Add("--no-default-browser-check");
            DriverArguments.Add("--no-first-run");


            HashSet<string> argumentKeys = new HashSet<string>(DriverArguments.Select(f => f.Split('=')[0]));


            if (!argumentKeys.Contains("--remote-debugging-host"))
            {
                DriverArguments.Add("--remote-debugging-host=127.0.0.1");
            }


            if (!argumentKeys.Contains("--remote-debugging-port"))
            {
                DriverArguments.Add("--remote-debugging-port=58164");
            }



            if (!argumentKeys.Contains("--log-level"))
            {
                DriverArguments.Add("--log-level=0");
            }

            var options = new OpenQA.Selenium.Chrome.ChromeOptions();
            foreach (var arg in DriverArguments)
            {
                options.AddArgument(arg);
            }



            options.AddExcludedArgument("enable-automation");
            options.AddExcludedArguments(new List<string>() { "enable-automation" });
            options.AddAdditionalCapability("useAutomationExtension", false);


            AddProfileArgumentToBaseDriver(options);

            var driver = new OpenQA.Selenium.Chrome.ChromeDriver(service, options);

            return driver;
        }


        protected override void DownloadLatestDriver()
        {
            base.DownloadLatestDriver();



            //string rawChromeDriverPath = DriversFolderPath() + "/" + base.DriverName();
            //File.Move(this.DriverPath(), rawChromeDriverPath);


            //int cdcSize = 22;
            //string newCdc = randomCdc(cdcSize);



            //using (BinaryReader reader = new BinaryReader(File.OpenRead(rawChromeDriverPath)))
            //{
            //    using(var memStream = new MemoryStream())
            //    {
            //        reader.BaseStream.CopyTo(memStream);

            //        var allBytes = memStream.ToArray();


            //        using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(this.DriverPath())))
            //        {

            //            var asciStr = Encoding.ASCII.GetString(allBytes);

            //            string newContent = Regex.Replace(asciStr, "cdc_.{" + cdcSize + "}", newCdc);


            //            var newBytes = Encoding.ASCII.GetBytes(newContent);

            //            writer.Write(newBytes);
            //        }
            //    }
            //}



            //string newContent = Regex.Replace(content, "cdc_.{" + cdcSize + "}", newCdc);

        }

        private static string randomCdc(int size)
        {
            Random random = new Random();

            const string chars = "abcdefghijklmnopqrstuvwxyz";


            char[] buffer = new char[size];
            for (int i = 0; i < size; i++)
            {
                buffer[i] = chars[random.Next(chars.Length)];
            }
            return new string(buffer);
        }

    }
}
