using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SpringSimulation
{
    [CreateAssetMenu(fileName = "SpringParticle", menuName = "RopeData/Particle")]
    public class ParticleProperty : ScriptableObject
    {
        public float m_mass;     // The particle's mass

        public float InvMass   // The inverse of the mass
        {
            get
            {
                return 1f / m_mass;
            }
        }

        public Vector3 m_position;   // The particle's position

        public Vector3 m_velocity;   // The particle's velocity

        public Vector3 m_acceleration;   // The particle's acceleration

        public Vector3 m_force;    // The force that apply to the particle

        public bool m_blocked;    // The particle's position is locked or not   
       
    }
}


