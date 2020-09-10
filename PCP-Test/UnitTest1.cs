using EU4_PCP;
using static EU4_PCP.PCP_Implementations;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

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
    }
}
