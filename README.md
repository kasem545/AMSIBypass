# AMSIBypass

A command-line tool that generates obfuscated AMSI (Antimalware Scan Interface) bypass payloads for authorized security testing and research.

```
   ___    __  _________ ____
  / _ |  /  |/  / __(_) _ )__ _____ ___ ____ ___ ___
 / __ | / /|_/ /\ \/ / _  / // / _ \/ _ `(_-<(_-<
/_/ |_|/_/  /_/___/_/____/\_, / .__/\_,_/___/___/
                         /___/_/
```

**⚠️ FOR AUTHORIZED SECURITY TESTING ONLY ⚠️**

Educational and research purposes only. Misuse may violate laws and regulations.

---

## Features

- 🎲 **Randomized Payload Generation** - Each execution produces unique, heavily obfuscated payloads
- 🔄 **Multiple Bypass Techniques** - Implements 6 different AMSI bypass methods with random selection
- 🛡️ **Advanced Obfuscation** - Multi-layer obfuscation pipeline including:
  - Variable name randomization
  - Sensitive string encoding
  - Integer obfuscation
  - Junk code insertion
  - Case randomization
  - Expression wrapping
- 🧪 **Automated Testing** - Test payloads until a working bypass is found
- 💉 **PowerShell Injection** - Automatically inject successful bypasses into new PowerShell sessions
- 📁 **File Output** - Save generated payloads to files

---

## Installation

### Prerequisites

- .NET 6.0 SDK or later ([Download](https://dotnet.microsoft.com/download))

### Build from Source

#### Windows

** Build (with verification):**
```cmd
build.bat
```

#### Linux / macOS

```bash
chmod +x build.sh
./build.sh
```

The executable will be located in:
```
bin/Release/net6.0/<platform>/publish/AMSIBypass.exe
```

---

## Usage

### Basic Usage

**Generate and display a single payload:**
```cmd
AMSIBypass.exe
```

**Save payload to file:**
```cmd
AMSIBypass.exe -f bypass.ps1
AMSIBypass.exe --file output.txt
```

**Test payloads until bypass found:**
```cmd
AMSIBypass.exe -b
AMSIBypass.exe --bypass 10    # Max 10 attempts
```

**Test and inject into PowerShell:**
```cmd
AMSIBypass.exe -b -i
AMSIBypass.exe --bypass 20 --inject
```

**Show help:**
```cmd
AMSIBypass.exe -h
AMSIBypass.exe --help
```

### Command-Line Options

| Option | Alias | Description |
|--------|-------|-------------|
| *(none)* | | Generate a single obfuscated AMSI bypass payload |
| `-f <path>` | `--file` | Save payload to specified file |
| `-b [n]` | `--bypass` | Test payloads until bypass found (max n attempts, default 5) |
| `-i` | `--inject` | Use with `-b` to inject bypassed payload into new PowerShell window |
| `-h` | `--help` | Show help message |

### Examples

```cmd
# Generate one payload and copy manually
AMSIBypass.exe

# Save to file for later use
AMSIBypass.exe -f amsi_bypass.ps1

# Find working bypass (display only)
AMSIBypass.exe -b

# Find working bypass and inject into PowerShell
AMSIBypass.exe -b -i

# Test up to 50 times until bypass found
AMSIBypass.exe -b 50

# Run saved payload
powershell.exe -ExecutionPolicy Bypass -File amsi_bypass.ps1
```

---

## How It Works

### Bypass Techniques

This tool implements multiple AMSI bypass techniques, randomly selecting one per payload generation:

1. **ForceError** - Corrupts `amsiContext` and `amsiSession` to force errors
2. **MattGRefl** - Sets `amsiInitFailed` reflection flag to disable AMSI
3. **MattGReflLog** - Delegate-based reflection to bypass WMF5 logging
4. **MattGRef02** - Overwrites `amsiContext` using `Marshal.WriteInt32`
5. **RastaBuf** - Memory patches `AmsiScanBuffer` function
6. **BlankAmsiProviders** - Nullifies `amsiContext` and `amsiSession` pointers

### Obfuscation Pipeline

Each payload undergoes a comprehensive obfuscation pipeline:

1. **Variable Renaming** - Randomizes all variable names (4-14 characters)
2. **Sensitive String Obfuscation** - Encodes strings like "AmsiUtils", "amsiInitFailed", "AmsiScanBuffer"
3. **Integer Obfuscation** - Replaces integers with arithmetic expressions
4. **Junk Insertion** - Adds benign statements and comments
5. **Case Randomization** - Randomly varies PowerShell keyword casing
6. **Expression Wrapping** - Wraps code in script blocks or `Invoke-Expression`

This multi-layer approach ensures every payload is unique and evades signature-based detection.


---

## Technical Details

- **Language:** C# (.NET 6.0)
- **Build Type:** Self-contained single-file executable
- **Target Platforms:** Windows x64, Linux x64, macOS x64/ARM64
- **Compression:** Enabled (single-file compression)
- **Trimming:** Disabled (preserves all reflection capabilities)

---

## Credits

This tool builds upon groundbreaking research by security researchers who discovered and documented AMSI bypass techniques. Massive credit to:

### Bypass Technique Authors

| Technique | Description | Author(s) |
|-----------|-------------|-----------|
| **ForceError** | amsiContext/amsiSession corruption | [S3cur3Th1sSh1t](https://github.com/S3cur3Th1sSh1t) |
| **MattGRefl** | amsiInitFailed reflection | [Matt Graeber](https://twitter.com/mattifestation) |
| **MattGReflLog** | Delegate-based reflection (WMF5 logging bypass) | [Matt Graeber](https://twitter.com/mattifestation) |
| **MattGRef02** | amsiContext WriteInt32 overwrite | [Matt Graeber](https://twitter.com/mattifestation) |
| **RastaBuf** | AmsiScanBuffer memory patch | [Rasta Mouse](https://twitter.com/_RastaMouse) |
| **FieldOffset** | amsiContext Marshal::Copy | [Matt Graeber](https://twitter.com/mattifestation) |
| **ScanBufferPatchAlt** | AmsiScanBuffer patch (no csc.exe) | [Rasta Mouse](https://twitter.com/_RastaMouse) + [MDSec](https://www.mdsec.co.uk/) |
| **ReflectionFromAssembly** | AppDomain assembly enumeration | [MDSec](https://www.mdsec.co.uk/) ([@am0nsec](https://twitter.com/am0nsec)) |
| **BlankAmsiProviders** | Null amsiContext + amsiSession | Context corruption variant |
| **HardwareBreakpoint** | VEH + debug registers | [@CCob](https://twitter.com/tiraniddo), adapted by [Rasta Mouse](https://twitter.com/_RastaMouse) |

### Tool Creator

- **[Flangvik](https://twitter.com/Flangvik)** - Original creator of [AMSI.fail](https://amsi.fail/)

### Additional References

- [AMSI.fail Website](https://amsi.fail/) - The original web-based AMSI bypass generator
- [Matt Graeber's Research](https://www.mdsec.co.uk/tag/amsi/) - Foundational AMSI bypass research
- [Rasta Mouse's Blog](https://rastamouse.me/) - Red team techniques and AMSI research
- [MDSec Research](https://www.mdsec.co.uk/knowledge-centre/research/) - Advanced security research

---

## Legal Disclaimer

This tool is intended for:
- ✅ Authorized penetration testing
- ✅ Security research in controlled environments
- ✅ Educational purposes
- ✅ Red team exercises with proper authorization

**You are responsible for:**
- Obtaining proper authorization before testing
- Complying with all applicable laws and regulations
- Using this tool ethically and legally

Unauthorized use of this tool may violate computer fraud and abuse laws. The authors and contributors are not responsible for misuse or damage caused by this tool.

---

## Contributing

Contributions are welcome! If you've discovered new AMSI bypass techniques or improvements to the obfuscation pipeline, please open an issue or pull request.

---

## License

This project is provided for educational and research purposes. Use responsibly and ethically.

---

## Acknowledgments

Special thanks to the information security community for continuous research into Windows security mechanisms and for sharing knowledge that helps improve defensive capabilities.

**Stay ethical. Stay legal. Happy hacking!** 🔒
