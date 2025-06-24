using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interact_Craft : InteractionObjectBase
{
    [SerializeField] private GameObject ball;
    [SerializeField] private float rotationSpeed = 30f;

    public override void OnInteract()
    {
        UIManager.Instance.DisplayInteractCraft();
    }

    private void Update()
    {
        SpinedBall();
    }

    public void SpinedBall()
    {
        ball.transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }
}
