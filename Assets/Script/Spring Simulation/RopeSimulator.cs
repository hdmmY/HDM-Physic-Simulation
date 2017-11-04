using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SpringSimulation
{
    [RequireComponent(typeof(MeshFilter))]
    public class RopeSimulator : MonoBehaviour
    {
        [Range(2, 1000)]
        public int m_segmentNum;   // The segment of the rope

        [Range(0.3f, 10)]
        public float m_ropeWidth;   // The rope width

        public Vector3 m_startPoint;   // Start position of the rope

        public Vector3 m_endPoint;   // End position of the rope

        public float m_DragCoef;   // The air drag coefficient

        public Vector3 m_WindForce;  // The wind force

        public ParticleProperty m_particlePrefab;

        public SpringProperty m_springPrefab;

        public int m_times;

        private List<ParticleProperty> _particles;

        private List<SpringProperty> _springs;

        // Render
        private Mesh _mesh;
        private MeshFilter _meshFilter;
        private List<Vector3> _normals;
        private List<Vector2> _uvs;
        private List<Vector3> _vertices;
        private int[] _triangles;


        private void Start()
        {
            _particles = new List<ParticleProperty>(m_segmentNum + 1);
            _springs = new List<SpringProperty>(m_segmentNum);

            // Initialize particles
            Vector3 deltVector = (m_endPoint - m_startPoint) / m_segmentNum;
            for (int i = 0; i < m_segmentNum + 1; i++)
            {
                ParticleProperty particle = Instantiate<ParticleProperty>(m_particlePrefab);
                particle.m_position = m_startPoint + deltVector * i;

                particle.m_velocity = Vector3.zero;
                particle.m_acceleration = Vector3.zero;
                particle.m_force = Vector3.zero;
                particle.m_blocked = false;

                _particles.Add(particle);
            }
            _particles[0].m_blocked = true;     // The first particle is blocked.


            // Initialize springs
            float initLength = deltVector.magnitude;
            for (int i = 0; i < m_segmentNum; i++)
            {
                SpringProperty spring = Instantiate<SpringProperty>(m_springPrefab);
                spring.m_startParticle = _particles[i];
                spring.m_endParticle = _particles[i + 1];
                spring.m_initLength = initLength;

                _springs.Add(spring);
            }


            // Init render property
            _mesh = new Mesh();
            _mesh.MarkDynamic();
            _meshFilter = GetComponent<MeshFilter>();
            _normals = new List<Vector3>(new Vector3[(m_segmentNum + 1) * 2]);
            _uvs = new List<Vector2>(new Vector2[(m_segmentNum + 1) * 2]);
            _vertices = new List<Vector3>(new Vector3[(m_segmentNum + 1) * 2]);
            _triangles = new int[m_segmentNum * 6];
            _meshFilter.mesh = _mesh;
        }


        private void FixedUpdate()
        {
            Vector3 accel = Vector3.zero;
            float deltTime = Time.fixedDeltaTime / m_times;

            for (int t = 0; t < m_times; t++)
            {
                CalForce();

                for (int i = 0; i < _particles.Count; i++)
                {
                    accel = _particles[i].m_force * _particles[i].InvMass;
                    _particles[i].m_acceleration = accel;
                    _particles[i].m_velocity += accel * deltTime;
                    _particles[i].m_position += _particles[i].m_velocity * deltTime;
                }
                for (int i = 0; i < _springs.Count; i++)
                {
                    _springs[i].m_springDirection = (_springs[i].m_startParticle.m_position -
                            _springs[i].m_endParticle.m_position).normalized;
                }
            }

            RenderGeometry();
        }


        /// <summary>
        /// Calculate every particle's force
        /// </summary>
        void CalForce()
        {
            Vector3 dragVector;

            // Reset force that apply on the particle
            for (int i = 0; i < _particles.Count; i++)
            {
                _particles[i].m_force.Set(0, 0, 0);
            }

            // Deal with gravity and resistance
            for (int i = 0; i < _particles.Count; i++)
            {
                ParticleProperty particle = _particles[i];

                if (particle.m_blocked) continue;

                // Gravity
                particle.m_force += Physics.gravity * particle.m_mass;

                // Air drag force
                dragVector = -particle.m_velocity;
                dragVector.Normalize();
                particle.m_force += dragVector * particle.m_velocity.sqrMagnitude * m_DragCoef;

                // Wind force
                particle.m_force += m_WindForce;
            }

            // Deal with spring force
            for (int i = 0; i < _springs.Count; i++)
            {
                ParticleProperty startParticle = _springs[i].m_startParticle;
                ParticleProperty endParticle = _springs[i].m_endParticle;

                Vector3 distance = endParticle.m_position - startParticle.m_position;
                Vector3 relateVelocity = endParticle.m_velocity - startParticle.m_velocity;
                float deltLength = distance.magnitude - _springs[i].m_initLength;

                float springForce = _springs[i].m_kConstant * deltLength;
                distance.Normalize();

                Vector3 springForceVector = distance * springForce +
                    (_springs[i].m_dampConstant * Vector3.Dot(relateVelocity, distance)) * distance;

                if (!startParticle.m_blocked) startParticle.m_force += springForceVector;
                if (!endParticle.m_blocked) endParticle.m_force -= springForceVector;
            }
        }


        void RenderGeometry()
        {
            for (int i = 0; i < m_segmentNum + 1; i++)
            {
                _vertices[i * 2] = transform.InverseTransformPoint(_particles[i].m_position);
                _vertices[i * 2 + 1] = _vertices[i * 2] + Vector3.right * m_ropeWidth;

                _normals[i * 2] = _normals[i * 2 + 1] = -transform.forward;

                _uvs[i * 2] = new Vector2(0, i / m_segmentNum);
                _uvs[i * 2 + 1] = new Vector2(1, i / m_segmentNum);
            }

            for (int i = 0; i < m_segmentNum; i++)
            {
                _triangles[i * 6] = i * 2;
                _triangles[i * 6 + 1] = i * 2 + 1;
                _triangles[i * 6 + 2] = i * 2 + 3;

                _triangles[i * 6 + 3] = i * 2;
                _triangles[i * 6 + 4] = i * 2 + 3;
                _triangles[i * 6 + 5] = i * 2 + 2;
            }

            _mesh.SetVertices(_vertices);
            _mesh.SetTriangles(_triangles, 0);
            _mesh.SetUVs(1, _uvs);
            _mesh.SetNormals(_normals);
        }




        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(m_startPoint, Vector3.one);

            Gizmos.color = Color.red;
            Gizmos.DrawCube(m_endPoint, Vector3.one);

            Gizmos.color = Color.white;
            Gizmos.DrawLine(m_startPoint, m_endPoint);
        }

        private void OnDrawGizmos()
        {


            if (_springs != null && _springs.Count > 0)
            {
                foreach (var spring in _springs)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(spring.m_startParticle.m_position, spring.m_endParticle.m_position);
                }

                foreach (var particle in _particles)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(particle.m_position, Vector3.one * 0.3f);
                }
            }

        }

    }
}


