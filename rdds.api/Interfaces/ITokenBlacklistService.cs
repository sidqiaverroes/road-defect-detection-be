using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rdds.api.Interfaces
{
    public interface ITokenBlacklistService
    {
        void AddTokenToBlacklist(string token);
        bool IsTokenBlacklisted(string token);
    }
}