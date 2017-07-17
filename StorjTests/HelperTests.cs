using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StorjDotNet;

namespace StorjTests
{
    [TestClass]
    public class HelperTests
    {
        [TestMethod]
        public void ConvertsUnixTimeToDateTime()
        {
            long unixTime = 643433015000; // May 23, 1990 3:23:35AM (UTC)
            DateTime convertedDateTime = Helpers.DateTimeFromUnixTime(unixTime);

            Assert.AreEqual(convertedDateTime, new DateTime(1990, 5, 23, 3, 23, 35));
        }
    }
}
