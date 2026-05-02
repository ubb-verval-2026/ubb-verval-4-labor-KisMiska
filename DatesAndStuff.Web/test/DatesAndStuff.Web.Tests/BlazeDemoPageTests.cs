using System.Globalization;
using FluentAssertions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace DatesAndStuff.Web.Tests;

[TestFixture]
public class BlazeDemoPageTests
{
    private IWebDriver driver;
    private const decimal ScreenshotPriceThreshold = 300m;
    private static readonly string ScreenshotOutputFolder = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

    [SetUp]
    public void SetupTest()
    {
        driver = new ChromeDriver();
    }

    [TearDown]
    public void TeardownTest()
    {
        try
        {
            driver.Quit();
            driver.Dispose();
        }
        catch
        {
        }
    }

    [Test]
    public void MexicoCityToDublin_ShouldHaveAtLeastThreeFlights()
    {
        driver.Navigate().GoToUrl("https://blazedemo.com/");

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

        wait.Until(ExpectedConditions.ElementExists(By.Name("fromPort")));

        driver.FindElement(By.XPath("//select[@name='fromPort']/option[normalize-space()='Mexico City']")).Click();
        driver.FindElement(By.XPath("//select[@name='toPort']/option[normalize-space()='Dublin']")).Click();

        driver.FindElement(By.CssSelector("input[type='submit']")).Click();

        wait.Until(ExpectedConditions.ElementExists(By.XPath("//table//tr[td]")));
        var flightRows = driver.FindElements(By.XPath("//table//tr[td]"));

        flightRows.Count.Should().BeGreaterThanOrEqualTo(3);

        var hasCheapFlight = flightRows.Any(row =>
        {
            var columns = row.FindElements(By.TagName("td"));
            if (columns.Count < 6)
                return false;

            var priceText = columns[5].Text.Replace("$", string.Empty).Trim();
            return decimal.TryParse(priceText, NumberStyles.Any, CultureInfo.InvariantCulture, out var price)
                   && price < ScreenshotPriceThreshold;
        });

        if (hasCheapFlight)
        {
            var screenshotPath = Path.Combine(
                ScreenshotOutputFolder,
                $"blazedemo-mexico-dublin.png");

            var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
            screenshot.SaveAsFile(screenshotPath);
        }
    }
}
