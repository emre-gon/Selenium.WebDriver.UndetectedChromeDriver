using NUnit.Framework;
using Selenium.WebDriver.UndetectedChromeDriver;
using System.IO;
using System.Threading;

namespace Tests
{
    public class Tests
    {

        const string undetected_chromedriver_path = "ChromeDrivers/undetected_chromedriver.exe";
        [SetUp]
        public void Setup()
        {
            UndetectedChromeDriver.KillAllChromeProcesses();

            if (File.Exists(undetected_chromedriver_path))
                File.Delete("ChromeDrivers/undetected_chromedriver.exe");
        }

        [TearDown]
        public void TearDown()
        {
            UndetectedChromeDriver.KillAllChromeProcesses();

            if (File.Exists(undetected_chromedriver_path))
                File.Delete("ChromeDrivers/undetected_chromedriver.exe");
        }

        [Test]
        public void Test_nowsecure_nl()
        {
            UndetectedChromeDriver.ENABLE_PATCHER = true;
            using (var driver = UndetectedChromeDriver.Instance())
            {
                driver.GoTo("https://nowsecure.nl");

                driver.RandomWait(5, 7);

                Assert.AreEqual(driver.GetTextOf("h1"), "OH YEAH, you passed!");
            }
        }
    }
}