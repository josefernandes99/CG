using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class Pistol2 : MonoBehaviour
{
    private float fireRate = 0.50f; // Adjust the fire rate as desired (e.g., time between each bullet)
    private float timeSinceLastShot = 0.50f;
    Transform rightArm;
    Transform assault1Transform;
    Renderer pistol2Renderer;
    Renderer assault1Renderer;
    GameObject copPrefab;
    Vector3 currentPosition = new Vector3(0f, 0f, 0f);
    Vector3 previousPosition = new Vector3(0f, 0f, 0f);

    // Start is called before the first frame update
    void Start()
    {
        rightArm = transform.parent;
        assault1Transform = rightArm.Find("Assault1");
        pistol2Renderer = this.GetComponent<Renderer>();
        assault1Renderer = assault1Transform.GetComponent<Renderer>();

        gameObject.transform.localPosition = new Vector3(1.5f, 0.58f, 0f);
        gameObject.transform.localRotation = Quaternion.Euler(0f, 90f, 90f);
        gameObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        copPrefab = GameObject.Find("cop");
    }

    // Update is called once per frame
    void Update()
    {
        var gun = 0;

        if (pistol2Renderer.enabled == true)
        {
            gun = 1;
        }
        else if (assault1Renderer.enabled == true)
        {
            gun = 2;
        }

        if (Input.GetMouseButtonDown(0) && gun == 1) // Check for left mouse button click
        {
            if (timeSinceLastShot >= fireRate * 2)
            {
                ShootPistol2();
                timeSinceLastShot = 0f;
            }
        }

        // Get the current position of the copPrefab
        currentPosition = copPrefab.transform.position;

        // Check if the copPrefab's position has changed since the last frame
        if (currentPosition != previousPosition)
        {
            // Update the time since the last shot
            timeSinceLastShot += Time.deltaTime;
        }

        // Store the current position for the next frame
        previousPosition = currentPosition;
    }

    // Method for shooting with the Pistol2 gun
    private void ShootPistol2()
    {
        GameObject bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        bullet.name = "Bullet Cop";
        // Attach the Bullet script to the bullet GameObject
        Bullet bulletScript = bullet.AddComponent<Bullet>();
    }
}
