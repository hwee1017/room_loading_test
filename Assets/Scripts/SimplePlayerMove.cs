using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimplePlayerMove : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    private CharacterController controller;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        float x = 0;
        float z = 0;

        if (Input.GetKey(KeyCode.W)) z += 1;
        if (Input.GetKey(KeyCode.S)) z -= 1;
        if (Input.GetKey(KeyCode.A)) x -= 1;
        if (Input.GetKey(KeyCode.D)) x += 1;

        Vector3 move = new Vector3(x, 0, z).normalized;
        controller.Move(move * speed * Time.deltaTime);
    }
}