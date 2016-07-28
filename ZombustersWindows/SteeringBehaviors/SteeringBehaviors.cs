using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
#if !WINDOWS_PHONE
using Microsoft.Xna.Framework.Storage;
#endif

namespace ZombustersWindows
{
    /// <summary>
    /// Posibles tipos de steering
    /// </summary>
    public enum BehaviorType
    {
        none = 0x00000,
        seek = 0x00002,
        flee = 0x00004,
        arrive = 0x00008,
        wander = 0x00010,
        cohesion = 0x00020,
        separation = 0x00040,
        allignment = 0x00080,
        obstacle_avoidance = 0x00100,
        wall_avoidance = 0x00200,
        follow_path = 0x00400,
        pursuit = 0x00800,
        evade = 0x01000,
        interpose = 0x02000,
        hide = 0x04000,
        flock = 0x08000,
        offset_pursuit = 0x10000,
    };

    public enum CombinationType
    {
        weighted,
        prioritized
    };

    public class SteeringBehaviors
    {
        /// <summary>
        /// Behaviors que van a ser ejecutados en el proceso update
        /// </summary>
        private List<BehaviorType> _behaviors;

        /// <summary>
        /// Mundo en el que se ejecutan los steering behaviors
        /// </summary>
        private GameWorld _gameWorld;

        private Vector2 _steeringForce;

        /// <summary>
        /// Fuerza máxima que puede llegar a alcanzar el behavior
        /// </summary>
        private float _maxForce;

        public float MaxForce
        {
            get
            {
                return _maxForce;
            }
        }

        //private Seek _seek;
        //private Flee _flee;
        private Pursuit _pursuit;
        private Arrive _arrive;
        //private Evade _evade;
        //private Wander _wander;
        private ObstacleAvoidance _obstacleAvoidance;
        private CombinationType _combinationType;

        private float weightObstacleAvoidance = 15.0f;
        //private float weightSeek = 0.22f;
        //private float weightEvade = 0.22f;
        //private float weightFlee = 0.22f;
        private float weightArrive = 0.22f;
        private float weightPursuit = 0.22f;
        //private float weightWander = 0.22f;

        /// <summary>
        /// Fuerza del steering creada por la combinación en la ejecución de todos los steerings
        /// </summary>
        public Vector2 SteeringForce
        {
            get
            {
                return _steeringForce;
            }
        }

/*
        /// <summary>
        /// Geeter del steering de tipo Evade
        /// </summary>
        public Evade Evade
        {
            get
            {
                if (_evade == null && this.HasBehavior(BehaviorType.evade))
                    _evade = new Evade(10.0f);

                return _evade;
            }
            set
            {
                _evade = value;
            }
        }

        /// <summary>
        /// Geeter del steering de tipo Flee
        /// </summary>
        public Flee Flee
        {
            get
            {
                if (_flee == null && this.HasBehavior(BehaviorType.flee))
                    _flee = new Flee(null);

                return _flee;
            }
            set
            {
                _flee = value;
            }
        }*/

        /// <summary>
        /// Geeter del steering de tipo Arrive
        /// </summary>
        public Arrive Arrive
        {
            get
            {
                if (_arrive == null && this.HasBehavior(BehaviorType.arrive))
                    _arrive = new Arrive(Arrive.Deceleration.normal, 10.0f);

                return _arrive;
            }
            set
            {
                _arrive = value;
            }
        }
        /*
        /// <summary>
        /// Geeter del steering de tipo Seek
        /// </summary>
        public Seek Seek
        {
            get
            {
                if (_seek == null && this.HasBehavior(BehaviorType.seek))
                    _seek = new Seek();

                return _seek;
            }
            set
            {
                _seek = value;
            }
        }
*/
        /// <summary>
        /// Geeter del steering de tipo Pursuit
        /// </summary>
        public Pursuit Pursuit
        {
            get
            {
                if (_pursuit == null && this.HasBehavior(BehaviorType.pursuit))
                    _pursuit = new Pursuit(Arrive.Deceleration.normal, 10.0f);

                return _pursuit;
            }
            set
            {
                _pursuit = value;
            }
        }

/*
        /// <summary>
        /// Geeter del steering de tipo Wander
        /// </summary>
        public Wander Wander
        {
            get
            {
                if (_wander == null && this.HasBehavior(BehaviorType.wander))
                    _wander = new Wander(50.0d, 100.0f, 50.0f, Arrive.Deceleration.normal, 10.0f);

                return _wander;
            }
            set
            {
                _wander = value;
            }
        }
*/
        /// <summary>
        /// Geeter del steering de tipo ObstacleAvoidance
        /// </summary>
        public ObstacleAvoidance ObstacleAvoidance
        {
            get
            {
                if (_obstacleAvoidance == null && this.HasBehavior(BehaviorType.obstacle_avoidance))
                {
                    GameWorld gameWorld = new GameWorld(false, false);

                    _obstacleAvoidance = new ObstacleAvoidance(ref gameWorld, 10.0f);
                }

                return _obstacleAvoidance;
            }
            set
            {
                _obstacleAvoidance = value;
            }
        }

