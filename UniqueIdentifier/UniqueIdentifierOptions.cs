using System.ComponentModel.DataAnnotations;

namespace UniqueIdentifier
{
    public class UniqueIdentifierOptions
    {
        public const int MaxServiceInstanceBits = 15;

        private DateTime epoch;
        private int serviceIdBits = 7;
        private int instanceIdBits = 8;

        public Func<DateTime> GetUtcNow { get; set; } = () => DateTime.UtcNow;

        public DateTime Epoch
        {
            get => this.epoch;
            set
            {
                if (value > DateTime.Today)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        value,
                        "Epoch date must not be in the future.");
                }

                this.epoch = value;
            }
        }

        public ulong ServiceId { get; init; } = 0UL;

        public ulong InstanceId { get; init; } = 0UL;

        public int ServiceIdBits
        {
            get => this.serviceIdBits;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        value,
                        $"Service Id bits can't be less than zero.");
                }

                this.serviceIdBits = value;
            }
        }

        public int InstanceIdBits
        {
            get => this.instanceIdBits;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        value,
                        $"Instance Id bits can't be less than zero.");
                }

                this.instanceIdBits = value;
            }
        }

        public ulong MaxServiceId => (ulong)(-1L ^ (-1L << this.ServiceIdBits));

        public ulong MaxInstanceId => (ulong)(-1L ^ (-1L << this.InstanceIdBits));

        public void Validate()
        {
            if (this.ServiceIdBits + this.InstanceIdBits != MaxServiceInstanceBits)
            {
                throw new ValidationException(
                    $"Service Id '{this.ServiceIdBits}' and Instance Id '{this.InstanceIdBits}' bits must be equal to {MaxServiceInstanceBits}.");
            }

            if (this.ServiceId > this.MaxServiceId)
            {
                throw new ValidationException(
                    $"Service Id '{this.ServiceId}' can't be greater than {this.MaxServiceId}.");
            }

            if (this.InstanceId > this.MaxInstanceId)
            {
                throw new ValidationException(
                    $"Instance Id '{this.InstanceId}' can't be greater than {this.MaxInstanceId}.");
            }
        }
    }
}