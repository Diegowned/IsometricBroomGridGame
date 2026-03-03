using UnityEngine;

public class Projectile : MonoBehaviour {
    public float speed = 20f;
    public float lifeTime = 2f;
    public float damage = 5f;

    void Start() {
        Destroy(gameObject, lifeTime);
    }

    void Update() {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

private void OnTriggerEnter(Collider other) {
    // Look for the BossController on the object we hit, OR its parent
    BossController boss = other.GetComponentInParent<BossController>();
    
    if (boss != null) {
        boss.TakeDamage(damage); // [cite: 224]
        Destroy(gameObject);
    }
}

}