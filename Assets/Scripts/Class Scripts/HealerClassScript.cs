using Assets.Scripts.Class_Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class HealerClassScript : BaseClass
{
    new int attack = -2;
    new int defense = 11;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    protected override int Attack
    {
        get { return -2; }
    }


    protected override int Defense
    {
        get { return 11; }
    }

    public override void SetTarget(List<Combatant> TeamOneBlobs, List<Combatant> TeamTwoBlobs, int team)
    {
        var combatant = gameObject.GetComponent<Combatant>();
        //Eventually hand this off to class
        if (team == 1)
        {
            var temp = TeamOneBlobs.ToList();
            temp.Remove(combatant);
            if (temp.Count == 0)
            {
                return;
            }
            target = TeamOneBlobs[UnityEngine.Random.Range(0, TeamOneBlobs.Count - 1)].gameObject;
        }
        else
        {
            var temp = TeamTwoBlobs.ToList();
            temp.Remove(combatant);
            if (temp.Count == 0)
            {
                return;
            }
            target = TeamTwoBlobs[UnityEngine.Random.Range(0, TeamTwoBlobs.Count - 1)].gameObject;
        }

    }

    public override bool IsValidTarget(string name)
    {
        var combatant = gameObject.GetComponent<Combatant>();
        if (combatant.Team == 1 && (Assets.God.Instance.GetTeamOne().Count == 1))
        {
            base.IsValidTarget(name);
        }
        if (combatant.Team == 2 && (Assets.God.Instance.GetTeamOne().Count == 2))
        {
            base.IsValidTarget(name);
        }

        if (gameObject.name.Contains("Red") && name.Contains("Red"))
        {
            return true;
        }
        if (gameObject.name.Contains("Blue") && name.Contains("Blue"))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <returns>skip rest of combat phase</returns>
    public override bool SpecialAttackBehavior(Combatant target)
    {
        var combatant = gameObject.GetComponent<Combatant>();
        if (combatant.Team == 1 && (Assets.God.Instance.GetTeamOne().Count == 1))
        {
            return base.IsValidTarget(name);
        }
        if (combatant.Team == 2 && (Assets.God.Instance.GetTeamOne().Count == 2))
        {
           return base.IsValidTarget(name);
        }
        if (combatant.Team != target.Team)
        {
            return base.IsValidTarget(name);
        }


        //GetComponent<Combatant>().RemoveHit(1);
        target.RemoveHit(1);
        return true;
    }
}
