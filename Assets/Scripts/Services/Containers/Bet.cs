using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Services.Containers
{
    public class Bet
    {
        public string ViewerName { get; private set; }
        public int Amount { get; private set; }

        public Bet(string name, int amount)
        {
            ViewerName = name;
            Amount = amount;
        }
    }
}
