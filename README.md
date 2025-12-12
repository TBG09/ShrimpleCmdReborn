# ShrimpleCmd (Reborn)

### A simple cli application, made for convenience and easy access to tools.

---

## Features

- **Internal Commands**: Use built-in commands like `!echo`, `!version`, and `!exit`.
- **Customizable Prefix**: The default command prefix is `!`, but it can be changed in the settings.
- **Easy-to-Use Interface**: A clean and straightforward CLI for all your needs.

---

## Installation & Building

### Prerequisites

[**.NET 9.0 SDK**](https://dotnet.microsoft.com/en-us/download/dotnet/9.0): This is needed, and is the only dependency you need to really install.

### Build instructions

This is the same across most os.

**Step one**: Clone the repository or download it via the page.
(Note: git is required to clone it.)
```bash
git clone https://github.com/TBG09/ShrimpleCmdReborn.git
```

**Step two**: Change your directory into it.
```bash
cd ShrimpleCmdReborn
```

**Step three**: Build the project either in Debug or Release, replace <configuration> with either of these.
```bash
dotnet build --configuration <configuration>
```

It should be built to bin/<configuration>/ as ShrimpleCmd.exe(or as an elf depending on your platform).



## NuGet packages

[**Newtonsoft.Json**](https://www.nuget.org/packages/newtonsoft.json/)   
[**Spectre.Console**](https://www.nuget.org/packages/spectre.console/)
