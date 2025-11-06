using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FlowAgent : MonoBehaviour
{
    [Header("Motion")]
    public float moveSpeed = 3f;                 // world units / s
    [Range(0.02f, 0.3f)] public float updateRate = 0.06f; // dir update rate
    [Tooltip("How quickly the agent's actual direction follows the desired direction")]
    public float steerSmooth = 10f;              // higher => faster rotation toward desired

    [Header("Footprint / Probe")]
    public float radius = 0.5f;                  // agent yarıçapı
    [Tooltip("Distance ahead to sample the flow (prefer forward smoothing)")]
    public float forwardProbe = 0.5f;            // default grid size 0.5f*
    [Range(0f, 1f)] public float forwardWeight = 0.35f; // centerWeight = 1 - forwardWeight

    [Header("Obstacle Avoidance")]
    public LayerMask obstacleMask;               // collider layer
    public float avoidanceCastExtra = 0.05f;     // circlecast dist
    [Tooltip("How strongly agent steers away from obstacle normal (0..2)")]
    public float avoidanceStrength = 1.2f;

    [Header("Stuck Handling")]
    public float stuckThreshold = 0.02f;         
    public float stuckTimeToReact = 2.0f;       
    public float randomJitterStrength = 0.35f;  

    [Header("Debug")]
    public bool debug = false;
    public Color flowColor = Color.cyan;
    public Color probeColor = Color.yellow;
    public Color avoidColor = Color.magenta;

    private FlowFieldController controller;
    private Rigidbody2D rb;
    private Vector2 currentDir = Vector2.up;    // Moving Dir
    private Vector2 desiredDir = Vector2.up;    // Target Dir (blend)
    private float nextUpdateTime = 0f;

    private Vector2 lastPos;
    private float stuckTimer = 0f;

    void Start()
    {
        controller = FlowFieldController.Instance;
        rb = GetComponent<Rigidbody2D>();

        if (controller == null)
        {
            Debug.LogError($"[FlowAgent] Controller bulunamadı! ({name})");
            enabled = false;
            return;
        }

        // Start Dir
        Vector3 initFlow = controller.GetFlowDirectionAtPosition(transform.position);
        currentDir = initFlow != Vector3.zero ? new Vector2(initFlow.x, initFlow.y).normalized : Vector2.right;
        desiredDir = currentDir;
        lastPos = rb.position;

        // safety defaults
        if (forwardProbe <= 0f) forwardProbe = Mathf.Max(0.25f, controller.grid != null ? controller.grid.cellSize * 0.5f : 0.5f);
    }

    void Update()
    {
        if (controller == null || controller.grid == null) return;

        // stuck detection (Basic)
        float moved = Vector2.Distance(rb.position, lastPos);
        if (moved < stuckThreshold)
        {
            stuckTimer += Time.fixedDeltaTime;
        }
        else
        {
            stuckTimer = 0f;
        }
        lastPos = rb.position;

        if (Time.time >= nextUpdateTime)
        {
            UpdateDesiredDirection();
            nextUpdateTime = Time.time + updateRate;
        }

        // if stuck for a while -> small random jitter added to desiredDir (no teleport)
        if (stuckTimer >= stuckTimeToReact)
        {
            desiredDir += Random.insideUnitCircle * randomJitterStrength;
            desiredDir.Normalize();
            stuckTimer = 0f;
            if (debug) Debug.Log($"[FlowAgent] stuck jitter: {name}");
        }

        // smooth steering: interpolate currentDir -> desiredDir
        float t = 1f - Mathf.Exp(-steerSmooth * Time.fixedDeltaTime);
        currentDir = Vector2.Lerp(currentDir, desiredDir, t).normalized;

        if(moveSpeed > 0)
            rb.MovePosition(rb.position + currentDir * moveSpeed * Time.fixedDeltaTime);

        if (debug)
        {
            Debug.DrawRay(transform.position, new Vector3(currentDir.x, currentDir.y, 0) * 0.8f, flowColor);
        }
    }

    void UpdateDesiredDirection()
    {
        Vector2 pos2 = rb.position;

        // 1) center flow
        Vector3 cflow3 = controller.GetFlowDirectionAtPosition(pos2);
        Vector2 centerFlow = (cflow3 == Vector3.zero) ? Vector2.zero : new Vector2(cflow3.x, cflow3.y).normalized;

        // 2) forward probe flow (sample a bit ahead according to centerFlow; if centerFlow zero, use target direction)
        Vector2 forwardSamplePos;
        if (centerFlow.sqrMagnitude > 0.0001f)
            forwardSamplePos = pos2 + centerFlow * forwardProbe;
        else if (controller.target != null)
            forwardSamplePos = pos2 + ((Vector2)controller.target.position - pos2).normalized * forwardProbe;
        else
            forwardSamplePos = pos2;

        Vector3 fflow3 = controller.GetFlowDirectionAtPosition(forwardSamplePos);
        Vector2 forwardFlow = (fflow3 == Vector3.zero) ? Vector2.zero : new Vector2(fflow3.x, fflow3.y).normalized;

        // 3) blend (center heavier)
        float centerWeight = Mathf.Clamp01(1f - forwardWeight);
        Vector2 blended = centerFlow * centerWeight + forwardFlow * forwardWeight;
        if (blended == Vector2.zero && controller.target != null)
        {
            blended = ((Vector2)controller.target.position - pos2).normalized;
        }

        // 4) simple physics avoidance: circlecast ahead in blended direction
        Vector2 dirToCast = blended.normalized;
        if (dirToCast == Vector2.zero) dirToCast = currentDir != Vector2.zero ? currentDir : Vector2.up;

        float castDistance = radius + avoidanceCastExtra + 0.05f; // small forward check
        RaycastHit2D hit = Physics2D.CircleCast(pos2, radius, dirToCast, castDistance, obstacleMask);
        if (hit.collider != null)
        {
            // compute steer: take perpendicular of hit normal and pick side that leads toward blended/target
            Vector2 hitNormal = hit.normal;
            Vector2 perp = new Vector2(-hitNormal.y, hitNormal.x); // perpendicular
            // choose sign of perp that points closer to blended direction
            if (Vector2.Dot(perp, blended) < 0) perp = -perp;
            Vector2 avoidance = perp * avoidanceStrength;
            desiredDir = (blended + avoidance).normalized;

            if (debug)
            {
                Debug.DrawRay(pos2, dirToCast * (castDistance), Color.red, updateRate);
                Debug.DrawRay(hit.point, new Vector3(hitNormal.x, hitNormal.y, 0) * 0.4f, Color.magenta, updateRate);
            }
            return;
        }

        // no obstacle: use blended
        desiredDir = blended.normalized;
    }

    private void OnDrawGizmosSelected()
    {
        if (!debug) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(desiredDir.x, desiredDir.y, 0) * 0.6f);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(currentDir.x, currentDir.y, 0) * 0.6f);
    }
}
