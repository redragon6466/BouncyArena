using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Class_Scripts
{
    public class BombClassScript : BaseClass
    {
        int Range = 15;

        protected override int MaxAttack
        {
            get { return 10; }
        }

        protected override int MinAttack
        {
            get { return 5; }
        }

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
            var combatant = gameObject.GetComponent<Combatant>();
            List<Combatant> enemyTeam = new List<Combatant>();
            if (combatant.Team == 1)
            {
                enemyTeam = God.Instance.GetTeamTwo();
            }
            else if (combatant.Team == 2)
            {
                enemyTeam = God.Instance.GetTeamOne();
            }

            foreach (var enemy in enemyTeam)
            {
                float dist = Vector3.Distance(enemy.gameObject.transform.position, transform.position);
                if (dist < Range)
                {
                    var thisCombatant = gameObject.GetComponent<Combatant>();
                    if (thisCombatant != null)
                    {
                        enemy.InExplosionRange(thisCombatant);
                    }
                    
                }
            }


            return false;
        }
    }
}
