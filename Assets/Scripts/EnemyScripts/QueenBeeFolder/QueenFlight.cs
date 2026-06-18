using UnityEngine;

public class QueenFlight : EnemyBehaviour
{
    private BossQueen queen;
    private int currentWaypointIndex = 0;

    public override void Init(Enemy enemy)
    {
        base.Init(enemy);
        queen = (BossQueen)enemy;
    }

    public override void execute()
    {
        if (queen.waypoints.Length < 2) return;

        Transform target = queen.waypoints[currentWaypointIndex];
        transform.position = Vector3.MoveTowards(transform.position, target.position, queen.speed * Time.deltaTime);

        RotateTowards(target.position);

        if (Vector3.Distance(transform.position, target.position) < 0.5f)
            currentWaypointIndex = (currentWaypointIndex + 1) % queen.waypoints.Length;
    }

    private void RotateTowards(Vector3 target)
    {
        Vector3 dir = (target - transform.position).normalized;
        if (dir != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
            Quaternion offset = Quaternion.Euler(0, -90, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot * offset, Time.deltaTime * 5f);
        }
    }
}