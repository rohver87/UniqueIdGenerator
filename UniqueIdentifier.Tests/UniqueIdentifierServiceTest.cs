namespace UniqueIdentifier.Tests
{
    public class UniqueIdentifierServiceTest
    {
        private static readonly DateTime DefaultEpoch = new DateTime(2022, 10, 2);

        [Fact]
        public void GenerateId_TwoDifferentServices_ProduceDifferentIdsAtSameGivenTime()
        {
            var nowDateTime = new DateTime(2022, 10, 3);
            var options1 = new UniqueIdentifierOptions()
            {
                Epoch = DefaultEpoch,
                ServiceId = 1,
                GetUtcNow = () => nowDateTime,
            };
            var options2 = new UniqueIdentifierOptions()
            {
                Epoch = DefaultEpoch,
                ServiceId = 2,
                GetUtcNow = () => nowDateTime,
            };

            IUniqueIdentifierService svc1 = new UniqueIdentifierService(options1);
            IUniqueIdentifierService svc2 = new UniqueIdentifierService(options2);

            var id1 = svc1.GenerateUniqueId();
            var id2 = svc2.GenerateUniqueId();

            Assert.NotEqual(id1, id2);
        }

        [Fact]
        public void GenerateId_TwoDifferentInstancesOfSameService_ProduceDifferentIdsAtSameGivenTime()
        {
            var nowDateTime = new DateTime(2022, 10, 3);
            var options1 = new UniqueIdentifierOptions()
            {
                Epoch = DefaultEpoch,
                ServiceId = 1,
                InstanceId = 1,
                GetUtcNow = () => nowDateTime,
            };
            var options2 = new UniqueIdentifierOptions()
            {
                Epoch = DefaultEpoch,
                ServiceId = 1,
                InstanceId = 2,
                GetUtcNow = () => nowDateTime,
            };
            IUniqueIdentifierService svc1 = new UniqueIdentifierService(options1);
            IUniqueIdentifierService svc2 = new UniqueIdentifierService(options2);

            var id1 = svc1.GenerateUniqueId();
            var id2 = svc2.GenerateUniqueId();

            Assert.NotEqual(id1, id2);
        }

        [Fact]
        public void GenerateId_DifferentThreadsInParallel_ProduceDifferentIds()
        {
            var nowDateTime = new DateTime(2022, 10, 3);
            var options1 = new UniqueIdentifierOptions()
            {
                Epoch = DefaultEpoch,
                ServiceId = 1,
                InstanceId = 1,
                GetUtcNow = () => nowDateTime,
            };
            List<ulong> ids = new List<ulong>();

            IUniqueIdentifierService svc1 = new UniqueIdentifierService(options1);

            Parallel.For(0,20,i => ids.Add(svc1.GenerateUniqueId()));

            List<ulong> idsUnique = ids.Distinct().ToList();

            Assert.Equal(ids.Count, idsUnique.Count);
        }

        [Fact]
        public void GenerateId_TwoDifferentServices_CorrectOrder_HigherTimeValueHigherIdValue()
        {
            var nowDateTime1 = new DateTime(2022, 10, 3, 0, 0, 0, 1);
            var nowDateTime2 = new DateTime(2022, 10, 3, 0, 0, 0, 2);

            var options1 = new UniqueIdentifierOptions()
            {
                Epoch = DefaultEpoch,
                ServiceId = 1,
                GetUtcNow = () => nowDateTime2,
            };
            var options2 = new UniqueIdentifierOptions()
            {
                Epoch = DefaultEpoch,
                ServiceId = 2,
                GetUtcNow = () => nowDateTime1,
            };
            IUniqueIdentifierService svc1 = new UniqueIdentifierService(options1);
            IUniqueIdentifierService svc2 = new UniqueIdentifierService(options2);

            var id1 = svc1.GenerateUniqueId();
            var id2 = svc2.GenerateUniqueId();

            Assert.True(id1 > id2);
        }

        [Fact]
        public void GenerateId_Default_GeneratesSequentialIds()
        {
            var options = new UniqueIdentifierOptions()
            {
                Epoch = DefaultEpoch,
            };

            IUniqueIdentifierService svc = new UniqueIdentifierService(options);

            ulong? id = null;
            ulong? lastId = null;
            for (var i = 0; i < 1_000_000; i++)
            {
                lastId = id;
                id = svc.GenerateUniqueId();

                if (lastId.HasValue)
                {
                    Assert.True(id.Value > lastId.Value);
                }
            }
        }

        [Fact]
        public void GenerateId_Validate_TimeStamp_ServiceId_InstanceId()
        {
            var nowDateTime = new DateTime(2022, 10, 3);

            var options = new UniqueIdentifierOptions()
            {
                Epoch = DefaultEpoch,
                ServiceId = 11,
                InstanceId = 123,
                GetUtcNow = () => nowDateTime,
            };
            IUniqueIdentifierService svc = new UniqueIdentifierService(options);

            var id = svc.GenerateUniqueId();

            var idComponents = svc.GetIdComponentsFromGeneratedId(id);

            //Validations
            var dateTime = DefaultEpoch.AddMilliseconds(idComponents.TimeStamp);
            Assert.Equal(nowDateTime, dateTime);

            Assert.Equal(11UL, idComponents.ServiceId);
            Assert.Equal(123UL, idComponents.InstanceId);
        }
    }
}