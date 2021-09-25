using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Data
{
    public class PingResponse
    {
        public bool success;
        public string message;
        public int blueTeamBets;
        public int redTeamBets;
        public List<string> activeUserEvent;
    }
}
