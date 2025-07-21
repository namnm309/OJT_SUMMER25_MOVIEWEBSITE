using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Constants
{
    public static class JwtConst
    {
        public const int ACCESS_TOKEN_EXP = 60 * 60; // Thời gian hết hạn Access Token: 60 phút
        public const int REFRESH_TOKEN_EXP = 3600 * 24 * 30; // Thời gian hết hạn Refresh Token: 30 ngày
        public const int REFRESH_TOKEN_LENGTH = 24;

        public const string PAYLOAD_KEY = "payload";

    }
}
