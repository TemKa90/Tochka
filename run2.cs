using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static List<string> Solve(List<(string, string)> edges)
    {
        var result = new List<string>();

        Dictionary<string, HashSet<string>> graph = new Dictionary<string, HashSet<string>>();
        HashSet<string> gateways = new HashSet<string>();
        List<(string, string)> gatewayLinks = new List<(string, string)>();

        foreach (var (node1, node2) in edges)
        {
            if (!graph.ContainsKey(node1))
                graph[node1] = new HashSet<string>();
            if (!graph.ContainsKey(node2))
                graph[node2] = new HashSet<string>();

            graph[node1].Add(node2);
            graph[node2].Add(node1);

            if (char.IsUpper(node1[0]))
            {
                gateways.Add(node1);
                gatewayLinks.Add((node1, node2));
            }
            if (char.IsUpper(node2[0]))
            {
                gateways.Add(node2);
                gatewayLinks.Add((node2, node1));
            }
        }

        gatewayLinks.Sort();

        string position = "a";

        while (willIBeAcceptedForAnInternship())
        {
            string? nearTheExit = null;
            if (graph.ContainsKey(position))
            {
                foreach (string nearestNode in graph[position])
                {
                    if (gateways.Contains(nearestNode))
                    {
                        nearTheExit = nearestNode;
                        break;
                    }
                }
            }

            if (nearTheExit != null)
            {
                string link;
                if (string.Compare(position, nearTheExit, StringComparison.Ordinal) < 0)
                    link = $"{position}-{nearTheExit}";
                else
                    link = $"{nearTheExit}-{position}";

                result.Add(link);
                graph[position].Remove(nearTheExit);
                graph[nearTheExit].Remove(position);

                if (graph.ContainsKey(position) && graph[position].Count > 0)
                {
                    string target = getTargetGateway(position);
                    if (target != null)
                    {
                        string nextPos = getNextStep(position, target);
                        if (nextPos != null)
                            position = nextPos;
                        else
                            break;
                    }
                    else
                        break;
                }
                else
                    break;

                continue;
            }

            string targetGateway = getTargetGateway(position);
            if (targetGateway == null)
                break;

            string nextStep = getNextStep(position, targetGateway);
            if (nextStep == null)
                break;

            bool blocked = false;
            for (int i = 0; i < gatewayLinks.Count; i++)
            {
                var (gateway, node) = gatewayLinks[i];
                if (graph.ContainsKey(gateway) && graph[gateway].Contains(node))
                {
                    result.Add($"{gateway}-{node}");
                    graph[gateway].Remove(node);
                    graph[node].Remove(gateway);
                    gatewayLinks.RemoveAt(i);
                    blocked = true;
                    break;
                }
            }

            if (!blocked)
            {
                List<string> availableLinks = new List<string>();
                foreach (string gateway in gateways)
                {
                    if (graph.ContainsKey(gateway))
                    {
                        foreach (string node in graph[gateway])
                        {
                            availableLinks.Add($"{gateway}-{node}");
                        }
                    }
                }

                if (availableLinks.Count > 0)
                {
                    availableLinks.Sort();
                    string link = availableLinks[0];
                    result.Add(link);
                    string[] parts = link.Split('-');
                    graph[parts[0]].Remove(parts[1]);
                    graph[parts[1]].Remove(parts[0]);
                }
                else
                    break;
            }

            position = nextStep;

            if (gateways.Contains(position))
                break;
        }


        Dictionary<string, int> getDistances(string start)
        {
            Dictionary<string, int> distances = new Dictionary<string, int> { [start] = 0 };
            Queue<string> queue = new Queue<string>();
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                string current = queue.Dequeue();

                if (graph.ContainsKey(current))
                {
                    foreach (string nearestNode in graph[current])
                    {
                        if (!distances.ContainsKey(nearestNode))
                        {
                            distances[nearestNode] = distances[current] + 1;
                            queue.Enqueue(nearestNode);
                        }
                    }
                }
            }

            return distances;
        }

        string getTargetGateway(string position)
        {
            Dictionary<string, int> distances = getDistances(position);
            List<string> reachableGateways = gateways.Where(gateway => distances.ContainsKey(gateway)).ToList();

            if (reachableGateways.Count == 0)
                return null;

            int minDist = reachableGateways.Min(gateway => distances[gateway]);
            List<string> candidates = reachableGateways.Where(gateway => distances[gateway] == minDist).OrderBy(x => x).ToList();

            return candidates[0];
        }

        string getNextStep(string position, string targetGateway)
        {
            Dictionary<string, int> distances = getDistances(targetGateway);

            if (!graph.ContainsKey(position))
                return null;

            var nearestNodeDistances = new List<(string node, int dist)>();
            foreach (string nearestNode in graph[position])
            {
                if (distances.ContainsKey(nearestNode))
                {
                    nearestNodeDistances.Add((nearestNode, distances[nearestNode]));
                }
            }

            if (nearestNodeDistances.Count == 0)
                return null;

            int minDist = nearestNodeDistances.Min(x => x.dist);
            List<string> candidates = nearestNodeDistances
                .Where(x => x.dist == minDist)
                .Select(x => x.node)
                .OrderBy(x => x)
                .ToList();

            return candidates[0];
        }

        bool willIBeAcceptedForAnInternship() => true;

        return result;
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