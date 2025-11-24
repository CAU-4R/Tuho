using UnityEngine;

namespace CAU4R.Tuho.Particle
{
    public class ParticleEmitter : MonoBehaviour
    {
        [SerializeField]
        private ParticleSystem _particleSystem;

        public void Emit(Vector3 position, Quaternion rotation, Color color)
        {
            if (_particleSystem == null) return;
            
            var emit = new ParticleSystem.EmitParams();
            
            emit.position = position;
            emit.startColor = color;
            emit.startSize = 1f;
            emit.velocity = Vector3.zero;
            
            _particleSystem.Emit(emit, 1);
        }
    }
}
