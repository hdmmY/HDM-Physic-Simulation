using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CollisionSimulation
{
    public class ParticleSimulator : MonoBehaviour
    {

        [SerializeField]
        private Camera _camera;

        public GameObject m_ParticlePrefab;
        public GameObject m_ObstaclePrefab;

        public float m_spawnAreaX;
        public float m_spawnAreaY;

        public int m_particleNumber;

        public int m_obstacleNumber;

        private List<ParticleProperty> _particles;
        private List<ParticleProperty> _obstacles;

        private float _windowHeight;
        private float _windowWidth;


        private void Start()
        {
            Vector3 windowSize = _camera.ScreenToWorldPoint(new Vector3(_camera.pixelWidth, _camera.pixelHeight, 0));
            _windowHeight = windowSize.y * 2;
            _windowWidth = windowSize.x * 2;

            _particles = new List<ParticleProperty>(m_particleNumber);
            for (int i = 0; i < m_particleNumber; i++)
            {
                var particle = Instantiate(m_ParticlePrefab, transform).GetComponent<ParticleProperty>();
                particle.transform.position = new Vector3(
                                Random.Range(-m_spawnAreaX, m_spawnAreaX),
                                Random.Range(_windowHeight / 2 - m_spawnAreaY, _windowHeight / 2),
                                0);
                particle.m_previousPosition = particle.transform.position;
                particle.transform.localScale = Vector3.one * particle.m_radius * 2f;
                _particles.Add(particle);
            }


            _obstacles = new List<ParticleProperty>(m_obstacleNumber);
            for (int i = 0; i < m_obstacleNumber; i++)
            {
                var obstacle = Instantiate(m_ObstaclePrefab, transform).GetComponent<ParticleProperty>();
                obstacle.transform.position = new Vector3(
                                Random.Range(-_windowWidth / 2 + 8 * obstacle.m_radius,
                                              _windowWidth / 2 - 8 * obstacle.m_radius),
                                Random.Range(-_windowHeight / 2 + 4 * obstacle.m_radius,
                                              _windowHeight / 2 - 10 * obstacle.m_radius),
                                0);
                obstacle.transform.localScale = Vector3.one * obstacle.m_radius * 2f;
                _obstacles.Add(obstacle);
            }
        }

        private void Update()
        {
            for (int i = 0; i < m_particleNumber; i++)
            {
                _particles[i].m_collision = CheckForCollision(_particles[i]);
                _particles[i].Calculoads();
                _particles[i].UpdateBodyIntegrator(Time.deltaTime);


                // Bound restriction
                Vector3 bound = Vector3.zero;
                Vector3 newPosition = _particles[i].transform.position;

                if (_particles[i].transform.position.x > _windowWidth / 2) bound.x = _windowWidth / 2;
                if (_particles[i].transform.position.x < -_windowWidth / 2) bound.x = -_windowWidth / 2;
                if (_particles[i].transform.position.y > _windowHeight / 2) bound.y = _windowHeight / 2;
                if (_particles[i].transform.position.y < -_windowHeight / 2) bound.y = -_windowHeight / 2;

                newPosition.x = bound.x == 0 ? newPosition.x : bound.x;
                newPosition.y = bound.y == 0 ? newPosition.y : bound.y;
                _particles[i].transform.position = newPosition;
            }


        }


        private bool CheckForCollision(ParticleProperty particle)
        {
            Vector3 n;     // collision normal
            Vector3 vr;    // relative velocity
            float vrn;
            float J;
            Vector3 Fi;
            bool hasCollision = false;

            particle.m_impactForce = Vector3.zero;

            // Check for collision with ground plane
            if (particle.transform.position.y <= -_windowHeight / 2 + particle.m_radius)
            {
                n = Vector3.up;
                vr = particle.m_velocity;
                vrn = Vector3.Dot(vr, n);

                // To see if particle is moving toward ground
                if (vrn <= 0.0)
                {
                    J = -(Vector3.Dot(vr, n)) * (particle.m_colRasittution + 1) * particle.m_mass;
                    Fi = n;
                    Fi *= J / Time.deltaTime;
                    particle.m_impactForce += Fi;

                    Vector3 newPosition = particle.transform.position;
                    newPosition.y = -_windowHeight / 2 + particle.m_radius;
                    newPosition.x = (newPosition.y - particle.m_previousPosition.y) /
                            (newPosition.y - particle.m_previousPosition.y) *
                            (newPosition.x - particle.m_previousPosition.x) +
                            newPosition.x;
                    particle.transform.position = newPosition;

                    hasCollision = true;
                }
            }


            // Check for collisions with obstacles
            for (int i = 0; i < m_obstacleNumber; i++)
            {
                float r = particle.m_radius + _obstacles[i].m_radius;
                Vector3 d = particle.transform.position - _obstacles[i].transform.position;
                float s = d.magnitude - r;

                if (s <= 0.0)
                {
                    d = d.normalized;
                    n = d;
                    vr = particle.m_velocity - _obstacles[i].m_velocity;
                    vrn = Vector3.Dot(vr, n);

                    if (vrn < 0.0)
                    {
                        J = -vrn * (particle.m_colRasittution + 1) /
                           (1f / particle.m_mass + 1f / _obstacles[i].m_mass);
                        Fi = n;
                        Fi *= J / Time.deltaTime;
                        particle.m_impactForce += Fi;

                        particle.transform.position -= n * s;
                        hasCollision = true;
                    }
                }

            }

            return hasCollision;
        }

    }
}


