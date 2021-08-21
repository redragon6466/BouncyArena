using Assets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class Combatant : MonoBehaviour
{
    [SerializeField]
    bool IsMovable = true;

    [SerializeField]
    public GameObject target;

    [SerializeField]
    Material[] breakingMaterials;

    [SerializeField]
    private int attack = 2;
    [SerializeField]
    private int defense = 13;
    [SerializeField]
    private int hitPoints = 4;

    public int ID { get; set; } = -1;

    /*float initialVelocity = 0.0f;
    float finalVelocity = 50.0f;
    float currentVelocity = 0.0f;
    float accelerationRate = 10f;
    float decelerationRate = 20f;
    float power = 0;
    */

    int hitCount = 0;
    int maxSpeed = 50;
    private List<int> activeAttackModifiers;
    private List<int> activeDefenseModifiers;


    // Start is called before the first frame update
    void Start()
    {
        activeAttackModifiers = new List<int>();
        activeDefenseModifiers = new List<int>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsMovable)
        {
            return;
        }

        if (target == null)
        {
            return;
        }

        if (!God.Instance.IsBattling)
        {
            return;
        }

        var loc = target.transform;


        Vector3 newVector = loc.position - transform.position;

        newVector.Normalize();
        //Debug.Log(" x: " + newVector.x + " y: " + newVector.y + " z: " + newVector.z);

        //propel the object forward
        GetComponent<Rigidbody>().AddForce(50f * newVector.x, 00, 50f * newVector.z);

        var body = GetComponent<Rigidbody>();

        if (body.velocity.magnitude > maxSpeed)
        {
            body.velocity = Vector3.ClampMagnitude(body.velocity, maxSpeed);
        }

       

    }

    public void Awake()
    {
        
            DontDestroyOnLoad(gameObject); //when the scene changes don't destroy the game object that owns this
    }

    public void SetTarget(List<Combatant> TeamOneBlobs, List<Combatant> TeamTwoBlobs, int team)
    {
        //Eventually hand this off to class
        if (team == 1)
        {
            target = TeamTwoBlobs[Random.Range(0, TeamTwoBlobs.Count-1)].gameObject;
        }
        else
        {
            target = TeamOneBlobs[Random.Range(0, TeamOneBlobs.Count - 1)].gameObject;
        }

    }

    public int GetAttack()
    {
        return attack;
    }

    public int GetDefense()
    {
        return defense;
    }

    public int GetHealth()
    {
        return hitPoints;
    }

    public int RollToAttack()
    {
        var totalMod = attack;
        foreach (var mod in activeAttackModifiers)
        {
            totalMod += mod;
        }

        var roll = Random.Range(1, 20);

        return roll + totalMod;
    }

    public void OnDestroy()
    {
        target = null;
    }


    void OnCollisionEnter(Collision collisionInfo)
    {

        var test = collisionInfo.GetContact(0);

        var sumOfNormal = test.normal.x + test.normal.y + test.normal.z;


        if (collisionInfo.gameObject.tag == "Combatant")
        {
            if (gameObject.name.Contains("Red") && collisionInfo.collider.name.Contains("Red"))
            {
                return;
            }
            if (gameObject.name.Contains("Blue") && collisionInfo.collider.name.Contains("Blue"))
            {
                return;
            }


            // Roll Attack
            var roll = collisionInfo.gameObject.GetComponent<Combatant>().RollToAttack();

            var totalDefense = defense;
            foreach (var item in activeAttackModifiers)
            {
                totalDefense += item;
            }
                                            

            if (roll > totalDefense)
            {
                //Debug.Log("I " + gameObject.name + ", Have been hit by " + collisionInfo.collider.name + ", with a roll of " + roll);
                hitCount++;
                if (hitCount <= hitPoints && hitCount <= breakingMaterials.Length - 1)
                {
                    GetComponent<Renderer>().material = breakingMaterials[hitCount];
                }
                if (hitCount > hitPoints)
                {
                    God.Instance.KillBlob(this);
                }
            }
            
        }

        if (collisionInfo.gameObject.tag == "Arena")
        {
            var rb = GetComponent<Rigidbody>();
            //0 out the current velocity
            rb.velocity.Set(0, rb.velocity.y, 0);
            rb.velocity.Set(0, rb.angularVelocity.y, 0);

            var loc = target.transform;


            Vector3 newVector = loc.position - transform.position;
            newVector.Normalize();
            //Debug.Log(" x: " + newVector.x + " y: " + newVector.y + " z: " + newVector.z);

            //propel the object forward
            GetComponent<Rigidbody>().AddForce(1000 * newVector.x, 0, 1000 * newVector.z);
        }

        //Debug.Log("Detected collision between " + gameObject.name + " and " + collisionInfo.collider.name);
        //Debug.Log("There are " + collisionInfo.contacts.Length + " point(s) of contacts");
        //Debug.Log("The contact normal is " + test.normal.);
    }
}
