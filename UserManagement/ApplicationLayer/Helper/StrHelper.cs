using MimeKit.Tnef;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Helper
{
    public class StrHelper
    {
        public static string GenerateRandomOTP()
        {
            const string charts = "0123456789";
            var random = new Random();
            return new string([.. Enumerable.Repeat(charts, 6).Select(s => s[random.Next(s.Length)])]);
        }
    }
}