        /// <summary>
        /// Indica si contiene un comportamiento concreto
        /// </summary>
        /// <param name="type">Tipo de comportamiento a comprobar</param>
        /// <returns></returns>
        public bool HasBehavior(BehaviorType type)
        {
            /*
            BehaviorType? match = _behaviors.Find(b => b == type);

            return match.HasValue && match.Value.ToString() != "none";
             */
            if (_behaviors.Contains(type))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

/*
        /// <summary>
        /// Añade el comportamiento de Seek al behavior. Todos los comportamientos que se
        /// añadan van a ser ejecutados en el método UPDATE
        /// </summary>
        /// <param name="steering">Steering a añadir</param>
        public void AddBehavior(Seek steering)
        {
            if (!this.HasBehavior(steering.Type))
                _behaviors.Add(steering.Type);

            this._seek = steering;
        }

        /// <summary>
        /// Añade el comportamiento de Flee al behavior. Todos los comportamientos que se
        /// añadan van a ser ejecutados en el método UPDATE
        /// </summary>
        /// <param name="steering">Steering a añadir</param>
        public void AddBehavior(Flee steering)
        {
            if (!this.HasBehavior(steering.Type))
                _behaviors.Add(steering.Type);

            this._flee = steering;
        }
*/

        /// <summary>
        /// Añade el comportamiento de Pursuit al behavior. Todos los comportamientos que se
        /// añadan van a ser ejecutados en el método UPDATE
        /// </summary>
        /// <param name="steering">Steering a añadir</param>
        public void AddBehavior(Pursuit steering)
        {
            if (!this.HasBehavior(steering.Type))
                _behaviors.Add(steering.Type);

            this._pursuit = steering;
        }


        /// <summary>
        /// Añade el comportamiento de Arrive al behavior. Todos los comportamientos que se
        /// añadan van a ser ejecutados en el método UPDATE
        /// </summary>
        /// <param name="steering">Steering a añadir</param>
        public void AddBehavior(Arrive steering)
        {
            if (!this.HasBehavior(steering.Type))
                _behaviors.Add(steering.Type);

            this._arrive = steering;
        }
/*
        /// <summary>
        /// Añade el comportamiento de Evade al behavior. Todos los comportamientos que se
        /// añadan van a ser ejecutados en el método UPDATE
        /// </summary>
        /// <param name="steering">Steering a añadir</param>
        public void AddBehavior(Evade steering)
        {
            if (!this.HasBehavior(steering.Type))
                _behaviors.Add(steering.Type);

            this._evade = steering;
        }

        /// <summary>
        /// Añade el comportamiento de Wander al behavior. Todos los comportamientos que se
        /// añadan van a ser ejecutados en el método UPDATE
        /// </summary>
        /// <param name="steering">Steering a añadir</param>
        public void AddBehavior(Wander steering)
        {
            if (!this.HasBehavior(steering.Type))
                _behaviors.Add(steering.Type);

            this._wander = steering;
        }
 */

        /// <summary>
        /// Añade el comportamiento de ObstacleAvoidance al behavior. Todos los comportamientos que se
        /// añadan van a ser ejecutados en el método UPDATE
        /// </summary>
        /// <param name="steering">Steering a añadir</param>
        public void AddBehavior(ObstacleAvoidance steering)
        {
            if (!this.HasBehavior(steering.Type))
                _behaviors.Add(steering.Type);

            this._obstacleAvoidance = steering;
        }


        /// <summary>
        /// Inicialización de la clase
        /// </summary>
        public SteeringBehaviors(float maxForce, CombinationType combinationType)
        {
            this._behaviors = new List<BehaviorType>();
            this._maxForce = maxForce;
            this._combinationType = combinationType;
        }

        /// <summary>
        /// Inicialización de la clase con info adicional de gameworld
        /// </summary>
        public SteeringBehaviors(float maxForce, GameWorld gameWorld, CombinationType combinationType)
        {
            this._behaviors = new List<BehaviorType>();
            this._maxForce = maxForce;
            this._gameWorld = gameWorld;
            this._combinationType = combinationType;
        }

        /// <summary>
        /// Aplica toda la fuerza necesaria a la entidad teniendo en cuenta la cantidad restante que queda para aplicar
        /// </summary>
        /// <returns></returns>
        private bool AccumulateForce(ref Vector2 RunningTot, Vector2 ForceToAdd, SteeringEntity entity)
        {

            //calculate how much steering force the vehicle has used so far
            float MagnitudeSoFar = RunningTot.Length();

            //calculate how much steering force remains to be used by this vehicle
            float MagnitudeRemaining = this._maxForce - MagnitudeSoFar;

            //return false if there is no more force left to use
            if (MagnitudeRemaining <= 0.0) return false;

            //calculate the magnitude of the force we want to add
            float MagnitudeToAdd = ForceToAdd.Length();

            //if the magnitude of the sum of ForceToAdd and the running total
            //does not exceed the maximum force available to this vehicle, just
            //add together. Otherwise add as much of the ForceToAdd vector is
            //possible without going over the max.
            if (MagnitudeToAdd < MagnitudeRemaining)
            {
                RunningTot += ForceToAdd;
            }

            else
            {
                //add it to the steering force
                RunningTot += (Vector2.Normalize(ForceToAdd) * MagnitudeRemaining);
            }

            return true;
        }

        /// <summary>
        /// Realiza el cálculo de los distintos steerings
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="entity"></param>
        public Vector2 Update(GameTime gameTime, SteeringEntity entity)
        {
            this._steeringForce = Vector2.Zero;

            switch (_combinationType)
            {
                case CombinationType.prioritized:

                    #region Prioritized

                    Vector2 force = Vector2.Zero;

                    if (this.HasBehavior(BehaviorType.obstacle_avoidance))
                    {
                        force = this._obstacleAvoidance.CalculateSteering(entity) * this.weightObstacleAvoidance;

                        if (!AccumulateForce(ref this._steeringForce, force, entity)) return this._steeringForce;
                    }

                    /*
                    if (this.HasBehavior(BehaviorType.seek))
                    {
                        force = this._seek.CalculateSteering(entity) * this.weightSeek;

                        if (!AccumulateForce(ref this._steeringForce, force, entity)) return this._steeringForce;
                    }

                    if (this.HasBehavior(BehaviorType.flee))
                    {
                        force = this._flee.CalculateSteering(entity) * this.weightFlee;

                        if (!AccumulateForce(ref this._steeringForce, force, entity)) return this._steeringForce;
                    }*/

                    if (this.HasBehavior(BehaviorType.arrive))
                    {
                        force = this._arrive.CalculateSteering(entity) * this.weightArrive;

                        if (!AccumulateForce(ref this._steeringForce, force, entity)) return this._steeringForce;
                    }

                    if (this.HasBehavior(BehaviorType.pursuit))
                    {
                        force = this._pursuit.CalculateSteering(entity) * this.weightPursuit;

                        if (!AccumulateForce(ref this._steeringForce, force, entity)) return this._steeringForce;
                    }

                    /*if (this.HasBehavior(BehaviorType.evade))
                    {
                        force = this._evade.CalculateSteering(entity) * this.weightEvade;

                        if (!AccumulateForce(ref this._steeringForce, force, entity)) return this._steeringForce;
                    }

                    if (this.HasBehavior(BehaviorType.wander))
                    {
                        this._wander._gameTime = gameTime;
                        force = this._wander.CalculateSteering(entity) * this.weightWander;

                        if (!AccumulateForce(ref this._steeringForce, force, entity)) return this._steeringForce;
                    }*/

                    #endregion

                    break;

                case CombinationType.weighted:

                    #region Weighted

                    /*
                    if (this.HasBehavior(BehaviorType.seek))
                        this._steeringForce += this._seek.CalculateSteering(entity) * this.weightSeek;

                    if (this.HasBehavior(BehaviorType.flee))
                        this._steeringForce += this._flee.CalculateSteering(entity) * this.weightFlee;
                    */
                    if (this.HasBehavior(BehaviorType.arrive))
                        this._steeringForce += this._arrive.CalculateSteering(entity) * this.weightArrive;
                    
                    if (this.HasBehavior(BehaviorType.pursuit))
                        this._steeringForce += this._pursuit.CalculateSteering(entity) * this.weightPursuit;
                    /*
                    if (this.HasBehavior(BehaviorType.evade))
                        this._steeringForce += this._evade.CalculateSteering(entity) * this.weightEvade;

                    if (this.HasBehavior(BehaviorType.wander))
                    {
                        this._wander._gameTime = gameTime;
                        this._steeringForce += this._wander.CalculateSteering(entity) * this.weightWander;
                    }
                    */
                    if (this.HasBehavior(BehaviorType.obstacle_avoidance))
                        this._steeringForce += this._obstacleAvoidance.CalculateSteering(entity) * this.weightObstacleAvoidance;

                    // Devolvemos la fuerza producira por el steering truncada a la limitación del valor máximo
                    this._steeringForce = VectorHelper.TruncateVector(this._steeringForce, this._maxForce);

                    #endregion

                    break;
            }

            return this._steeringForce;
        }
    }
}