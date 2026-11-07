# Decisions — Task 1

1. **Use C# and Selenium with Chrome.** This matches the requested stack. The URL and test values are command-line arguments so the same executable can be run against modified pages.
2. **Find fields by meaning, not DOM position.** I chose exact visible captions, computed accessibility names, control types, and the nearest container holding a suitable control instead of a fixed ID, class, absolute XPath, or field order. The same rules are used on new URLs.
3. **Keep the test scoped to the active root context (top-level form by default).** CloudQA duplicates the same form across multiple nested iFrames and Shadow Roots. An unscoped global traversal risks mutating the wrong form instance and producing a false positive. The locator engine is intentionally scoped to the top-level form by default; embedded roots should be targeted deterministically via an explicit scope/section parameter rather than a blind global search.
4. **Fail when a target is missing or ambiguous.** Picking the first similar-looking field is not resilience; it is an undetected wrong result. The program reports FAIL with a reason instead.
5. **Verify values without submitting.** First Name, Gender and State are each checked after interaction. Submission requires other mandatory fields and is outside the three-field scope.

**Trade-off:** Semantic labels/grouping must remain interpretable, and State must remain a native select. This does not claim to work after arbitrary semantic changes or with custom controls.

**Recording explanation:** “Because AutomationPracticeForm duplicates the same form across multiple nested iFrames and Shadow Roots, an unscoped global traversal risks mutating the wrong form instance and producing a false positive. I intentionally scoped the locator engine to the active root context (top-level form by default), and designed it so embedded roots can be targeted deterministically via an explicit scope/section parameter rather than blind global matching.”
