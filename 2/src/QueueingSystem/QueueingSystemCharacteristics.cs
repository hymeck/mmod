using Ardalis.GuardClauses;

namespace QueueingSystem
{
    /// <summary>
    /// Contains values describing queueing system. 
    /// </summary>
    public class QueueingSystemCharacteristics
    {
        public QueueingSystemCharacteristics(double arrivalRate, double serviceRate, int serverCount, int queueCapacity, double waitingTime)
        {
            ArrivalRate = Guard.Against.Negative(arrivalRate, nameof(arrivalRate));
            ServiceRate = Guard.Against.Negative(serviceRate, nameof(serviceRate));
            ServerCount = Guard.Against.NegativeOrZero(serverCount, nameof(serverCount));
            QueueCapacity = Guard.Against.NegativeOrZero(queueCapacity, nameof(queueCapacity));
            WaitingTime = Guard.Against.Negative(waitingTime, nameof(waitingTime));
        }

        /// <summary>
        /// Mean rate of arrival.
        /// </summary>
        public double ArrivalRate { get; }
        
        /// <summary>
        /// Mean service rate.
        /// </summary>
        public double ServiceRate { get; }
        
        /// <summary>
        /// Number of servers in a system.
        /// </summary>
        public int ServerCount { get; }
        
        /// <summary>
        /// Capacity of a queue.
        /// </summary>
        public int QueueCapacity { get; }
        
        /// <summary>
        /// Mean of waiting time.
        /// </summary>
        public double WaitingTime { get; }
    }
}
