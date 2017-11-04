using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CollisionSimulation
{       
    public class ParticleProperty : MonoBehaviour
    {
        // Total mass
        public float m_mass;

        // Position in world space
        //public Vector3 m_position;

        // Velocity in world space
        public Vector3 m_velocity;

        // Mangnitude of the velocity.
        public float m_speed;

        // Total force acting on the particle
        public Vector3 m_force;

        // Particle radius used for collision detection
        public float m_radius;

        // Gravity force vector
        public Vector3 m_gravity;

        // Air density
        public float m_AIRDENSITY;

        // Sphere drag coefficient
        public float m_DRAGCOEFFICIENT;

        // Wind speed
        public float m_WINDSPEED;

        // Collision restitution
        public float m_colRasittution;

        // Collision
        public Vector3 m_previousPosition;
        public Vector3 m_impactForce;
        public bool m_collision;

        public ParticleProperty()
        {
            m_mass = 1.0f;
            m_velocity = Vector3.zero;
            m_speed = 0.0f;
            m_force = Vector3.zero;
            m_radius = 0.1f;
            m_gravity = m_mass * Physics.gravity;
            m_colRasittution = 0.6f;

            m_AIRDENSITY = 1.23f;
            m_DRAGCOEFFICIENT = 0.6f;
            m_WINDSPEED = 10;
        }


        // Calculate the force that apply on particle
        public void Calculoads()
        {
            // Reset force
            m_force = Vector3.zero;

            if (m_collision)
            {
                m_force += m_impactForce;
            }
            else
            {
                // Aggregate force
                m_force += m_gravity;

                // Still air drag
                Vector3 vDrag;
                float fDrag;

                vDrag = -m_velocity.normalized;
                fDrag = 0.5f * m_AIRDENSITY * m_speed * m_speed *
                        (Mathf.PI * m_radius * m_radius) * m_DRAGCOEFFICIENT;
                vDrag *= fDrag;
                m_force += vDrag;

                // Wind
                float wind = 0.5f * m_AIRDENSITY * m_WINDSPEED * m_WINDSPEED *
                             (Mathf.PI * m_radius * m_radius) * m_DRAGCOEFFICIENT;
                m_force += new Vector3(wind, 0, 0);
            }
        }


        /// <summary>
        /// Integrator for update particle movement. User Simple-Euler integrator.
        /// </summary>
        public void UpdateBodyIntegrator(float dt)
        {
            Vector3 a;          // acceleration
            Vector3 dv;       // new velocity at time t + dt
            Vector3 ds;       // new position at time t + dt

            a = m_force / m_mass;

            dv = a * dt;
            m_velocity += dv;

            ds = m_velocity * dt;
            transform.position += ds;

            m_speed = m_velocity.magnitude;
        }
    }
}

