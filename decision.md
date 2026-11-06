# Decisions — Task 1

1. **Use C# and Selenium with Chrome.** This matches the requested stack. The URL and test values are command-line arguments so the same executable can be run against modified pages.
2. **Find fields by meaning, not DOM position.** I chose exact visible captions, computed accessibility names, control types, and the nearest container holding a suitable control instead of a fixed ID, class, absolute XPath, or field order. The same rules are used on new URLs.
3. **Keep the test scoped to the main document.** CloudQA includes nearly identical fields in iframes and shadow roots. Searching those too could turn a wrong match into a false pass. A reviewer wanting those variants would need to specify which frame/shadow root to test.
4. **Fail when a target is missing or ambiguous.** Picking the first similar-looking field is not resilience; it is an undetected wrong result. The program reports FAIL with a reason instead.
5. **Verify values without submitting.** First Name, Gender and State are each checked after interaction. Submission requires other mandatory fields and is outside the three-field scope.

**Trade-off:** Semantic labels/grouping must remain interpretable, and State must remain a native select. This does not claim to work after arbitrary semantic changes or with custom controls.
