using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

class Program
{

    static List<string> Solve(List<(string, string)> edges)
    {
        var result = new List<string>();

        string position = "a";
        List<(string, string)> gateways = getGateways(edges);
        (string, string) nearestGateway = getNearestGateway(position);


        while (gateways.Count > 1)
        {
            gateways.Remove(nearestGateway);
            result.Add(string.Format("{0}-{1}", nearestGateway.Item1, nearestGateway.Item2));
            position = virusStep(position);
            nearestGateway = getNearestGateway(position);
        }

        result.Add(string.Format("{0}-{1}", nearestGateway.Item1, nearestGateway.Item2));

        return result;



        List<(string, string)> getGateways(List<(string, string)> edges)
        {
            List<(string, string)> gateways = new List<(string, string)>();

            foreach ((string, string) edge in edges)
            {
                if (char.IsUpper(edge.Item1[0]) || char.IsUpper(edge.Item2[0]))
                {
                    if (char.IsUpper(edge.Item2[0]))
                        gateways.Add((edge.Item2, edge.Item1));
                    else
                        gateways.Add(edge);
                }
            }

            return gateways;
        }

        (string, string) getNearestGateway(string position)
        {
            (string, string) nearestGateway = ("", "");
            int minDistance = int.MaxValue;

            foreach ((string, string) gateway in gateways)
            {
                string exit = gateway.Item1;
                string node = gateway.Item2;

                int distance = getDistance(position, node);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestGateway = gateway;
                }
                else if (distance == minDistance)
                {
                    if (exit.CompareTo(nearestGateway.Item1) < 0)
                    {
                        nearestGateway = gateway;
                    }
                }
            }

            return nearestGateway;
        }

        int getDistance(string start, string end)
        {
            HashSet<string> visited = new HashSet<string>();
            var queue = new Queue<(string node, int distance)>();

            queue.Enqueue((start, 0));
            visited.Add(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (current.node == end)
                    return current.distance;

                foreach ((string, string) edge in edges)
                {
                    string neighbor = null;
                    if (edge.Item1 == current.node && !visited.Contains(edge.Item2))
                    {
                        neighbor = edge.Item2;
                    }
                    else if (edge.Item2 == current.node && !visited.Contains(edge.Item1))
                    {
                        neighbor = edge.Item1;
                    }

                    if (neighbor != null)
                    {
                        visited.Add(neighbor);
                        queue.Enqueue((neighbor, current.distance + 1));
                    }
                }
            }

            return int.MaxValue;
        }

        string virusStep(string position)
        {
            (string, string) nearestGateway = getNearestGateway(position);
            string targetGateway = nearestGateway.Item1;

            List<string> path = findShortestPath(position, targetGateway);

            if (path.Count > 1)
                return path[1];
            else
                return position;
        }

        List<string> findShortestPath(string start, string end)
        {
            HashSet<string> visited = new HashSet<string>();
            Dictionary<string, string> previous = new Dictionary<string, string>();
            Queue<string> queue = new Queue<string>();

            queue.Enqueue(start);
            visited.Add(start);
            previous[start] = null;

            while (queue.Count > 0)
            {
                string current = queue.Dequeue();

                if (current == end)
                {
                    List<string> path = new List<string>();
                    string node = end;
                    while (node != null)
                    {
                        path.Add(node);
                        node = previous[node];
                    }
                    path.Reverse();
                    return path;
                }

                foreach ((string, string) edge in edges)
                {
                    string neighbor = null;
                    if (edge.Item1 == current && !visited.Contains(edge.Item2))
                    {
                        neighbor = edge.Item2;
                    }
                    else if (edge.Item2 == current && !visited.Contains(edge.Item1))
                    {
                        neighbor = edge.Item1;
                    }

                    if (neighbor != null)
                    {
                        visited.Add(neighbor);
                        previous[neighbor] = current;
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return new List<string>();
        }
    }

    static void Main()
    {
        var edges = new List<(string, string)>();
        string line;

        while ((line = Console.ReadLine()) != null)
        {
            line = line.Trim();
            if (!string.IsNullOrEmpty(line))
            {
                var parts = line.Split('-');
                if (parts.Length == 2)
                {
                    edges.Add((parts[0], parts[1]));
                }
            }
        }

        var result = Solve(edges);
        foreach (var edge in result)
        {
            Console.WriteLine(edge);
        }
    }
}