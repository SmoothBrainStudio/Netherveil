using System;
using UnityEngine;

public class ErecrosWeaponBehaviour : MonoBehaviour
{
    Rigidbody rb;

    [HideInInspector] public bool hitMap = false;

    [Serializable]
    public class WeaponSound
    {
        public Sound flying;
        public Sound hitmap;
    }
    [SerializeField] WeaponSound sounds;

    [HideInInspector] public bool ignoreCollisions = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        Reset();
    }

    private void Update()
    {
        if (transform.position.y <= -0.1f && !hitMap && !ignoreCollisions)
        {
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeAll;
            rb.velocity = Vector3.zero;

            sounds.flying.Stop();
            sounds.hitmap.Play(transform.position);
            hitMap = true;
        }
    }

    public void PlayFlying()
    {
        sounds.flying.Play(transform.position);
    }

    public void Reset()
    {
        hitMap = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hitMap && !ignoreCollisions)
        {
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeAll;
            rb.velocity = Vector3.zero;

            sounds.flying.Stop();
            sounds.hitmap.Play(transform.position);
            hitMap = true;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!hitMap && !ignoreCollisions)
        {
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeAll;
            rb.velocity = Vector3.zero;

            sounds.flying.Stop();
            sounds.hitmap.Play(transform.position);
            hitMap = true;
        }
    }
}