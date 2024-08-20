using UnityEngine;

namespace PolygonArsenal
{
    public class PolygonProjectileScript : MonoBehaviour
    {
        public GameObject impactParticle;
        public GameObject projectileParticle;
        public GameObject muzzleParticle;
        public GameObject[] trailParticles;
        [Header("Adjust if not using Sphere Collider")]
        public float colliderRadius = 1f;
        [Range(0f, 1f)]
        public float collideOffset = 0.15f;


        private SphereCollider _sphereCollider;
        private Rigidbody _rigidbody;

        void Start()
        {
            _sphereCollider = GetComponent<SphereCollider>();
            _rigidbody = GetComponent<Rigidbody>();

            projectileParticle = Instantiate(projectileParticle, transform.position, transform.rotation);
            projectileParticle.transform.parent = transform;

            if (muzzleParticle)
            {
                muzzleParticle = Instantiate(muzzleParticle, transform.position, transform.rotation);
                Destroy(muzzleParticle, 1.5f); // Lifetime of muzzle effect.
            }
        }

        void FixedUpdate()
        {
            RaycastHit hit;

            float rad = _sphereCollider ? _sphereCollider.radius : colliderRadius;

            Vector3 dir = _rigidbody.velocity;
            if (_rigidbody.useGravity)
                dir += Physics.gravity * Time.deltaTime;
            dir = dir.normalized;

            float dist = _rigidbody.velocity.magnitude * Time.deltaTime;

            if (Physics.SphereCast(transform.position, rad, dir, out hit, dist))
            {
                transform.position = hit.point + (hit.normal * collideOffset);

                GameObject impactP = Instantiate(impactParticle, transform.position, Quaternion.FromToRotation(Vector3.up, hit.normal));

                if (hit.transform.tag == "Destructible") // Projectile will destroy objects tagged as Destructible
                {
                    Destroy(hit.transform.gameObject);
                }

                foreach (GameObject trail in trailParticles)
                {
                    GameObject curTrail = transform.Find(projectileParticle.name + "/" + trail.name).gameObject;
                    curTrail.transform.parent = null;
                    Destroy(curTrail, 3f);
                }
                Destroy(projectileParticle, 3f);
                Destroy(impactP, 5.0f);
                Destroy(gameObject);

                ParticleSystem[] trails = GetComponentsInChildren<ParticleSystem>();
                for (int i = 1; i < trails.Length; i++)
                {
                    ParticleSystem trail = trails[i];

                    if (trail.gameObject.name.Contains("Trail"))
                    {
                        trail.transform.SetParent(null);
                        Destroy(trail.gameObject, 2f);
                    }
                }
            }
        }


    }
}