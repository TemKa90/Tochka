using System;
using System.Collections.Generic;
using System.Linq;

class run
{

    // для второго коммита

    class State
    {
        public char?[] Hall;
        public char[][] Rooms;

        public State(char?[] hall, char[][] rooms)
        {
            Hall = hall;
            Rooms = rooms;
        }


        public int GetHash()
        {
            HashCode hash = new HashCode();

            foreach (char? item in Hall)
                hash.Add(item);

            foreach (char[] room in Rooms)
                foreach (char item in room)
                    hash.Add(item);

            return hash.ToHashCode();
        }
    }

    static int Solve(List<string> lines)
    {
        HashSet<int> forbiddenStops = new HashSet<int> { 2, 4, 6, 8 };

        Dictionary<char, int> targetRooms = new()
        {
            ['A'] = 0,
            ['B'] = 1,
            ['C'] = 2,
            ['D'] = 3
        };

        Dictionary<char, int> moveCosts = new Dictionary<char, int>
        {
            ['A'] = 1,
            ['B'] = 10,
            ['C'] = 100,
            ['D'] = 1000
        };

        State initState = getInitState(lines);

        PriorityQueue<State, int> heap = new PriorityQueue<State, int>();
        Dictionary<int, int> visited = new Dictionary<int, int>();

        heap.Enqueue(initState, 0);
        visited[initState.GetHash()] = 0;

        while (heap.Count > 0)
        {
            heap.TryDequeue(out State current, out int currentCost);

            if (visited.TryGetValue(current.GetHash(), out int knownCost) && knownCost < currentCost)
                continue;

            if (isFinish(current))
                return currentCost;

            foreach (var (moveCost, nextState) in getPossibleMoves(current, targetRooms, moveCosts, forbiddenStops))
            {
                int newCost = currentCost + moveCost;
                int nextHash = nextState.GetHash();

                if (!visited.TryGetValue(nextHash, out int existingCost) || newCost < existingCost)
                {
                    visited[nextHash] = newCost;
                    heap.Enqueue(nextState, newCost);
                }
            }
        }

        throw new Exception(message: "Не удалось найти решение :(");

        State getInitState(List<string> lines)
        {
            int hallLength = lines[1].Length - 2;
            char?[] hall = new char?[hallLength];
            char[][] rooms = getRooms(lines);

            return new State(hall, rooms);
        }

        char[][] getRooms(List<string> lines)
        {
            List<string> roomLines = lines.Skip(2).Take(lines.Count - 3).ToList<string>();
            int roomDepth = roomLines.Count;
            int roomCount = 4;

            char[][] rooms = new char[roomCount][];
            for (int i = 0; i < roomCount; i++)
            {
                rooms[i] = new char[roomDepth];
            }

            for (int depth = 0; depth < roomDepth; depth++)
            {
                string line = roomLines[depth];
                string cleanLine = line.Trim().Replace("#", "");

                for (int roomIndex = 0; roomIndex < roomCount; roomIndex++)
                {
                    rooms[roomIndex][depth] = cleanLine[roomIndex];
                }
            }

            return rooms;
        }

        List<(int cost, State state)> getPossibleMoves(State state, Dictionary<char, int> targetRooms,
        Dictionary<char, int> moveCosts, HashSet<int> forbiddenStops)
        {
            List<(int, State)> moves = new List<(int, State)>();

            for (int roomIndex = 0; roomIndex < 4; roomIndex++)
            {
                int depth = findTopObject(state.Rooms[roomIndex]);
                if (depth == -1) continue;

                char objType = state.Rooms[roomIndex][depth];

                if (!canLeaveRoom(state, roomIndex, depth, objType, targetRooms))
                    continue;

                int roomHallPos = 2 + roomIndex * 2;

                foreach (int targetPos in new[] { 0, 1, 3, 5, 7, 9, 10 })
                {
                    if (isPathClear(state.Hall, roomHallPos, targetPos))
                    {
                        int steps = (depth + 1) + Math.Abs(targetPos - roomHallPos);
                        int cost = steps * moveCosts[objType];

                        State newState = createRoomToHallMove(state, roomIndex, depth, targetPos);
                        moves.Add((cost, newState));
                    }
                }
            }

            for (int hallPos = 0; hallPos < state.Hall.Length; hallPos++)
            {
                if (state.Hall[hallPos] == null || forbiddenStops.Contains(hallPos))
                    continue;

                char objType = state.Hall[hallPos].Value;
                int targetRoomIndex = targetRooms[objType];

                if (!canEnterRoom(state, targetRoomIndex, objType))
                    continue;

                int targetRoomHallPos = 2 + targetRoomIndex * 2;

                if (!isPathClear(state.Hall, hallPos, targetRoomHallPos))
                    continue;

                int targetDepth = findBottomFreeSlot(state.Rooms[targetRoomIndex]);
                if (targetDepth == -1) continue;

                int steps = Math.Abs(hallPos - targetRoomHallPos) + (targetDepth + 1);
                int cost = steps * moveCosts[objType];

                State newState = createHallToRoomMove(state, hallPos, targetRoomIndex, targetDepth);
                moves.Add((cost, newState));
            }

            return moves;
        }

        bool canEnterRoom(State state, int roomIndex, char objType)
        {
            char[] room = state.Rooms[roomIndex];

            for (int i = 0; i < room.Length; i++)
            {
                if (room[i] != '\0' && room[i] != objType)
                    return false;
            }

            return true;
        }

        int findBottomFreeSlot(char[] room)
        {
            for (int i = room.Length - 1; i >= 0; i--)
            {
                if (room[i] == '\0')
                    return i;
            }
            return -1;
        }

        State createHallToRoomMove(State state, int hallPos, int roomIndex, int depth)
        {
            char?[] newHall = new char?[state.Hall.Length];
            Array.Copy(state.Hall, newHall, state.Hall.Length);

            char[][] newRooms = new char[state.Rooms.Length][];
            for (int i = 0; i < state.Rooms.Length; i++)
            {
                newRooms[i] = new char[state.Rooms[i].Length];
                Array.Copy(state.Rooms[i], newRooms[i], state.Rooms[i].Length);
            }

            char objType = newHall[hallPos].Value;
            newHall[hallPos] = null;
            newRooms[roomIndex][depth] = objType;

            return new State(newHall, newRooms);
        }

        int findTopObject(char[] room)
        {
            for (int i = 0; i < room.Length; i++)
                if (room[i] != '\0') return i;
            return -1;
        }

        bool canLeaveRoom(State state, int roomIndex, int depth, char objType, Dictionary<char, int> targetRooms)
        {
            if (targetRooms[objType] != roomIndex)
                return true;

            for (int i = depth + 1; i < state.Rooms[roomIndex].Length; i++)
                if (state.Rooms[roomIndex][i] != objType)
                    return true;

            return false;
        }

        bool isPathClear(char?[] hall, int from, int to)
        {
            int step = to > from ? 1 : -1;
            for (int pos = from + step; pos != to + step; pos += step)
                if (hall[pos] != null)
                    return false;
            return true;
        }

        State createRoomToHallMove(State state, int roomIndex, int depth, int hallPos)
        {
            char?[] newHall = new char?[state.Hall.Length];
            Array.Copy(state.Hall, newHall, state.Hall.Length);

            char[][] newRooms = new char[state.Rooms.Length][];
            for (int i = 0; i < state.Rooms.Length; i++)
            {
                newRooms[i] = new char[state.Rooms[i].Length];
                Array.Copy(state.Rooms[i], newRooms[i], state.Rooms[i].Length);
            }

            char objType = newRooms[roomIndex][depth];
            newHall[hallPos] = objType;
            newRooms[roomIndex][depth] = '\0';

            return new State(newHall, newRooms);
        }

        bool isFinish(State state)
        {
            if (state.Hall.Any(x => x != null)) return false;

            for (int i = 0; i < state.Rooms.Length; i++)
            {
                char[] room = state.Rooms[i];
                if (room.Any(objType => targetRooms[objType] != i)) return false;
            }
            return true;
        }
    }


    static void Main2()
    {
        var lines = new List<string>();
        string line;

        while ((line = Console.ReadLine()) != null)
        {
            lines.Add(line);
        }

        int result = Solve(lines);
        Console.WriteLine(result);
    }
}