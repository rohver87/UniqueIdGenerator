using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniqueIdentifier
{

    public class UniqueIdentifierService: IUniqueIdentifierService
    {
        private const int SequenceBits = 8;
        private readonly int ServiceIdShift;
        private readonly int InstanceIdShift;
        private readonly int TimestampLeftShift;
        private readonly ulong SequenceMask;
        private ulong lastTimestamp;
        private ulong sequence;
        private readonly object lockObj = new object();

        public UniqueIdentifierService(UniqueIdentifierOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);
            options.Validate();

            this.Options = options;

            InstanceIdShift = SequenceBits;
            ServiceIdShift = SequenceBits + this.Options.InstanceIdBits;
            TimestampLeftShift = SequenceBits + this.Options.InstanceIdBits + this.Options.ServiceIdBits;
            SequenceMask = -1L ^ (-1L << SequenceBits);
        }

        public UniqueIdentifierOptions Options { get; }

        public ulong GenerateUniqueId()
        {
            lock (lockObj)
            {
                var timestamp = this.GetTimestampInMilliseconds();

                if (timestamp < this.lastTimestamp)
                {
                    throw new InvalidOperationException(
                        $"Clock moved backwards. Refusing to generate Id for {this.lastTimestamp - timestamp} milliseconds.");
                }

                if (this.lastTimestamp == timestamp)
                {
                    this.sequence = (this.sequence + 1UL) & SequenceMask;

                    if (this.sequence == 0UL)
                    {
                        timestamp = this.GetNextMillisecond(this.lastTimestamp);
                    }
                }
                else
                {
                    this.sequence = 0UL;
                }
                this.lastTimestamp = timestamp;

                return (timestamp << TimestampLeftShift) |
                   (this.Options.ServiceId << ServiceIdShift) |
                   (this.Options.InstanceId << InstanceIdShift) |
                   this.sequence;
            }
        }

        public IdComponents GetIdComponentsFromGeneratedId(ulong id)
        {
            return new IdComponents()
            {
                TimeStamp = GetTimestampValue(id),
                ServiceId  = GetServiceIdValue(id),
                InstanceId  = GetServiceInstanceIdValue(id)
            };
        }

        private ulong GetTimestampValue(ulong id)
        {
            return id >> TimestampLeftShift;
        }
        private ulong GetServiceIdValue(ulong id)
        {
            var shiftBits = 64 - TimestampLeftShift;
            var timestampShift = id << shiftBits;
            return timestampShift >> (shiftBits + ServiceIdShift);
        }

        private ulong GetServiceInstanceIdValue(ulong id)
        {
            var shiftBits = 64 - TimestampLeftShift + this.Options.ServiceIdBits;
            var timestampShift = id << shiftBits;
            return timestampShift >> (shiftBits + InstanceIdShift);
        }

        private ulong GetNextMillisecond(ulong lastTimestamp)
        {
            var timestamp = this.GetTimestampInMilliseconds();
            while (timestamp <= lastTimestamp)
            {
                timestamp = this.GetTimestampInMilliseconds();
            }

            return timestamp;
        }

        private ulong GetTimestampInMilliseconds() =>
            (ulong)(this.Options.GetUtcNow() - this.Options.Epoch).TotalMilliseconds;
    }
}
