using UnityEngine;

public class SonielProjectile : Projectile
{
    Vector3 direction;
    float swordHeight;
    float aliveTimer;
    [HideInInspector] public bool pickMeUp;
    Transform wrist;
    bool getBack = false;
    bool ignoreCollisions = false;
    //Quaternion originalRota;
    //Vector3 originalPos;
    [HideInInspector] public Vector3 rotationPoint;
    float yPos;

    bool isLeft = false;
    float acceleration = 0f;

    Sound spinSound;
    Sound hitMapSound;

    // Start is called before the first frame update
    protected override void Awake()
    {
        swordHeight = GetComponentInParent<Renderer>().bounds.size.y;
    }

    private void OnEnable()
    {
        Reset();
    }

    private void OnDisable()
    {
        spinSound.Stop();
        Reset();
    }

    // Update is called once per frame
    protected override void Update()
    {
        aliveTimer += Time.deltaTime;

        if (!pickMeUp)
            pickMeUp = aliveTimer >= 3f;

        spinSound.Play(transform.position);

        if (getBack)
        {
            if (Vector3.SqrMagnitude(wrist.position - transform.position) < 4f * 4f)
            {
                ignoreCollisions = true;
                direction = wrist.transform.position - transform.position;
                direction.y = 0f;
            }
        }

        if (getBack && ignoreCollisions)
        {
            acceleration += Time.deltaTime * 0.2f;

            direction = wrist.transform.position - transform.position;
            direction.y = 0f;

            if (Vector3.SqrMagnitude(wrist.position - transform.position) < 4f)
            {
                transform.parent = wrist;

                transform.localRotation = Quaternion.Euler(isLeft ? 0 : 180, -90, isLeft ? 100 : -100);
                transform.localPosition = new Vector3(0.065f, 0.035f, 0);
                if (isLeft) transform.localPosition *= -1;

                this.enabled = false;
                return;
            }
        }
        else
        {
            if (transform.position.y > yPos)
            {
                direction.y = -1;
            }
        }

        Move(direction);
        direction.y = 0f;
    }

    public override void Move(Vector3 _direction)
    {
        _direction.Normalize();
        transform.Translate(_direction * (speed * Time.deltaTime + acceleration), Space.World);
        rotationPoint = transform.position + transform.forward * swordHeight / 2f;
        transform.RotateAround(rotationPoint, Vector3.up, Time.deltaTime * 1000f);
    }

    protected override void OnTriggerEnter(Collider other)
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!ignoreCollisions && transform.parent == null)
        {
            hitMapSound.Play(transform.position);

            if (!getBack)
            {
                Vector3 normal = collision.contacts[0].normal;
                normal.y = 0f;
                float angle = Vector3.Angle(normal, -direction);

                float sign = Mathf.Sign(Vector3.Cross(normal, -direction).y);

                // le random range est là pour éviter que si le projectile est lancé perpendiculairement à la surface sur laquelle il rebondit il fasse des allers-retours à l'infini
                direction = Quaternion.Euler(0, -(angle * sign) + Random.Range(-10f, 10f), 0) * normal;
            }
            else
            {
                direction = wrist.transform.position - transform.position;
                direction.y = 0f;
                ignoreCollisions = true;
            }
        }
    }

    public void SetDirection(Vector3 _direction)
    {
        direction = _direction;
        direction.y = 0f;
        transform.rotation = Quaternion.Euler(0, 0, 90);
    }

    void Reset()
    {
        pickMeUp = false;
        getBack = false;
        ignoreCollisions = false;
        aliveTimer = 0f;
        acceleration = 0f;
    }

    public void SetParent(Transform _wrist, float _yPos)
    {
        wrist = _wrist;
        yPos = _yPos;
    }

    public void GetBack()
    {
        getBack = true;
    }

    public void SetLeft(bool _state)
    {
        isLeft = _state;
    }

    public void SetSounds(Sound _hitmapSound, Sound _spinSound)
    {
        hitMapSound = _hitmapSound;
        spinSound = _spinSound;
    }

    public void ForceBack()
    {
        direction = wrist.transform.position - transform.position;
        direction.y = 0f;
        ignoreCollisions = true;
    }
}
