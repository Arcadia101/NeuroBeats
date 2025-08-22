using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Moves a GameObject in a zig-zag path towards a target,
/// then continues a bit further in the same direction
/// to create an "exit zone".
/// Fires OnMovementComplete after the full path is done.
/// </summary>
public class ZigZagMover : MonoBehaviour
{
   [Header("ZigZag Settings")]
   [SerializeField] private int minSegments = 2;
   [SerializeField] private int maxSegments = 5;
   // For now we are always using just maxSegments to
   [Header("Path Shape")]
   [SerializeField, Range(0f, 85f)] private float maxTurnAngleDeg = 60f; // cone half-angle for turns
   [SerializeField] private Vector3 planeNormal = default;  // axis used for rotations (2D: (0,0,1))


   [Header("Exit Zone")]
   [Tooltip("Extra distance beyond the target, in the approach direction.")]
   [SerializeField] private float exitDistance = 1f;


   [SerializeField] private float speed = 6f; // movement speed
   private List<Vector3> path;
   private int segmentIndex;


   /// <summary>
   /// Fired after the object completes the full path (including exit zone).
   /// </summary>
   public event Action OnMovementComplete = delegate { };


  
   // ------------ Set the plane we are going to rotate around ----------- //
   /// <summary>
   /// This is a unity event based method, gets called when you add the script in the inspector or when you hit "reset" in the inspector
   /// </summary>
   private void Reset()
   {
       planeNormal = Vector3.forward; // default for 2D (XY plane)
   }


   /// <summary>
   /// Build and prepare a zig-zag path starting from the target and working backwards.
   /// </summary>
   public void Initialize(Vector3 start, Vector3 end, float travelTime)
   {
       planeNormal = Vector3.forward;
       // 1) ----------------  Calculate required total travel distance ---------------- //
       float requiredDistance = speed * travelTime;


       // 2)  ----------------  Decide number of segments and length per segment ---------------- //
       int segmentCount = maxSegments;
       float segmentLength = requiredDistance / segmentCount;


       // 3)  ----------------  Build path starting from destination ---------------- //
       path = new List<Vector3> { end }; //
       Vector3 currentWaypoint = end;
       Vector3 baseDirection = (start- end).normalized;
       Vector3 currentDirection = baseDirection;


       //Figureout where our vertical limits are
       GetVerticalBounds(out float screenTop, out float screenBottom);
       // ----------------  we start filling the path list from the destination backway to the start point  ---------------- //
       for (int i = 0; i < segmentCount; i++)
       {
          
           //Define new direction
           float angle = UnityEngine.Random.Range(-maxTurnAngleDeg, maxTurnAngleDeg); //- arbitrarily I chose +60° to -60° because why not. First one is
           //Operate the quaternion and direction we will rotate around our current direction.
           Vector3 stepDir = StepDirDefiner(angle, currentDirection); //We define a step quantity we will rotate our current direction towards.
           // We create our next waypoint
           Vector3 next = CreateNextWaypoint(currentWaypoint, stepDir, currentDirection, angle, segmentLength, screenTop, screenBottom);
           /// proceed to add it to the path
           path.Add(next);
          
           // ------------------------------------------------------------ //
          
           // Update for next iteration
           currentWaypoint = next;
           currentDirection = stepDir;
       }
       // ------------------------------------------------  we finished creating the path  ------------------------------------------------ //
      
       //Right now path is Destination → … → Spawn ; so let's reverse it
       path.Reverse(); // Reverse so path goes Spawn → … → Destination


       // Add extra exit point beyond destination
       Vector3 approachDir = (end - path[path.Count - 2]).normalized; //This is to know from where we are moving towards where.
       Vector3 exitPoint = end + approachDir * exitDistance; //In that same direction, we create another waypoint a little distance ahead. This is the exit point
       path.Add(exitPoint); //We add the exit point waypoint to the path list


       // Prepare for movement
       segmentIndex = 0; // we need a counter to move through the list.
       transform.position = path[0]; //We start counting from the Start point (remember we revesersed the list)
   }


