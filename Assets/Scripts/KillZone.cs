public class KillZone : AABBCollisionReceiver
{
    protected override void HandleCollision(AABBCollider other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            player.Respawn();
        }
    }
}
