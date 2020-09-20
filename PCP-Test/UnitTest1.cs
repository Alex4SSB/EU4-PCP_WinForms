using Microsoft.VisualStudio.TestTools.UnitTesting;
using PCP_Test.Properties;
using System;
using System.IO;
using System.Linq;
using static EU4_PCP.PCP_Implementations;

namespace EU4_PCP.Tests
{
    [TestClass()]
    public class UnitTest1
    {
        [TestMethod()]
        public void AddTest()
        {
            var arr = new string[1];

            for (int i = 0; i < 10; i++)
            {
                Add(ref arr, $"{i}");
            }

            Assert.IsTrue(arr.Count(cell => !string.IsNullOrEmpty(cell)) == 10);
        }

        [TestMethod()]
        public void RangeTestPositive()
        {
            int x = 5, a = 0, b = 10;

            Assert.IsTrue(x.Range(a, b));
        }

        [TestMethod()]
        public void RangeTestNegative()
        {
            int x = 15, a = 0, b = 10;

            Assert.IsFalse(x.Range(a, b));
        }

        [TestMethod()]
        public void ToIntTest()
        {
            string s = "30";

            int n = s.ToInt();

            Assert.AreEqual(30, n);
        }

        [TestMethod()]
        public void ToByteTestPositive()
        {
            string[] s = { "10", "135", "240" };

            Assert.IsTrue(s.ToByte(out byte[] res, 0));

            Assert.AreEqual(3, res.Length);

            Assert.AreEqual(10, res[0]);
            Assert.AreEqual(135, res[1]);
            Assert.AreEqual(240, res[2]);
        }

        [TestMethod()]
        public void ToByteTestNegative()
        {
            string[] s = { "-1", "500", "a" };

            Assert.IsFalse(s.ToByte(out byte[] res, 0));

            Assert.AreEqual(3, res.Length);

            for (int i = 0; i < 3; i++)
            {
                Assert.AreEqual(0, res[i]);
            }
        }

        [TestMethod()]
        public void GtTestPositive()
        {
            string a = "4", b = "6";

            Assert.IsTrue(b.Gt(a));
        }

        [TestMethod]
        public void GtTestNegative()
        {
            string a = "4", b = "6";

            Assert.IsFalse(a.Gt(b));

            Assert.IsFalse(a.Gt(a));
        }

        [TestMethod]
        public void GeTestPositive()
        {
            string a = "4", b = "6";

            Assert.IsTrue(b.Ge(a));

            Assert.IsTrue(a.Ge(a));
        }

        [TestMethod]
        public void GeTestNegative()
        {
            string a = "4", b = "6";

            Assert.IsFalse(a.Ge(b));
        }

        [TestMethod]
        public void IncTest()
        {
            string n = "30";

            Assert.AreEqual("35", Inc(n, 5));
        }

        [TestMethod()]
        public void DefinProvNameTestPositive()
        {
            string[] defin = {
                "1;128;34;64;Stockholm;x",
                "4825;110;137;45;Badain Jaran",
                "4826;10;167;55;Nayon",
                "4138;219;23;134;x;RNW" };
            string[] names = {
                "Stockholm",
                "Badain Jaran",
                "Nayon",
                "RNW"
            };

            for (int i = 0; i < defin.Length; i++)
            {
                Assert.AreEqual(names[i], DefinProvName(defin[i].Split(';')));
            }
        }

        [TestMethod()]
        public void DefinProvNameTestNegative()
        {
            string[] defin = {
                "4843;30;47;75;;",
                "4844;150;77;85;",
                "4845;70;107;95;",
                "4846;190;137;105;",
                "4847;110;167;115;",
                "4848;;197;125",
                "4849;;;135",
                "4850;;;145",
                "4851;;",
                "4852;",
                "4853" };
            var names = new string[defin.Length];

            for (int i = 0; i < defin.Length; i++)
            {
                names[i] = DefinProvName(defin[i].Split(';'));
            }

            Assert.IsTrue(names.Count(s => string.IsNullOrEmpty(s)) == defin.Length);
        }

        [TestMethod()]
        public void NextLineTestPositive()
        {
            string[] lines =
            {
                "  owner = SWE",
                "owner = SWE#",
                "\towner = SWE",
                "\t \t owner = SWE"
            };
            string[] lines2 =
            {
                "no assignment",
                "}",
                "{"
            };

            for (int i = 0; i < lines.Length; i++)
            {
                Assert.IsFalse(NextLine(lines[i]));

                if (lines2.Length > i)
                    Assert.IsFalse(NextLine(lines2[i], true));
            }
        }

        [TestMethod]
        public void NextLineTestNegative()
        {
            string[] lines =
            {
                "#  owner = SWE",
                "\t",
                "\t\t",
                "  ",
                "  #owner = SWE",
                "\t#owner = SWE",
                "\t \t #owner = SWE",
                "no assignment"
            };

            for (int i = 0; i < lines.Length; i++)
            {
                Assert.IsTrue(NextLine(lines[i]));
            }
        }

        [TestMethod]
        public void DateParserTestPositive()
        {
            string[] dates =
            {
                "1444.11.11",
                "2.12.31",
                "790.4.20",
                "70.11.3"
            };
            DateTime[] original = {
                new DateTime(1444, 11, 11),
                new DateTime(2, 12, 31),
                new DateTime(790, 4, 20),
                new DateTime(70, 11, 3)
            };

            for (int i = 0; i < dates.Length; i++)
            {
                Assert.AreEqual(original[i], DateParser(dates[i], true));
            }

            Assert.AreEqual(original[0], DateParser(dates[0]));
        }

        [TestMethod]
        public void DateParserTestNegative()
        {
            string[] dates =
            {
                "2000.2.31",
                "",
                "4.5",
                "-1",
                "0",
                "not_a_date"
            };

            for (int i = 0; i < dates.Length; i++)
            {
                Assert.AreEqual(DateTime.MinValue, DateParser(dates[i], true));
                Assert.AreEqual(DateTime.MinValue, DateParser(dates[i]));
            }
        }

        [TestMethod]
        public void LastEventTest()
        {
            DateTime[] dates = {
                new DateTime(500, 1, 1),
                new DateTime(1500, 1, 1)
            };

            string[] owners = {
                "BYZ",
                "TUR"
            };

            for (int i = 0; i < dates.Length; i++)
            {
                Assert.AreEqual(owners[i], LastEvent(Resources.Prov_151, EventType.Province, dates[i]));
            }
        }
    }
}
