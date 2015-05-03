namespace DVRPTaskSolver
{
    public class DVRPData
    {
        public int DepotsCount;
        public Depot[] Depots;

        public int RequestsCount;
        public Request[] Requests;

        public int VehicleCount;
        public int VehicleCapacity;
        public double VehicleSpeed;

        public static DVRPData GetFromBytes(byte[] bytes)
        {
            return new DVRPData();
        }
    }
}
