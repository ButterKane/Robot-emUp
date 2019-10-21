using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwissArmyKnife
{
    public static Vector3 GetFlattedDownPosition(Vector3 vector, Vector3 self)
    {
        return new Vector3(vector.x, self.y, vector.z);
    }
}
