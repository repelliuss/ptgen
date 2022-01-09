using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class Player : MonoBehaviour {
    FreeFlyCamera free;
    RigidbodyFirstPersonController fps;
    Rigidbody rigid;

    void Awake()
    {
        free = GetComponent<FreeFlyCamera>();
        fps = GetComponent<RigidbodyFirstPersonController>();
        rigid = GetComponent<Rigidbody>();
    }

    public string GetCameraName()
    {
        if(fps.enabled) return "FIRST PERSON";
        else return "FREE CAM";
    }

    public void SwitchCamera()
    {
        if(fps.enabled)
        {
            fps.enabled = false;
            free.enabled = true;
            rigid.useGravity = false;
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }
        else
        {
            fps.enabled = true;
            free.enabled = false;
            rigid.useGravity = true;
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }
    }
}
