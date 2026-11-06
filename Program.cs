using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

var url = Option("--url", "https://app.cloudqa.io/home/AutomationPracticeForm");
var firstName = Option("--first-name", "Atul");
var gender = Option("--gender", "Female");
var state = Option("--state", "India");
var headless = args.Contains("--headless");

string Option(string key, string fallback)
{
    var index = Array.IndexOf(args, key);
    if (index < 0) return fallback;
    if (index + 1 >= args.Length) throw new ArgumentException($"Missing value for {key}");
    return args[index + 1];
}

try
{
    if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https"))
        throw new ArgumentException("--url must be an absolute HTTP(S) URL");

    var options = new ChromeOptions();
    if (headless) options.AddArgument("--headless=new");
    using var driver = new ChromeDriver(options);
    driver.Navigate().GoToUrl(url);
    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
    wait.Until(d => d.FindElements(By.CssSelector("input, select")).Any());

    // Only the top-level document: CloudQA also embeds near-identical forms in frames/shadow roots.
    var first = FindField(driver, "First Name", e => e.TagName == "input" &&
        (e.GetDomProperty("type") ?? "text").ToLowerInvariant() is "text" or "search");
    first.Clear();
    first.SendKeys(firstName);
    Assert(first.GetDomProperty("value") == firstName, "First Name did not retain the typed value");

    var genderGroup = FindSection(driver, "Gender", section =>
        section.FindElements(By.CssSelector("input[type='radio']")).Count > 0);
    var radios = genderGroup.FindElements(By.CssSelector("input[type='radio']"))
        .Where(e => e.Displayed && e.Enabled).ToList();
    var selectedRadio = Unique(radios.Where(r => Matches(r.GetDomProperty("value"), gender) ||
        Matches(AccessibleName(driver, r), gender)).ToList(), $"Gender choice '{gender}'");
    selectedRadio.Click();
    Assert(selectedRadio.Selected, $"Gender choice '{gender}' is not selected");

    var dropdown = FindField(driver, "State", e => e.TagName == "select");
    var select = new SelectElement(dropdown);
    var choice = Unique(select.Options.Where(o => Matches(o.Text, state) ||
        Matches(o.GetDomProperty("value"), state)).ToList(), $"State option '{state}'");
    select.SelectByIndex(select.Options.ToList().IndexOf(choice));
    Assert(Matches(select.SelectedOption.Text, state) ||
           Matches(select.SelectedOption.GetDomProperty("value"), state), "State selection failed");

    Console.WriteLine($"PASS: First Name={firstName}, Gender={gender}, State={state}");
}
catch (Exception error)
{
    Console.Error.WriteLine($"FAIL: {error.Message}");
    Environment.ExitCode = 1;
}

static bool Matches(string? actual, string expected) =>
    string.Equals(actual?.Trim(), expected.Trim(), StringComparison.OrdinalIgnoreCase);

static void Assert(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

static T Unique<T>(List<T> matches, string description)
{
    if (matches.Count != 1)
        throw new InvalidOperationException($"Expected exactly one {description}, found {matches.Count}. Refusing to guess.");
    return matches[0];
}

static IWebElement FindSection(IWebDriver driver, string label, Func<IWebElement, bool> hasControls)
{
    // Find an exact visible caption, then walk outwards only to the smallest container
    // with the required control(s). This supports sibling labels, label[for] and wrappers.
    var captions = driver.FindElements(By.CssSelector("label, legend, span"))
        .Where(e => e.Displayed && Matches(e.Text, label)).ToList();
    var sections = new List<IWebElement>();
    foreach (var caption in captions)
    {
        var ancestor = caption;
        for (var level = 0; level < 4; level++)
        {
            ancestor = ancestor.FindElement(By.XPath(".."));
            if (hasControls(ancestor)) { sections.Add(ancestor); break; }
            if (ancestor.TagName is "body" or "html") break;
        }
    }
    return Unique(sections.Distinct().ToList(), $"section labeled '{label}'");
}

static IWebElement FindField(IWebDriver driver, string label, Func<IWebElement, bool> valid)
{
    var controls = driver.FindElements(By.CssSelector("input, select, textarea"))
        .Where(e => e.Displayed && e.Enabled && valid(e)).ToList();
    var named = controls.Where(e => Matches(AccessibleName(driver, e), label)).ToList();
    if (named.Count == 1) return named[0];
    if (named.Count > 1) return Unique(named, $"field named '{label}'");

    var section = FindSection(driver, label, s =>
        s.FindElements(By.CssSelector("input, select, textarea")).Any(valid));
    return Unique(section.FindElements(By.CssSelector("input, select, textarea"))
        .Where(e => e.Displayed && e.Enabled && valid(e)).ToList(), $"field under '{label}'");
}

static string AccessibleName(IWebDriver driver, IWebElement element)
{
    // Accessible ARIA labels work without relying on id/name/class; visible captions
    // and label associations are resolved separately by FindSection.
    var labelledBy = element.GetDomAttribute("aria-labelledby");
    if (!string.IsNullOrWhiteSpace(labelledBy))
    {
        var names = labelledBy.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(id => driver.FindElements(By.Id(id)).FirstOrDefault()?.Text ?? "");
        return string.Join(" ", names).Trim();
    }
    return element.GetDomAttribute("aria-label") ?? element.GetDomAttribute("placeholder") ?? "";
}
