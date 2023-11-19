using NUnit.Framework;
using Selenium.WebDriver.UndetectedChromeDriver;
using Sl.Selenium.Extensions.Chrome;
using System;
using System.IO;
using System.Threading;

namespace Tests
{
    public class Tests
    {

        const string undetected_chromedriver_path = "ChromeDrivers/undetected_chromedriver.exe";

        private void killAndDeleteDriver()
        {
            UndetectedChromeDriver.KillAllChromeProcesses();

            FileInfo finFo = new FileInfo(undetected_chromedriver_path);
            finFo.Directory.Create();
            foreach (DirectoryInfo di in finFo.Directory.GetDirectories())
            {
                di.Delete(true);
            }

            foreach (FileInfo fi in finFo.Directory.GetFiles())
            {
                fi.Delete();
            }

        }
        [SetUp]
        public void Setup()
        {
            killAndDeleteDriver();
        }

        [TearDown]
        public void TearDown()
        {
            killAndDeleteDriver();
        }

        [Test]
        public void Test_nowsecure_nl()
        {
            UndetectedChromeDriver.ENABLE_PATCHER = true;

            var aprams = new ChromeDriverParameters()
            {
                Timeout = TimeSpan.FromSeconds(10)
            };

            using (var driver = UndetectedChromeDriver.Instance(aprams))
            {
                driver.GoTo("https://nowsecure.nl");

                driver.Wait(300);

                Assert.AreEqual("OH YEAH, you passed!", driver.GetTextOf("h1"));
            }
        }
    }
}