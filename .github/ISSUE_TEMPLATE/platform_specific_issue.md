---
name: "\U0001F4BB Platform Specific Issue"
about: Report an issue that only occurs on specific platforms or environments
title: "[PLAT] Brief description of the platform-specific issue"
labels: bug
assignees: ''

---

**Thank you for reporting a platform-specific issue! This helps us ensure SoundFlow works well everywhere.**

**1. Affected Platform(s)**

Please specify the platform(s) where this issue occurs. Be as specific as possible.
*   **Operating System & Version:** [e.g., Windows 10 Pro 22H2, macOS Monterey 12.5, Ubuntu 20.04 LTS]
*   **Architecture:** [e.g., x64, ARM64, x86]
*   **.NET Runtime & Version:** [e.g., .NET Core 3.1, .NET 7, Mono 6.12]
*   **Audio Backend Used (if known):** [e.g., MiniAudioEngine with WASAPI, CoreAudio, ALSA]
*   **Specific Hardware (if relevant):** [e.g., M1 Mac, Raspberry Pi 4, specific sound card model]

**2. Description of the Issue**

A clear and concise description of the problem as it manifests on the affected platform(s).
*   How does the behavior differ from other platforms (if you've tested)?

**3. Does it work correctly on other platforms?**
*   [ ] Yes (Please specify which platforms work correctly: ____________)
*   [ ] No (This might be a general bug, consider the Bug Report template)
*   [ ] Not Tested / Don't Know

**4. Steps to Reproduce (on the affected platform)**

Please provide detailed steps to reproduce the behavior *on the affected platform(s)*.
1.
2.
3.

**5. Expected Behavior (on the affected platform)**

What should happen on this platform?

**6. Current Behavior (on the affected platform)**

What actually happens on this platform?

**7. Minimal Reproducible Example (MRE)**

If possible, provide a *minimal* code snippet that demonstrates the issue on the affected platform.
```csharp
// Your MRE code here
```

**8. Error Messages and Stack Trace (if applicable, from the affected platform)**
```
(Paste full error message and stack trace here)
```

**9. SoundFlow Version:** [e.g., 1.0.0, or commit SHA]

**10. Additional Context**

Any other details that might be relevant to this platform-specific issue.
*   Have you tried any platform-specific configurations or workarounds?
*   Are there any known quirks or limitations of the OS, runtime, or hardware involved?

**Requirements:**
*   [ ] I have searched the existing issues to ensure this platform-specific issue has not already been reported.
*   [ ] I have clearly specified the affected platform(s) and environment details.
*   [ ] I have provided detailed steps to reproduce the issue on the affected platform.
*   [ ] I have indicated if the issue is confirmed to work correctly on other platforms.
