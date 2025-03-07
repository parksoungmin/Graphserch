using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Animator animator;
    public float speed = 2f;

    public void Update()
    {
        //transform.position
    } 
    public IEnumerable MoverPlayer()
    {
        yield return new WaitForSeconds(speed);
    }
}