   [Obsolete("Kept for reference. Use Initialize instead.")]
   public void Initialize_backup(Vector3 start, Vector3 end, float travelTime)
   {
       path = new List<Vector3> { start };
       Vector3 dirToTarget = (end - start).normalized;
       int segments = UnityEngine.Random.Range(minSegments, maxSegments + 1);


       for (int i = 1; i < segments; i++)
       {
           float t = (float)i / segments;
           Vector3 pt = Vector3.Lerp(start, end, t);
           Vector3 perp = Vector3.Cross(dirToTarget, Vector3.forward).normalized;
           float dev = UnityEngine.Random.Range(1f, 2f) * (UnityEngine.Random.value > 0.5f ? 1 : -1);
           pt += perp * dev;
           path.Add(pt);
       }
       path.Add(end);


       // Adjust speed based on actual distance
       float distToTarget = 0f;
       for (int i = 0; i < path.Count - 1; i++)
           distToTarget += Vector3.Distance(path[i], path[i + 1]);
       speed = distToTarget / Mathf.Max(travelTime, 0.01f);


       // Add exit point
       Vector3 approachDir = (end - path[path.Count - 2]).normalized;
       Vector3 exitPoint = end + approachDir * exitDistance;
       path.Add(exitPoint);


       segmentIndex = 0;
       transform.position = path[0];
   }


   private void Update()
   {
       //ALl this section is basically, every frame we move from current position towards the next waypoint a step defined by the speed
       if (path == null || segmentIndex >= path.Count - 1)
           return;


       float step = speed * Time.deltaTime;
       Vector3 next = path[segmentIndex + 1];
       transform.position = Vector3.MoveTowards(transform.position, next, step);


       if (Vector3.Distance(transform.position, next) < 0.01f)
       {
           segmentIndex++;
           if (segmentIndex >= path.Count - 1)
               OnMovementComplete.Invoke();
       }
   }


   [SerializeField] private float biasFactor = 0.5f;
   private Vector3 StepDirDefiner(float angle, Vector3 currentDirection) {


       Quaternion q = Quaternion.AngleAxis(angle, planeNormal.normalized); // Now this is the messy part. We create a quaternion around the vector normal to the plane we work with (XY, therefore Z), with a specified angle (selected in the line above)
       Vector3 stepDir = (q * currentDirection).normalized; // Here we rotate our current direction around the normal to the plane vector, the quantity we specified above (the quaternion) and then normalize it to ensure we have an actual direction.          
       return stepDir;
   }


   private float ReadjustAngle(float angle,float counterAngle) {
       float newAngle = Mathf.Lerp(angle, counterAngle, biasFactor);
       return newAngle;
   }


   private Vector3 CreateNextWaypoint(Vector3 currentWaypoint, Vector3 stepDir, Vector3 currentDirection, float angle, float segmentLength, float screenTop, float screenBottom){
       Vector3 next = currentWaypoint + stepDir * segmentLength;


       int safetyCount = 1;
       Debug.Log("our next waypoint Y pos is: " + next.y);
       while(next.y > screenTop || next.y < screenBottom)
       {
           if(next.y > screenTop){
               Debug.Log("Bottom limit reached at "+ angle);
               switch (currentDirection.x)
               {
                   case >0: angle = ReadjustAngle(angle, -maxTurnAngleDeg*safetyCount/2);
                       break;
                   case < 0:
                       angle = ReadjustAngle(angle, maxTurnAngleDeg*safetyCount/2);
                       break;
               }
               Debug.Log("This is new angle "+ angle);
           }
          
           if(next.y < screenBottom){
               Debug.Log("Bottom limit reached at "+ angle);
               switch (currentDirection.x)
               {
                   case >0: angle = ReadjustAngle(angle, maxTurnAngleDeg*safetyCount/2);
                       break;
                   case < 0:
                       angle = ReadjustAngle(angle, -maxTurnAngleDeg*safetyCount/2);
                       break;
               }
              
               Debug.Log("This is new angle "+ angle);
           }
          
           safetyCount++;
          
           stepDir = StepDirDefiner(angle, currentDirection);
           if (safetyCount > 6)
           {
               Debug.Log("Safety threshold met while trying to readjust the angle");
               break; //In case anything weird happens, we won't have more than 10 attempts to correct this
           }
           if (stepDir.x * currentDirection.x <= 0)
           {
               continue;
           }
          
           next = currentWaypoint + stepDir * segmentLength;
       }
        


       return next;
   }
  
  






   [SerializeField] private Transform topConstrainer;
   [SerializeField] private Transform bottomConstrainer;
   //[SerializeField] private GameObject[] constrainers;
   private void GetVerticalBounds(out float screenTop, out float screenBottom)
   {
       screenTop = 0f;
       screenBottom = 0f;
       GameObject[] constrainers = GameObject.FindGameObjectsWithTag("ScreenConstrainers");


       foreach (GameObject constrainer in constrainers)
       {
           if (constrainer.gameObject.name == "screenConstrainerTop")
           {
               topConstrainer = constrainer.transform;
               screenTop = constrainer.transform.position.y;
               //Debug.Log("Our top constrainer " +screenTop);
           }


           if (constrainer.gameObject.name == "screenConstrainerBottom")
           {
               bottomConstrainer = constrainer.transform;
               screenBottom = constrainer.transform.position.y;
              // Debug.Log("Our bottom constrainer "+screenBottom);
              
           }
       }
      
   }






