namespace Adriva.Extensions.Worker
{
    public readonly struct LockStatus
    {
        public string RunningInstanceId { get; }

        public bool HasLock { get; }

        public LockStatus(string runningInstanceId, bool hasLock)
        {
            this.RunningInstanceId = runningInstanceId;
            this.HasLock = hasLock;
        }
    }
}
