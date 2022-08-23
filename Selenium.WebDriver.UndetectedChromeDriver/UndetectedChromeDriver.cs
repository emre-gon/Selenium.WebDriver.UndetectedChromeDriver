using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using Selenium.Extensions;
using Sl.Selenium.Extensions;
using Sl.Selenium.Extensions.Chrome;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Selenium.WebDriver.UndetectedChromeDriver
{
    public class UndetectedChromeDriver : Sl.Selenium.Extensions.ChromeDriver
    {
        protected UndetectedChromeDriver(ChromeDriverParameters args)
            : base(args)
        {

        }


        private readonly static string[] ProcessNames = { "chrome", "chromedriver", "undetected_chromedriver" };
        public static new void KillAllChromeProcesses()
        {
            foreach (var name in ProcessNames)
            {
                foreach (var process in Process.GetProcessesByName(name))
                {
                    try
                    {
                        process.Kill();
                    }
                    catch
                    {
                        //ignore errors
                    }
                }
            }

            SlDriver.ClearDrivers(SlDriverBrowserType.Chrome);
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
            return Instance(DriverArguments, new HashSet<string>(), ProfileName, Headless);
        }

        public static new SlDriver Instance(ISet<string> DriverArguments, ISet<string> ExcludedArguments, String ProfileName, bool Headless = false)
        {
            var parameters = new ChromeDriverParameters()
            {
                DriverArguments = DriverArguments,
                ExcludedArguments = ExcludedArguments,
                Headless = Headless,
                ProfileName = ProfileName
            };

            return Instance(parameters);
        }


        public static new SlDriver Instance(ChromeDriverParameters args)
        {
            if(args.DriverArguments == null)
                args.DriverArguments = new HashSet<string>();

            if(args.ExcludedArguments == null)
                args.ExcludedArguments = new HashSet<string>();

            if (args.ProfileName == null)
                args.ProfileName = "sl_selenium_chrome";

            if (!_openDrivers.IsOpen(SlDriverBrowserType.Chrome, args.ProfileName))
            {
                UndetectedChromeDriver cDriver = new UndetectedChromeDriver(args);

                _openDrivers.OpenDriver(cDriver);
            }
            return _openDrivers.GetDriver(SlDriverBrowserType.Chrome, args.ProfileName);
        }

        public override void GoTo(string URL)
        {
            var webDriverResult = this.ExecuteScript("return navigator.webdriver");

            if (webDriverResult != null)
            {
                BaseDriver.ExecuteCdpCommand("Page.addScriptToEvaluateOnNewDocument",
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


            BaseDriver.ExecuteCdpCommand("Network.setUserAgentOverride",
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
                BaseDriver.ExecuteCdpCommand("Page.addScriptToEvaluateOnNewDocument",

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
            options.AddAdditionalChromeOption("useAutomationExtension", false);

            foreach (var excluded in ChromeDriverParameters.ExcludedArguments)
            {
                options.AddExcludedArgument(excluded);
            }

            AddProfileArgumentToBaseDriver(options);

            if (ChromeDriverParameters.Timeout != default)
            {
                return new OpenQA.Selenium.Chrome.ChromeDriver(service, options, ChromeDriverParameters.Timeout);
            }
            else
            {
                return new OpenQA.Selenium.Chrome.ChromeDriver(service, options);
            }
        }

        public static bool ENABLE_PATCHER = true;
        protected override void DownloadLatestDriver()
        {
            base.DownloadLatestDriver();

            #region patcher
            if (ENABLE_PATCHER)
            {
                PatchDriver();  
            }
            #endregion
        }

        private void PatchDriver()
        {
            string newCdc = randomCdc(26);
            using (FileStream stream = new FileStream(this.DriverPath(), FileMode.Open, FileAccess.ReadWrite))
            {
                var buffer = new byte[1];
                var str = new StringBuilder("....");

                var read = 0;
                while (true)
                {
                    read = stream.Read(buffer, 0, buffer.Length);
                    if (read == 0)
                        break;

                    str.Remove(0, 1);
                    str.Append((char)buffer[0]);

                    if (str.ToString() == "cdc_")
                    {
                        stream.Seek(-4, SeekOrigin.Current);
                        var bytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(newCdc);
                        stream.Write(bytes, 0, bytes.Length);
                    }
                }
            }
        }

        private static string randomCdc(int size)
        {
            Random random = new Random((int)DateTime.Now.Ticks);

            const string chars = "abcdefghijklmnopqrstuvwxyz";


            char[] buffer = new char[size];
            for (int i = 0; i < size; i++)
            {
                buffer[i] = chars[random.Next(chars.Length)];
            }

            buffer[2] = buffer[0];
            buffer[3] = '_';
            return new string(buffer);
        }
    }
}