   private void OnDrawGizmos()
   {
       if (path == null || path.Count < 2) return;


       // Draw connecting lines
       Gizmos.color = Color.cyan;
       for (int i = 0; i < path.Count - 1; i++)
           Gizmos.DrawLine(path[i], path[i + 1]);


       // Draw small spheres at each waypoint
       foreach (var p in path)
           Gizmos.DrawSphere(p, 0.05f);


       // Highlight exit point
       Gizmos.color = Color.magenta;
       Gizmos.DrawSphere(path[path.Count - 1], 0.18f);
      
       //Let's draw screenbounds
       GetVerticalBounds(out float screenTop, out float screenBottom);
       Gizmos.color = Color.magenta;
       Gizmos.DrawLine(new Vector3(0f, screenTop,0f), new Vector3(0f, screenBottom,0f));
   }
}


// Old System.

/*using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mueve un GameObject en zig-zag hasta el target y luego un tramo extra
/// en la misma dirección de llegada para la zona de “Good”.  
/// Dispara OnMovementComplete solo al finalizar todo el recorrido.
/// </summary>
public class ZigZagMover : MonoBehaviour
{
    [Header("ZigZag Settings")]
    [SerializeField] private float minDeviation = 1f;
    [SerializeField] private float maxDeviation = 2f;
    [SerializeField] private int minSegments = 2;
    [SerializeField] private int maxSegments = 5;

    [Header("Exit Zone")]
    [Tooltip("Distancia extra más allá del target, en la dirección de llegada.")]
    [SerializeField] private float exitDistance = 2f;

    private List<Vector3> path;
    private float speed;
    private int segmentIndex;

    /// <summary>Invocado tras completar todo el recorrido (incluida la salida).</summary>
    public event Action OnMovementComplete = delegate { };

    /// <summary>
    /// Inicializa la ruta y la velocidad.  
    /// travelTime es el tiempo para llegar hasta el target (sin contar la salida).
    /// </summary>
    public void Initialize(Vector3 start, Vector3 end, float travelTime)
    {
        // 1) Generar ruta zig-zag hasta el target
        path = new List<Vector3> { start };
        Vector3 dirToTarget = (end - start).normalized;
        int segments = UnityEngine.Random.Range(minSegments, maxSegments + 1);

        for (int i = 1; i < segments; i++)
        {
            float t = (float)i / segments;
            Vector3 pt = Vector3.Lerp(start, end, t);
            Vector3 perp = Vector3.Cross(dirToTarget, Vector3.forward).normalized;
            float dev = UnityEngine.Random.Range(minDeviation, maxDeviation) * (UnityEngine.Random.value > 0.5f ? 1 : -1);
            pt += perp * dev;
            path.Add(pt);
        }
        path.Add(end);

        // 2) Calcular velocidad en base a la distancia hasta el target
        float distToTarget = 0f;
        for (int i = 0; i < path.Count - 1; i++)
            distToTarget += Vector3.Distance(path[i], path[i + 1]);

        speed = distToTarget / Mathf.Max(travelTime, 0.01f);

        // 3) Obtener la dirección de llegada (último segmento)
        Vector3 fromPrevToTarget = (end - path[path.Count - 2]).normalized;

        // 4) Añadir punto de salida en esa misma dirección
        Vector3 exitPoint = end + fromPrevToTarget * exitDistance;
        path.Add(exitPoint);

        // 5) Preparar movimiento
        segmentIndex = 0;
        transform.position = path[0];
    }

    private void Update()
    {
        if (path == null || segmentIndex >= path.Count - 1)
            return;

        float step = speed * Time.deltaTime;
        Vector3 next = path[segmentIndex + 1];
        transform.position = Vector3.MoveTowards(transform.position, next, step);

        if (Vector3.Distance(transform.position, next) < 0.01f)
        {
            segmentIndex++;
            if (segmentIndex >= path.Count - 1)
            {
                OnMovementComplete.Invoke();
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        if (path == null || path.Count < 2) return;

        // Dibuja líneas conectando cada punto de la ruta
        Gizmos.color = Color.cyan;
        for (int i = 0; i < path.Count - 1; i++)
        {
            Gizmos.DrawLine(path[i], path[i + 1]);
        }

        // Dibuja un pequeño gizmo en cada punto
        for (int i = 0; i < path.Count; i++)
        {
            Gizmos.DrawSphere(path[i], 0.05f);
        }

        // Marca con un color distinto el punto de salida
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(path[path.Count - 1], 0.18f);
    }
}
*/
