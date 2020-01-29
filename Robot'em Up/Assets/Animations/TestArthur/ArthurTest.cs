using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArthurTest : MonoBehaviour
{

    public Animator myAnim;
    public float speed;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Vector3.forward * speed * Time.deltaTime;
    }

    public void Test()
    {
        float test1 = Random.Range(-1f, 1f);
        float test2 = Random.Range(-1f, 1f);
        print(test1);
        print(test2);
        myAnim.SetFloat("Blend", test1);
        myAnim.SetFloat("Blend2", test2);
        myAnim.SetTrigger("PassTrigger");
    }
}
