# CloudQA resilient form check

C# / Selenium WebDriver smoke test for the **top-level** First Name, Gender and State fields. It fills the fields and checks their in-browser values; it does **not** submit the form.

## Run

Install the .NET 8 SDK and Chrome. Selenium Manager (included in Selenium 4) obtains a matching ChromeDriver on first run; internet access may be needed.

```powershell
dotnet restore
dotnet run -- --url https://app.cloudqa.io/home/AutomationPracticeForm --first-name Atul --gender Female --state India
dotnet run -- --url https://example.com/modified-form --first-name Maya --gender Male --state Canada --headless
```

Exit code `0` = PASS, `1` = FAIL; errors say what could not be resolved. `--url` defaults to the CloudQA page. Values are configurable and the chosen radio/option must already exist.

## Approach / limitations

Uses exact visible field captions, accessible ARIA labels, control type, and the smallest nearby containing group. Radio buttons are matched by visible accessible name or value. It never assumes a field position or a particular `id`, `name`, `class`, or fixed full-page XPath; ambiguous matches **fail closed**, rather than silently testing the wrong lookalike. The CloudQA page includes lookalike fields in iframes and shadow roots, so only the main document is intentionally searched.

It cannot infer a field if both its caption/accessibility metadata and surrounding group are removed, resolve two genuinely indistinguishable controls, or handle custom non-native dropdowns, closed shadow roots, and iframe-only replacements. If a modified page changes the field semantics entirely, adapt the grouping logic rather than guessing. A successful check proves client-side interaction/selection, not server-side submission.

See `decision.md` for the decisions behind this design. For the screen recording, run once on the live page with visible Chrome, show the PASS output, rerun with a different `--url` or test values, and demonstrate a missing option producing FAIL. Explain the main-document scope and limitations.

## Local mutation demonstration

`fixtures/scrambled-form.html` moves the fields, changes their IDs/names/classes, and adds confusing lookalikes. Start a local server from this directory:

```powershell
python -m http.server 8765
```

In another terminal, run:

```powershell
dotnet run -- --url http://localhost:8765/fixtures/scrambled-form.html --first-name Atul --gender Female --state India
```

The fixture demonstrates a specific class of mutations, not a guarantee against arbitrary page changes. The .NET 8 SDK and Chrome must be installed before running it.
