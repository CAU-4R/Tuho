using UnityEngine;

namespace CAU4R.Tuho.Particle
{
    public class TuhoParticleEmitter : MonoBehaviour
    {
        [SerializeField]
        private ParticleSystem _particleSystem;

        public void Emit(Color color)
        {
            if (_particleSystem == null) return;
            
            var emit = new ParticleSystem.EmitParams();
            
            emit.startColor = color;

            _particleSystem.Emit(emit, 1);
        }
    }
}
