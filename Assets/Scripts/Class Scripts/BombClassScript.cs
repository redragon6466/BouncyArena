using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Class_Scripts
{
    public class BombClassScript : BaseClass
    {
        protected override int MaxDefense
        {
            get { return 0; }
        }
        protected override int MinDefense
        {
            get { return 0; }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <returns>skip rest of combat phase</returns>
        public override bool SpecialAttackBehavior(Combatant target)
        {
            


            return false;
        }
    }
}
