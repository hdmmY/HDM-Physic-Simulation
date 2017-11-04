using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SpringSimulation
{
    [CreateAssetMenu(menuName = "RopeData/Spring", fileName = "RopeSpring")]
    public class SpringProperty :ScriptableObject
    {
        public ParticleProperty m_startParticle;   // The first particle that this spring is connected

        public ParticleProperty m_endParticle;   // The second particle that this spring is connnected

        public float m_kConstant;  // Spring tension coefficient

        public float m_dampConstant;     // Spring damping coefficient

        public float m_initLength;    // The spring length when there is no force apply on the spring

        public Vector3 m_springDirection;   // The spring direction
    }

}

