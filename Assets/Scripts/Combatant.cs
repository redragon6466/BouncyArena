using Assets;
using Assets.Scripts.Class_Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class Combatant : MonoBehaviour
{
    #region SerializedFields
    [SerializeField]
    bool IsMovable = true;
    [SerializeField]
    GameObject ParticleMan;
    [SerializeField]
    public Vector3 GameScale;
    [SerializeField]
    public Vector3 LineupScale;
    [SerializeField]
    public BaseClass ClassScriptField;

    [SerializeField]
    Material[] breakingMaterials;

    private Rigidbody _body;
    

    #endregion

    #region properties
    public int ID { get; set; } = -1;

   
    public int Team { get; set; } = -1;

    public BaseClass ClassScript
    { 
        get
        {
            if (ClassScriptField == null)
            {
                ClassScriptField = new BaseClass();
            }
            return ClassScriptField;
        }
    }

    #endregion

    /*float initialVelocity = 0.0f;
    float finalVelocity = 50.0f;
    float currentVelocity = 0.0f;
    float accelerationRate = 10f;
    float decelerationRate = 20f;
    float power = 0;
    */

    [SerializeField]
    int hitCount = 0;

    int maxSpeed = 500;

    #region Unity Methods
    // Start is called before the first frame update
    void Start()
    {
        //safety strats use default behavior if nothing is assigned
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsMovable)
        {
            return;
        }

        if (ClassScript.target == null)
        {
            return;
        }

        if (!God.Instance.IsBattling)
        {
            return;
        }

        var loc = ClassScript.target.transform;


        Vector3 newVector = loc.position - transform.position;

        newVector.Normalize();
        //Debug.Log(" x: " + newVector.x + " y: " + newVector.y + " z: " + newVector.z);

        //propel the object forward
        if (_body == null)
        {
            _body = GetComponent<Rigidbody>();
        }
        //_body.AddForce(5000f * Time.deltaTime * newVector.x, 500f * Time.deltaTime * newVector.y, 5000f * Time.deltaTime * newVector.z);

        

        if (_body.velocity.magnitude > maxSpeed)
        {
            //_body.velocity = Vector3.ClampMagnitude(_body.velocity, maxSpeed);
        }

       

    }

    public void Awake()
    {
            DontDestroyOnLoad(gameObject); //when the scene changes don't destroy the game object that owns this
    }

    public void OnDestroy()
    {
        ClassScript.target = null;
    }

    void OnCollisionEnter(Collision collisionInfo)
    {


        var test = collisionInfo.GetContact(0);

        var sumOfNormal = test.normal.x + test.normal.y + test.normal.z;



        if (collisionInfo.gameObject.tag == "Combatant")
        {
            if (!ClassScript.IsValidTarget(collisionInfo.collider.name))
            {
                return;
            }


            var target = collisionInfo.gameObject.GetComponent<Combatant>();
           

            // Roll Attack
            var roll = target.RollToAttack();

            if (roll > ClassScript.GetTotalDefense())
            {

                SpawnParticlesOnHit();
                var skip = ClassScript.SpecialAttackBehavior(target);
                if (skip)
                {
                    return;
                }
                //Debug.Log("I " + gameObject.name + ", Have been hit by " + collisionInfo.collider.name + ", with a roll of " + roll);
                hitCount++;
                if (hitCount <= ClassScript.GetHealth() && hitCount <= breakingMaterials.Length - 1)
                {
                    GetComponent<Renderer>().material = breakingMaterials[hitCount];
                }
                if (hitCount > ClassScript.GetHealth())
                {
                    God.Instance.KillBlob(this);
                    return;
                }
            }

            if (_body == null)
            {
                _body = GetComponent<Rigidbody>();
            }

            _body.velocity = Vector3.zero;
            _body.angularVelocity = Vector3.zero;
            var loc = collisionInfo.GetContact(0);

            Vector3 newVector = loc.point - transform.position;

            newVector.Normalize();
            //Debug.Log(" x: " + newVector.x + " y: " + newVector.y + " z: " + newVector.z);

            //propel the object backward on a hit}
            _body.AddForce(-20000f * newVector.x, 500f , -20000f * newVector.z);

        }

        if (collisionInfo.gameObject.tag == "Arena")
        {
            var audio = GetComponent<AudioSource>();
            if (audio != null)
            {
                audio.Play();
            }

            var rb = GetComponent<Rigidbody>();
            //0 out the current velocity
            rb.velocity.Set(0, rb.velocity.y, 0);
            rb.velocity.Set(0, rb.angularVelocity.y, 0);

            if (ClassScript.target != null)
            {
                var loc = ClassScript.target.transform;


                Vector3 newVector = loc.position - transform.position;
                newVector.Normalize();
                //Debug.Log(" x: " + newVector.x + " y: " + newVector.y + " z: " + newVector.z);

                //propel the object forward

                rb.velocity = new Vector3(30 * newVector.x, 10, 30 * newVector.z);
                //GetComponent<Rigidbody>().AddForce(5000 * newVector.x, 500, 5000 * newVector.z);
            }

        }

        //Debug.Log("Detected collision between " + gameObject.name + " and " + collisionInfo.collider.name);
        //Debug.Log("There are " + collisionInfo.contacts.Length + " point(s) of contacts");
        //Debug.Log("The contact normal is " + test.normal.);
    }

    #endregion

    #region Public Methods
    public void SetTarget(List<Combatant> TeamOneBlobs, List<Combatant> TeamTwoBlobs, int team)
    {
        //Eventually hand this off to class
        ClassScript.SetTarget(TeamOneBlobs, TeamTwoBlobs, team);

    }

    /// <summary>
    /// Special Method to heal combatants, added for healer class
    /// </summary>
    /// <param name="countToAdd"></param>
    public void RemoveHit(int countToAdd)
    {
        hitCount -= countToAdd;
        if (hitCount < 0)
        {
            hitCount = 0;
        }
        if (hitCount <= breakingMaterials.Length - 1)
        {
            
            GetComponent<Renderer>().material = breakingMaterials[hitCount];
        }

    }

    public int GetAttack()
    {
        return ClassScript.GetAttack();
    }

    public int GetDefense()
    {
        return ClassScript.GetDefense();
    }

    public int GetHealth()
    {
        return ClassScript.GetHealth();
    }

    public int RollToAttack()
    {
        return ClassScript.RollToAttack();
    }

    public void GenerateStats()
    {
        ClassScript.GenerateStats();
    }

    
    public void SpawnParticlesOnHit() 
    {
        GameObject newParticle = Instantiate(ParticleMan, new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z), transform.rotation);

       /* ParticleSystem.MainModule main = newParticle.GetComponent<ParticleSystem>().main;
        if (Team == 1)
        {
            main.startColor = new ParticleSystem.MinMaxGradient(Color.blue);
        }
        else
        {
            main.startColor = new ParticleSystem.MinMaxGradient(Color.red);

        }*/
    }

    #endregion

    
}
