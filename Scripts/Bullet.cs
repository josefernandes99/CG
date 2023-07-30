using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class Bullet : MonoBehaviour
{
    GameObject copPrefab;
    Transform person;
    Transform rightArm;
    GameObject gunObject = null;
    Vector3 bulletVelocity = new Vector3(0f, 0f, 0f);
    Vector3 currentPosition = new Vector3(0f, 0f, 0f);
    Vector3 previousPosition = new Vector3(0f, 0f, 0f);
    Rigidbody bulletRigidbody = null;
    public float speed = 20f; // Speed of the bullet
    public float destroyDelay = 2f; // Delay before destroying the bullet

    // Distance traveled by the bullet
    private float distanceTraveled = 0f;
    public float maxDistance = 100f; // Maximum distance the bullet can travel

    // Start is called before the first frame update
    void Start()
    {
        copPrefab = GameObject.Find("cop");
        if (this.name.Split(" ")[1] == "Cop")
        {
            person = GameObject.Find("cop").transform;
        }
        else if (this.name.Split(" ")[1] == "robber")
        {
            string id = this.name.Split(" ")[2];
            person = GameObject.Find("robber " + id).transform;
        }
        rightArm = person.transform.Find("rightarm");
        this.tag = "Bullet";

        // Reset the distance traveled
        distanceTraveled = 0f;
        Transform pistol2Transform = rightArm.Find("Pistol2");
        Renderer pistol2Renderer = pistol2Transform.GetComponent<Renderer>();

        if (pistol2Renderer.enabled == true)
        {
            gunObject = rightArm.Find("Pistol2").gameObject;
            Vector3 higherPistol = new Vector3(0f, 0.2f, 0f);
            this.transform.position =
                gunObject.transform.position + gunObject.transform.forward * 0.5f + higherPistol; // Place the bullet slightly in front of the gun
        }
        else
        {
            gunObject = rightArm.Find("Assault1").gameObject;
            // Set the bullet's position slightly in front of the gun
            Vector3 bulletOffset = new Vector3(0f, 0.25f, 0f);
            this.transform.position =
                gunObject.transform.position + gunObject.transform.forward * 1.5f + bulletOffset;
        }

        // Adjust the size and color of the bullet
        this.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        this.transform.GetComponent<Renderer>().material.color = Color.black;

        // Add a Rigidbody component to the bullet for movement
        bulletRigidbody = gameObject.AddComponent<Rigidbody>();
        bulletRigidbody.useGravity = false;

        // Calculate the bullet's velocity based on the cop's forward direction
        bulletVelocity = person.transform.forward * 10f; // Adjust the bullet speed as desired
        bulletRigidbody.velocity = bulletVelocity;
    }

    // Update is called once per frame
    void Update()
    {
        // Get the current position of the copPrefab
        currentPosition = copPrefab.transform.position;

        // Check if the copPrefab's position has changed since the last frame
        if (currentPosition != previousPosition)
        {
            bulletRigidbody.velocity = bulletVelocity;
        }
        else
        {
            bulletRigidbody.velocity = new Vector3(0f, 0f, 0f);
        }

        // Store the current position for the next frame
        previousPosition = currentPosition;

        // Update the distance traveled
        distanceTraveled += bulletRigidbody.velocity.magnitude * Time.deltaTime;

        // Check if the bullet has traveled the maximum distance
        if (distanceTraveled >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the collision is with the Cop or Robber object
        Debug.Log(
            "Bullet hit "
                + collision.gameObject.name
                + " and was shot by "
                + this.transform.name.Split(" ")[1]
        );
        if (
            (collision.gameObject.name == "cop" && this.transform.name.Split(" ")[1] != "Cop")
            || (
                collision.gameObject.name.Split(" ")[0] == "robber"
                && this.transform.name.Split(" ")[1] != "robber"
            )
            || collision.gameObject.name == "BuildingComplex"
        )
        {
            // Destroy the bullet GameObject
            Destroy(gameObject);
        }
    }
}
