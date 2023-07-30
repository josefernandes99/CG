using UnityEngine;

public class AddCollidersToChildren : MonoBehaviour
{
    void Start()
    {
        AddCollidersRecursively(transform);
    }

    void AddCollidersRecursively(Transform parent)
    {
        foreach (Transform child in parent)
        {
            // Add collider to the child object
            if (child.GetComponent<Rigidbody>() == null)
            {
                Rigidbody childCollider = child.gameObject.AddComponent<Rigidbody>();
                //childCollider.center -= new Vector3(0f, 2f, 0f);
                //childCollider.constraints =
                //  BoxColliderConstraints.FreezeRotation || BoxColliderConstraints.FreezePosition;
            }

            // Recursively call the function for any child objects
            AddCollidersRecursively(child);
        }
    }
}
