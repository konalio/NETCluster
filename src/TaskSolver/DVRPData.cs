using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace DVRPTaskSolver
{
    public class DVRPData
    {
        public string ProblemName;

        public int DepotsCount;
        public List<Depot> Depots;

        public int RequestsCount;
        public List<Request> Requests;

        public int VehicleCount;
        public int VehicleCapacity;
        public double VehicleSpeed;

        private struct Location
        {
            public int Id;
            public Point LocationPoint;
        }

        public static DVRPData GetFromBytes(byte[] bytes)
        {
            var vrpFile = Encoding.UTF8.GetString(bytes);
            var result = new DVRPData();
            var numLocations = 0;
            var locations = new List<Location>();
            var linesOfVrp = vrpFile.Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < linesOfVrp.Length; i++)
            {
                var lineElems = linesOfVrp[i].Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                switch (lineElems[0])
                {
                    case "NAME:":
                        result.ProblemName = lineElems[1];
                        break;
                    case "NUM_DEPOTS:":
                        result.DepotsCount = int.Parse(lineElems[1]);
                        break;
                    case "NUM_VISITS:":
                        result.RequestsCount = int.Parse(lineElems[1]);
                        break;
                    case "NUM_LOCATIONS:":
                        numLocations = int.Parse(lineElems[1]);
                        break;
                    case "NUM_VEHICLES:":
                        result.VehicleCount = int.Parse(lineElems[1]);
                        break;
                    case "CAPACITIES:":
                        result.VehicleCapacity = int.Parse(lineElems[1]);
                        break;
                    case "DEPOTS":
                        {
                            i++;
                            result.Depots = new List<Depot>();
                            for (var j = 0; j < result.DepotsCount; j++, i++)
                            {
                                var depoData = linesOfVrp[i].Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                var depoId = int.Parse(depoData[0]);
                                result.Depots.Add(new Depot { Id = depoId });
                            }
                            i--;
                            break;
                        }
                    case "DEMAND_SECTION":
                        {
                            i++;
                            result.Requests = new List<Request>();
                            for (var j = 0; j < result.RequestsCount; j++, i++)
                            {
                                var requestData = linesOfVrp[i].Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                var requestId = int.Parse(requestData[0]);
                                var requestWeight = int.Parse(requestData[1]);
                                result.Requests.Add(new Request { Id = requestId, Quantity = requestWeight });
                            }
                            i--;
                            break;
                        }
                    case "LOCATION_COORD_SECTION":
                        {
                            i++;
                            for (var j = 0; j < numLocations; j++, i++)
                            {
                                var locationData = linesOfVrp[i].Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                var locationId = int.Parse(locationData[0]);
                                var locationX = int.Parse(locationData[1]);
                                var locationY = int.Parse(locationData[2]);
                                locations.Add(new Location { Id = locationId, LocationPoint = new Point { X = locationX, Y = locationY } });
                            }
                            i--;
                            break;
                        }
                    case "DEPOT_LOCATION_SECTION":
                        {
                            i++;
                            for (var j = 0; j < result.DepotsCount; j++, i++)
                            {
                                var depoLocationData = linesOfVrp[i].Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                var depoId = int.Parse(depoLocationData[0]);
                                var locationId = int.Parse(depoLocationData[1]);
                                var depoIndex = result.Depots.FindIndex(x => x.Id == depoId);
                                var locationIndex = locations.FindIndex(x => x.Id == locationId);
                                result.Depots[depoIndex].Location = locations[locationIndex].LocationPoint;
                            }
                            i--;
                            break;
                        }
                    case "VISIT_LOCATION_SECTION":
                        {
                            i++;
                            for (var j = 0; j < result.RequestsCount; j++, i++)
                            {
                                var requestLocationData = linesOfVrp[i].Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                var requestId = int.Parse(requestLocationData[0]);
                                var locationId = int.Parse(requestLocationData[1]);
                                var requestIndex = result.Requests.FindIndex(x => x.Id == requestId);
                                var locationIndex = locations.FindIndex(x => x.Id == locationId);
                                result.Requests[requestIndex].Location = locations[locationIndex].LocationPoint;
                            }
                            i--;
                            break;
                        }
                    case "DURATION_SECTION":
                        {
                            i++;
                            for (var j = 0; j < result.RequestsCount; j++, i++)
                            {
                                var requestDurationData = linesOfVrp[i].Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                var requestId = int.Parse(requestDurationData[0]);
                                var duration = int.Parse(requestDurationData[1]);
                                var requestIndex = result.Requests.FindIndex(x => x.Id == requestId);
                                result.Requests[requestIndex].UnloadDuration = duration;
                            }
                            i--;
                            break;
                        }
                    case "DEPOT_TIME_WINDOW_SECTION":
                        {
                            i++;
                            for (var j = 0; j < result.DepotsCount; j++, i++)
                            {
                                var depotTimeWindowData = linesOfVrp[i].Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                var depoId = int.Parse(depotTimeWindowData[0]);
                                var start = int.Parse(depotTimeWindowData[1]);
                                var end = int.Parse(depotTimeWindowData[2]);
                                var depoIndex = result.Depots.FindIndex(x => x.Id == depoId);
                                result.Depots[depoIndex].TimeWindow = new TimeWindow { End = end, Start = start };
                            }
                            i--;
                            break;
                        }
                    case "TIME_AVAIL_SECTION":
                        {
                            i++;
                            for (var j = 0; j < result.RequestsCount; j++, i++)
                            {
                                var requestAvailTimeData = linesOfVrp[i].Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                var requestId = int.Parse(requestAvailTimeData[0]);
                                var availableTime = int.Parse(requestAvailTimeData[1]);
                                var requestIndex = result.Requests.FindIndex(x => x.Id == requestId);
                                result.Requests[requestIndex].AvailableTime = availableTime;
                            }
                            i--;
                            break;
                        }
                    default:
                        break;
                }
            }

            return result;
        }
    }
}
