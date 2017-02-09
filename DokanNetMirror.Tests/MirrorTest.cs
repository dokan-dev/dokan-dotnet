using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DokanNetMirror.Tests
{
    [TestClass]
    public class MirrorTest
    {
        struct MatchTest
        {
            public String Expression;
            public String Name;
            public Boolean IgnoreCase;
            /* Flag for tests which shouldn't be tested in checked builds */
            public Boolean AssertsInChecked;
            /* Expected test result */
            public Boolean Expected;
            public MatchTest(String expression, String name, Boolean ignoreCase, Boolean assertsInChecked, Boolean expected)
            {
                Expression = expression;
                Name = name;
                IgnoreCase = ignoreCase;
                AssertsInChecked = assertsInChecked;
                Expected = expected;
            }
        }

        [TestMethod]
        public void DokanIsNameInExpression_should_match_as_expected()
        {
            var tests = new MatchTest[] {
                new MatchTest( "",                      "",                            false,  true,   true  ),
                new MatchTest( "",                      "a",                           false,  true,   false ),
                new MatchTest( "*",                     "a",                           false,  true,   true  ),
                new MatchTest( "*",                     "",                            false,  true,   false ),
                new MatchTest( "**",                    "",                            false,  true,   false ),
                new MatchTest( "**",                    "a",                           false,  false,  true  ),
                new MatchTest( "ntdll.dll",             ".",                           false,  true,   false ),
                new MatchTest( "ntdll.dll",             "~1",                          false,  true,   false ),
                new MatchTest( "ntdll.dll",             "..",                          false,  true,   false ),
                new MatchTest( "ntdll.dll",             "ntdll.dll",                   false,  true,   true  ),
                new MatchTest( "smss.exe",              ".",                           false,  true,   false ),
                new MatchTest( "smss.exe",              "~1",                          false,  true,   false ),
                new MatchTest( "smss.exe",              "..",                          false,  true,   false ),
                new MatchTest( "smss.exe",              "ntdll.dll",                   false,  true,   false ),
                new MatchTest( "smss.exe",              "NTDLL.DLL",                   false,  true,   false ),
                new MatchTest( "nt??krnl.???",          "ntoskrnl.exe",                false,  false,  true  ),
                new MatchTest( "he*o",                  "hello",                       false,  false,  true  ),
                new MatchTest( "he*o",                  "helo",                        false,  false,  true  ),
                new MatchTest( "he*o",                  "hella",                       false,  false,  false ),
                new MatchTest( "he*",                   "hello",                       false,  false,  true  ),
                new MatchTest( "he*",                   "helo",                        false,  false,  true  ),
                new MatchTest( "he*",                   "hella",                       false,  false,  true  ),
                new MatchTest( "*.cpl",                 "kdcom.dll",                   false,  false,  false ),
                new MatchTest( "*.cpl",                 "bootvid.dll",                 false,  false,  false ),
                new MatchTest( "*.cpl",                 "ntoskrnl.exe",                false,  false,  false ),
                new MatchTest( ".",                     "NTDLL.DLL",                   false,  false,  false ),
                new MatchTest( "F0_*.*",                ".",                           false,  false,  false ),
                new MatchTest( "F0_*.*",                "..",                          false,  false,  false ),
                new MatchTest( "F0_*.*",                "SETUP.EXE",                   false,  false,  false ),
                new MatchTest( "F0_*.*",                "f0_",                         false,  false,  false ),
                new MatchTest( "F0_*.*",                "f0_",                         true,   false,  false ),
                new MatchTest( "F0_*.*",                "F0_",                         false,  false,  false ),
                new MatchTest( "F0_*.*",                "f0_.",                        false,  false,  false ),
                new MatchTest( "F0_*.*",                "f0_.",                        true,   false,  true  ),
                new MatchTest( "F0_*.*",                "F0_.",                        false,  false,  true  ),
                new MatchTest( "F0_*.*",                "F0_001",                      false,  false,  false ),
                new MatchTest( "F0_*.*",                "F0_001",                      true,   false,  false ),
                new MatchTest( "F0_*.*",                "f0_001",                      false,  false,  false ),
                new MatchTest( "F0_*.*",                "f0_001",                      true,   false,  false ),
                new MatchTest( "F0_*.*",                "F0_001.",                     false,  false,  true  ),
                new MatchTest( "F0_*.*",                "f0_001.txt",                  false,  false,  false ),
                new MatchTest( "F0_*.*",                "f0_001.txt",                  true,   false,  true  ),
                new MatchTest( "F0_*.*",                "F0_001.txt",                  false,  false,  true  ),
                new MatchTest( "F0_*.*",                "F0_001.txt",                  true,   false,  true  ),
                new MatchTest( "F0_*.",                 ".",                           false,  false,  false ),
                new MatchTest( "F0_*.",                 "..",                          false,  false,  false ),
                new MatchTest( "F0_*.",                 "SETUP.EXE",                   false,  false,  false ),
                new MatchTest( "F0_*.",                 "f0_",                         false,  false,  false ),
                new MatchTest( "F0_*.",                 "f0_",                         true,   false,  false ),
                new MatchTest( "F0_*.",                 "F0_",                         false,  false,  false ),
                new MatchTest( "F0_*.",                 "f0_.",                        false,  false,  false ),
                new MatchTest( "F0_*.",                 "f0_.",                        true,   false,  true  ),
                new MatchTest( "F0_*.",                 "F0_.",                        false,  false,  true  ),
                new MatchTest( "F0_*.",                 "F0_001",                      false,  false,  false ),
                new MatchTest( "F0_*.",                 "F0_001",                      true,   false,  false ),
                new MatchTest( "F0_*.",                 "f0_001",                      false,  false,  false ),
                new MatchTest( "F0_*.",                 "f0_001",                      true,   false,  false ),
                new MatchTest( "F0_*.",                 "F0_001.",                     false,  false,  true  ),
                new MatchTest( "F0_*.",                 "f0_001.txt",                  false,  false,  false ),
                new MatchTest( "F0_*.",                 "f0_001.txt",                  true,   false,  false ),
                new MatchTest( "F0_*.",                 "F0_001.txt",                  false,  false,  false ),
                new MatchTest( "F0_*.",                 "F0_001.txt",                  true,   false,  false ),
                new MatchTest( "F0_<\"*",               ".",                           false,  false,  false ),
                new MatchTest( "F0_<\"*",               "..",                          false,  false,  false ),
                new MatchTest( "F0_<\"*",               "SETUP.EXE",                   false,  false,  false ),
                new MatchTest( "F0_<\"*",               "f0_",                         true,   false,  true  ),
                new MatchTest( "F0_<\"*",               "F0_",                         false,  false,  true  ),
                new MatchTest( "F0_<\"*",               "f0_.",                        false,  false,  false ),
                new MatchTest( "F0_<\"*",               "f0_.",                        true,   false,  true  ),
                new MatchTest( "F0_<\"*",               "F0_.",                        false,  false,  true  ),
                new MatchTest( "F0_<\"*",               "F0_001",                      false,  false,  true  ),
                new MatchTest( "F0_<\"*",               "F0_001",                      true,   false,  true  ),
                new MatchTest( "F0_<\"*",               "f0_001",                      false,  false,  false ),
                new MatchTest( "F0_<\"*",               "f0_001",                      true,   false,  true  ),
                new MatchTest( "F0_<\"*",               "F0_001.",                     false,  false,  true  ),
                new MatchTest( "F0_<\"*",               "f0_001.txt",                  false,  false,  false ),
                new MatchTest( "F0_<\"*",               "f0_001.txt",                  true,   false,  true  ),
                new MatchTest( "F0_<\"*",               "F0_001.txt",                  false,  false,  true  ),
                new MatchTest( "F0_<\"*",               "F0_001.txt",                  true,   false,  true  ),
                new MatchTest( "*.TTF",                 ".",                           false,  false,  false ),
                new MatchTest( "*.TTF",                 "..",                          false,  false,  false ),
                new MatchTest( "*.TTF",                 "SETUP.INI",                   false,  false,  false ),
                new MatchTest( "*",                     ".",                           false,  false,  true  ),
                new MatchTest( "*",                     "..",                          false,  false,  true  ),
                new MatchTest( "*",                     "SETUP.INI",                   false,  false,  true  ),
                new MatchTest( ".*",                    "1",                           false,  false,  false ),
                new MatchTest( ".*",                    "01",                          false,  false,  false ),
                new MatchTest( ".*",                    " ",                           false,  false,  false ),
                new MatchTest( ".*",                    "",                            false,  true,   false ),
                new MatchTest( ".*",                    ".",                           false,  false,  true  ),
                new MatchTest( ".*",                    "1.txt",                       false,  false,  false ),
                new MatchTest( ".*",                    " .txt",                       false,  false,  false ),
                new MatchTest( ".*",                    ".txt",                        false,  false,  true  ),
                new MatchTest( "\"ntoskrnl.exe",        "ntoskrnl.exe",                false,  false,  false ),
                new MatchTest( "ntoskrnl\"exe",         "ntoskrnl.exe",                false,  false,  true  ),
                new MatchTest( "ntoskrn\".exe",         "ntoskrnl.exe",                false,  false,  false ),
                new MatchTest( "ntoskrn\"\"exe",        "ntoskrnl.exe",                false,  false,  false ),
                new MatchTest( "ntoskrnl.\"exe",        "ntoskrnl.exe",                false,  false,  false ),
                new MatchTest( "ntoskrnl.exe\"",        "ntoskrnl.exe",                false,  false,  true  ),
                new MatchTest( "ntoskrnl.exe",          "ntoskrnl.exe",                false,  false,  true  ),
                new MatchTest( "*.c.d",                 "a.b.c.d",                     false,  false,  true  ),
                new MatchTest( "*.?.c.d",               "a.b.c.d",                     false,  false,  true  ),
                new MatchTest( "**.?.c.d",              "a.b.c.d",                     false,  false,  true  ),
                new MatchTest( "a.**.c.d",              "a.b.c.d",                     false,  false,  true  ),
                new MatchTest( "a.b.??.d",              "a.b.c1.d",                    false,  false,  true  ),
                new MatchTest( "a.b.??.d",              "a.b.c.d",                     false,  false,  false ),
                new MatchTest( "a.b.*?.d",              "a.b.c.d",                     false,  false,  true  ),
                new MatchTest( "a.b.*??.d",             "a.b.ccc.d",                   false,  false,  true  ),
                new MatchTest( "a.b.*??.d",             "a.b.cc.d",                    false,  false,  true  ),
                new MatchTest( "a.b.*??.d",             "a.b.c.d",                     false,  false,  false ),
                new MatchTest( "a.b.*?*.d",             "a.b.c.d",                     false,  false,  true  ),
                new MatchTest( "*?",                    "",                            false,  true,   false ),
                new MatchTest( "*?",                    "a",                           false,  false,  true  ),
                new MatchTest( "*?",                    "aa",                          false,  false,  true  ),
                new MatchTest( "*?",                    "aaa",                         false,  false,  true  ),
                new MatchTest( "?*?",                   "",                            false,  true,   false ),
                new MatchTest( "?*?",                   "a",                           false,  false,  false ),
                new MatchTest( "?*?",                   "aa",                          false,  false,  true  ),
                new MatchTest( "?*?",                   "aaa",                         false,  false,  true  ),
                new MatchTest( "?*?",                   "aaaa",                        false,  false,  true  ),
                new MatchTest( "C:\\ReactOS\\**",       "C:\\ReactOS\\dings.bmp",      false,  false,  true  ),
                new MatchTest( "C:\\ReactOS\\***",      "C:\\ReactOS\\dings.bmp",      false,  false,  true  ),
                new MatchTest( "C:\\Windows\\*a*",      "C:\\ReactOS\\dings.bmp",      false,  false,  false ),
                new MatchTest( "C:\\ReactOS\\*.bmp",    "C:\\Windows\\explorer.exe",   false,  false,  false ),
                new MatchTest( "*.bmp;*.dib",           "winhlp32.exe",                false,  false,  false ),
                new MatchTest( "*.*.*.*",               "127.0.0.1",                   false,  false,  true  ),
                new MatchTest( "*?*?*?*",               "1.0.0.1",                     false,  false,  true  ),
                new MatchTest( "?*?*?*?",               "1.0.0.1",                     false,  false,  true  ),
                new MatchTest( "?.?.?.?",               "1.0.0.1",                     false,  false,  true  ),
                new MatchTest( "*a*ab*abc",             "aabaabcdadabdabc",            false,  false,  true  ),
                new MatchTest( "ab<exe",                "abcd.exe",                    false,  false,  true  ),
                new MatchTest( "ab<exe",                "ab.exe",                      false,  false,  true  ),
                new MatchTest( "ab<exe",                "abcdexe",                     false,  false,  true  ),
                new MatchTest( "ab<exe",                "acd.exe",                     false,  false,  false ),
                new MatchTest( "a.b<exe",               "a.bcd.exe",                   false,  false,  true  ),
                new MatchTest( "a<b.exe",               "a.bcd.exe",                   false,  false,  false ),
                new MatchTest( "a.b.exe",               "a.bcd.exe",                   false,  false,  false ),
                new MatchTest( "abc.exe",               "abc.exe",                     false,  false,  true  ),
                new MatchTest( "abc.exe",               "abc.exe.",                    false,  false,  false ),
                new MatchTest( "abc.exe",               "abc.exe.back",                false,  false,  false ),
                new MatchTest( "abc.exe",               "abc.exes",                    false,  false,  false ),
                new MatchTest( "a>c.exe",               "abc.exe",                     false,  false,  true  ),
                new MatchTest( "a>c.exe",               "ac.exe",                      false,  false,  false ),
                new MatchTest( "a>>>exe",               "abc.exe",                     false,  false,  false ),
                new MatchTest( "a>>>exe",               "ac.exe",                      false,  false,  false ),
                new MatchTest( "<.exe",                 "test.exe",                    false,  false,  true  ),
                new MatchTest( "<.EXE",                 "test.exe",                    true,   false,  true  ),
            };

            foreach (var test in tests)
            {
                var testResult = Mirror.DokanIsNameInExpression(test.Expression, test.Name, test.IgnoreCase);
                Assert.AreEqual(test.Expected, testResult, $"{nameof(Mirror.DokanIsNameInExpression)}({test.Expression},{test.Name},{test.IgnoreCase}): Expected {test.Expected}, got {testResult}");
            }
        }
    }
}
