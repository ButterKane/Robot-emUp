using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AR3AnimationEvent : MonoBehaviour
{
    public ArenaClusterCubes clusterCube1;
    public ArenaClusterCubes clusterCube2;
    public Animator myAnim;

    public void ActivateClusterCube()
    {
        clusterCube1.ToggleClusterLayout();
        clusterCube2.NextClusterLayout();
    }

    public void AppearingTrigger()
    {
        myAnim.SetTrigger("AppearingTrigger");
    }

    public void DisappearingTrigger()
    {
        myAnim.SetTrigger("DisappearingTrigger");
    }
}
