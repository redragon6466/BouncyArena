using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Class_Scripts
{
    public partial class BaseClass : MonoBehaviour
    {
        public GameObject target;
        private List<int> activeAttackModifiers;
        private List<int> activeDefenseModifiers;


        private int _attack = 2;
        private int _defense = 0;


        #region Unity Methods
        void Start()
        {
            activeAttackModifiers = new List<int>();
            activeDefenseModifiers = new List<int>();
        }

        // Update is called once per frame
        void Update()
        {

        }
        #endregion


        #region Properties

        protected virtual int MaxAttack
        {
            get { return 3; }
        }
        protected virtual int MinAttack
        {
            get { return 1; }
        }
        protected virtual int MaxDefense
        {
            get { return 1; }
        }
        protected virtual int MinDefense
        {
            get { return 0; }
        }

        protected virtual int Attack
        {
            get { return _attack; }
            set { _attack = value; }
        }


        protected virtual int Defense
        {
            get { return _defense; }
            set { _defense = value; }
        }

        protected virtual int HitPoints
        {
            get { return 4; }
        }

        #endregion

        public virtual void SetTarget(List<Combatant> TeamOneBlobs, List<Combatant> TeamTwoBlobs, int team)
        {
            //Eventually hand this off to class
            if (team == 1)
            {
                target = TeamTwoBlobs[UnityEngine.Random.Range(0, TeamTwoBlobs.Count - 1)].gameObject;
            }
            else
            {
                target = TeamOneBlobs[UnityEngine.Random.Range(0, TeamOneBlobs.Count - 1)].gameObject;
            }

        }

        public int GetAttack()
        {
            return Attack;
        }

        public int GetDefense()
        {
            return Defense;
        }

        public int GetHealth()
        {
            return HitPoints;
        }

        public int RollToAttack()
        {
            var totalMod = Attack;
            if (activeAttackModifiers == null)
            {
                activeAttackModifiers = new List<int>();
            }
            foreach (var mod in activeAttackModifiers)
            {
                totalMod += mod;
            }

            var roll = UnityEngine.Random.Range(1, 20);

            return roll + totalMod;
        }

        public void GenerateStats()
        {
            Attack = UnityEngine.Random.Range(MinAttack, MaxAttack+1);
            Defense = UnityEngine.Random.Range(MinDefense, MaxDefense+1);
        }

        public virtual bool IsValidTarget(string name)
        {
            if (gameObject.name.Contains("Red") && name.Contains("Red"))
            {
                return false;
            }
            if (gameObject.name.Contains("Blue") && name.Contains("Blue"))
            {
                return false;
            }
            return true;
        }

        public virtual bool SpecialAttackBehavior(Combatant target)
        {
            return false;
        }

        public int GetTotalDefense()
        {
            var totalDefense = Defense;
            if (activeAttackModifiers == null)
            {
                activeAttackModifiers = new List<int>();
            }
            foreach (var item in activeAttackModifiers)
            {
                totalDefense += item;
            }
            return totalDefense;
        }
    }
}
