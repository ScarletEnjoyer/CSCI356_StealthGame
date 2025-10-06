using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterConrtol : MonoBehaviour
{
    public float speed = 5f;
    public float crouchSpeed = 2f;
    private CharacterController controller;
    private bool isCrouching = false;

    void Start() { controller = GetComponent<CharacterController>(); }

    void Update()
    {
        float moveSpeed = isCrouching ? crouchSpeed : speed;
        Vector3 move = transform.forward * Input.GetAxis("Vertical") * moveSpeed + transform.right * Input.GetAxis("Horizontal") * moveSpeed;
        controller.Move(move * Time.deltaTime + Physics.gravity * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.C))
        {
            isCrouching = !isCrouching;
            controller.height = isCrouching ? 1f : 2f;
        }
    }
}
