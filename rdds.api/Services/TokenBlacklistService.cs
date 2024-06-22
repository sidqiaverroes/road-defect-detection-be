using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using rdds.api.Interfaces;

namespace rdds.api.Services
{
    public class TokenBlacklistService : ITokenBlacklistService
    {
        private readonly HashSet<string> _blacklist = new HashSet<string>();

        public void AddTokenToBlacklist(string token)
        {
            _blacklist.Add(token);
        }

        public bool IsTokenBlacklisted(string token)
        {
            return _blacklist.Contains(token);
        }
    }
}